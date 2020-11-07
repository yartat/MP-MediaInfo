#region Copyright (C) 2017-2020 Yaroslav Tatarenko

// Copyright (C) 2017-2020 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2020 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.Model
{
  /// <summary>
  /// Provides properties and overridden methods for the analyze stream 
  /// and contains information about media stream.
  /// </summary>
  /// <seealso cref="MediaStream" />
  public abstract class LanguageMediaStream : MediaStream
  {
    /// <summary>
    /// Gets the media stream language.
    /// </summary>
    /// <value>
    /// The media stream language.
    /// </value>
    public string Language { get; set; }

    /// <summary>
    /// Gets the media stream LCID.
    /// </summary>
    /// <value>
    /// The media stream LCID.
    /// </value>
    public int Lcid { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="LanguageMediaStream"/> is default.
    /// </summary>
    /// <value>
    ///   <c>true</c> if default; otherwise, <c>false</c>.
    /// </value>
    public bool Default { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="LanguageMediaStream"/> is forced.
    /// </summary>
    /// <value>
    ///   <c>true</c> if forced; otherwise, <c>false</c>.
    /// </value>
    public bool Forced { get; set; }
  }
}