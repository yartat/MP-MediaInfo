#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System.Runtime.InteropServices;

namespace MediaInfo.Analysis.Native;

/// <summary>
/// Creates media handles backed by the MediaInfo native library.
/// </summary>
public sealed class MediaInfoLibFactory : INativeMediaInfoFactory
{
  /// <summary>
  /// Gets a shared instance of the factory.
  /// </summary>
  public static MediaInfoLibFactory Instance { get; } = new();

  /// <inheritdoc />
  public INativeMediaInfo Create()
  {
    var adapter = new MediaInfoLibAdapter(new MediaInfo());
    if (adapter.IsAvailable)
    {
      NativeLocale.Shared.Ensure(adapter);
    }

    return adapter;
  }
}
