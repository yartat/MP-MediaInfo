#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.Model
{
    /// <summary>
    /// Defines the normalized subtitle and text codec values exposed by the wrapper.
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
    HdmvTextst,

    /// <summary>
    /// The N19/STL subtitles.
    /// </summary>
    N19,

    /// <summary>
    /// The PAC subtitles.
    /// </summary>
    Pac,

    /// <summary>
    /// The Timed Text Markup Language subtitles, including TTML and IMSC based tracks.
    /// </summary>
    Ttml,

    /// <summary>
    /// The WebVTT subtitle format.
    /// </summary>
    WebVtt,

    /// <summary>
    /// The DVB Subtitle format.
    /// </summary>
    DvbSubtitle,

    /// <summary>
    /// The Kate (Karaoke And Text Encapsulation) subtitle format.
    /// </summary>
    Kate,

    /// <summary>
    /// EIA-608 closed captions.
    /// </summary>
    Eia608,

    /// <summary>
    /// EIA-708 closed captions.
    /// </summary>
    Eia708,

    /// <summary>
    /// The Apple text (QuickTime) subtitle format.
    /// </summary>
    AppleText,

    /// <summary>
    /// The 3GPP/QuickTime Timed Text subtitle format (tx3g).
    /// </summary>
    TimedText,

    /// <summary>
    /// The DivX Subtitle format (AVI).
    /// </summary>
    DivxSubtitle
  }
}