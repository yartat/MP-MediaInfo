#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;

namespace MediaInfo.Analysis.Tests.Fakes;

/// <summary>
/// An analyzer that records what it was asked and answers with whatever the test told it to.
/// </summary>
public sealed class StubAnalyzer : IMediaInfoAnalyzer, IDisposable
{
  private readonly Func<IMediaSource, int, CancellationToken, Task<MediaAnalysisResult>> _behaviour;

  /// <summary>Initializes an analyzer that always reports success.</summary>
  public StubAnalyzer()
    : this((_, _, _) => Task.FromResult(new MediaAnalysisResult { Success = true }))
  {
  }

  /// <summary>Initializes an analyzer that answers with the given result.</summary>
  public StubAnalyzer(MediaAnalysisResult result)
    : this((_, _, _) => Task.FromResult(result))
  {
  }

  /// <summary>Initializes an analyzer that answers by invoking the given behaviour with the call ordinal.</summary>
  public StubAnalyzer(Func<IMediaSource, int, CancellationToken, Task<MediaAnalysisResult>> behaviour)
  {
    _behaviour = behaviour;
  }

  /// <summary>Gets the media the analyzer was asked about, in order.</summary>
  public List<IMediaSource> Calls { get; } = [];

  /// <summary>Gets the number of times the analyzer was called.</summary>
  public int CallCount => Calls.Count;

  /// <summary>Gets the greatest number of calls that were in flight at the same time.</summary>
  public int PeakConcurrency { get; private set; }

  /// <summary>Gets a value indicating whether the analyzer was disposed.</summary>
  public bool IsDisposed { get; private set; }

  private int _inFlight;

  /// <inheritdoc />
  public async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    CancellationToken cancellationToken = default)
  {
    int ordinal;
    lock (Calls)
    {
      Calls.Add(source);
      ordinal = Calls.Count;
    }

    var current = Interlocked.Increment(ref _inFlight);
    lock (Calls)
    {
      if (current > PeakConcurrency)
      {
        PeakConcurrency = current;
      }
    }

    try
    {
      return await _behaviour(source, ordinal, cancellationToken).ConfigureAwait(false);
    }
    finally
    {
      Interlocked.Decrement(ref _inFlight);
    }
  }

  /// <inheritdoc />
  public void Dispose() => IsDisposed = true;
}
