#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaInfo.Analysis.Results;

/// <summary>
/// Describes a single stream clip of a Blu-ray structure.
/// </summary>
/// <param name="ClipId">The identifier of the clip, which is the file name of the transport stream without extension.</param>
/// <param name="FilePath">The path of the transport stream file of the clip.</param>
/// <param name="Duration">The duration of the clip.</param>
/// <param name="Size">The size of the clip in bytes.</param>
public sealed record BluRayClip(string ClipId, string FilePath, TimeSpan Duration, long Size);

/// <summary>
/// Describes a single playlist of a Blu-ray structure.
/// </summary>
/// <remarks>
/// The clips of a playlist are only populated when the disc reader in use parses the playlist tables; the reader that
/// discovers titles from the folder layout maps one title to one clip and leaves the playlist file unresolved.
/// </remarks>
public sealed record BluRayPlaylist : IDiscTitle
{
  /// <summary>
  /// Initializes a new instance of the <see cref="BluRayPlaylist"/> class.
  /// </summary>
  /// <param name="number">The ordinal of the title on the disc, starting at one.</param>
  /// <param name="name">The name of the title, which is the playlist file name when one is known.</param>
  /// <param name="duration">The duration of the title.</param>
  /// <param name="clips">The clips that make up the title, in playback order.</param>
  /// <param name="playlistFile">The playlist file of the title, when the disc declares one.</param>
  public BluRayPlaylist(
    int number,
    string name,
    TimeSpan duration,
    IReadOnlyList<BluRayClip> clips,
    string? playlistFile = null)
  {
    Number = number;
    Name = name;
    Duration = duration;
    Clips = clips;
    PlaylistFile = playlistFile;
  }

  /// <inheritdoc />
  public int Number { get; }

  /// <summary>
  /// Gets the name of the title.
  /// </summary>
  public string Name { get; }

  /// <inheritdoc />
  public TimeSpan Duration { get; }

  /// <summary>
  /// Gets the clips that make up the title, in playback order.
  /// </summary>
  public IReadOnlyList<BluRayClip> Clips { get; }

  /// <summary>
  /// Gets the playlist file of the title, when the disc declares one.
  /// </summary>
  public string? PlaylistFile { get; }

  /// <inheritdoc />
  public long Size => Clips.Sum(x => x.Size);

  /// <inheritdoc />
  public string PrimaryFile => Clips.Count > 0 ? Clips[0].FilePath : string.Empty;

  /// <inheritdoc />
  public IReadOnlyList<string> Files => Clips.Select(x => x.FilePath).ToArray();
}

/// <summary>
/// Describes the structure of a Blu-ray disc.
/// </summary>
public sealed record BluRayStructure : DiscStructure
{
  /// <summary>
  /// Initializes a new instance of the <see cref="BluRayStructure"/> class.
  /// </summary>
  /// <param name="rootPath">The BDMV folder of the disc.</param>
  /// <param name="totalSize">The total size of the disc content in bytes.</param>
  /// <param name="playlists">The titles of the disc.</param>
  /// <param name="playlistFiles">The playlist files present on the disc.</param>
  public BluRayStructure(
    string rootPath,
    long totalSize,
    IReadOnlyList<BluRayPlaylist> playlists,
    IReadOnlyList<string>? playlistFiles = null)
    : base(DiscKind.BluRay, rootPath, totalSize)
  {
    Playlists = playlists;
    PlaylistFiles = playlistFiles ?? [];
  }

  /// <summary>
  /// Gets the titles of the disc.
  /// </summary>
  public IReadOnlyList<BluRayPlaylist> Playlists { get; }

  /// <summary>
  /// Gets the playlist files present on the disc.
  /// </summary>
  public IReadOnlyList<string> PlaylistFiles { get; }

  /// <inheritdoc />
  public override IReadOnlyList<IDiscTitle> Titles => Playlists.Cast<IDiscTitle>().ToArray();
}
