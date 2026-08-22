#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Globalization;

namespace MediaInfo.Analysis.Pipeline;

/// <summary>
/// Reads the duration of an opened media.
/// </summary>
/// <remarks>
/// The container is asked first, and the video and audio streams are consulted in turn when it does not know. A media
/// whose length the library cannot determine, which happens when a transport stream is parsed without being able to
/// seek, reports a duration that does not fit in a <see cref="TimeSpan"/>. Such a value is treated as unknown rather
/// than being allowed to overflow.
/// </remarks>
internal static class DurationReader
{
  private static readonly NumberFormatInfo NumberProvider = new() { NumberDecimalSeparator = "." };

  /// <summary>
  /// Reads the duration of the opened media.
  /// </summary>
  /// <param name="reader">The opened media.</param>
  /// <returns>Returns the duration, or <see cref="TimeSpan.Zero"/> when it could not be determined.</returns>
  public static TimeSpan Read(IMediaInfoReader reader)
  {
    var milliseconds = ReadMilliseconds(reader, StreamKind.General, (int)NativeMethods.General.General_Duration);
    if (!IsUsable(milliseconds))
    {
      milliseconds = ReadMilliseconds(reader, StreamKind.Video, (int)NativeMethods.Video.Video_Duration);
    }

    if (!IsUsable(milliseconds))
    {
      milliseconds = ReadMilliseconds(reader, StreamKind.Audio, (int)NativeMethods.Audio.Audio_Duration);
    }

    return IsUsable(milliseconds) ? TimeSpan.FromMilliseconds(milliseconds) : TimeSpan.Zero;
  }

  private static double ReadMilliseconds(IMediaInfoReader reader, StreamKind kind, int parameter) =>
    double.TryParse(
      reader.Get(kind, 0, parameter),
      NumberStyles.AllowDecimalPoint,
      NumberProvider,
      out var value)
      ? value
      : 0d;

  private static bool IsUsable(double milliseconds) =>
    milliseconds > 0d &&
    !double.IsNaN(milliseconds) &&
    milliseconds < TimeSpan.MaxValue.TotalMilliseconds;
}
