#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models;

/// <summary>
/// Information about media.
/// </summary>
[DataContract]
public class MediaInfo
{
    /// <summary>
    /// A value indicating whether this media has video.
    /// </summary>
    [DataMember(Name = "video")]
    [JsonPropertyName("video")]
    public bool HasVideo { get; set; }

    /// <summary>
    /// A value indicating whether media is 3D.
    /// </summary>
    [DataMember(Name = "3d")]
    [JsonPropertyName("3d")]
    public bool Is3D { get; set; }

    /// <summary>
    /// A value indicating whether this media is HDR video.
    /// </summary>
    [DataMember(Name = "hdr")]
    [JsonPropertyName("hdr")]
    public bool IsHdr { get; set; }

    /// <summary>
    /// A video streams.
    /// </summary>
    [DataMember(Name = "videos")]
    [JsonPropertyName("videos")]
    public IList<VideoStream> VideoStreams { get; set; }

    /// <summary>
    /// A video codec.
    /// </summary>
    [DataMember(Name = "videoCodec")]
    [JsonPropertyName("videoCodec")]
    public string VideoCodec { get; set; }

    /// <summary>
    /// A video frame rate.
    /// </summary>
    [DataMember(Name = "framerate")]
    [JsonPropertyName("framerate")]
    public double Framerate { get; set; }

    /// <summary>
    /// A video picture width.
    /// </summary>
    [DataMember(Name = "width")]
    [JsonPropertyName("width")]
    public int Width { get; set; }

    /// <summary>
    /// A video picture height.
    /// </summary>
    [DataMember(Name = "height")]
    [JsonPropertyName("height")]
    public int Height { get; set; }

    /// <summary>
    /// A video aspect ratio.
    /// </summary>
    [DataMember(Name = "aspectRatio")]
    [JsonPropertyName("aspectRatio")]
    public string AspectRatio { get; set; }

    /// <summary>
    /// A type of the scan.
    /// </summary>
    [DataMember(Name = "scanType")]
    [JsonPropertyName("scanType")]
    public string ScanType { get; set; }

    /// <summary>
    /// A value indicating whether this video is interlaced.
    /// </summary>
    [DataMember(Name = "interlaced")]
    [JsonPropertyName("interlaced")]
    public bool IsInterlaced { get; set; }

    /// <summary>
    /// A video resolution.
    /// </summary>
    [DataMember(Name = "videoResolution")]
    [JsonPropertyName("videoResolution")]
    public string VideoResolution { get; set; }

    /// <summary>
    /// A video rate.
    /// </summary>
    [DataMember(Name = "videoRate")]
    [JsonPropertyName("videoRate")]
    public int VideoRate { get; set; }

    /// <summary>
    /// The audio streams.
    /// </summary>
    [DataMember(Name = "audios")]
    [JsonPropertyName("audios")]
    public IList<AudioStream> AudioStreams { get; set; }

    /// <summary>
    /// An audio codec.
    /// </summary>
    [DataMember(Name = "audioCodec")]
    [JsonPropertyName("audioCodec")]
    public string AudioCodec { get; set; }

    /// <summary>
    /// An audio rate.
    /// </summary>
    [DataMember(Name = "audioRate")]
    [JsonPropertyName("audioRate")]
    public int AudioRate { get; set; }

    /// <summary>
    /// An audio sample rate.
    /// </summary>
    [DataMember(Name = "audioSampleRate")]
    [JsonPropertyName("audioSampleRate")]
    public int AudioSampleRate { get; set; }

    /// <summary>
    /// An audio channels friendly.
    /// </summary>
    [DataMember(Name = "audioChannelsFriendly")]
    [JsonPropertyName("audioChannelsFriendly")]
    public string AudioChannelsFriendly { get; set; }

    /// <summary>
    /// The subtitle streams.
    /// </summary>
    [DataMember(Name = "subtitles")]
    [JsonPropertyName("subtitles")]
    public IList<SubtitleStream> Subtitles { get; set; }

    /// <summary>
    /// A value indicating whether this video has external subtitles.
    /// </summary>
    [DataMember(Name = "externalSubtitles")]
    [JsonPropertyName("externalSubtitles")]
    public bool HasExternalSubtitles { get; set; }

    /// <summary>
    /// The chapter streams.
    /// </summary>
    [DataMember(Name = "chapters")]
    [JsonPropertyName("chapters")]
    public IList<ChapterStream> Chapters { get; set; }

    /// <summary>
    /// The menu streams.
    /// </summary>
    [DataMember(Name = "menus")]
    [JsonPropertyName("menus")]
    public IList<MenuStream> MenuStreams { get; set; }

    /// <summary>
    /// A value indicating whether this video is DVD format.
    /// </summary>
    [DataMember(Name = "dvd")]
    [JsonPropertyName("dvd")]
    public bool IsDvd { get; set; }

    /// <summary>
    /// A media format.
    /// </summary>
    [DataMember(Name = "format")]
    [JsonPropertyName("format")]
    public string Format { get; set; }

    /// <summary>
    /// A value indicating whether this media is streamable.
    /// </summary>
    [DataMember(Name = "streamable")]
    [JsonPropertyName("streamable")]
    public bool IsStreamable { get; set; }

    /// <summary>
    /// A writing media application.
    /// </summary>
    [DataMember(Name = "writingApplication")]
    [JsonPropertyName("writingApplication")]
    public string WritingApplication { get; set; }

    /// <summary>
    /// A writing media library.
    /// </summary>
    [DataMember(Name = "writingLibrary")]
    [JsonPropertyName("writingLibrary")]
    public string WritingLibrary { get; set; }

    /// <summary>
    /// The media attachments.
    /// </summary>
    [DataMember(Name = "attachments")]
    [JsonPropertyName("attachments")]
    public string Attachments { get; set; }

    /// <summary>
    /// A media format version.
    /// </summary>
    [DataMember(Name = "formatVersion")]
    [JsonPropertyName("formatVersion")]
    public string FormatVersion { get; set; }

    /// <summary>
    /// A media profile.
    /// </summary>
    [DataMember(Name = "profile")]
    [JsonPropertyName("profile")]
    public string Profile { get; set; }

    /// <summary>
    /// A media codec.
    /// </summary>
    [DataMember(Name = "codec")]
    [JsonPropertyName("codec")]
    public string Codec { get; set; }

    /// <summary>
    /// A value indicating whether this media is Blu-Ray.
    /// </summary>
    [DataMember(Name = "bluRay")]
    [JsonPropertyName("bluRay")]
    public bool IsBluRay { get; set; }

    /// <summary>
    /// A media duration.
    /// </summary>
    [DataMember(Name = "duration")]
    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// A media size.
    /// </summary>
    [DataMember(Name = "size")]
    [JsonPropertyName("size")]
    public long Size { get; set; }
}
