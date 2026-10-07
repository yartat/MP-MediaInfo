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
/// Repeats an analysis that failed for a reason that may not persist.
/// </summary>
/// <remarks>
/// Only a failure that could plausibly resolve itself is repeated. A media that does not exist, is empty or is of an
/// unsupported kind will fail identically every time, so it is returned at once. The delay doubles with each attempt.
/// </remarks>
public sealed class RetryingAnalyzer : MediaInfoAnalyzerDecorator
{
  private readonly int _attempts;
  private readonly TimeSpan _delay;
  private readonly Func<MediaAnalysisResult, bool> _shouldRetry;

  /// <summary>
  /// Initializes a new instance of the <see cref="RetryingAnalyzer"/> class.
  /// </summary>
  /// <param name="inner">The analyzer to wrap.</param>
  /// <param name="attempts">The greatest number of attempts to make, including the first one.</param>
  /// <param name="delay">How long to wait before the second attempt, or <see langword="null"/> for 200 milliseconds.</param>
  /// <param name="shouldRetry">Decides whether an outcome is worth repeating, or <see langword="null"/> for the default.</param>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="attempts"/> is less than one.</exception>
  public RetryingAnalyzer(
    IMediaInfoAnalyzer inner,
    int attempts = 3,
    TimeSpan? delay = null,
    Func<MediaAnalysisResult, bool>? shouldRetry = null)
    : base(inner)
  {
    if (attempts < 1)
    {
      throw new ArgumentOutOfRangeException(nameof(attempts), attempts, "At least one attempt must be made.");
    }

    _attempts = attempts;
    _delay = delay ?? TimeSpan.FromMilliseconds(200);
    _shouldRetry = shouldRetry ?? IsWorthRepeating;
  }

  /// <summary>
  /// Determines whether an outcome describes a failure that may not persist.
  /// </summary>
  /// <param name="result">The outcome of an attempt.</param>
  /// <returns>Returns <see langword="true"/> when the analysis is worth repeating; otherwise, <see langword="false"/>.</returns>
  public static bool IsWorthRepeating(MediaAnalysisResult result) =>
    !result.Success &&
    result.Failure?.Reason is AnalysisFailureReason.NativeOpenFailed or AnalysisFailureReason.Unknown;

  /// <inheritdoc />
  public override async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    CancellationToken cancellationToken = default)
  {
    var delay = _delay;
    MediaAnalysisResult result;

    for (var attempt = 1; ; attempt++)
    {
      result = await Inner.AnalyzeAsync(source, cancellationToken).ConfigureAwait(false);
      if (attempt >= _attempts || !_shouldRetry(result))
      {
        return result;
      }

      await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
      delay += delay;
    }
  }
}
