using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MediaInfo
{
  public static class FileNameExtensions
  {
    private static readonly Regex TsBufferMatch = new Regex(@"(live\d+-\d+\.ts(\.tsbuffer(\d+\.ts)?)?)$", RegexOptions.Compiled);

    #region Extensions

    private static readonly Dictionary<string, bool> PlaylistExtensions = new Dictionary<string, bool>
    {
      { ".M3U", true },
      { ".M3U8", true },
      { ".PLS", true },
      { ".B4S", true },
      { ".WPL", true },
      { ".CUE", true }
    };

    private static readonly Dictionary<string, bool> PictureExtensions = new Dictionary<string, bool>
    {
      { ".JPG", true },
      { ".JPEG", true },
      { ".GIF", true },
      { ".BMP", true },
      { ".BITMAP", true },
      { ".PNG", true },
      { ".RAW", true },
      { ".TIF", true },
      { ".TIFF", true },
      { ".JFIF", true },
      { ".EXIF", true },
      { ".PPM", true },
      { ".PGM", true },
      { ".PBM", true },
      { ".PNM", true },
      { ".WEBP", true },
      { ".RIFF", true },
      { ".HEIF", true },
      { ".PCX", true },
      { ".TGA", true },
      { ".SGI", true },
      { ".PGF", true },
      { ".PAM", true },
      { ".IMG", true },
      { ".IMAGE", true },
      { ".ICO", true },
      { ".ICON", true },
    };

    private static readonly Dictionary<string, bool> VideoExtensions = new Dictionary<string, bool>
    {
      { ".AVI", true },
      { ".BDMV", true },
      { ".MPG", true },
      { ".MPEG", true },
      { ".MP4", true },
      { ".DIVX", true },
      { ".OGM", true },
      { ".MKV", true },
      { ".WMV", true },
      { ".QT", true },
      { ".RM", true },
      { ".MOV", true },
      { ".MOVIE", true },
      { ".MTS", true },
      { ".M2TS", true },
      { ".SBE", true },
      { ".DVR-MS", true },
      { ".TS", true },
      { ".DAT", true },
      { ".IFO", true },
      { ".FLV", true },
      { ".M4V", true },
      { ".3GP", true },
      { ".WTV", true },
      { ".OGV", true },
      { ".MK3D", true },
      { ".MPLS", true },
      { ".MPE", true },
      { ".M1V", true },
      { ".M2V", true },
      { ".IFLV", true },
      { ".3GPP", true },
      { ".MPV4", true },
      { ".HDMOV", true },
      { ".MP4V", true },
    };

    private static readonly Dictionary<string, bool> AudioExtensions = new Dictionary<string, bool>
    {
      { ".ASX", true },
      { ".DTS", true },
      { ".DTSHD", true },
      { ".AC3", true },
      { ".AC3-HD", true },
      { ".AC3HD", true },
      { ".MOD", true },
      { ".MO3", true },
      { ".S3M", true },
      { ".XM", true },
      { ".IT", true },
      { ".MTM", true },
      { ".UMX", true },
      { ".MDZ", true },
      { ".S3Z", true },
      { ".ITZ", true },
      { ".XMZ", true },
      { ".MP3", true },
      { ".OGG", true },
      { ".WAV", true },
      { ".MP2", true },
      { ".MP1", true },
      { ".AIFF", true },
      { ".M2A", true },
      { ".MPA", true },
      { ".M1A", true },
      { ".SWA", true },
      { ".AIF", true },
      { ".MP3PRO", true },
      { ".MKA", true },
      { ".CDA", true },
      { ".AAC", true },
      { ".MP4A", true },
      { ".MP4B", true },
      { ".M4P", true },
      { ".APE", true },
      { ".APL", true },
      { ".DSF", true },
      { ".FLAC", true },
      { ".OPUS", true },
      { ".WMA", true },
      { ".WMAPRO", true },
      { ".WMA3", true },
      { ".MIDI", true },
      { ".MID", true },
      { ".RMI", true },
      { ".KAR", true },
      { ".MPC", true },
      { ".MPP", true },
      { ".MP+", true },
      { ".OFR", true },
      { ".OFS", true },
      { ".SPX", true },
      { ".TTA", true },
      { ".WV", true },
    };

    #endregion

    /// <summary>
    /// Checks is path a live TV
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsLiveTv(this string path)
    {
      return !string.IsNullOrEmpty(path) && TsBufferMatch.Match(path).Success;
    }

    /// <summary>
    /// Checks is path a RTSP stream
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsRTSP(this string path)
    {
      return !string.IsNullOrEmpty(path) && path.IndexOf("rtsp:", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>
    /// Checks is path a network video
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsNetworkVideo(this string path)
    {
      if (string.IsNullOrEmpty(path)) return false;
      return path.StartsWith("rtsp:", StringComparison.OrdinalIgnoreCase) ||
          (path.StartsWith("mms:", StringComparison.OrdinalIgnoreCase) && path.EndsWith(".ymvp", StringComparison.OrdinalIgnoreCase)) ||
          path.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
          path.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ||
          path.StartsWith("udp:", StringComparison.OrdinalIgnoreCase) ||
          path.StartsWith("rtmp:", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks is path a video
    /// </summary>
    /// <param name="path">path to media</param>
    /// <returns>Whether file is a video file</returns>
    public static bool IsVideo(string path)
    {
      if (string.IsNullOrEmpty(path) || path.IsLastFmStream()) return false;
      if (path.IsNetworkVideo()) return true;
      if (path.Contains(Path.DirectorySeparatorChar) || path.Contains(Path.AltDirectorySeparatorChar) || !path.HasExtension()) return false;
      var extensionFile = path.GetExtension();
      return !extensionFile.IsPlayList() && 
              !extensionFile.IsPicture() && 
              VideoExtensions.ContainsKey(extensionFile.ToUpper());
    }

    public static bool IsPicture(this string path)
    {
      if (string.IsNullOrEmpty(path) || path.Contains(Path.DirectorySeparatorChar) || path.Contains(Path.AltDirectorySeparatorChar)) return false;
      return path.HasExtension() && 
              !path.IsPlayList() && 
              PictureExtensions.ContainsKey(path.GetExtension().ToUpper());
    }

    private static bool IsPlayList(this string extensionFile)
    {
      return PlaylistExtensions.ContainsKey(extensionFile.ToUpper());
    }

    private static bool HasExtension(this string path)
    {
      var items = path.Split('/', '\\');
      return items.LastOrDefault()?.Contains(".") ?? false;
    }

    private static string GetExtension(this string path)
    {
      var items = path.Split('/', '\\');
      var parts = items.LastOrDefault()?.Split('.') ?? new [] { string.Empty };
      return parts.LastOrDefault() ?? string.Empty;
    }

    public static bool IsLastFmStream(this string path)
    {
      return path.StartsWith(@"http://play.last.fm", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsNetwork(this string path)
    {
      return !string.IsNullOrEmpty(path) && 
             path.Length >= 2 && 
             (path.StartsWith($"{Path.DirectorySeparatorChar}") ||
              path.Substring(0, 2).GetDriveType() == 4);
    }

    public static int GetDriveType(this string drive)
    {
      if (string.IsNullOrEmpty(drive)) return 2;
      if ((NativeMethods.GetDriveType(drive) & 5) == 5) return 5; //cd
      if ((NativeMethods.GetDriveType(drive) & 3) == 3) return 3; //fixed
      if ((NativeMethods.GetDriveType(drive) & 2) == 2) return 2; //removable
      if ((NativeMethods.GetDriveType(drive) & 4) == 4) return 4; //remote disk
      if ((NativeMethods.GetDriveType(drive) & 6) == 6) return 6; //ram disk
      return 0;
    }

    public static bool IsUNCNetwork(string strPath)
    {
      return !string.IsNullOrEmpty(strPath) && strPath.StartsWith(@"\\");
    }

    public static bool IsAvStream(this string strPath)
    {
      return !string.IsNullOrEmpty(strPath) &&
             (strPath.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
             strPath.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ||
             strPath.StartsWith("mms:", StringComparison.OrdinalIgnoreCase) ||
             strPath.StartsWith("udp:", StringComparison.OrdinalIgnoreCase) ||
             strPath.StartsWith("rtmp:", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsRemoteUrl(string strPath)
    {
      Uri playbackUri;
      return Uri.TryCreate(strPath, UriKind.Absolute, out playbackUri) && playbackUri.Scheme != "file";
    }

    public static bool IsAudio(this string path)
    {
      if (string.IsNullOrEmpty(path) || path.Contains(Path.DirectorySeparatorChar) || path.Contains(Path.AltDirectorySeparatorChar)) return false;
      if (path.IsLastFmStream()) return true;
      if (!path.HasExtension()) return false;
      var extensionFile = path.GetExtension();
      return !extensionFile.IsPlayList() && AudioExtensions.ContainsKey(extensionFile.ToUpper());
    }
  }
}
