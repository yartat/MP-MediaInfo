#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using MediaInfo.Model;

namespace MediaInfo.Builder;

/// <summary>
/// Provides HDR format helpers shared by the video and image stream builders.
/// </summary>
internal static class HdrUtils
{
  private static readonly Dictionary<string, Hdr> HdrFormats = new(StringComparer.OrdinalIgnoreCase)
  {
    { "Dolby Vision", Hdr.DolbyVision },
    { "Dolby Vision Metadata", Hdr.DolbyVision },
    { "SMPTE ST 2094-10", Hdr.DolbyVision },
    { "HDR10", Hdr.HDR10 },
    { "PQ", Hdr.HDR10 },
    { "SMPTE ST 2086", Hdr.HDR10 },
    { "HDR10 Plus", Hdr.HDR10Plus },
    { "HDR10+", Hdr.HDR10Plus },
    { "SMPTE ST 2094 App 4", Hdr.HDR10Plus },
    { "HDR10+ Profile A", Hdr.HDR10Plus },
    { "HDR10+ Profile B", Hdr.HDR10Plus },
    { "SL-HDR", Hdr.SLHDR1 },
    { "SL-HDR1", Hdr.SLHDR1 },
    { "SL-HDR2", Hdr.SLHDR2 },
    { "SL-HDR3", Hdr.SLHDR3 },
    { "HLG", Hdr.HLG },
    { "HLG+", Hdr.HLGPlus },
    { "HDR Vivid", Hdr.HdrVivid },
    { "HDR Vivid Metadata", Hdr.HdrVivid },
    { "T/UWA 005 (HDR Vivid)", Hdr.HdrVivid },
  };

  /// <summary>
  /// Tries to map the HDR format name reported by the library to the <see cref="Hdr"/> value.
  /// </summary>
  /// <param name="source">The HDR format name.</param>
  /// <param name="result">The HDR format.</param>
  /// <returns><c>true</c> if the format is recognized; otherwise, <c>false</c>.</returns>
  public static bool TryGetHdr(this string source, out Hdr result)
  {
    result = Hdr.None;
    return !string.IsNullOrEmpty(source) && HdrFormats.TryGetValue(source, out result);
  }
}
