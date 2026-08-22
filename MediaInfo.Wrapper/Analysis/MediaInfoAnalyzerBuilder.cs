#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Decorators;
using MediaInfo.Analysis.Native;
using MediaInfo.Analysis.Pipeline;
using MediaInfo.Analysis.Strategies;
using MediaInfo.Analysis.Strategies.Selection;
using Microsoft.Extensions.Logging;

namespace MediaInfo.Analysis;

/// <summary>
/// Assembles an analyzer from a core pipeline and the cross-cutting concerns a caller asks for.
/// </summary>
/// <remarks>
/// The order the concerns are requested in does not matter. They are always composed outermost first as logging,
/// validation, caching, timeout, retry, concurrency limit and subtitle discovery, so that logging sees every outcome
/// including a rejected media, a cached answer costs nothing, and a latency budget covers the wait for a free slot as
/// well as the analysis itself.
/// </remarks>
public sealed class MediaInfoAnalyzerBuilder
{
  private readonly List<IMediaAnalysisStrategy> _strategies = [];

  private INativeMediaInfoFactory? _nativeFactory;
  private IFileSystem? _fileSystem;
  private IStreamSelectionStrategy? _selection;
  private MediaAnalysisOptions? _options;
  private IProgress<AnalysisProgress>? _progress;

  private ILogger? _logger;
  private bool _validation;
  private bool _caching;
  private IAnalysisCache? _cache;
  private TimeSpan? _cacheTimeToLive;
  private TimeSpan? _timeout;
  private int? _retryAttempts;
  private TimeSpan? _retryDelay;
  private int? _concurrency;
  private bool _externalSubtitles;

  /// <summary>
  /// Creates a builder.
  /// </summary>
  /// <returns>Returns a new builder.</returns>
  public static MediaInfoAnalyzerBuilder Create() => new();

  /// <summary>
  /// Uses the specified factory to create media handles instead of the native library.
  /// </summary>
  /// <param name="factory">The factory to use.</param>
  /// <returns>Returns this builder.</returns>
  public MediaInfoAnalyzerBuilder UseNativeFactory(INativeMediaInfoFactory factory)
  {
    _nativeFactory = factory ?? throw new ArgumentNullException(nameof(factory));
    return this;
  }

  /// <summary>
  /// Uses the specified file system instead of the local one.
  /// </summary>
  /// <param name="fileSystem">The file system to use.</param>
  /// <returns>Returns this builder.</returns>
  public MediaInfoAnalyzerBuilder UseFileSystem(IFileSystem fileSystem)
  {
    _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    return this;
  }

  /// <summary>
  /// Uses the specified policy to choose the streams that best represent a media.
  /// </summary>
  /// <param name="selection">The policy to use.</param>
  /// <returns>Returns this builder.</returns>
  public MediaInfoAnalyzerBuilder UseStreamSelection(IStreamSelectionStrategy selection)
  {
    _selection = selection ?? throw new ArgumentNullException(nameof(selection));
    return this;
  }

  /// <summary>
  /// Uses the specified options.
  /// </summary>
  /// <param name="options">The options to use.</param>
  /// <returns>Returns this builder.</returns>
  public MediaInfoAnalyzerBuilder UseOptions(MediaAnalysisOptions options)
  {
    _options = options ?? throw new ArgumentNullException(nameof(options));
    return this;
  }

  /// <summary>
  /// Adjusts the options the analyzer will use.
  /// </summary>
  /// <param name="configure">The adjustment to apply.</param>
  /// <returns>Returns this builder.</returns>
  public MediaInfoAnalyzerBuilder Configure(Action<MediaAnalysisOptions> configure)
  {
    if (configure is null)
    {
      throw new ArgumentNullException(nameof(configure));
    }

    _options ??= new MediaAnalysisOptions();
    configure(_options);
    return this;
  }

  /// <summary>
  /// Reports the progress of an analysis to the specified receiver.
  /// </summary>
  /// <param name="progress">The receiver of the reports.</param>
  /// <returns>Returns this builder.</returns>
  public MediaInfoAnalyzerBuilder UseProgress(IProgress<AnalysisProgress> progress)
  {
    _progress = progress ?? throw new ArgumentNullException(nameof(progress));
    return this;
  }

  /// <summary>
  /// Adds a strategy to those the analyzer chooses from.
  /// </summary>
  /// <remarks>
  /// Adding any strategy replaces the default set, so a caller that wants both must add the defaults as well.
  /// </remarks>
  /// <param name="strategy">The strategy to add.</param>
  /// <returns>Returns this builder.</returns>
  public MediaInfoAnalyzerBuilder AddStrategy(IMediaAnalysisStrategy strategy)
  {
    _strategies.Add(strategy ?? throw new ArgumentNullException(nameof(strategy)));
    return this;
  }

