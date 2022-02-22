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
    /// Describes video chroma sub sampling
    /// </summary>
    [DataContract]
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum ChromaSubSampling
    {
        /// <summary>
        /// 3:3:2
        /// </summary>
        [EnumMember(Value = "3:3:2")]
        Sampling332,

        /// <summary>
        /// 4:1:0
        /// </summary>
        [EnumMember(Value = "4:1:0")]
        Sampling410,

        /// <summary>
        /// 4:1:1
        /// </summary>
        [EnumMember(Value = "4:1:1")]
        Sampling411,

        /// <summary>
        /// 4:2:0
        /// </summary>
        [EnumMember(Value = "4:2:0")]
        Sampling420,

        /// <summary>
        /// 4:2:2
        /// </summary>
        [EnumMember(Value = "4:2:2")]
        Sampling422,

        /// <summary>
        /// 4:4:4
        /// </summary>
        [EnumMember(Value = "4:4:4")]
        Sampling444,

        /// <summary>
        /// 4:4:4:4
        /// </summary>
        [EnumMember(Value = "4:4:4:4")]
        Sampling4444,

        /// <summary>
        /// 5:5:5
        /// </summary>
        [EnumMember(Value = "5:5:5")]
        Sampling555,

        /// <summary>
        /// 5:6:5
        /// </summary>
        [EnumMember(Value = "5:6:5")]
        Sampling565,

        /// <summary>
        /// 8:8:8
        /// </summary>
        [EnumMember(Value = "8:8:8")]
        Sampling888
    }
}
