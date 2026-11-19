#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;

namespace MediaInfo.Analysis.Decorators;

/// <summary>
/// Returns the stored outcome of an earlier analysis of the same media instead of reading it again.
/// </summary>
/// <remarks>
/// Only a file and a disc folder are cached, and only when the analysis succeeded.
/// <list type="bullet">
///   <item>A file is keyed by its path, size and last write time, so an edit invalidates the entry.</item>
///   <item>
///     A disc folder is keyed by its path and its own last write time. The content of a disc does not change, so this
///     is sound for the case the cache exists to serve; a folder whose files are edited in place without the folder
///     itself being touched would keep a stale entry.
///   </item>
///   <item>A stream cannot be identified without consuming it, and a network media has no cheap validator, so both are passed straight through.</item>
///   <item>A failed analysis is never stored, because the condition that caused it is usually temporary.</item>
/// </list>
/// Callers who ask for the same media while it is being analyzed share that one analysis. It is cancelled only when
/// every one of them has given up. Every caller receives its own copy of the result, so changing a stream does not
/// change what the cache hands to the next caller.
/// </remarks>
public sealed class CachingAnalyzer : MediaInfoAnalyzerDecorator
{
  private readonly IAnalysisCache _cache;
  private readonly IFileSystem _fileSystem;
  private readonly object _gate = new();
  private readonly Dictionary<string, Flight> _flights = new(StringComparer.Ordinal);

  /// <summary>
  /// Initializes a new instance of the <see cref="CachingAnalyzer"/> class.
  /// </summary>
  /// <param name="inner">The analyzer to wrap.</param>
  /// <param name="cache">The store the outcomes are kept in.</param>
  /// <param name="fileSystem">The file system the keys are derived from.</param>
  /// <exception cref="ArgumentNullException"><paramref name="cache"/> or <paramref name="fileSystem"/> is <see langword="null"/>.</exception>
  public CachingAnalyzer(IMediaInfoAnalyzer inner, IAnalysisCache cache, IFileSystem fileSystem)
    : base(inner)
  {
    _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
  }

  /// <inheritdoc />
  public override async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    CancellationToken cancellationToken = default)
  {
    var key = TryCreateKey(source);
    if (key is null)
    {
      return await Inner.AnalyzeAsync(source, cancellationToken).ConfigureAwait(false);
    }

    if (_cache.TryGet(key, out var cached))
    {
      return ResultCloner.Clone(cached);
    }

    cancellationToken.ThrowIfCancellationRequested();
    var flight = Join(key, out var created);
    if (created)
    {
      _ = RunAsync(key, source, flight);
    }

    var result = await WaitAsync(flight, cancellationToken).ConfigureAwait(false);
    return ResultCloner.Clone(result);
  }

  // Never throws: the outcome goes to the flight, which every waiter reads.
  private async Task RunAsync(string key, IMediaSource source, Flight flight)
  {
    await Task.Yield();
    try
    {
      var result = await Inner.AnalyzeAsync(source, flight.Token).ConfigureAwait(false);
      if (result.Success)
      {
        // The stored object is never handed out, only copies of it.
        _cache.Set(key, ResultCloner.Clone(result));
      }

      flight.Completion.TrySetResult(result);
    }
    catch (OperationCanceledException exception)
    {
      flight.Completion.TrySetCanceled(exception.CancellationToken);
    }
    catch (Exception exception)
    {
      flight.Completion.TrySetException(exception);
    }
    finally
    {
      lock (_gate)
      {
        if (_flights.TryGetValue(key, out var current) && ReferenceEquals(current, flight))
        {
          _flights.Remove(key);
        }
      }

      flight.Dispose();
    }
  }

  private Flight Join(string key, out bool created)
  {
    lock (_gate)
    {
      created = !_flights.TryGetValue(key, out var flight);
      if (created)
      {
        flight = new Flight(key);
        _flights[key] = flight;
      }

      flight!.Waiters++;
      return flight;
    }
  }

  private void Leave(string key, Flight flight)
  {
    lock (_gate)
    {
      if (--flight.Waiters > 0)
      {
        return;
      }

      // Nobody is waiting for it any more: stop the analysis, and let the next caller start a new one.
      if (_flights.TryGetValue(key, out var current) && ReferenceEquals(current, flight))
      {
        _flights.Remove(key);
      }

      flight.Cancel();
    }
  }

  private async Task<MediaAnalysisResult> WaitAsync(Flight flight, CancellationToken cancellationToken)
  {
    var key = flight.Key;
    var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    using (cancellationToken.Register(static state => ((TaskCompletionSource<bool>)state!).TrySetResult(true), cancelled))
    {
      if (await Task.WhenAny(flight.Completion.Task, cancelled.Task).ConfigureAwait(false) != flight.Completion.Task)
      {
        Leave(key, flight);
        cancellationToken.ThrowIfCancellationRequested();
      }
    }

    Leave(key, flight);
    return await flight.Completion.Task.ConfigureAwait(false);
  }

  private sealed class Flight : IDisposable
  {
    private readonly CancellationTokenSource _cancellation = new();

    public Flight(string key) => Key = key;

    public string Key { get; }

    public int Waiters { get; set; }

    public CancellationToken Token => _cancellation.Token;

    public TaskCompletionSource<MediaAnalysisResult> Completion { get; } =
      new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void Cancel()
    {
      try
      {
        _cancellation.Cancel();
      }
      catch (ObjectDisposedException)
      {
      }
    }

    public void Dispose() => _cancellation.Dispose();
  }

  // A path the file system cannot make sense of is not worth caching, and is left for the analysis to report.
  private string? TryCreateKey(IMediaSource source)
  {
    try
    {
      return CreateKey(source);
    }
    catch (Exception exception) when (
      exception is ArgumentException or NotSupportedException or IOException or UnauthorizedAccessException)
    {
      return null;
    }
  }

  private string? CreateKey(IMediaSource source)
  {
    switch (source)
    {
      case FileMediaSource file when !string.IsNullOrWhiteSpace(file.Path):
        return FormattableString.Invariant(
          $"file|{file.Path}|{_fileSystem.GetFileLength(file.Path)}|{_fileSystem.GetLastWriteTimeUtc(file.Path).UtcTicks}");

      case DirectoryMediaSource directory when !string.IsNullOrWhiteSpace(directory.Path):
        return FormattableString.Invariant(
          $"dir|{directory.Path}|{_fileSystem.GetLastWriteTimeUtc(directory.Path).UtcTicks}");

      default:
        return null;
    }
  }
}
