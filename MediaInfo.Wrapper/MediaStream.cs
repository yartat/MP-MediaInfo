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
    /// Converts the string representation of a value to specified type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source value.</param>
    /// <param name="result">The result.</param>
    /// <returns><b>true</b> if s was converted successfully; otherwise, <b>false</b>.</returns>
    protected delegate bool ParseDelegate<T>(string source, out T result);

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaStream"/> class.
    /// </summary>
    /// <param name="info">The media information.</param>
    /// <param name="number">The stream number.</param>
    /// <param name="position">The stream position.</param>
    protected MediaStream(MediaInfo info, int number, int position)
    {
      StreamNumber = number;
      StreamPosition = position;
      Info = info;
    }

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

    /// <summary>
    /// Gets the media info object to access to low-level functions.
    /// </summary>
    /// <value>
    /// The media info object.
    /// </value>
    [PublicAPI]
    protected MediaInfo Info { get; }

    /// <summary>
    /// Analyzes the stream.
    /// </summary>
    protected virtual void AnalyzeInternal()
    {
      Id = Get<int>("ID", int.TryParse);
      Name = Get("Title");
    }

    /// <summary>
    /// Analyzes media stream.
    /// </summary>
    public void Analyze()
    {
      AnalyzeInternal();
    }

    /// <summary>
    /// Gets the property <typeparamref name="T">value</typeparamref> by the <paramref name="parameter">property name</paramref>.
    /// </summary>
    /// <param name="parameter">The stream parameter name.</param>
    /// <param name="convert"></param>
    /// <param name="extractResult">The manual extract result function.</param>
    /// <returns>Returns property <typeparamref name="T">value</typeparamref> of specified stream <paramref name="parameter">property name</paramref>.</returns>
    protected T Get<T>(string parameter, ParseDelegate<T> convert, Func<string, string> extractResult = null)
    {
      if (convert == null)
      {
        throw new ArgumentNullException(nameof(convert));
      }

      T parsedValue;
      return convert(Get(parameter, extractResult), out parsedValue) ? parsedValue : default(T);
    }

    /// <summary>
    /// Gets the property <typeparamref name="T">value</typeparamref> by the <paramref name="parameter">property index</paramref>.
    /// </summary>
    /// <param name="parameter">The stream property index.</param>
    /// <param name="infoKind">The kind of property value</param>
    /// <param name="convert"></param>
    /// <param name="extractResult">The manual extract result function.</param>
    /// <returns>Returns property <typeparamref name="T">value</typeparamref> of specified stream <paramref name="parameter">property index</paramref>.</returns>
    protected T Get<T>(int parameter, InfoKind infoKind, ParseDelegate<T> convert, Func<string, string> extractResult = null)
    {
      if (convert == null)
      {
        throw new ArgumentNullException(nameof(convert));
      }

      T parsedValue;
      return convert(Get(parameter, infoKind, extractResult), out parsedValue) ? parsedValue : default(T);
    }

    /// <summary>
    /// Gets the specified property value by <paramref name="parameter">property name</paramref>.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="extractResult">The extract result.</param>
    /// <returns>Returns property value by name. If property does not defined will return <see cref="string.Empty"/>.</returns>
    protected string Get(string parameter, Func<string, string> extractResult = null)
    {
      var result = Info.Get(StreamKind, StreamPosition, parameter);
      if (extractResult != null)
      {
        result = extractResult(result) ?? result;
      }

      return result ?? string.Empty;
    }

    /// <summary>
    /// Gets the specified property value by the <paramref name="parameter">property index</paramref>.
    /// </summary>
    /// <param name="parameter">The property index.</param>
    /// <param name="infoKind">The kind of property value</param>
    /// <param name="extractResult">The extract result.</param>
    /// <returns>Returns property value by name. If property does not defined will return <see cref="string.Empty"/>.</returns>
    protected string Get(int parameter, InfoKind infoKind, Func<string, string> extractResult = null)
    {
      var result = Info.Get(StreamKind, StreamPosition, parameter, infoKind);
      if (extractResult != null)
      {
        result = extractResult(result) ?? result;
      }

      return result ?? string.Empty;
    }
  }
}