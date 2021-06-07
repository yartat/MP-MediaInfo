#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models
{
    /// <summary>
    /// Describes video color space
    /// </summary>
    [DataContract]
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum ColorSpace
    {
        /// <summary>
        /// Generic film
        /// </summary>
        [EnumMember(Value = "generic")]
        Generic,

        /// <summary>
        /// Printing density
        /// </summary>
        [EnumMember(Value = "printingDensity")]
        PrintingDensity,

        /// <summary>
        /// BT.601 NTSC
        /// </summary>
        [EnumMember(Value = "ntsc")]
        NTSC,

        /// <summary>
        /// BT.601 PAL
        /// </summary>
        [EnumMember(Value = "pal")]
        PAL,

        /// <summary>
        /// ADX
        /// </summary>
        [EnumMember(Value = "adx")]
        ADX,

        /// <summary>
        /// BT.470 System M
        /// </summary>
        [EnumMember(Value = "bt.470m")]
        BT470M,

        /// <summary>
        /// BT.470 System B/G
        /// </summary>
        [EnumMember(Value = "bt.470bg")]
        BT470BG,

        /// <summary>
        /// BT.601 PAL or NTSC
        /// </summary>
        [EnumMember(Value = "bt.601")]
        BT601,

        /// <summary>
        /// BT.709
        /// </summary>
        [EnumMember(Value = "bt.709")]
        BT709,

        /// <summary>
        /// BT.1361
        /// </summary>
        [EnumMember(Value = "bt.1361")]
        BT1361,

        /// <summary>
        /// BT.2020 (10 bit or 12 bit)
        /// </summary>
        [EnumMember(Value = "bt.2020")]
        BT2020,

        /// <summary>
        /// BT.2100
        /// </summary>
        [EnumMember(Value = "bt.2100")]
        BT2100,

        /// <summary>
        /// EBU Tech 3213
        /// </summary>
        [EnumMember(Value = "ebu3213")]
        EBUTech3213,

        /// <summary>
        /// SMPTE 240M
        /// </summary>
        [EnumMember(Value = "smpte240m")]
        SMPTE240M,

        /// <summary>
        /// SMPTE 274M
        /// </summary>
        [EnumMember(Value = "smpte274m")]
        SMPTE274M,

        /// <summary>
        /// SMPTE 428M
        /// </summary>
        [EnumMember(Value = "smpte428m")]
        SMPTE428M,

        /// <summary>
        ///  SMPTE ST 2065-1
        /// </summary>
        [EnumMember(Value = "aces")]
        ACES,

        /// <summary>
        /// SMPTE ST 2067-40 / ISO 11664-3
        /// </summary>
        [EnumMember(Value = "xyz")]
        XYZ,

        /// <summary>
        /// DCI-P3
        /// </summary>
        [EnumMember(Value = "dci-p3")]
        DCIP3,

        /// <summary>
        /// Display P3
        /// </summary>
        [EnumMember(Value = "display-p3")]
        DisplayP3
    }
}