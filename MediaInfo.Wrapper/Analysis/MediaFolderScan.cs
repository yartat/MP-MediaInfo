#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;

namespace MediaInfo.Analysis;

/// <summary>
/// Describes how a folder of media is walked.
/// </summary>
public sealed class MediaFolderScanOptions
{
  /// <summary>
  /// Gets the default options.
  /// </summary>
  public static MediaFolderScanOptions Default { get; } = new();

  /// <summary>
  /// Gets or sets a value indicating whether subfolders are walked as well.
  /// </summary>
  /// <value>The default value is <see langword="true"/>.</value>
  public bool Recursive { get; set; } = true;

  /// <summary>
  /// Gets or sets the pattern the file names are matched against.
  /// </summary>
  /// <value>The default value is <c>*</c>.</value>
  public string SearchPattern { get; set; } = "*";

  /// <summary>
  /// Gets or sets a value indicating whether files that do not carry a known audio or video extension are skipped.
  /// </summary>
  /// <remarks>
  /// Skipping them avoids handing artwork, subtitles and text files to the native library only to be told that they
  /// hold no streams.
  /// </remarks>
  /// <value>The default value is <see langword="true"/>.</value>
  public bool MediaFilesOnly { get; set; } = true;

  /// <summary>
  /// Gets or sets a value indicating whether a subfolder holding a disc structure is analyzed as one disc.
  /// </summary>
  /// <remarks>
  /// When this is set, a folder that holds VIDEO_TS or BDMV produces a single result describing the disc, and the
  /// files inside it are not reported individually.
  /// </remarks>
  /// <value>The default value is <see langword="true"/>.</value>
  public bool TreatDiscFoldersAsDiscs { get; set; } = true;
}

/// <summary>
/// Walks a folder and analyzes the media it holds.
/// </summary>
public static class MediaFolderScanExtensions
{
  private const string VideoTsFolderName = "VIDEO_TS";
  private const string BdmvFolderName = "BDMV";

  /// <summary>
  /// Analyzes every media in the specified folder, one after another.
  /// </summary>
  /// <remarks>
  /// The results are produced as they are ready, so a caller can start reporting before the walk has finished, and
  /// the walk stops as soon as the enumeration is abandoned or the token is cancelled. Failures are yielded like any
  /// other outcome; one unreadable file does not end the scan.
  /// </remarks>
  /// <param name="analyzer">The analyzer.</param>
  /// <param name="folder">The folder to walk.</param>
  /// <param name="options">How the folder is walked, or <see langword="null"/> for the defaults.</param>
  /// <param name="fileSystem">The file system to walk, or <see langword="null"/> for the local one.</param>
  /// <param name="cancellationToken">The token that stops the scan.</param>
  /// <returns>Returns the outcome for each media found.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="analyzer"/> is <see langword="null"/>.</exception>
  public static async IAsyncEnumerable<MediaAnalysisResult> AnalyzeFolderAsync(
    this IMediaInfoAnalyzer analyzer,
    string folder,
    MediaFolderScanOptions? options = null,
    IFileSystem? fileSystem = null,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    if (analyzer is null)
    {
      throw new ArgumentNullException(nameof(analyzer));
    }

    var scan = options ?? MediaFolderScanOptions.Default;
    var files = fileSystem ?? MediaInfoFileSystem.Instance;

    if (!files.DirectoryExists(folder))
    {
      yield return MediaAnalysisResult.Failed(
        new DirectoryMediaSource(folder),
        AnalysisFailureReason.SourceNotFound,
        $"The folder '{folder}' does not exist.");
      yield break;
    }

    foreach (var source in Enumerate(files, folder, scan, 0))
    {
      cancellationToken.ThrowIfCancellationRequested();
      yield return await analyzer.AnalyzeAsync(source, cancellationToken).ConfigureAwait(false);
    }
  }

  // Folders nest, and a link can make them nest forever, so the walk gives up below this depth.
  private const int MaximumDepth = 64;

  private static IEnumerable<IMediaSource> Enumerate(
    IFileSystem files,
    string folder,
    MediaFolderScanOptions scan,
    int depth)
  {
    // A folder that is itself a disc describes one media, not a pile of VOB files.
    if (scan.TreatDiscFoldersAsDiscs && IsDiscFolder(files, folder))
    {
      yield return new DirectoryMediaSource(folder);
      yield break;
    }

    var children = files.GetDirectories(folder).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray();
    var discs = scan.TreatDiscFoldersAsDiscs
      ? children.Where(x => IsDiscFolder(files, x)).ToArray()
      : [];

    foreach (var disc in discs)
    {
      yield return new DirectoryMediaSource(disc);
    }

    var found = files.GetFiles(folder, scan.SearchPattern, SearchOption.TopDirectoryOnly);
    foreach (var file in found
               .Where(file => !scan.MediaFilesOnly || file.IsVideo() || file.IsAudio())
               .OrderBy(file => file, StringComparer.OrdinalIgnoreCase))
    {
      yield return new FileMediaSource(file);
    }

    if (!scan.Recursive || depth >= MaximumDepth)
    {
      yield break;
    }

    // A disc is not walked into: its files are the disc, not media of their own, and each disc is one result.
    foreach (var child in children.Except(discs, StringComparer.OrdinalIgnoreCase))
    {
      foreach (var source in Enumerate(files, child, scan, depth + 1))
      {
        yield return source;
      }
    }
  }

  private static bool IsDiscFolder(IFileSystem files, string folder) =>
    files.GetDirectories(folder).Any(child =>
      string.Equals(NameOf(child), VideoTsFolderName, StringComparison.OrdinalIgnoreCase) ||
      string.Equals(NameOf(child), BdmvFolderName, StringComparison.OrdinalIgnoreCase)) ||
    string.Equals(NameOf(folder), VideoTsFolderName, StringComparison.OrdinalIgnoreCase) ||
    string.Equals(NameOf(folder), BdmvFolderName, StringComparison.OrdinalIgnoreCase);

  private static string NameOf(string path) => Path.GetFileName(path.TrimEnd('/', '\\'));
}
