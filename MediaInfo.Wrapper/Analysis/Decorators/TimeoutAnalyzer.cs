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
/// Gives an analysis a total latency budget.
/// </summary>
/// <remarks>
/// Running out of budget produces a failed result rather than an exception, because it says something about the
/// media rather than about the caller. A token the caller cancelled still surfaces as an
/// <see cref="OperationCanceledException"/>, so the two cases stay distinguishable.
/// <para>
/// The budget is only observed where the pipeline observes cancellation. Opening a file or a network media is a
/// blocking call inside the native library that runs to completion regardless.
/// </para>
/// </remarks>
public sealed class TimeoutAnalyzer : MediaInfoAnalyzerDecorator
{
  private readonly TimeSpan _timeout;

  /// <summary>
  /// Initializes a new instance of the <see cref="TimeoutAnalyzer"/> class.
  /// </summary>
  /// <param name="inner">The analyzer to wrap.</param>
  /// <param name="timeout">The budget an analysis is given.</param>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is not greater than zero.</exception>
  public TimeoutAnalyzer(IMediaInfoAnalyzer inner, TimeSpan timeout)
    : base(inner)
  {
    if (timeout <= TimeSpan.Zero)
    {
      throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "The timeout must be greater than zero.");
    }

    _timeout = timeout;
  }

  /// <inheritdoc />
  public override async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    CancellationToken cancellationToken = default)
  {
    using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    budget.CancelAfter(_timeout);

    try
    {
      return await Inner.AnalyzeAsync(source, budget.Token).ConfigureAwait(false);
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.Timeout,
        $"The analysis of '{source?.DisplayName}' did not complete within {_timeout}.");
    }
  }
}
