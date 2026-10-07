#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

#if !NETFRAMEWORK
using System.Runtime.CompilerServices;
#endif
using System.Text.RegularExpressions;

namespace MediaInfo
{
  /// <summary>
  /// Static extensions for file paths
  /// </summary>
  public static class FileNameExtensions
  {
    private static readonly Regex TsBufferMatch = new(@"(live\d+-\d+\.ts(\.tsbuffer(\d+\.ts)?)?)$", RegexOptions.Compiled);

    #region Extensions

    private static readonly Dictionary<string, bool> PlaylistExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
      { ".M3U", true },
      { ".M3U8", true },
      { ".PLS", true },
      { ".B4S", true },
      { ".WPL", true },
      { ".CUE", true }
    };

    private static readonly Dictionary<string, bool> PictureExtensions = new(StringComparer.OrdinalIgnoreCase)
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

    private static readonly Dictionary<string, bool> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
      { ".AVI", true },
      { ".BDMV", true },
      { ".MPG", true },
      { ".MPGV", true },
      { ".MPEG", true },
      { ".M2G", true },
      { ".EVO", true },
      { ".MP4", true },
      { ".DIVX", true },
      { ".XVID", true },
      { ".OGM", true },
      { ".MKV", true },
      { ".WMV", true },
      { ".QT", true },
      { ".RM", true },
      { ".MOV", true },
      { ".MOVIE", true },
      { ".MTS", true },
      { ".M2TS", true },
      { ".M2T", true },
      { ".M4T", true },
      { ".M4TS", true },
      { ".M4S", true },
      { ".TP", true },
      { ".TRP", true },
      { ".SBE", true },
      { ".DVR-MS", true },
      { ".TS", true },
      { ".DAT", true },
      { ".IFO", true },
      { ".FLV", true },
      { ".M4V", true },
      { ".3GP", true },
      { ".3GPP", true },
      { ".3GPP2", true },
      { ".OGV", true },
      { ".MK3D", true },
      { ".MPLS", true },
      { ".MPE", true },
      { ".M1V", true },
      { ".M2V", true },
      { ".IFLV", true },
      { ".MPV4", true },
      { ".MQV", true },
      { ".HDMOV", true },
      { ".MP4V", true },
      { ".APV", true },
      { ".WTV", true },
      { ".GVI", true },
      { ".ASF", true },
      { ".HEVC", true },
      { ".AMV", true },
      { ".BDAV", true },
      { ".MPD", true },
      { ".DV", true },
      { ".VC1", true },
      { ".AVC", true },
    };

    private static readonly Dictionary<string, bool> AudioExtensions = new(StringComparer.OrdinalIgnoreCase) 
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
      { ".MP2V", true },
      { ".MP1", true },
      { ".MP1V", true },
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
    /// Returns <c>true</c> if the specified path is live TV; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsLiveTv(this string? path) =>
      !string.IsNullOrEmpty(path) && TsBufferMatch.Match(path).Success;

    /// <summary>
    /// Determines whether this instance is RTSP.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    /// Returns <c>true</c> if the specified path is RTSP; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsRtsp(this string? path) =>
      !string.IsNullOrEmpty(path) && path!.IndexOf("rtsp:", StringComparison.OrdinalIgnoreCase) >= 0;

    /// <summary>
    /// Determines whether path is network video.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    /// Returns <c>true</c> if the specified path is network video; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNetworkVideo(this string? path) =>
      !string.IsNullOrEmpty(path) && (path!.StartsWith("rtsp:", StringComparison.OrdinalIgnoreCase) ||
        (path.StartsWith("mms:", StringComparison.OrdinalIgnoreCase) && path.EndsWith(".ymvp", StringComparison.OrdinalIgnoreCase)) ||
        path.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("udp:", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("rtmp:", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Determines whether the specified path is video.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    /// Returns <c>true</c> if the specified path is video; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsVideo(this string? path)
    {
      if (string.IsNullOrEmpty(path) || path.IsLastFmStream())
      {
        return false;
      }

      if (path.IsNetworkVideo()) return true;
      if (!path.HasExtension())
      {
        return false;
      }

      var extensionFile = path.GetExtension();
      return !extensionFile.IsPlayList() &&
              !extensionFile.IsPicture() &&
              VideoExtensions.ContainsKey(extensionFile);
    }

    /// <summary>
    /// Determines whether this instance is picture.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    /// Returns <c>true</c> if the specified path is picture; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsPicture(this string? path)
    {
      if (string.IsNullOrEmpty(path)) return false;
      var extensionFile = path.GetExtension();
      return path.HasExtension() &&
              !extensionFile.IsPlayList() &&
              PictureExtensions.ContainsKey(extensionFile);
    }

    /// <summary>
    /// Determines whether is LastFM stream.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    /// Returns <c>true</c> if the specified path is LastFM stream; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsLastFmStream(this string? path) =>
      !string.IsNullOrEmpty(path) && path!.StartsWith("http://play.last.fm", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether specified path is network path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    /// Returns <c>true</c> if the specified path is network path; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNetwork(this string? path) =>
      !string.IsNullOrEmpty(path) &&
        path!.Length > 2 &&
        (path.StartsWith(string.Concat(Path.DirectorySeparatorChar, Path.DirectorySeparatorChar)) ||
        path.Substring(0, 2).GetDriveType() == 4);

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
    public static int GetDriveType(this string? drive)
    {
      if (string.IsNullOrEmpty(drive)) return 2;
      var info = new DriveInfo(drive);
      return (int)info.DriveType;
    }

    /// <summary>
    /// Determines whether the specified string path is UNC network.
    /// </summary>
    /// <param name="strPath">The string path.</param>
    /// <returns>
    /// Returns <c>true</c> if the specified string path is UNC network; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsUncNetwork(this string? strPath) =>
      !string.IsNullOrEmpty(strPath) && strPath!.StartsWith(@"\\");

    /// <summary>
    /// Determines whether the specified string path is RTMP stream.
    /// </summary>
    /// <param name="strPath">The string path.</param>
    /// <returns>
    /// Returns <c>true</c> if the specified string path is RTMP stream; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsRtmp(this string? strPath) =>
      !string.IsNullOrEmpty(strPath) &&
        strPath!.StartsWith("rtmp:", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the specified string path is MMS stream.
    /// </summary>
    /// <param name="strPath">The string path.</param>
    /// <returns>
    /// Returns <c>true</c> if the specified string path is MMS stream; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsMms(this string? strPath) =>
      !string.IsNullOrEmpty(strPath) &&
        strPath!.StartsWith("mms:", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the specified string path is A/V stream.
    /// </summary>
    /// <param name="strPath">The string path.</param>
    /// <returns>
    /// Returns <c>true</c> if the specified string path is A/V stream; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsAvStream(this string? strPath) =>
      !string.IsNullOrEmpty(strPath) &&
        (strPath!.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
        strPath.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ||
        strPath.StartsWith("mms:", StringComparison.OrdinalIgnoreCase) ||
        strPath.StartsWith("udp:", StringComparison.OrdinalIgnoreCase) ||
        strPath.StartsWith("rtmp:", StringComparison.OrdinalIgnoreCase) ||
        strPath.StartsWith("rtsp:", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Determines whether the specified string path is remote URL.
    /// </summary>
    /// <param name="strPath">The string path.</param>
    /// <returns>
    /// Returns <c>true</c> if the specified string path is remote URL; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsRemoteUrl(this string? strPath) =>
      !string.IsNullOrEmpty(strPath) && Uri.TryCreate(strPath, UriKind.Absolute, out var playbackUri) && playbackUri.Scheme != "file";

    /// <summary>
    /// Determines whether specified path is audio.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    /// Returns <c>true</c> if the specified path is audio; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsAudio(this string? path)
    {
      if (string.IsNullOrEmpty(path)) return false;
      if (path.IsLastFmStream()) return true;
      if (!path.HasExtension()) return false;
      var extensionFile = path.GetExtension();
      return !extensionFile.IsPlayList() && AudioExtensions.ContainsKey(extensionFile);
    }

    /// <summary>
    /// Determines whether the specified path represents a DVD structure by checking for the presence of an IFO
    /// file.
    /// </summary>
    /// <remarks>If the path is a directory, the method searches all subdirectories for files with the
    /// ".IFO" extension. If the path is a file with the ".IFO" extension, it is considered a valid DVD structure.
    /// The search is case-insensitive.</remarks>
    /// <param name="path">The file or directory path to examine. Can be a path to a directory or an IFO file. May be null.</param>
    /// <param name="pathToIfoFile">When this method returns, contains the full path to the first IFO file found if the path is identified as a
    /// DVD structure; otherwise, null. This parameter is passed uninitialized.</param>
    /// <returns>true if the specified path is a DVD structure or an IFO file; otherwise, false.</returns>
    public static bool IsDvD(this string? path, [NotNullWhen(true)] out string? pathToIfoFile) =>
      path.IsComplexStructure(".IFO", out pathToIfoFile);

    /// <summary>
    /// Determines whether the specified path represents a Blu-ray disc structure and retrieves the path to the BDMV
    /// file if found.
    /// </summary>
    /// <param name="path">The file system path to examine. Can be null.</param>
    /// <param name="pathToBdmvFile">When this method returns <see langword="true"/>, contains the full path to the BDMV file within the Blu-ray
    /// structure; otherwise, null.</param>
    /// <returns><see langword="true"/> if the path represents a valid Blu-ray disc structure and the BDMV file is found;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool IsBluRay(this string? path, [NotNullWhen(true)] out string? pathToBdmvFile) =>
      path.IsComplexStructure(".BDMV", out pathToBdmvFile);

    private static bool IsComplexStructure(this string? path, string pattern, [NotNullWhen(true)] out string? pathToResult)
    {
      if (string.IsNullOrEmpty(path))
      {
        pathToResult = null;
        return false;
      }

      if (!path!.IsDirectory() && path!.EndsWith(pattern, StringComparison.OrdinalIgnoreCase))
      {
        pathToResult = path;
        return true;
      }

      if (path!.IsDirectory())
      {
        var files = Directory.GetFiles(
          path,
          "*" + pattern,
#if NETFRAMEWORK
          SearchOption.AllDirectories);
#else
          new EnumerationOptions { IgnoreInaccessible = true, MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = true });
#endif
        if (files.Any())
        {
          pathToResult = files.First();
          return true;
        }
      }

      pathToResult = null;
      return false;
    }

    /// <summary>
    /// Determines whether the specified path refers to an existing directory.
    /// </summary>
    /// <remarks>
    /// This method uses <see cref="Directory.Exists(string)"/> and returns <see langword="false"/> if the path
    /// is invalid, does not exist, or is inaccessible due to insufficient permissions.
    /// </remarks>
    /// <param name="path">The file system path to check. This can be either a relative or absolute path.</param>
    /// <returns><see langword="true"/> if the specified path refers to an existing directory; otherwise, <see langword="false"/>.</returns>
    public static bool IsDirectory(this string path) =>
      Directory.Exists(path);

    private static bool IsPlayList(this string? extensionFile) =>
      !string.IsNullOrEmpty(extensionFile) && PlaylistExtensions.ContainsKey(extensionFile!);

#if !NETFRAMEWORK
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static bool HasExtension(this string? path) =>
      !string.IsNullOrEmpty(path) && Path.HasExtension(path);

#if !NETFRAMEWORK
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private static string GetExtension(this string? path) =>
      !string.IsNullOrEmpty(path) ? Path.GetExtension(path) : string.Empty;
  }
}
