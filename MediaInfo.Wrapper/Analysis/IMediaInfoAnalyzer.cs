#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;

namespace MediaInfo.Analysis;

/// <summary>
/// Analyzes a media and describes the streams it contains.
/// </summary>
/// <remarks>
/// An analyzer never throws for a media it cannot read. A media that does not exist, is of an unsupported kind or
/// cannot be opened produces a result whose <see cref="MediaAnalysisResult.Failure"/> states why. Cancellation is the
/// exception to this and is surfaced as an <see cref="OperationCanceledException"/>.
/// </remarks>
public interface IMediaInfoAnalyzer
{
  /// <summary>
  /// Analyzes the specified media.
  /// </summary>
  /// <param name="source">The media to analyze.</param>
  /// <param name="cancellationToken">The token that cancels the analysis.</param>
  /// <returns>Returns the outcome of the analysis.</returns>
  Task<MediaAnalysisResult> AnalyzeAsync(IMediaSource source, CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides convenience overloads for <see cref="IMediaInfoAnalyzer"/>.
/// </summary>
public static class MediaInfoAnalyzerExtensions
{
  /// <summary>
  /// Analyzes the media at the specified path or location.
  /// </summary>
  /// <remarks>
  /// A folder that holds a VIDEO_TS or BDMV structure is analyzed as a disc.
  /// </remarks>
  /// <param name="analyzer">The analyzer.</param>
  /// <param name="pathOrLocation">The path of a file or folder, or the location of a network media.</param>
  /// <param name="cancellationToken">The token that cancels the analysis.</param>
  /// <returns>Returns the outcome of the analysis.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="analyzer"/> is <see langword="null"/>.</exception>
  public static Task<MediaAnalysisResult> AnalyzeAsync(
    this IMediaInfoAnalyzer analyzer,
    string pathOrLocation,
    CancellationToken cancellationToken = default)
  {
    if (analyzer is null)
    {
      throw new ArgumentNullException(nameof(analyzer));
    }

    if (string.IsNullOrWhiteSpace(pathOrLocation))
    {
      return Task.FromResult(
        MediaAnalysisResult.Failed(
          null,
          AnalysisFailureReason.SourceNotSpecified,
          "The path or location of the media must be specified."));
    }

    return analyzer.AnalyzeAsync(MediaSource.From(pathOrLocation), cancellationToken);
  }

  /// <summary>
  /// Analyzes the media held by the specified stream.
  /// </summary>
  /// <param name="analyzer">The analyzer.</param>
  /// <param name="stream">The stream that holds the media data.</param>
  /// <param name="leaveOpen">
  /// <see langword="true"/> to leave the stream open after the analysis; otherwise, <see langword="false"/>.
  /// </param>
  /// <param name="cancellationToken">The token that cancels the analysis.</param>
  /// <returns>Returns the outcome of the analysis.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="analyzer"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
  public static Task<MediaAnalysisResult> AnalyzeAsync(
    this IMediaInfoAnalyzer analyzer,
    Stream stream,
    bool leaveOpen = true,
    CancellationToken cancellationToken = default)
  {
    if (analyzer is null)
    {
      throw new ArgumentNullException(nameof(analyzer));
    }

    if (stream is null)
    {
      throw new ArgumentNullException(nameof(stream));
    }

    return analyzer.AnalyzeAsync(MediaSource.From(stream, leaveOpen), cancellationToken);
  }
}
