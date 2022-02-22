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
    /// Provides properties and overridden methods for the analyze chapter in media
    /// and contains information about chapter.
    /// </summary>
    /// <seealso cref="MediaStream" />
    [DataContract]
    public class ChapterStream : MediaStream
    {
        /// <summary>
        /// A chapter offset.
        /// </summary>
        /// <example>11.3</example>
        [DataMember(Name = "offset")]
        [JsonPropertyName("offset")]
        public double Offset { get; }

        /// <summary>
        /// A chapter description.
        /// </summary>
        /// <example>Chapter description</example>
        [DataMember(Name = "description")]
        [JsonPropertyName("description")]
        public string Description { get; }
    }
}