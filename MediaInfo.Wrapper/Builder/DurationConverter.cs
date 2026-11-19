#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Converts the durations a media reports, which a damaged or crafted file can make absurd, into time spans.
  /// </summary>
  internal static class DurationConverter
  {
    private static readonly double LongestMilliseconds = (long.MaxValue / TimeSpan.TicksPerMillisecond) - 1d;

    /// <summary>
    /// Converts milliseconds into a time span.
    /// </summary>
    /// <param name="milliseconds">The duration the media reports.</param>
    /// <returns>Returns the duration, or <see cref="TimeSpan.Zero"/> when the value is not a duration a time span can hold.</returns>
    public static TimeSpan FromMilliseconds(double milliseconds) =>
      milliseconds > 0d && milliseconds < LongestMilliseconds
        ? TimeSpan.FromMilliseconds(milliseconds)
        : TimeSpan.Zero;
  }
}
