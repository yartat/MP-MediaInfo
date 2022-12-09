#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.Model;

/// <summary>
/// Describes bitrate possible modes
/// </summary>
public enum BitrateMode : byte
{
    /// <summary>
    /// Constant quality mode
    /// </summary>
    Cq,

    /// <summary>
    /// Constant bitrate mode
    /// </summary>
    Cbr,

    /// <summary>
    /// Variable bitrate mode
    /// </summary>
    Vbr
}
