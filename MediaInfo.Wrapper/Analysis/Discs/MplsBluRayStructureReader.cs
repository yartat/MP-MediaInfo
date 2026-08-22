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
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Discs.Parsers;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Strategies;

namespace MediaInfo.Analysis.Discs;

/// <summary>
/// Discovers the titles of a Blu-ray by reading its playlists.
/// </summary>
/// <remarks>
/// A Blu-ray title is a playlist, not a stream file. The folder based reader has to treat every clip as its own
/// title, which splits a feature assembled from several clips and cannot tell the feature apart from the copies of it
/// that the disc keeps for its seamless branching. Reading the playlists gives the real titles, in order, with their
/// chapters, and without opening a single stream file.
/// <para>
/// Discs carry many playlists, most of them menus, trailers and decoys. Playlists that name no clip that exists, or
/// that run for less than a minute, are dropped, and a playlist that plays exactly the same clips as one already kept
/// is dropped as a duplicate.
/// </para>
/// </remarks>
public sealed class MplsBluRayStructureReader : IDiscStructureReader
{
  private const string StreamFolderName = "STREAM";
  private const string PlaylistFolderName = "PLAYLIST";

  private static readonly TimeSpan ShortestTitle = TimeSpan.FromMinutes(1);

  private readonly IFileSystem _fileSystem;

  /// <summary>
  /// Initializes a new instance of the <see cref="MplsBluRayStructureReader"/> class.
  /// </summary>
  /// <param name="fileSystem">The file system the disc is read from.</param>
  /// <exception cref="ArgumentNullException"><paramref name="fileSystem"/> is <see langword="null"/>.</exception>
  public MplsBluRayStructureReader(IFileSystem fileSystem)
  {
    _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
  }

  /// <inheritdoc />
  public DiscKind Kind => DiscKind.BluRay;

  /// <inheritdoc />
  public int Priority => 10;

  /// <inheritdoc />
  public bool CanRead(string path, [NotNullWhen(true)] out string? discRoot) =>
    new BluRayStructureReader(_fileSystem).CanRead(path, out discRoot);

  /// <inheritdoc />
  public Task<DiscStructure?> ReadAsync(
    string discRoot,
    MediaAnalysisContext context,
    CancellationToken cancellationToken)
  {
    var streamFolder = Path.Combine(discRoot, StreamFolderName);
    var playlistFolder = Path.Combine(discRoot, PlaylistFolderName);

    var playlistFiles = _fileSystem
      .GetFiles(playlistFolder, "*.mpls", SearchOption.TopDirectoryOnly)
      .OrderBy(x => Path.GetFileName(x), StringComparer.OrdinalIgnoreCase)
      .ToArray();

    if (playlistFiles.Length == 0)
    {
      return Task.FromResult<DiscStructure?>(null);
    }

    var clips = _fileSystem
      .GetFiles(streamFolder, "*.m2ts", SearchOption.TopDirectoryOnly)
      .ToDictionary(x => Path.GetFileNameWithoutExtension(x), x => x, StringComparer.OrdinalIgnoreCase);

    var totalSize = _fileSystem.GetFiles(discRoot, "*", SearchOption.AllDirectories).Sum(_fileSystem.GetFileLength);

    var titles = new List<BluRayPlaylist>();
    var seen = new HashSet<string>(StringComparer.Ordinal);

    foreach (var playlistFile in playlistFiles)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var playlist = MplsParser.Parse(_fileSystem.FileExists(playlistFile) ? Read(playlistFile) : []);
      if (playlist.IsEmpty || playlist.Duration < ShortestTitle)
      {
        continue;
      }

      var resolved = playlist.Items
        .Where(item => clips.ContainsKey(item.ClipId))
        .Select(item => new BluRayClip(
          item.ClipId,
          clips[item.ClipId],
          item.Duration,
          _fileSystem.GetFileLength(clips[item.ClipId])))
        .ToArray();

      if (resolved.Length == 0)
      {
        continue;
      }

      // Seamless branching leaves several playlists that play the same clips; only the first is a title.
      if (!seen.Add(string.Join("|", resolved.Select(x => x.ClipId))))
      {
        continue;
      }

      titles.Add(
        new BluRayPlaylist(
          titles.Count + 1,
          Path.GetFileNameWithoutExtension(playlistFile),
          playlist.Duration,
          resolved,
          playlistFile));
    }

    if (titles.Count == 0)
    {
      return Task.FromResult<DiscStructure?>(null);
    }

    context.Report(
      new AnalysisProgress(
        AnalysisPhase.DiscoveringStructure,
        Detail: $"Read {titles.Count} title(s) from {playlistFiles.Length} Blu-ray playlist(s) in '{discRoot}'."));

    return Task.FromResult<DiscStructure?>(
      new BluRayStructure(discRoot, totalSize, titles, playlistFiles));
  }

  private byte[] Read(string path)
  {
    try
    {
      using var stream = _fileSystem.OpenRead(path);
      using var buffer = new MemoryStream();
      stream.CopyTo(buffer);
      return buffer.ToArray();
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
      return [];
    }
  }
}

/// <summary>
/// Tries one disc reader and falls back to another when it produces nothing.
/// </summary>
/// <remarks>
/// The reader that parses the navigation tables describes a disc far better than the one that walks the folder, but
/// it depends on those tables being present and well formed. Pairing the two means a disc with readable tables is
/// described in full, and a disc without them is still described.
/// </remarks>
public sealed class FallbackDiscStructureReader : IDiscStructureReader
{
  private readonly IDiscStructureReader _preferred;
  private readonly IDiscStructureReader _fallback;

  /// <summary>
  /// Initializes a new instance of the <see cref="FallbackDiscStructureReader"/> class.
  /// </summary>
  /// <param name="preferred">The reader to try first.</param>
  /// <param name="fallback">The reader to use when the preferred one produces nothing.</param>
  /// <exception cref="ArgumentNullException">A reader is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentException">The two readers describe different kinds of disc.</exception>
  public FallbackDiscStructureReader(IDiscStructureReader preferred, IDiscStructureReader fallback)
  {
    _preferred = preferred ?? throw new ArgumentNullException(nameof(preferred));
    _fallback = fallback ?? throw new ArgumentNullException(nameof(fallback));

    if (preferred.Kind != fallback.Kind)
    {
      throw new ArgumentException(
        $"Both readers must describe the same kind of disc, but they describe {preferred.Kind} and {fallback.Kind}.",
        nameof(fallback));
    }
  }

  /// <inheritdoc />
  public DiscKind Kind => _preferred.Kind;

  /// <inheritdoc />
  public int Priority => Math.Min(_preferred.Priority, _fallback.Priority);

  /// <inheritdoc />
  public bool CanRead(string path, [NotNullWhen(true)] out string? discRoot) =>
    _preferred.CanRead(path, out discRoot) || _fallback.CanRead(path, out discRoot);

  /// <inheritdoc />
  public async Task<DiscStructure?> ReadAsync(
    string discRoot,
    MediaAnalysisContext context,
    CancellationToken cancellationToken)
  {
    var structure = await _preferred.ReadAsync(discRoot, context, cancellationToken).ConfigureAwait(false);
    return structure ?? await _fallback.ReadAsync(discRoot, context, cancellationToken).ConfigureAwait(false);
  }
}
