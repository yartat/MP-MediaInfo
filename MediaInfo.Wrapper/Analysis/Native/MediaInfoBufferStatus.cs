#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;

namespace MediaInfo.Analysis.Native;

/// <summary>
/// Describes the state reported by the library after a block of media data has been submitted for parsing.
/// </summary>
[Flags]
public enum MediaInfoBufferStatus
{
  /// <summary>
  /// Nothing has been reported.
  /// </summary>
  None = 0x00,

  /// <summary>
  /// The buffer has been accepted.
  /// </summary>
  Accepted = 0x01,

  /// <summary>
  /// The buffer has been filled.
  /// </summary>
  Filled = 0x02,

  /// <summary>
  /// The media information has been updated.
  /// </summary>
  Updated = 0x04,

  /// <summary>
  /// The library has all the information it needs and no more data should be submitted.
  /// </summary>
  Finalized = 0x08
}
