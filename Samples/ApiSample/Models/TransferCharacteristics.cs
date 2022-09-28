#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models;

/// <summary>
/// Describes video transfer characteristics
/// </summary>
[DataContract]
[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum TransferCharacteristic
{
    /// <summary>
    /// Printing density
    /// </summary>
    [EnumMember(Value = "printingDensity")]
    PrintingDensity,

    /// <summary>
    /// The linear transfer
    /// </summary>
    [EnumMember(Value = "linear")]
    Linear,

    /// <summary>
    /// The logarithmic transfer
    /// </summary>
    [EnumMember(Value = "logarithmic")]
    Logarithmic,

    /// <summary>
    /// BT.601 NTSC
    /// </summary>
    [EnumMember(Value = "bt.601.ntsc")]
    BT601NTSC,

    /// <summary>
    /// BT.601 PAL
    /// </summary>
    [EnumMember(Value = "bt.601.pal")]
    BT601PAL,

    /// <summary>
    /// Composite NTSC
    /// </summary>
    [EnumMember(Value = "composite.ntsc")]
    CompositeNTSC,

    /// <summary>
    /// Composite PAL
    /// </summary>
    [EnumMember(Value = "composite.pal")]
    CompositePAL,

    /// <summary>
    /// BT.709
    /// </summary>
    [EnumMember(Value = "bt.709")]
    BT709,

    /// <summary>
    /// ADX
    /// </summary>
    [EnumMember(Value = "adx")]
    ADX,

    /// <summary>
    /// SMPTE 274M
    /// </summary>
    [EnumMember(Value = "smpte.274m")]
    SMPTE274M,

    /// <summary>
    /// Z (depth) - linear
    /// </summary>
    [EnumMember(Value = "zlinear")]
    ZLinear,

    /// <summary>
    /// Z (depth) - homogeneous
    /// </summary>
    [EnumMember(Value = "zhomogeneous")]
    ZHomogeneous
}