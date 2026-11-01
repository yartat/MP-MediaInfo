#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

namespace MediaInfo.Model
{
  /// <summary>
  /// Describes the HDR properties of a video stream.
  /// </summary>
  /// <remarks>
  /// Text values are kept as reported by the library. An absent value is an empty string. When a stream carries several HDR
  /// formats, the values of the format details are separated by " / ".
  /// </remarks>
  public sealed class HdrInfo
  {
    /// <summary>
    /// Gets the detected HDR format.
    /// </summary>
    public Hdr Format { get; init; }

    /// <summary>
    /// Gets the commercial name of the HDR format.
    /// </summary>
    public string Commercial { get; init; } = string.Empty;

    /// <summary>
    /// Gets the version of the HDR format.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets the profile of the HDR format, for example "dvhe.08".
    /// </summary>
    public string Profile { get; init; } = string.Empty;

    /// <summary>
    /// Gets the level of the HDR format.
    /// </summary>
    public string Level { get; init; } = string.Empty;

    /// <summary>
    /// Gets the settings (layers) of the HDR format, for example "BL+RPU".
    /// </summary>
    public string Settings { get; init; } = string.Empty;

    /// <summary>
    /// Gets the compression of the HDR format.
    /// </summary>
    public string Compression { get; init; } = string.Empty;

    /// <summary>
    /// Gets the compatibility of the HDR format with commercial formats, for example "HDR10".
    /// </summary>
    public string Compatibility { get; init; } = string.Empty;

    /// <summary>
    /// Gets the Dolby Vision metadata, or <see langword="null"/> when the stream has none.
    /// </summary>
    public DolbyVisionInfo? DolbyVision { get; init; }
  }
}
