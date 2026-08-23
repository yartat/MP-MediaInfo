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
/// Limits how many analyses run at the same time.
/// </summary>
/// <remarks>
/// Each analysis holds a native handle and a parsing buffer, so scanning a folder without a bound can consume a great
/// deal of native memory. Waiting for a slot counts against any latency budget placed outside this decorator, which
/// is intended: a caller that gave the analysis thirty seconds meant thirty seconds in total.
/// </remarks>
public sealed class ThrottlingAnalyzer : MediaInfoAnalyzerDecorator
{
  private readonly SemaphoreSlim _slots;

  /// <summary>
  /// Initializes a new instance of the <see cref="ThrottlingAnalyzer"/> class.
  /// </summary>
  /// <param name="inner">The analyzer to wrap.</param>
  /// <param name="concurrency">The greatest number of analyses to run at the same time.</param>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="concurrency"/> is less than one.</exception>
  public ThrottlingAnalyzer(IMediaInfoAnalyzer inner, int concurrency)
    : base(inner)
  {
    if (concurrency < 1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(concurrency),
        concurrency,
        "At least one analysis must be allowed to run.");
    }

    Concurrency = concurrency;
    _slots = new SemaphoreSlim(concurrency, concurrency);
  }

  /// <summary>
  /// Gets the greatest number of analyses that run at the same time.
  /// </summary>
  public int Concurrency { get; }

  /// <inheritdoc />
  public override async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    CancellationToken cancellationToken = default)
  {
    await _slots.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      return await Inner.AnalyzeAsync(source, cancellationToken).ConfigureAwait(false);
    }
    finally
    {
      _slots.Release();
    }
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      _slots.Dispose();
    }
  }
}
