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
  /// Provides properties and overridden methods for the analyze subtitle stream 
  /// and contains information about subtitle.
  /// </summary>
  /// <seealso cref="LanguageMediaStream" />
  [Serializable]
  [DataContract]
  public class SubtitleStream : LanguageMediaStream
  {
    /// <summary>
    /// Gets the subtitle format.
    /// </summary>
    /// <value>
    /// The subtitle format.
    /// </value>
    [DataMember(Name = "format")]
    public string Format { get; set; }

    /// <summary>
    /// Gets the subtitle codec.
    /// </summary>
    /// <value>
    /// The subtitle codec.
    /// </value>
    [DataMember(Name = "codec")]
    public SubtitleCodec Codec { get; set; }

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Text;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Text;
  }
}