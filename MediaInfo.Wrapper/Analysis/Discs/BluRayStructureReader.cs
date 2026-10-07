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
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Strategies;

namespace MediaInfo.Analysis.Discs;

/// <summary>
/// Discovers the titles of a Blu-ray disc from the layout of its BDMV folder.
/// </summary>
/// <remarks>
/// A Blu-ray keeps its transport streams in BDMV/STREAM and describes how they are played back in the playlists of
/// BDMV/PLAYLIST. This reader treats every stream clip as a title and probes the media to rank them, which identifies
/// the feature and describes what is on the disc. It does not parse the playlists, so a title that is assembled from
/// several clips is reported as several titles, and the playlist files are only listed by name.
/// </remarks>
public sealed class BluRayStructureReader : IDiscStructureReader
{
  private const string BdmvFolderName = "BDMV";
  private const string StreamFolderName = "STREAM";
  private const string PlaylistFolderName = "PLAYLIST";
  private const string IndexFileName = "index.bdmv";

  private readonly IFileSystem _fileSystem;

  /// <summary>
  /// Initializes a new instance of the <see cref="BluRayStructureReader"/> class.
  /// </summary>
  /// <param name="fileSystem">The file system the disc is read from.</param>
  /// <exception cref="ArgumentNullException"><paramref name="fileSystem"/> is <see langword="null"/>.</exception>
  public BluRayStructureReader(IFileSystem fileSystem)
  {
    _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
  }

  /// <inheritdoc />
  public DiscKind Kind => DiscKind.BluRay;

  /// <inheritdoc />
  public int Priority => 100;

  /// <inheritdoc />
  public bool CanRead(string path, [NotNullWhen(true)] out string? discRoot)
  {
    discRoot = null;
    if (string.IsNullOrEmpty(path) || !_fileSystem.DirectoryExists(path))
    {
      return false;
    }

    // The folder is the BDMV folder itself.
    if (IsBdmvFolder(path))
    {
      discRoot = path;
      return true;
    }

    // The folder is the root of the disc and holds a BDMV folder.
    var nested = _fileSystem
      .GetDirectories(path)
      .FirstOrDefault(x => string.Equals(GetName(x), BdmvFolderName, StringComparison.OrdinalIgnoreCase));

    if (nested is not null && IsBdmvFolder(nested))
    {
      discRoot = nested;
      return true;
    }

    return false;
  }

  /// <inheritdoc />
  public async Task<DiscStructure?> ReadAsync(
    string discRoot,
    MediaAnalysisContext context,
    CancellationToken cancellationToken)
  {
    var streamFolder = Combine(discRoot, StreamFolderName);
    var playlistFolder = Combine(discRoot, PlaylistFolderName);

    var totalSize = _fileSystem
      .GetFiles(discRoot, "*", SearchOption.AllDirectories)
      .Sum(_fileSystem.GetFileLength);

    var playlistFiles = _fileSystem
      .GetFiles(playlistFolder, "*.mpls", SearchOption.TopDirectoryOnly)
      .OrderBy(x => GetName(x), StringComparer.OrdinalIgnoreCase)
      .ToArray();

    var clipFiles = _fileSystem
      .GetFiles(streamFolder, "*.m2ts", SearchOption.TopDirectoryOnly)
      .Select(path => (Path: path, Size: _fileSystem.GetFileLength(path)))
      .Where(x => x.Size >= context.Options.MinimumDiscTitleSize)
      // The largest clips are probed first, so that a probe budget is spent on the clips most likely to be the feature.
      .OrderByDescending(x => x.Size)
      .ToList();

    if (clipFiles.Count == 0)
    {
      return null;
    }

    context.Report(
      new AnalysisProgress(
        AnalysisPhase.DiscoveringStructure,
        Detail: $"Found {clipFiles.Count} Blu-ray clip(s) and {playlistFiles.Length} playlist(s) in '{discRoot}'."));

    var probesLeft = context.Options.MaxProbedDiscTitles;
    var titles = new List<BluRayPlaylist>(clipFiles.Count);

    foreach (var clipFile in clipFiles)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var duration = TimeSpan.Zero;
      if (probesLeft > 0)
      {
        --probesLeft;
        duration = await context.Probe
          .ProbeDurationAsync(clipFile.Path, context.Options, cancellationToken)
          .ConfigureAwait(false);
      }

      var clipId = Path.GetFileNameWithoutExtension(clipFile.Path);
      var clip = new BluRayClip(clipId, clipFile.Path, duration, clipFile.Size);
      titles.Add(new BluRayPlaylist(titles.Count + 1, clipId, duration, [clip]));
    }

    return new BluRayStructure(discRoot, totalSize, titles, playlistFiles);
  }

  private bool IsBdmvFolder(string path)
  {
    if (_fileSystem.FileExists(Combine(path, IndexFileName)))
    {
      return true;
    }

    return _fileSystem.DirectoryExists(Combine(path, StreamFolderName)) &&
      string.Equals(GetName(path), BdmvFolderName, StringComparison.OrdinalIgnoreCase);
  }

  private static string Combine(string path, string name) => Path.Combine(path, name);

  private static string GetName(string path) => Path.GetFileName(path.TrimEnd('/', '\\'));
}
