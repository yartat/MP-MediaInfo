#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models
{
    /// <summary>
    /// Provides properties and overridden methods for the analyze audio stream
    /// and contains information about audio stream.
    /// </summary>
    /// <seealso cref="LanguageMediaStream" />
    [DataContract]
    public class AudioStream : LanguageMediaStream
    {
        /// <summary>
        /// The audio codec.
        /// </summary>
        /// <example>DTS-HD</example>
        [DataMember(Name = "codec")]
        [JsonPropertyName("codec")]
        public AudioCodec Codec { get; set; }

        /// <summary>
        /// The codec friendly name.
        /// </summary>
        /// <example>DTS-HD</example>
        [DataMember(Name = "codecFriendly")]
        [JsonPropertyName("codecFriendly")]
        public string CodecFriendly { get; set; }

        /// <summary>
        /// A duration of the stream in seconds.
        /// </summary>
        /// <example>100.0</example>
        [DataMember(Name = "duration")]
        [JsonPropertyName("duration")]
        [JsonConverter(typeof(JsonTimeSpanConverter))]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The audio bitrate.
        /// </summary>
        /// <example>395264</example>
        [DataMember(Name = "bitrate")]
        [JsonPropertyName("bitrate")]
        public double Bitrate { get; set; }

        /// <summary>
        /// Amount of the audio channels.
        /// </summary>
        /// <example>7</example>
        [DataMember(Name = "channel")]
        [JsonPropertyName("channel")]
        public int Channel { get; set; }

        /// <summary>
        /// The audio sampling rate.
        /// </summary>
        /// <example>44100</example>
        [DataMember(Name = "samplingRate")]
        [JsonPropertyName("samplingRate")]
        public double SamplingRate { get; set; }

        /// <summary>
        /// Bit depth of the stream.
        /// </summary>
        /// <example>24</example>
        [DataMember(Name = "bitDepth")]
        [JsonPropertyName("bitDepth")]
        public int BitDepth { get; set; }

        /// <summary>
        /// Bitrate mode of the stream.
        /// </summary>
        /// <example>2</example>
        [DataMember(Name = "bitrateMode")]
        [JsonPropertyName("bitrateMode")]
        public BitrateMode BitrateMode { get; set; }

        /// <summary>
        /// The audio format.
        /// </summary>
        /// <example>DTS</example>
        [DataMember(Name = "format")]
        [JsonPropertyName("format")]
        public string Format { get; set; }

        /// <summary>
        /// The audio codec name.
        /// </summary>
        /// <example>AVC</example>
        [DataMember(Name = "codecName")]
        [JsonPropertyName("codecName")]
        public string CodecName { get; set; }

        /// <summary>
        /// The audio codec description.
        /// </summary>
        /// <example>DTS-HD</example>
        [DataMember(Name = "codecDescription")]
        [JsonPropertyName("codecDescription")]
        public string CodecDescription { get; set; }

        /// <summary>
        /// The audio channels friendly.
        /// </summary>
        /// <example>7.1</example>
        [DataMember(Name = "audioChannelsFriendly")]
        [JsonPropertyName("audioChannelsFriendly")]
        public string AudioChannelsFriendly { get; set; }

        /// <summary>
        /// The stream tags.
        /// </summary>
        [DataMember(Name = "tags")]
        [JsonPropertyName("tags")]
        public AudioTags Tags { get; set; } = new AudioTags();
    }
}