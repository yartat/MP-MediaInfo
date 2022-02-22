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
    /// Provides properties and overridden methods for the analyze stream
    /// and contains information about media stream.
    /// </summary>
    /// <seealso cref="MediaStream" />
    [DataContract]
    public abstract class LanguageMediaStream : MediaStream
    {
        /// <summary>
        /// A media stream language.
        /// </summary>
        /// <example>en</example>
        [DataMember(Name = "language")]
        [JsonPropertyName("language")]
        public string Language { get; set; }

        /// <summary>
        /// A media stream LCID.
        /// </summary>
        /// <example>413</example>
        [DataMember(Name = "lcid")]
        [JsonPropertyName("lcid")]
        public int Lcid { get; set; }

        /// <summary>
        /// A value indicating whether this <see cref="LanguageMediaStream"/> is default.
        /// </summary>
        [DataMember(Name = "default")]
        [JsonPropertyName("default")]
        public bool Default { get; set; }

        /// <summary>
        /// A value indicating whether this <see cref="LanguageMediaStream"/> is forced.
        /// </summary>
        [DataMember(Name = "forced")]
        [JsonPropertyName("forced")]
        public bool Forced { get; set; }
    }
}