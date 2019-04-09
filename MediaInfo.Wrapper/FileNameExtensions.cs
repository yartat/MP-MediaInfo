#region Copyright (C) 2005-2019 Team MediaPortal

// Copyright (C) 2005-2019 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MediaInfo
{
  /// <summary>
  /// Static extensions for file paths
  /// </summary>
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
    /// Determines whether path is live TV.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   <c>true</c> if the specified path is live TV; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsLiveTv(this string path)
    {
      return !string.IsNullOrEmpty(path) && TsBufferMatch.Match(path).Success;
    }

    /// <summary>
    /// Determines whether this instance is RTSP.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   <c>true</c> if the specified path is RTSP; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsRtsp(this string path)
    {
      return !string.IsNullOrEmpty(path) && path.IndexOf("rtsp:", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>
    /// Determines whether path is network video.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   <c>true</c> if the specified path is network video; otherwise, <c>false</c>.
    /// </returns>
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
    /// Determines whether the specified path is video.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   <c>true</c> if the specified path is video; otherwise, <c>false</c>.
    /// </returns>
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

    /// <summary>
    /// Determines whether this instance is picture.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   <c>true</c> if the specified path is picture; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsPicture(this string path)
    {
      if (string.IsNullOrEmpty(path) || path.Contains(Path.DirectorySeparatorChar) || path.Contains(Path.AltDirectorySeparatorChar)) return false;
      return path.HasExtension() && 
              !path.IsPlayList() && 
              PictureExtensions.ContainsKey(path.GetExtension().ToUpper());
    }

    /// <summary>
    /// Determines whether is LastFM stream.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   <c>true</c> if the specified path is LastFM stream; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsLastFmStream(this string path)
    {
      return path.StartsWith(@"http://play.last.fm", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether specified path is network path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   <c>true</c> if the specified path is network path; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNetwork(this string path)
    {
      return !string.IsNullOrEmpty(path) && 
             path.Length >= 2 && 
             (path.StartsWith($"{Path.DirectorySeparatorChar}") ||
              path.Substring(0, 2).GetDriveType() == 4);
    }

    /// <summary>
    /// Gets the type of the drive.
    /// </summary>
    /// <param name="drive">The drive.</param>
    /// <returns>Returns drive type.
    /// <b>0</b> - undefined
    /// <b>2</b> - removable drive (Flash, Floppy)
    /// <b>3</b> - fixed drive (HDD)
    /// <b>4</b> - remote drive (network share)
    /// <b>5</b> - CD/DVD drive
    /// <b>6</b> - RAM disk drive
    /// </returns>
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

    /// <summary>
    /// Determines whether the specified string path is UNC network.
    /// </summary>
    /// <param name="strPath">The string path.</param>
    /// <returns>
    ///   <c>true</c> if the specified string path is UNC network; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsUncNetwork(string strPath)
    {
      return !string.IsNullOrEmpty(strPath) && strPath.StartsWith(@"\\");
    }

    /// <summary>
    /// Determines whether the specified string path is A/V stream.
    /// </summary>
    /// <param name="strPath">The string path.</param>
    /// <returns>
    ///   <c>true</c> if the specified string path is A/V stream; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsAvStream(this string strPath)
    {
      return !string.IsNullOrEmpty(strPath) &&
             (strPath.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
             strPath.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ||
             strPath.StartsWith("mms:", StringComparison.OrdinalIgnoreCase) ||
             strPath.StartsWith("udp:", StringComparison.OrdinalIgnoreCase) ||
             strPath.StartsWith("rtmp:", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines whether the specified string path is remote URL.
    /// </summary>
    /// <param name="strPath">The string path.</param>
    /// <returns>
    ///   <c>true</c> if the specified string path is remote URL; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsRemoteUrl(string strPath)
    {
      return Uri.TryCreate(strPath, UriKind.Absolute, out Uri playbackUri) && playbackUri.Scheme != "file";
    }

    /// <summary>
    /// Determines whether specified path is audio.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   <c>true</c> if the specified path is audio; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsAudio(this string path)
    {
      if (string.IsNullOrEmpty(path) || path.Contains(Path.DirectorySeparatorChar) || path.Contains(Path.AltDirectorySeparatorChar)) return false;
      if (path.IsLastFmStream()) return true;
      if (!path.HasExtension()) return false;
      var extensionFile = path.GetExtension();
      return !extensionFile.IsPlayList() && AudioExtensions.ContainsKey(extensionFile.ToUpper());
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
      var parts = items.LastOrDefault()?.Split('.') ?? new[] { string.Empty };
      return parts.LastOrDefault() ?? string.Empty;
    }
  }
}
