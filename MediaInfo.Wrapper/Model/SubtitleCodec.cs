#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Runtime.Serialization;

namespace MediaInfo.Model
{
  /// <summary>
  /// Defines constants for different kind of subtitles.
  /// </summary>
  public enum SubtitleCodec
  {

    /// <summary>
    /// The undefined type.
    /// </summary>
    [EnumMember(Value = "undefined")]
    Undefined,

    /// <summary>
    /// The Advanced SubStation Alpha subtitles.
    /// </summary>
    [EnumMember(Value = "ass")]
    Ass,

    /// <summary>
    /// The BMP image subtitles.
    /// </summary>
    [EnumMember(Value = "bmp")]
    ImageBmp,

    /// <summary>
    /// The  SubStation Alpha subtitles.
    /// </summary>
    [EnumMember(Value = "ssa")]
    Ssa,

    /// <summary>
    /// The Advanced SubStation Alpha text subtitles.
    /// </summary>
    [EnumMember(Value = "text-ass")]
    TextAss,

    /// <summary>
    /// The SubStation Alpha text subtitles.
    /// </summary>
    [EnumMember(Value = "text-ssa")]
    TextSsa,

    /// <summary>
    /// The Universal Subtitle Format text subtitles.
    /// </summary>
    [EnumMember(Value = "text-usf")]
    TextUsf,

    /// <summary>
    /// The Unicode text subtitles.
    /// </summary>
    [EnumMember(Value = "text-utf8")]
    TextUtf8,

    /// <summary>
    /// The Universal Subtitle Format subtitles.
    /// </summary>
    [EnumMember(Value = "usf")]
    Usf,

    /// <summary>
    /// The Unicode subtitles.
    /// </summary>
    [EnumMember(Value = "utf8")]
    Utf8,

    /// <summary>
    /// The VOB SUB subtitles (DVD subtitles).
    /// </summary>
    [EnumMember(Value = "vobsup")]
    Vobsub,

    /// <summary>
    /// The Presentation Grapic Stream Subtitle Format subtitles
    /// </summary>
    [EnumMember(Value = "hdmv-pgs")]
    HdmvPgs,

    /// <summary>
    /// The HDMV Text Subtitle Format subtitles
    /// </summary>
    [EnumMember(Value = "hdmv-text")]
    HdmvTextst
  }
}