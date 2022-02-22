#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models
{
    /// <summary>
    /// Describes HDR modes
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum Hdr
    {
        /// <summary>
        /// No HDR
        /// </summary>
        [EnumMember(Value = "none")]
        None,

        /// <summary>
        /// HDR10
        /// </summary>
        [EnumMember(Value = "hdr10")]
        HDR10,

        /// <summary>
        /// HDR10+
        /// </summary>
        [EnumMember(Value = "hdr10plus")]
        HDR10Plus,

        /// <summary>
        /// Dolby Vision
        /// </summary>
        [EnumMember(Value = "dolbyVision")]
        DolbyVision,

        /// <summary>
        /// Hybrid Log Gamma
        /// </summary>
        [EnumMember(Value = "hlg")]
        HLG,

        /// <summary>
        /// Advanced HDR by Technicolor (SL-HDR1)
        /// </summary>
        [EnumMember(Value = "sl-hdr1")]
        SLHDR1,

        /// <summary>
        /// Advanced HDR by Technicolor (SL-HDR2)
        /// </summary>
        [EnumMember(Value = "sl-hdr2")]
        SLHDR2,

        /// <summary>
        /// Advanced HDR by Technicolor (SL-HDR3)
        /// </summary>
        [EnumMember(Value = "sl-hdr3")]
        SLHDR3
    }
}
