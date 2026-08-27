#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Discs;
using MediaInfo.Analysis.Native;
using MediaInfo.Analysis.Pipeline;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Strategies;
using MediaInfo.Analysis.Strategies.Selection;

namespace MediaInfo.Analysis;

/// <summary>
/// Analyzes a media by handing it to the strategy that understands its kind.
/// </summary>
/// <remarks>
/// This is the core of the pipeline and deliberately does nothing beyond choosing a strategy and running it. Logging,
/// caching, retrying, timeouts and concurrency limits are added by wrapping an instance of this class.
/// </remarks>
public sealed class MediaInfoAnalyzer : IMediaInfoAnalyzer
{
  private readonly IMediaAnalysisStrategySelector _selector;
  private readonly MediaAnalysisContext _context;

  /// <summary>
  /// Initializes a new instance of the <see cref="MediaInfoAnalyzer"/> class.
  /// </summary>
  /// <param name="selector">The selector that chooses the strategy for a media.</param>
  /// <param name="context">The services and settings the strategies need.</param>
  /// <exception cref="ArgumentNullException"><paramref name="selector"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
  public MediaInfoAnalyzer(IMediaAnalysisStrategySelector selector, MediaAnalysisContext context)
  {
    _selector = selector ?? throw new ArgumentNullException(nameof(selector));
    _context = context ?? throw new ArgumentNullException(nameof(context));
  }

  /// <summary>
  /// Creates an analyzer that uses the native library, the local file system and the default strategies.
  /// </summary>
  /// <param name="options">The options that describe how an analysis should be performed.</param>
  /// <param name="progress">The receiver of progress reports, when the caller wants them.</param>
  /// <returns>Returns a new analyzer.</returns>
  public static MediaInfoAnalyzer CreateDefault(
    MediaAnalysisOptions? options = null,
    IProgress<AnalysisProgress>? progress = null) =>
    Create(
      MediaInfoLibFactory.Instance,
      MediaInfoFileSystem.Instance,
      DefaultStreamSelectionStrategy.Instance,
      options,
      progress);

  /// <summary>
  /// Creates an analyzer that uses the specified services and the default strategies.
  /// </summary>
  /// <param name="nativeFactory">The factory that creates media handles.</param>
  /// <param name="fileSystem">The file system the analysis reads from.</param>
  /// <param name="selection">The strategy that selects the streams that best represent a media.</param>
  /// <param name="options">The options that describe how an analysis should be performed.</param>
  /// <param name="progress">The receiver of progress reports, when the caller wants them.</param>
  /// <returns>Returns a new analyzer.</returns>
  /// <exception cref="ArgumentNullException">A required service is <see langword="null"/>.</exception>
  public static MediaInfoAnalyzer Create(
    INativeMediaInfoFactory nativeFactory,
    IFileSystem fileSystem,
    IStreamSelectionStrategy? selection = null,
    MediaAnalysisOptions? options = null,
    IProgress<AnalysisProgress>? progress = null)
  {
    if (nativeFactory is null)
    {
      throw new ArgumentNullException(nameof(nativeFactory));
    }

    if (fileSystem is null)
    {
      throw new ArgumentNullException(nameof(fileSystem));
    }

    var probe = new MediaProbe(nativeFactory, selection ?? DefaultStreamSelectionStrategy.Instance);
    var context = new MediaAnalysisContext(probe, fileSystem, options, progress);
    return new MediaInfoAnalyzer(new PriorityStrategySelector(CreateDefaultStrategies(fileSystem)), context);
  }

  /// <summary>
  /// Creates the strategies that are registered by default.
  /// </summary>
  /// <param name="fileSystem">The file system the disc strategies read from.</param>
  /// <returns>Returns the default strategies.</returns>
  public static IEnumerable<IMediaAnalysisStrategy> CreateDefaultStrategies(IFileSystem fileSystem) =>
  [
    new UnsupportedSourceStrategy(),

    // The navigation tables describe a disc far better than its folder layout does, but they have to be present
    // and well formed. Pairing each parser with the folder reader means a disc is always described, as well as
    // the disc allows.
    new DvdAnalysisStrategy(
      new FallbackDiscStructureReader(
        new IfoDvdStructureReader(fileSystem),
        new DvdStructureReader(fileSystem))),
    new BluRayAnalysisStrategy(
      new FallbackDiscStructureReader(
        new MplsBluRayStructureReader(fileSystem),
        new BluRayStructureReader(fileSystem))),

    new NetworkStreamAnalysisStrategy(),
    new SingleFileAnalysisStrategy(),
    new StreamAnalysisStrategy()
  ];

  /// <inheritdoc />
  public async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    CancellationToken cancellationToken = default)
  {
    if (source is null)
    {
      return MediaAnalysisResult.Failed(
        null,
        AnalysisFailureReason.SourceNotSpecified,
        "The media to analyze must be specified.");
    }

    var strategy = _selector.Select(source);
    if (strategy is null)
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.UnsupportedSource,
        $"No analysis strategy handles a media of kind {source.Kind}.");
    }

    var stopwatch = Stopwatch.StartNew();
    try
    {
      var result = await strategy.AnalyzeAsync(source, _context, cancellationToken).ConfigureAwait(false);
      return result with { Elapsed = stopwatch.Elapsed };
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      throw;
    }
    catch (Exception exception)
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.Unknown,
        $"The analysis of '{source.DisplayName}' failed: {exception.Message}",
        exception)
        with
      { Elapsed = stopwatch.Elapsed };
    }
  }
}
