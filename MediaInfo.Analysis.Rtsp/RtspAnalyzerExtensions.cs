#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Linq;
using MediaInfo.Analysis.Abstractions;

namespace MediaInfo.Analysis.Rtsp;

/// <summary>
/// Adds RTSP support to an analyzer.
/// </summary>
public static class RtspAnalyzerExtensions
{
  /// <summary>
  /// Teaches the analyzer to describe a media served over RTSP.
  /// </summary>
  /// <remarks>
  /// Without this, an RTSP location is declined, because the native library cannot open one. The default strategies
  /// are registered alongside, so a builder that only asks for RTSP still analyzes files, discs and everything else.
  /// </remarks>
  /// <param name="builder">The builder.</param>
  /// <param name="configure">The adjustment to apply to the capture options.</param>
  /// <returns>Returns the builder.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
  public static MediaInfoAnalyzerBuilder WithRtsp(
    this MediaInfoAnalyzerBuilder builder,
    Action<RtspAnalysisOptions>? configure = null)
  {
    if (builder is null)
    {
      throw new ArgumentNullException(nameof(builder));
    }

    var options = new RtspAnalysisOptions();
    configure?.Invoke(options);

    // Adding any strategy replaces the default set, so the defaults come along explicitly.
    foreach (var strategy in MediaInfoAnalyzer.CreateDefaultStrategies(MediaInfoFileSystem.Instance))
    {
      builder.AddStrategy(strategy);
    }

    return builder.AddStrategy(new RtspAnalysisStrategy(options));
  }
}
