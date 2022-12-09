#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.Model;

/// <summary>
/// Describes video transfer characteristics
/// </summary>
public enum TransferCharacteristic
{
    /// <summary>
    /// Printing density
    /// </summary>
    PrintingDensity,

    /// <summary>
    /// The linear transfer
    /// </summary>
    Linear,

    /// <summary>
    /// The logarithmic transfer
    /// </summary>
    Logarithmic,

    /// <summary>
    /// BT.601 NTSC
    /// </summary>
    BT601NTSC,

    /// <summary>
    /// BT.601 PAL
    /// </summary>
    BT601PAL,

    /// <summary>
    /// Composite NTSC
    /// </summary>
    CompositeNTSC,

    /// <summary>
    /// Composite PAL
    /// </summary>
    CompositePAL,

    /// <summary>
    /// BT.709
    /// </summary>
    BT709,

    /// <summary>
    /// ADX
    /// </summary>
    ADX,

    /// <summary>
    /// SMPTE 274M
    /// </summary>
    SMPTE274M,

    /// <summary>
    /// Z (depth) - linear
    /// </summary>
    ZLinear,

    /// <summary>
    /// Z (depth) - homogeneous
    /// </summary>
    ZHomogeneous
}