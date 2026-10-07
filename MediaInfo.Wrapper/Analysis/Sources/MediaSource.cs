#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;

namespace MediaInfo.Analysis.Sources;

/// <summary>
/// Describes the kind of a media to analyze.
/// </summary>
public enum MediaSourceKind
{
  /// <summary>
  /// The kind of the media could not be determined.
  /// </summary>
  Unknown,

  /// <summary>
  /// A single media file.
  /// </summary>
  File,

  /// <summary>
  /// A readable stream of media data.
  /// </summary>
  Stream,

  /// <summary>
  /// A folder that holds an optical disc structure.
  /// </summary>
  Directory,

  /// <summary>
  /// A media served over a network protocol.
  /// </summary>
  Network
}

/// <summary>
/// Describes a media to analyze.
/// </summary>
/// <remarks>
/// A media source carries no behaviour. It states what should be analyzed, and an analysis strategy decides how.
/// </remarks>
public interface IMediaSource
{
  /// <summary>
  /// Gets the kind of the media.
  /// </summary>
  MediaSourceKind Kind { get; }

  /// <summary>
  /// Gets a name that identifies the media in logs and results.
  /// </summary>
  string DisplayName { get; }
}

/// <summary>
/// Describes a single media file to analyze.
/// </summary>
/// <param name="path">The path of the media file.</param>
public sealed class FileMediaSource(string path) : IMediaSource
{
  /// <summary>
  /// Gets the path of the media file.
  /// </summary>
  public string Path { get; } = path;

  /// <inheritdoc />
  public MediaSourceKind Kind => MediaSourceKind.File;

  /// <inheritdoc />
  public string DisplayName => Path;
}

/// <summary>
/// Describes a stream of media data to analyze.
/// </summary>
/// <param name="stream">The stream that holds the media data.</param>
/// <param name="leaveOpen">
/// <see langword="true"/> to leave the stream open after the analysis; otherwise, <see langword="false"/>.
/// </param>
/// <param name="name">A name that identifies the stream in logs and results.</param>
public sealed class StreamMediaSource(Stream stream, bool leaveOpen = true, string? name = null) : IMediaSource
{
  /// <summary>
  /// Gets the stream that holds the media data.
  /// </summary>
  public Stream Stream { get; } = stream ?? throw new ArgumentNullException(nameof(stream));

  /// <summary>
  /// Gets a value indicating whether the stream is left open after the analysis.
  /// </summary>
  public bool LeaveOpen { get; } = leaveOpen;

  /// <inheritdoc />
  public MediaSourceKind Kind => MediaSourceKind.Stream;

  /// <inheritdoc />
  public string DisplayName { get; } = name ?? (stream is FileStream fileStream ? fileStream.Name : "<stream>");
}

/// <summary>
/// Describes a folder that holds an optical disc structure to analyze.
/// </summary>
/// <param name="path">The path of the folder.</param>
public sealed class DirectoryMediaSource(string path) : IMediaSource
{
  /// <summary>
  /// Gets the path of the folder.
  /// </summary>
  public string Path { get; } = path;

  /// <inheritdoc />
  public MediaSourceKind Kind => MediaSourceKind.Directory;

  /// <inheritdoc />
  public string DisplayName => Path;
}

/// <summary>
/// Describes a media served over a network protocol to analyze.
/// </summary>
/// <param name="location">The location of the media.</param>
public sealed class NetworkMediaSource(string location) : IMediaSource
{
  /// <summary>
  /// Gets the location of the media.
  /// </summary>
  public string Location { get; } = location;

  /// <inheritdoc />
  public MediaSourceKind Kind => MediaSourceKind.Network;

  /// <inheritdoc />
  public string DisplayName => Location;
}

/// <summary>
/// Creates media sources from paths, locations and streams.
/// </summary>
public static class MediaSource
{
  /// <summary>
  /// Creates a media source that describes the specified path or location.
  /// </summary>
  /// <remarks>
  /// A folder that holds a VIDEO_TS or BDMV structure, and a file inside such a structure, both produce a
  /// <see cref="DirectoryMediaSource"/> rooted at the disc folder, so that the disc is analyzed as a whole.
  /// </remarks>
  /// <param name="pathOrLocation">The path of a file or folder, or the location of a network media.</param>
  /// <returns>Returns a media source that describes the specified path or location.</returns>
  /// <exception cref="ArgumentException"><paramref name="pathOrLocation"/> is <see langword="null"/> or empty.</exception>
  public static IMediaSource From(string pathOrLocation)
  {
    if (string.IsNullOrWhiteSpace(pathOrLocation))
    {
      throw new ArgumentException("The path or location of the media must be specified.", nameof(pathOrLocation));
    }

    if (pathOrLocation.IsAvStream() || pathOrLocation.IsRtsp() || pathOrLocation.IsRtmp() || pathOrLocation.IsMms())
    {
      return new NetworkMediaSource(pathOrLocation);
    }

    if (pathOrLocation.IsDvD(out var ifoPath))
    {
      return new DirectoryMediaSource(System.IO.Path.GetDirectoryName(ifoPath) ?? pathOrLocation);
    }

    if (pathOrLocation.IsBluRay(out var bdmvPath))
    {
      return new DirectoryMediaSource(System.IO.Path.GetDirectoryName(bdmvPath) ?? pathOrLocation);
    }

    return Directory.Exists(pathOrLocation)
      ? new DirectoryMediaSource(pathOrLocation)
      : new FileMediaSource(pathOrLocation);
  }

  /// <summary>
  /// Creates a media source that describes the specified stream.
  /// </summary>
  /// <param name="stream">The stream that holds the media data.</param>
  /// <param name="leaveOpen">
  /// <see langword="true"/> to leave the stream open after the analysis; otherwise, <see langword="false"/>.
  /// </param>
  /// <param name="name">A name that identifies the stream in logs and results.</param>
  /// <returns>Returns a media source that describes the specified stream.</returns>
  public static IMediaSource From(Stream stream, bool leaveOpen = true, string? name = null) =>
    new StreamMediaSource(stream, leaveOpen, name);
}
