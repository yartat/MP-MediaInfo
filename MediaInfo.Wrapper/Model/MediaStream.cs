#region Copyright (C) 2005-2017 Team MediaPortal

// Copyright (C) 2005-2017 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;

using JetBrains.Annotations;

namespace MediaInfo
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
    [PublicAPI]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of stream.
    /// </summary>
    /// <value>
    /// The name of stream.
    /// </value>
    [PublicAPI]
    public string Name { get; set; }

    /// <summary>
    /// Gets the kind of media stream.
    /// </summary>
    /// <value>
    /// The kind of media stream.
    /// </value>
    [PublicAPI]
    public abstract MediaStreamKind Kind { get; }

    /// <summary>
    /// Gets the kind of the stream.
    /// </summary>
    /// <value>
    /// The kind of the stream.
    /// </value>
    [PublicAPI]
    protected abstract StreamKind StreamKind { get; }

    /// <summary>
    /// Gets the stream position.
    /// </summary>
    /// <value>
    /// The stream position.
    /// </value>
    [PublicAPI]
    public int StreamPosition { get; set; }

    /// <summary>
    /// Gets the logical stream number.
    /// </summary>
    /// <value>
    /// The logical stream number.
    /// </value>
    [PublicAPI]
    public int StreamNumber { get; set; }
  }
}