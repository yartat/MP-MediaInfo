#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;

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
    Video,

    /// <summary>
    /// The audio stream
    /// </summary>
    Audio,

    /// <summary>
    /// The subtitle stream
    /// </summary>
    Text,

    /// <summary>
    /// The image stream
    /// </summary>
    Image,

    /// <summary>
    /// Menu
    /// </summary>
    Menu
  }

  /// <summary>
  /// Provides basic properties and instance methods for the analyze stream
  /// and contains information about media stream.
  /// </summary>
  /// <seealso cref="MarshalByRefObject" />
  public abstract class MediaStream : MarshalByRefObject
  {
    /// <summary>
    /// Gets or sets the media steam id.
    /// </summary>
    /// <value>
    /// The media steam id.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of stream.
    /// </summary>
    /// <value>
    /// The name of stream.
    /// </value>
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
    public int StreamPosition { get; set; }

    /// <summary>
    /// Gets the logical stream number.
    /// </summary>
    /// <value>
    /// The logical stream number.
    /// </value>
    public int StreamNumber { get; set; }
  }
}