#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.Model
{
  /// <summary>
  /// Defines the HDR formats recognized by the wrapper.
  /// </summary>
  public enum Hdr
  {
    /// <summary>
    /// The stream does not expose HDR metadata.
    /// </summary>
    None,

    /// <summary>
    /// HDR10 static-metadata HDR format.
    /// </summary>
    HDR10,

    /// <summary>
    /// HDR10+ dynamic-metadata HDR format.
    /// </summary>
    HDR10Plus,

    /// <summary>
    /// Dolby Vision HDR format.
    /// </summary>
    DolbyVision,

    /// <summary>
    /// Hybrid Log-Gamma (HLG) HDR format.
    /// </summary>
    HLG,

    /// <summary>
    /// Advanced HDR by Technicolor profile SL-HDR1.
    /// </summary>
    SLHDR1,

    /// <summary>
    /// Advanced HDR by Technicolor profile SL-HDR2.
    /// </summary>
    SLHDR2,

    /// <summary>
    /// Advanced HDR by Technicolor profile SL-HDR3.
    /// </summary>
    SLHDR3,

    /// <summary>
    /// HDR Vivid dynamic HDR format.
    /// </summary>
    HdrVivid,

    /// <summary>
    /// HDR10+ Profile A combined with HLG transfer characteristics.
    /// </summary>
    HLGPlus
  }
}
