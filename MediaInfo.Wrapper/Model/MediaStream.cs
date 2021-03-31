#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Runtime.Serialization;

namespace MediaInfo.Model
{
  /// <summary>
  /// Defines constants for media stream kinds.
  /// </summary>
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
  /// <seealso cref="MarshalByRefObject" />
  [Serializable]
  [DataContract]
  public abstract class MediaStream : MarshalByRefObject
  {
    /// <summary>
    /// Gets or sets the media steam id.
    /// </summary>
    /// <value>
    /// The media steam id.
    /// </value>
    [DataMember(Name = "id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of stream.
    /// </summary>
    /// <value>
    /// The name of stream.
    /// </value>
    [DataMember(Name = "name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets the kind of media stream.
    /// </summary>
    /// <value>
    /// The kind of media stream.
    /// </value>
    public abstract MediaStreamKind Kind { get; }

    /// <summary>
    /// Gets the kind of the stream.
    /// </summary>
    /// <value>
    /// The kind of the stream.
    /// </value>
    protected abstract StreamKind StreamKind { get; }

    /// <summary>
    /// Gets the stream position.
    /// </summary>
    /// <value>
    /// The stream position.
    /// </value>
    [DataMember(Name = "streamPosition")]
    public int StreamPosition { get; set; }

    /// <summary>
    /// Gets the logical stream number.
    /// </summary>
    /// <value>
    /// The logical stream number.
    /// </value>
    [DataMember(Name = "streamNumber")]
    public int StreamNumber { get; set; }
  }
}