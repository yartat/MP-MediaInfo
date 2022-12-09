#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.Model;

/// <summary>
/// Describes video aspect ratio
/// </summary>
public enum AspectRatio
{
    /// <summary>
    /// The opaque (1:1)
    /// </summary>
    Opaque,

    /// <summary>
    /// The high end data graphics (5:4)
    /// </summary>
    HighEndDataGraphics,

    /// <summary>
    /// The full screen (4:3)
    /// </summary>
    FullScreen,

    /// <summary>
    /// The standard slides (3:3)
    /// </summary>
    StandardSlides,

    /// <summary>
    /// The digital SLR cameras (3:2)
    /// </summary>
    DigitalSlrCameras,

    /// <summary>
    /// The High Definition TV (16:9)
    /// </summary>
    HighDefinitionTv,

    /// <summary>
    /// The wide screen display (16:10)
    /// </summary>
    WideScreenDisplay,

    /// <summary>
    /// The wide screen (1.85:1)
    /// </summary>
    WideScreen,

    /// <summary>
    /// The cinema scope (21:9)
    /// </summary>
    CinemaScope
}