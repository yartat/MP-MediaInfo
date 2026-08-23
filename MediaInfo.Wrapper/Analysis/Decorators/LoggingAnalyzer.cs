#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using Microsoft.Extensions.Logging;

namespace MediaInfo.Analysis.Decorators;

/// <summary>
/// Records what an analysis did, without the analysis having to know how to log.
/// </summary>
/// <remarks>
/// A failure is logged as a warning rather than an error, because a media that cannot be described is an expected
/// outcome of asking about an arbitrary file. Cancellation is logged and rethrown.
/// </remarks>
public sealed class LoggingAnalyzer : MediaInfoAnalyzerDecorator
{
  private readonly ILogger _logger;

  /// <summary>
  /// Initializes a new instance of the <see cref="LoggingAnalyzer"/> class.
  /// </summary>
  /// <param name="inner">The analyzer to wrap.</param>
  /// <param name="logger">The logger that records the analysis.</param>
  /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/>.</exception>
  public LoggingAnalyzer(IMediaInfoAnalyzer inner, ILogger logger)
    : base(inner)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <inheritdoc />
  public override async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    CancellationToken cancellationToken = default)
  {
    var name = source?.DisplayName ?? "<none>";
    _logger.LogDebug("Analyzing {kind} media {media}.", source?.Kind, name);

    try
    {
      var result = await Inner.AnalyzeAsync(source!, cancellationToken).ConfigureAwait(false);

      if (result.Success)
      {
        _logger.LogDebug(
          "Analyzed {media} in {elapsed}. Video={video}, Audio={audio}, Subtitle={subtitle}, Chapters={chapters}, Duration={duration}.",
          name,
          result.Elapsed,
          result.VideoStreams.Count,
          result.AudioStreams.Count,
          result.Subtitles.Count,
          result.Chapters.Count,
          result.General.Duration);

        if (result.Disc is not null)
        {
          _logger.LogDebug(
            "{kind} structure at {root} holds {titles} title(s), {size} bytes. Main title read from {file}.",
            result.Disc.Kind,
            result.Disc.RootPath,
            result.Disc.Titles.Count,
            result.Disc.TotalSize,
            result.AnalyzedPath);
        }
      }
      else
      {
        _logger.LogWarning(
          result.Failure?.Exception,
          "Could not analyze {media}: {reason} — {message}",
          name,
          result.Failure?.Reason,
          result.Failure?.Message);
      }

      return result;
    }
    catch (OperationCanceledException)
    {
      _logger.LogDebug("Analysis of {media} was cancelled.", name);
      throw;
    }
  }
}
