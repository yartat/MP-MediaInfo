#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Runtime.InteropServices;
using System.Threading;

namespace MediaInfo.Analysis.Native;

/// <summary>
/// Sets the character locale of the process the way the native library needs it, once.
/// </summary>
/// <remarks>
/// On Linux and macOS the library converts the wchar_t path passed to Open() into a byte path via wcstombs(), which
/// depends on the process LC_CTYPE locale. .NET does not call setlocale() itself, so without this the conversion runs
/// in the "C" locale and multi-byte file names fail to open. The locale belongs to the process, not to a handle, and
/// setlocale() is not safe to call while other threads use the locale, so it is set by the first handle that is
/// created, before any media is opened, and never again.
/// </remarks>
internal sealed class NativeLocale(bool applies)
{
  private readonly object _sync = new();
  private bool _configured;

  /// <summary>
  /// Gets the instance that stands for the locale of this process. Windows does not need the locale to be set.
  /// </summary>
  public static NativeLocale Shared { get; } = new(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

  /// <summary>
  /// Sets the locale of the process, unless an earlier call has already done it.
  /// </summary>
  /// <param name="mediaInfo">A handle of the native library.</param>
  public void Ensure(IMediaInfoReader mediaInfo)
  {
    if (!applies || Volatile.Read(ref _configured))
    {
      return;
    }

    lock (_sync)
    {
      if (_configured)
      {
        return;
      }

      mediaInfo.Option("setlocale_LC_CTYPE", "C.UTF-8");
      Volatile.Write(ref _configured, true);
    }
  }
}
