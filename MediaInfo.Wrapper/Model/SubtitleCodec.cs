#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.Model;

/// <summary>
/// Defines constants for different kind of subtitles.
/// </summary>
public enum SubtitleCodec
{
    /// <summary>
    /// The undefined type.
    /// </summary>
    Undefined,

    /// <summary>
    /// The Advanced SubStation Alpha subtitles.
    /// </summary>
    Ass,

    /// <summary>
    /// The BMP image subtitles.
    /// </summary>
    ImageBmp,

    /// <summary>
    /// The  SubStation Alpha subtitles.
    /// </summary>
    Ssa,

    /// <summary>
    /// The Advanced SubStation Alpha text subtitles.
    /// </summary>
    TextAss,

    /// <summary>
    /// The SubStation Alpha text subtitles.
    /// </summary>
    TextSsa,

    /// <summary>
    /// The Universal Subtitle Format text subtitles.
    /// </summary>
    TextUsf,

    /// <summary>
    /// The Unicode text subtitles.
    /// </summary>
    TextUtf8,

    /// <summary>
    /// The Universal Subtitle Format subtitles.
    /// </summary>
    Usf,

    /// <summary>
    /// The Unicode subtitles.
    /// </summary>
    Utf8,

    /// <summary>
    /// The VOB SUB subtitles (DVD subtitles).
    /// </summary>
    Vobsub,

    /// <summary>
    /// The Presentation Grapic Stream Subtitle Format subtitles
    /// </summary>
    HdmvPgs,

    /// <summary>
    /// The HDMV Text Subtitle Format subtitles
    /// </summary>
    HdmvTextst
}