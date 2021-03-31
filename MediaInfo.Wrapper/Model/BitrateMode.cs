#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Runtime.Serialization;

namespace MediaInfo.Model
{
  /// <summary>
  /// Describes bitrate possible modes
  /// </summary>
  public enum BitrateMode : byte
  {
    /// <summary>
    /// Constant quality mode
    /// </summary>
    [EnumMember(Value = "constantQuality")]
    Cq,

    /// <summary>
    /// Constant bitrate mode
    /// </summary>
    [EnumMember(Value = "constantBitrate")]
    Cbr,

    /// <summary>
    /// Variable bitrate mode
    /// </summary>
    [EnumMember(Value = "variableBitrate")]
    Vbr
  }
}
