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
/// Discovers the titles of a DVD by reading its navigation tables.
/// </summary>
/// <remarks>
/// This is what the folder based reader cannot do. The tables name every title on the disc, which title set plays it,
/// how many camera angles it has and where each of its chapters starts, and they give the playback time without any
/// media file having to be opened. A disc whose tables cannot be read yields no structure, so that the reader wrapping
/// this one can fall back to the folder layout.
/// </remarks>
public sealed class IfoDvdStructureReader : IDiscStructureReader
{
  private const string VideoManagerFileName = "VIDEO_TS.IFO";

  private readonly IFileSystem _fileSystem;

  /// <summary>
  /// Initializes a new instance of the <see cref="IfoDvdStructureReader"/> class.
  /// </summary>
  /// <param name="fileSystem">The file system the disc is read from.</param>
  /// <exception cref="ArgumentNullException"><paramref name="fileSystem"/> is <see langword="null"/>.</exception>
  public IfoDvdStructureReader(IFileSystem fileSystem)
  {
    _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
  }

  /// <inheritdoc />
  public DiscKind Kind => DiscKind.Dvd;

  /// <inheritdoc />
  public int Priority => 10;

  /// <inheritdoc />
  public bool CanRead(string path, [NotNullWhen(true)] out string? discRoot) =>
    new DvdStructureReader(_fileSystem).CanRead(path, out discRoot);

  /// <inheritdoc />
  public Task<DiscStructure?> ReadAsync(
    string discRoot,
    MediaAnalysisContext context,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    var videoManagerPath = Path.Combine(discRoot, VideoManagerFileName);
    if (!_fileSystem.FileExists(videoManagerPath))
    {
      return Task.FromResult<DiscStructure?>(null);
    }

    var declared = IfoParser.ReadTitles(Read(videoManagerPath));
    if (declared.Count == 0)
    {
      return Task.FromResult<DiscStructure?>(null);
    }

    var files = _fileSystem.GetFiles(discRoot, "*", SearchOption.TopDirectoryOnly);
    var totalSize = files.Sum(_fileSystem.GetFileLength);

    var titles = new List<DvdTitle>(declared.Count);
    var chainsBySet = new Dictionary<int, IReadOnlyDictionary<int, ProgramChain>>();
    var mapsBySet = new Dictionary<int, IReadOnlyDictionary<int, int>>();

    foreach (var title in declared)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var informationFile = FindTitleSetFile(files, title.TitleSetNumber, ".IFO");
      if (informationFile is null)
      {
        continue;
      }

      if (!chainsBySet.TryGetValue(title.TitleSetNumber, out var chains))
      {
        var content = Read(informationFile);
        chains = IfoParser.ReadProgramChains(content);
        mapsBySet[title.TitleSetNumber] = IfoParser.ReadTitleToChainMap(content);
        chainsBySet[title.TitleSetNumber] = chains;
      }

      var map = mapsBySet[title.TitleSetNumber];
      var chain = map.TryGetValue(title.TitleNumberInSet, out var number) && chains.TryGetValue(number, out var found)
        ? found
        : default;

      var vobs = FindTitleSetVobs(files, title.TitleSetNumber);

      titles.Add(
        new DvdTitle(
          title.Number,
          title.TitleSetNumber,
          chain.Duration,
          vobs.Sum(_fileSystem.GetFileLength),
          vobs,
          informationFile,
          chain.Chapters ?? [],
          title.AngleCount));
    }

    if (titles.Count == 0 || titles.All(x => x.VobFiles.Count == 0))
    {
      return Task.FromResult<DiscStructure?>(null);
    }

    context.Report(
      new AnalysisProgress(
        AnalysisPhase.DiscoveringStructure,
        Detail: $"Read {titles.Count} title(s) from the DVD navigation tables in '{discRoot}'."));

    return Task.FromResult<DiscStructure?>(
      new DvdStructure(discRoot, totalSize, titles, videoManagerPath));
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

  private static string? FindTitleSetFile(IEnumerable<string> files, int titleSet, string extension) =>
    files.FirstOrDefault(x =>
      string.Equals(Path.GetFileName(x), $"VTS_{titleSet:00}_0{extension}", StringComparison.OrdinalIgnoreCase));

  private static IReadOnlyList<string> FindTitleSetVobs(IEnumerable<string> files, int titleSet) =>
    files
      .Select(path => (Path: path, Name: Path.GetFileName(path)))
      .Where(x =>
        x.Name.StartsWith($"VTS_{titleSet:00}_", StringComparison.OrdinalIgnoreCase) &&
        x.Name.EndsWith(".VOB", StringComparison.OrdinalIgnoreCase) &&
        !x.Name.EndsWith("_0.VOB", StringComparison.OrdinalIgnoreCase))
      .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
      .Select(x => x.Path)
      .ToArray();
}
