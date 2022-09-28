#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models;

/// <summary>
/// Defines constants for media stream kinds.
/// </summary>
[DataContract]
[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum MediaStreamKind
{
    /// <summary>
    /// The video stream
    /// </summary>
    [EnumMember(Value = "video")]
    Video,

    /// <summary>
    /// The audio stream
    /// </summary>
    [EnumMember(Value = "audio")]
    Audio,

    /// <summary>
    /// The subtitle stream
    /// </summary>
    [EnumMember(Value = "text")]
    Text,

    /// <summary>
    /// The image stream
    /// </summary>
    [EnumMember(Value = "image")]
    Image,

    /// <summary>
    /// Menu
    /// </summary>
    [EnumMember(Value = "menu")]
    Menu
}

/// <summary>
/// Provides basic properties and instance methods for the analyze stream
/// and contains information about media stream.
/// </summary>
[DataContract]
public abstract class MediaStream
{
    /// <summary>
    /// A media steam id.
    /// </summary>
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// A name of stream.
    /// </summary>
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// A kind of media stream.
    /// </summary>
    [DataMember(Name = "kind")]
    [JsonPropertyName("kind")]
    public MediaStreamKind Kind { get; }

    /// <summary>
    /// A stream position.
    /// </summary>
    [DataMember(Name = "streamPosition")]
    [JsonPropertyName("streamPosition")]
    public int StreamPosition { get; set; }

    /// <summary>
    /// A logical stream number.
    /// </summary>
    [DataMember(Name = "streamNumber")]
    [JsonPropertyName("streamNumber")]
    public int StreamNumber { get; set; }
}