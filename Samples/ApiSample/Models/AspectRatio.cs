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
    /// Describes video aspect ratio
    /// </summary>
    [DataContract]
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum AspectRatio
    {
        /// <summary>
        /// The opaque (1:1)
        /// </summary>
        [EnumMember(Value = "opaque")]
        Opaque,

        /// <summary>
        /// The high end data graphics (5:4)
        /// </summary>
        [EnumMember(Value = "highEndDataGraphics")]
        HighEndDataGraphics,

        /// <summary>
        /// The full screen (4:3)
        /// </summary>
        [EnumMember(Value = "fullScreen")]
        FullScreen,

        /// <summary>
        /// The standard slides (3:3)
        /// </summary>
        [EnumMember(Value = "standardSlides")]
        StandardSlides,

        /// <summary>
        /// The digital SLR cameras (3:2)
        /// </summary>
        [EnumMember(Value = "digitalSlrCameras")]
        DigitalSlrCameras,

        /// <summary>
        /// The High Definition TV (16:9)
        /// </summary>
        [EnumMember(Value = "hdtv")]
        HighDefinitionTv,

        /// <summary>
        /// The wide screen display (16:10)
        /// </summary>
        [EnumMember(Value = "wideScreenDisplay")]
        WideScreenDisplay,

        /// <summary>
        /// The wide screen (1.85:1)
        /// </summary>
        [EnumMember(Value = "wideScreen")]
        WideScreen,

        /// <summary>
        /// The cinema scope (21:9)
        /// </summary>
        [EnumMember(Value = "cinemaScope")]
        CinemaScope
    }
}