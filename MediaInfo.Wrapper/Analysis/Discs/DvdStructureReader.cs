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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Strategies;

namespace MediaInfo.Analysis.Discs;

/// <summary>
/// Discovers the titles of a DVD video disc from the layout of its VIDEO_TS folder.
/// </summary>
/// <remarks>
/// A DVD groups its content into video title sets. Every set is named VTS_nn and consists of one IFO file that holds
/// the navigation tables, one optional menu VOB numbered zero, and the content VOBs numbered one and above. This
/// reader groups the files by set and probes the media to rank the sets, which is enough to identify the feature and
/// describe what is on the disc. It does not parse the navigation tables, so it reports no chapters and assumes a
/// single camera angle.
/// </remarks>
public sealed class DvdStructureReader : IDiscStructureReader
{
  private const string VideoTsFolderName = "VIDEO_TS";
  private const string VideoManagerFileName = "VIDEO_TS.IFO";

  private static readonly Regex TitleSetVob = new(
    @"^VTS_(?<set>\d{2})_(?<part>\d)\.VOB$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

  private static readonly Regex TitleSetIfo = new(
    @"^VTS_(?<set>\d{2})_0\.IFO$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

  private readonly IFileSystem _fileSystem;

  /// <summary>
  /// Initializes a new instance of the <see cref="DvdStructureReader"/> class.
  /// </summary>
  /// <param name="fileSystem">The file system the disc is read from.</param>
  /// <exception cref="ArgumentNullException"><paramref name="fileSystem"/> is <see langword="null"/>.</exception>
  public DvdStructureReader(IFileSystem fileSystem)
  {
    _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
  }

  /// <inheritdoc />
  public DiscKind Kind => DiscKind.Dvd;

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

    // The folder is the VIDEO_TS folder itself.
    if (HasTitleSets(path))
    {
      discRoot = path;
      return true;
    }

    // The folder is the root of the disc and holds a VIDEO_TS folder.
    var nested = _fileSystem
      .GetDirectories(path)
      .FirstOrDefault(x => string.Equals(GetName(x), VideoTsFolderName, StringComparison.OrdinalIgnoreCase));

    if (nested is not null && HasTitleSets(nested))
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
    var files = _fileSystem.GetFiles(discRoot, "*", SearchOption.TopDirectoryOnly);
    var totalSize = files.Sum(_fileSystem.GetFileLength);

    var titleSets = GroupTitleSets(files);
    if (titleSets.Count == 0)
    {
      return null;
    }

    var informationFiles = files
      .Select(path => (Path: path, Match: TitleSetIfo.Match(GetName(path))))
      .Where(x => x.Match.Success)
      .ToDictionary(
        x => int.Parse(x.Match.Groups["set"].Value),
        x => x.Path);

    var videoManager = files.FirstOrDefault(
      x => string.Equals(GetName(x), VideoManagerFileName, StringComparison.OrdinalIgnoreCase));

    context.Report(
      new AnalysisProgress(
        AnalysisPhase.DiscoveringStructure,
        Detail: $"Found {titleSets.Count} DVD title set(s) in '{discRoot}'."));

    // The largest sets are probed first, so that a probe budget is spent on the sets most likely to be the feature.
    var ordered = titleSets
      .Select(set => (
        Set: set.Key,
        Vobs: set.Value,
        Size: set.Value.Sum(_fileSystem.GetFileLength)))
      .Where(x => x.Size >= context.Options.MinimumDiscTitleSize)
      .OrderByDescending(x => x.Size)
      .ToList();

    if (ordered.Count == 0)
    {
      return null;
    }

    var probesLeft = context.Options.MaxProbedDiscTitles;
    var titles = new List<DvdTitle>(ordered.Count);

    foreach (var candidate in ordered)
    {
      cancellationToken.ThrowIfCancellationRequested();

      informationFiles.TryGetValue(candidate.Set, out var informationFile);
      var duration = TimeSpan.Zero;

      if (probesLeft > 0)
      {
        // The information file is the navigation table of the set and is a fraction of the size of a VOB,
        // so it is the cheaper thing to probe. A VOB is the fallback: the library recognises a VTS_nn_*
        // sequence and reports the joined title, so either answers for the whole title.
        if (informationFile is not null)
        {
          --probesLeft;
          duration = await context.Probe
            .ProbeDurationAsync(informationFile, context.Options, cancellationToken)
            .ConfigureAwait(false);
        }

        if (duration <= TimeSpan.Zero && probesLeft > 0 && candidate.Vobs.Count > 0)
        {
          --probesLeft;
          duration = await context.Probe
            .ProbeDurationAsync(candidate.Vobs[0], context.Options, cancellationToken)
            .ConfigureAwait(false);
        }
      }

      titles.Add(
        new DvdTitle(
          titles.Count + 1,
          candidate.Set,
          duration,
          candidate.Size,
          candidate.Vobs,
          informationFile));
    }

    return new DvdStructure(discRoot, totalSize, titles, videoManager);
  }

  private bool HasTitleSets(string path) =>
    _fileSystem
      .GetFiles(path, "*", SearchOption.TopDirectoryOnly)
      .Any(x => TitleSetVob.IsMatch(GetName(x)) || TitleSetIfo.IsMatch(GetName(x)));

  private static Dictionary<int, List<string>> GroupTitleSets(IEnumerable<string> files)
  {
    var groups = new Dictionary<int, List<string>>();
    var parts = files
      .Select(path => (Path: path, Match: TitleSetVob.Match(GetName(path))))
      .Where(x => x.Match.Success)
      .Select(x => (
        x.Path,
        Set: int.Parse(x.Match.Groups["set"].Value),
        Part: int.Parse(x.Match.Groups["part"].Value)))
      // Part zero holds the menus of the set rather than the feature.
      .Where(x => x.Part > 0)
      .OrderBy(x => x.Set)
      .ThenBy(x => x.Part);

    foreach (var part in parts)
    {
      if (!groups.TryGetValue(part.Set, out var vobs))
      {
        vobs = [];
        groups[part.Set] = vobs;
      }

      vobs.Add(part.Path);
    }

    return groups;
  }

  private static string GetName(string path) => Path.GetFileName(path.TrimEnd('/', '\\'));
}