  /// <summary>
  /// Records the analysis through the specified logger.
  /// </summary>
  /// <param name="logger">The logger to record through.</param>
  /// <returns>Returns this builder.</returns>
  public MediaInfoAnalyzerBuilder UseLogger(ILogger logger)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    return this;
  }

  /// <summary>
  /// Rejects a media that cannot be analyzed before any work is started on it.
  /// </summary>
  /// <returns>Returns this builder.</returns>
  public MediaInfoAnalyzerBuilder WithValidation()
  {
    _validation = true;
    return this;
  }

  /// <summary>
  /// Reuses the outcome of an earlier analysis of the same file or disc.
  /// </summary>
  /// <param name="timeToLive">How long an entry stays valid, or <see langword="null"/> to keep entries indefinitely.</param>
  /// <param name="cache">The store to keep the outcomes in, or <see langword="null"/> for an in-memory store.</param>
  /// <returns>Returns this builder.</returns>
  public MediaInfoAnalyzerBuilder WithCaching(TimeSpan? timeToLive = null, IAnalysisCache? cache = null)
  {
    _caching = true;
    _cacheTimeToLive = timeToLive;
    _cache = cache;
    return this;
  }

  /// <summary>
  /// Gives an analysis a total latency budget.
  /// </summary>
  /// <param name="timeout">The budget an analysis is given.</param>
  /// <returns>Returns this builder.</returns>
  public MediaInfoAnalyzerBuilder WithTimeout(TimeSpan timeout)
  {
    _timeout = timeout;
    return this;
  }

  /// <summary>
  /// Repeats an analysis that failed for a reason that may not persist.
  /// </summary>
  /// <param name="attempts">The greatest number of attempts to make, including the first one.</param>
  /// <param name="delay">How long to wait before the second attempt.</param>
  /// <returns>Returns this builder.</returns>
  public MediaInfoAnalyzerBuilder WithRetry(int attempts = 3, TimeSpan? delay = null)
  {
    _retryAttempts = attempts;
    _retryDelay = delay;
    return this;
  }

  /// <summary>
  /// Limits how many analyses run at the same time.
  /// </summary>
  /// <param name="concurrency">The greatest number of analyses to run at the same time.</param>
  /// <returns>Returns this builder.</returns>
  public MediaInfoAnalyzerBuilder WithConcurrencyLimit(int concurrency)
  {
    _concurrency = concurrency;
    return this;
  }

  /// <summary>
  /// Reports whether subtitle files sit next to the analyzed media.
  /// </summary>
  /// <returns>Returns this builder.</returns>
  public MediaInfoAnalyzerBuilder WithExternalSubtitles()
  {
    _externalSubtitles = true;
    return this;
  }

  /// <summary>
  /// Assembles the analyzer.
  /// </summary>
  /// <remarks>
  /// The result holds a disposable resource when a concurrency limit was requested. It implements
  /// <see cref="IDisposable"/> in that case and disposing it releases every wrapper in the chain.
  /// </remarks>
  /// <returns>Returns the assembled analyzer.</returns>
  public IMediaInfoAnalyzer Build()
  {
    var fileSystem = _fileSystem ?? MediaInfoFileSystem.Instance;
    var probe = new MediaProbe(
      _nativeFactory ?? MediaInfoLibFactory.Instance,
      _selection ?? DefaultStreamSelectionStrategy.Instance);

    var context = new MediaAnalysisContext(probe, fileSystem, _options, _progress);
    var strategies = _strategies.Count > 0
      ? _strategies
      : MediaInfoAnalyzer.CreateDefaultStrategies(fileSystem).ToList();

    // Composed from the core outwards, so that the outermost wrapper is the first one in the documented order.
    IMediaInfoAnalyzer analyzer = new MediaInfoAnalyzer(new PriorityStrategySelector(strategies), context);

    if (_externalSubtitles)
    {
      analyzer = new ExternalSubtitleAnalyzer(analyzer, fileSystem);
    }

    if (_concurrency is { } concurrency)
    {
      analyzer = new ThrottlingAnalyzer(analyzer, concurrency);
    }

    if (_retryAttempts is { } attempts)
    {
      analyzer = new RetryingAnalyzer(analyzer, attempts, _retryDelay);
    }

    if (_timeout is { } timeout)
    {
      analyzer = new TimeoutAnalyzer(analyzer, timeout);
    }

    if (_caching)
    {
      analyzer = new CachingAnalyzer(analyzer, _cache ?? new MemoryAnalysisCache(_cacheTimeToLive), fileSystem);
    }

    if (_validation)
    {
      analyzer = new ValidatingAnalyzer(analyzer);
    }

    if (_logger is not null)
    {
      analyzer = new LoggingAnalyzer(analyzer, _logger);
    }

    return analyzer;
  }
}
