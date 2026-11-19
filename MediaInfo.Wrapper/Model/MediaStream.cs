#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.Model
{
  /// <summary>
  /// Defines the kind of media stream.
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
    Menu,

    /// <summary>
    /// A chapter of the media
    /// </summary>
    Chapter
  }

  /// <summary>
  /// Represents an abstract base class for a media stream, providing common properties for identifying and describing
  /// a stream within a media container.
  /// </summary>
  /// <remarks>This class serves as the foundation for specific types of media streams, such as audio or
  /// video streams. It exposes properties for stream identification, naming, and classification. Derived classes
  /// should implement the abstract members to specify the kind and characteristics of the stream.</remarks>
  public abstract class MediaStream
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
    public string Name { get; set; }  = default!;

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