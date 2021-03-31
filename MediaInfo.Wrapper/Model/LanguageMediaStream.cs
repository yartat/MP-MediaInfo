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
  /// Provides properties and overridden methods for the analyze stream 
  /// and contains information about media stream.
  /// </summary>
  /// <seealso cref="MediaStream" />
  [Serializable]
  [DataContract]
  public abstract class LanguageMediaStream : MediaStream
  {
    /// <summary>
    /// Gets the media stream language.
    /// </summary>
    /// <value>
    /// The media stream language.
    /// </value>
    [DataMember(Name = "language")]
    public string Language { get; set; }

    /// <summary>
    /// Gets the media stream LCID.
    /// </summary>
    /// <value>
    /// The media stream LCID.
    /// </value>
    [DataMember(Name = "lcid")]
    public int Lcid { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="LanguageMediaStream"/> is default.
    /// </summary>
    /// <value>
    ///   <c>true</c> if default; otherwise, <c>false</c>.
    /// </value>
    [DataMember(Name = "default")]
    public bool Default { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="LanguageMediaStream"/> is forced.
    /// </summary>
    /// <value>
    ///   <c>true</c> if forced; otherwise, <c>false</c>.
    /// </value>
    [DataMember(Name = "forced")]
    public bool Forced { get; set; }
  }
}