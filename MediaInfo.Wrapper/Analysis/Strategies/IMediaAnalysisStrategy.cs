#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Pipeline;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;

namespace MediaInfo.Analysis.Strategies;

/// <summary>
/// Carries the services and settings an analysis strategy needs.
/// </summary>
public sealed class MediaAnalysisContext
{
  /// <summary>
  /// Initializes a new instance of the <see cref="MediaAnalysisContext"/> class.
  /// </summary>
  /// <param name="probe">The probe that opens media through the native library.</param>
  /// <param name="fileSystem">The file system the analysis reads from.</param>
  /// <param name="options">The options that describe how the analysis should be performed.</param>
  /// <param name="progress">The receiver of progress reports, when the caller asked for them.</param>
  /// <exception cref="ArgumentNullException"><paramref name="probe"/> or <paramref name="fileSystem"/> is <see langword="null"/>.</exception>
  public MediaAnalysisContext(
    MediaProbe probe,
    IFileSystem fileSystem,
    MediaAnalysisOptions? options = null,
    IProgress<AnalysisProgress>? progress = null)
  {
    Probe = probe ?? throw new ArgumentNullException(nameof(probe));
    FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    Options = options ?? MediaAnalysisOptions.Default;
    Progress = progress;
  }

  /// <summary>
  /// Gets the probe that opens media through the native library.
  /// </summary>
  public MediaProbe Probe { get; }

  /// <summary>
  /// Gets the file system the analysis reads from.
  /// </summary>
  public IFileSystem FileSystem { get; }

  /// <summary>
  /// Gets the options that describe how the analysis should be performed.
  /// </summary>
  public MediaAnalysisOptions Options { get; }

  /// <summary>
  /// Gets the receiver of progress reports, when the caller asked for them.
  /// </summary>
  public IProgress<AnalysisProgress>? Progress { get; }

  /// <summary>
  /// Reports the progress of the analysis, when the caller asked for reports.
  /// </summary>
  /// <param name="progress">The progress to report.</param>
  public void Report(AnalysisProgress progress) => Progress?.Report(progress);
}

/// <summary>
/// Analyzes one kind of media.
/// </summary>
/// <remarks>
/// Registering a strategy is the only thing needed to support a new kind of media. No existing strategy has to change.
/// </remarks>
public interface IMediaAnalysisStrategy
{
  /// <summary>
  /// Gets the order the strategy is considered in. The strategy with the lowest value that can handle a media wins.
  /// </summary>
  int Priority { get; }

  /// <summary>
  /// Determines whether the strategy can analyze the specified media.
  /// </summary>
  /// <param name="source">The media to analyze.</param>
  /// <returns>Returns <see langword="true"/> when the strategy can analyze the media; otherwise, <see langword="false"/>.</returns>
  bool CanHandle(IMediaSource source);

  /// <summary>
  /// Analyzes the specified media.
  /// </summary>
  /// <param name="source">The media to analyze.</param>
  /// <param name="context">The services and settings the analysis needs.</param>
  /// <param name="cancellationToken">The token that cancels the analysis.</param>
  /// <returns>Returns the outcome of the analysis.</returns>
  Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    MediaAnalysisContext context,
    CancellationToken cancellationToken);
}

/// <summary>
/// Chooses the strategy that analyzes a media.
/// </summary>
public interface IMediaAnalysisStrategySelector
{
  /// <summary>
  /// Chooses the strategy that analyzes the specified media.
  /// </summary>
  /// <param name="source">The media to analyze.</param>
  /// <returns>Returns the chosen strategy, or <see langword="null"/> when no strategy can analyze the media.</returns>
  IMediaAnalysisStrategy? Select(IMediaSource source);
}

/// <summary>
/// Chooses the first strategy that can analyze a media, in ascending order of <see cref="IMediaAnalysisStrategy.Priority"/>.
/// </summary>
public sealed class PriorityStrategySelector : IMediaAnalysisStrategySelector
{
  private readonly IMediaAnalysisStrategy[] _strategies;

  /// <summary>
  /// Initializes a new instance of the <see cref="PriorityStrategySelector"/> class.
  /// </summary>
  /// <param name="strategies">The strategies to choose from.</param>
  /// <exception cref="ArgumentNullException"><paramref name="strategies"/> is <see langword="null"/>.</exception>
  public PriorityStrategySelector(IEnumerable<IMediaAnalysisStrategy> strategies)
  {
    if (strategies is null)
    {
      throw new ArgumentNullException(nameof(strategies));
    }

    _strategies = strategies.OrderBy(x => x.Priority).ToArray();
  }

  /// <summary>
  /// Gets the strategies in the order they are considered.
  /// </summary>
  public IReadOnlyList<IMediaAnalysisStrategy> Strategies => _strategies;

  /// <inheritdoc />
  public IMediaAnalysisStrategy? Select(IMediaSource source)
  {
    if (source is null)
    {
      return null;
    }

    foreach (var strategy in _strategies)
    {
      if (strategy.CanHandle(source))
      {
        return strategy;
      }
    }

    return null;
  }
}
