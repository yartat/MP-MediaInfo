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
    /// Describes bitrate possible modes
    /// </summary>
    [DataContract]
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum BitrateMode : byte
    {
        /// <summary>
        /// Constant quality mode
        /// </summary>
        [EnumMember(Value = "constantQuality")]
        Cq,

        /// <summary>
        /// Constant bitrate mode
        /// </summary>
        [EnumMember(Value = "constantBitrate")]
        Cbr,

        /// <summary>
        /// Variable bitrate mode
        /// </summary>
        [EnumMember(Value = "variableBitrate")]
        Vbr
    }
}
