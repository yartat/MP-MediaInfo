#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Native;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Strategies.Selection;

namespace MediaInfo.Analysis.Pipeline;

/// <summary>
/// Opens a media through the native library and reads what it contains.
/// </summary>
/// <remarks>
/// Every operation creates its own handle and disposes it before returning, because a handle of the library is not
/// thread safe and holding one open across an await would allow two analyses to share it.
/// </remarks>
public sealed class MediaProbe
{
  private readonly INativeMediaInfoFactory _factory;
  private readonly MediaStreamCollector _collector;

  /// <summary>
  /// Initializes a new instance of the <see cref="MediaProbe"/> class.
  /// </summary>
  /// <param name="factory">The factory that creates media handles.</param>
  /// <param name="selection">The strategy that selects the streams that best represent a media.</param>
  /// <exception cref="ArgumentNullException"><paramref name="factory"/> or <paramref name="selection"/> is <see langword="null"/>.</exception>
  public MediaProbe(INativeMediaInfoFactory factory, IStreamSelectionStrategy selection)
  {
    _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    _collector = new MediaStreamCollector(selection ?? throw new ArgumentNullException(nameof(selection)));
  }

  /// <summary>
  /// Opens the specified media and reads the streams it contains.
  /// </summary>
  /// <param name="path">The path or location of the media.</param>
  /// <param name="options">The options that describe how the analysis should be performed.</param>
  /// <param name="knownSize">The size of the media when it is already known, or 0 to read it from the media.</param>
  /// <param name="cancellationToken">The token that cancels the analysis.</param>
  /// <returns>Returns the outcome of the analysis.</returns>
  public Task<MediaAnalysisResult> OpenAndCollectAsync(
    string path,
    MediaAnalysisOptions options,
    long knownSize = 0L,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();
    return Run(() => OpenAndCollect(path, knownSize), options, cancellationToken);
  }

  /// <summary>
  /// Opens the specified media and reads only its duration.
  /// </summary>
  /// <remarks>
  /// This is used to rank the titles of a disc without paying for a full analysis of every one of them.
  /// </remarks>
  /// <param name="path">The path of the media.</param>
  /// <param name="options">The options that describe how the analysis should be performed.</param>
  /// <param name="cancellationToken">The token that cancels the analysis.</param>
  /// <returns>Returns the duration of the media, or <see cref="TimeSpan.Zero"/> when it could not be determined.</returns>
  public Task<TimeSpan> ProbeDurationAsync(
    string path,
    MediaAnalysisOptions options,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();
    return Run(() => ProbeDuration(path), options, cancellationToken);
  }

  /// <summary>
  /// Creates a media handle.
  /// </summary>
  /// <returns>Returns a new media handle that the caller owns.</returns>
  public INativeMediaInfo CreateHandle() => _factory.Create();

  /// <summary>
  /// Reads the streams of a media that has already been opened.
  /// </summary>
  /// <param name="reader">The opened media.</param>
  /// <param name="knownSize">The size of the media when it is already known, or 0 to read it from the media.</param>
  /// <returns>Returns the outcome of the analysis.</returns>
  public MediaAnalysisResult Collect(IMediaInfoReader reader, long knownSize = 0L) => _collector.Collect(reader, knownSize);

  private static Task<T> Run<T>(Func<T> action, MediaAnalysisOptions options, CancellationToken cancellationToken) =>
    options.OffloadBlockingCalls
      ? Task.Run(action, cancellationToken)
      : Task.FromResult(action());

  private MediaAnalysisResult OpenAndCollect(string path, long knownSize)
  {
    using var handle = _factory.Create();
    if (!handle.IsAvailable)
    {
      return new MediaAnalysisResult
      {
        Success = false,
        Failure = new AnalysisFailure(
          AnalysisFailureReason.NativeLibraryUnavailable,
          "The MediaInfo native library could not be loaded.")
      };
    }

    var version = handle.LibraryVersion;
    if (!handle.Open(path))
    {
      return new MediaAnalysisResult
      {
        Success = false,
        LibraryVersion = version,
        Failure = new AnalysisFailure(
          AnalysisFailureReason.NativeOpenFailed,
          $"The MediaInfo native library could not open '{path}'.")
      };
    }

    return _collector.Collect(handle, knownSize) with { LibraryVersion = version, AnalyzedPath = path };
  }

  private TimeSpan ProbeDuration(string path)
  {
    using var handle = _factory.Create();
    return handle.IsAvailable && handle.Open(path) ? DurationReader.Read(handle) : TimeSpan.Zero;
  }
}
