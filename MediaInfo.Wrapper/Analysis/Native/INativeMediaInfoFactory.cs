#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

namespace MediaInfo.Analysis.Native;

/// <summary>
/// Creates media handles of the MediaInfo library.
/// </summary>
public interface INativeMediaInfoFactory
{
  /// <summary>
  /// Creates a new media handle.
  /// </summary>
  /// <remarks>
  /// The caller owns the returned handle and is responsible for disposing it.
  /// </remarks>
  /// <returns>Returns a new media handle.</returns>
  INativeMediaInfo Create();
}
