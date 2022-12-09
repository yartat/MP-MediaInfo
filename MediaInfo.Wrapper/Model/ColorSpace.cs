#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.Model;

/// <summary>
/// Describes video color space
/// </summary>
public enum ColorSpace
{
    /// <summary>
    /// Generic film
    /// </summary>
    Generic,

    /// <summary>
    /// Printing density
    /// </summary>
    PrintingDensity,

    /// <summary>
    /// BT.601 NTSC
    /// </summary>
    NTSC,

    /// <summary>
    /// BT.601 PAL
    /// </summary>
    PAL,

    /// <summary>
    /// ADX
    /// </summary>
    ADX,

    /// <summary>
    /// BT.470 System M
    /// </summary>
    BT470M,

    /// <summary>
    /// BT.470 System B/G
    /// </summary>
    BT470BG,

    /// <summary>
    /// BT.601 PAL or NTSC
    /// </summary>
    BT601,

    /// <summary>
    /// BT.709
    /// </summary>
    BT709,

    /// <summary>
    /// BT.1361
    /// </summary>
    BT1361,

    /// <summary>
    /// BT.2020 (10 bit or 12 bit)
    /// </summary>
    BT2020,

    /// <summary>
    /// BT.2100
    /// </summary>
    BT2100,

    /// <summary>
    /// EBU Tech 3213
    /// </summary>
    EBUTech3213,

    /// <summary>
    /// SMPTE 240M
    /// </summary>
    SMPTE240M,

    /// <summary>
    /// SMPTE 274M
    /// </summary>
    SMPTE274M,

    /// <summary>
    /// SMPTE 428M
    /// </summary>
    SMPTE428M,

    /// <summary>
    ///  SMPTE ST 2065-1
    /// </summary>
    ACES,

    /// <summary>
    /// SMPTE ST 2067-40 / ISO 11664-3
    /// </summary>
    XYZ,

    /// <summary>
    /// DCI-P3
    /// </summary>
    DCIP3,

    /// <summary>
    /// Display P3
    /// </summary>
    DisplayP3
}
