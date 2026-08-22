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
    if (adapter.IsAvailable && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      // On Linux and macOS the library converts the wchar_t path passed to Open() into a byte path via wcstombs(),
      // which depends on the process LC_CTYPE locale. .NET does not call setlocale() itself, so without this the
      // conversion runs in the "C" locale and multi-byte file names fail to open.
      adapter.Option("setlocale_LC_CTYPE", "C.UTF-8");
    }

    return adapter;
  }
}
