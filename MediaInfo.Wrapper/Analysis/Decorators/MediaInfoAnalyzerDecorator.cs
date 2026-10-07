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

namespace MediaInfo.Analysis.Decorators;

/// <summary>
/// Adds one cross-cutting concern to an analyzer without the analyzer knowing about it.
/// </summary>
/// <remarks>
/// Logging, validation, caching, timeouts, retries, concurrency limits and subtitle discovery are all orthogonal to
/// how a media is parsed. Keeping each of them in its own wrapper leaves the core free of them and lets a caller
/// compose only what it needs.
/// </remarks>
public abstract class MediaInfoAnalyzerDecorator : IMediaInfoAnalyzer, IDisposable
{
  private bool _disposed;

  /// <summary>
  /// Initializes a new instance of the <see cref="MediaInfoAnalyzerDecorator"/> class.
  /// </summary>
  /// <param name="inner">The analyzer to wrap.</param>
  /// <exception cref="ArgumentNullException"><paramref name="inner"/> is <see langword="null"/>.</exception>
  protected MediaInfoAnalyzerDecorator(IMediaInfoAnalyzer inner)
  {
    Inner = inner ?? throw new ArgumentNullException(nameof(inner));
  }

  /// <summary>
  /// Gets the wrapped analyzer.
  /// </summary>
  protected IMediaInfoAnalyzer Inner { get; }

  /// <inheritdoc />
  public abstract Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Releases the resources held by this decorator and by everything it wraps.
  /// </summary>
  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    Dispose(true);
    (Inner as IDisposable)?.Dispose();
    GC.SuppressFinalize(this);
  }

  /// <summary>
  /// Releases the resources held by this decorator.
  /// </summary>
  /// <param name="disposing"><see langword="true"/> when called from <see cref="Dispose()"/>.</param>
  protected virtual void Dispose(bool disposing)
  {
  }
}
