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
    /// Describes properties of the video stream and method to analyze stream
    /// </summary>
    /// <seealso cref="LanguageMediaStream" />
    [DataContract]
    public class VideoStream : LanguageMediaStream
    {
        /// <summary>
        /// A video frame rate.
        /// </summary>
        [DataMember(Name = "frameRate")]
        [JsonPropertyName("frameRate")]
        public double FrameRate { get; set; }

        /// <summary>
        /// A video width.
        /// </summary>
        [DataMember(Name = "width")]
        [JsonPropertyName("width")]
        public int Width { get; set; }

        /// <summary>
        /// A video height.
        /// </summary>
        [DataMember(Name = "height")]
        [JsonPropertyName("height")]
        public int Height { get; set; }

        /// <summary>
        /// A video bitrate.
        /// </summary>
        [DataMember(Name = "bitrate")]
        [JsonPropertyName("bitrate")]
        public double Bitrate { get; set; }

        /// <summary>
        /// A video aspect ratio.
        /// </summary>
        [DataMember(Name = "aspectRatio")]
        [JsonPropertyName("aspectRatio")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AspectRatio AspectRatio { get; set; }

        /// <summary>
        /// A value indicating whether this <see cref="VideoStream"/> is interlaced.
        /// </summary>
        [DataMember(Name = "interlaced")]
        [JsonPropertyName("interlaced")]
        public bool Interlaced { get; set; }

        /// <summary>
        /// A video stereoscopic mode.
        /// </summary>
        [DataMember(Name = "stereoscopic")]
        [JsonPropertyName("stereoscopic")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StereoMode Stereoscopic { get; set; }

        /// <summary>
        /// A video format.
        /// </summary>
        [DataMember(Name = "format")]
        [JsonPropertyName("format")]
        public string Format { get; set; }

        /// <summary>
        /// A video codec.
        /// </summary>
        [DataMember(Name = "codec")]
        [JsonPropertyName("codec")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VideoCodec Codec { get; set; }

        /// <summary>
        /// A video codec profile.
        /// </summary>
        [DataMember(Name = "codecProfile")]
        [JsonPropertyName("codecProfile")]
        public string CodecProfile { get; set; }

        /// <summary>
        /// A video standard.
        /// </summary>
        [DataMember(Name = "standard")]
        [JsonPropertyName("standard")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VideoStandard Standard { get; set; }

        /// <summary>
        /// A video color space.
        /// </summary>
        [DataMember(Name = "colorSpace")]
        [JsonPropertyName("colorSpace")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ColorSpace ColorSpace { get; set; }

        /// <summary>
        /// A video transfer characteristics.
        /// </summary>
        [DataMember(Name = "transferCharacteristics")]
        [JsonPropertyName("transferCharacteristics")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TransferCharacteristic TransferCharacteristics { get; set; }

        /// <summary>
        /// A video chroma subsampling.
        /// </summary>
        [DataMember(Name = "subSampling")]
        [JsonPropertyName("subSampling")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChromaSubSampling SubSampling { get; set; }

        /// <summary>
        /// A stream duration.
        /// </summary>
        [DataMember(Name = "duration")]
        [JsonPropertyName("duration")]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// A video HDR type.
        /// </summary>
        [DataMember(Name = "hdr")]
        [JsonPropertyName("hdr")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Hdr Hdr { get; set; }

        /// <summary>
        /// A video bit depth.
        /// </summary>
        [DataMember(Name = "bitDepth")]
        [JsonPropertyName("bitDepth")]
        public int BitDepth { get; set; }

        /// <summary>
        /// A name of the video codec.
        /// </summary>
        [DataMember(Name = "codecName")]
        [JsonPropertyName("codecName")]
        public string CodecName { get; set; }

        /// <summary>
        /// A video resolution.
        /// </summary>
        [DataMember(Name = "resolution")]
        [JsonPropertyName("resolution")]
        public string Resolution { get; set; }

        /// <summary>
        /// A video stream tags.
        /// </summary>
        [DataMember(Name = "tags")]
        [JsonPropertyName("tags")]
        public VideoTags Tags { get; set; }
    }
}