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
/// Describes the kind of an optical disc structure.
/// </summary>
public enum DiscKind
{
  /// <summary>
  /// The media is not an optical disc structure.
  /// </summary>
  None,

  /// <summary>
  /// A DVD video structure, rooted at a VIDEO_TS folder.
  /// </summary>
  Dvd,

  /// <summary>
  /// A Blu-ray structure, rooted at a BDMV folder.
  /// </summary>
  BluRay
}

/// <summary>
/// Describes a single playable title of an optical disc.
/// </summary>
public interface IDiscTitle
{
  /// <summary>
  /// Gets the ordinal of the title on the disc, starting at one.
  /// </summary>
  int Number { get; }

  /// <summary>
  /// Gets the duration of the title.
  /// </summary>
  TimeSpan Duration { get; }

  /// <summary>
  /// Gets the total size of the media files of the title, in bytes.
  /// </summary>
  long Size { get; }

  /// <summary>
  /// Gets the media file the analysis of the title should be performed against.
  /// </summary>
  string PrimaryFile { get; }

  /// <summary>
  /// Gets the media files that make up the title, in playback order.
  /// </summary>
  IReadOnlyList<string> Files { get; }
}

/// <summary>
/// Describes a chapter of a disc title.
/// </summary>
/// <param name="Number">The ordinal of the chapter within the title, starting at one.</param>
/// <param name="Start">The offset of the chapter from the start of the title.</param>
/// <param name="Duration">The duration of the chapter.</param>
/// <param name="Name">The name of the chapter, when the disc declares one.</param>
public sealed record DiscChapter(int Number, TimeSpan Start, TimeSpan Duration, string? Name = null);

/// <summary>
/// Describes the structure of an optical disc.
/// </summary>
public abstract record DiscStructure
{
  /// <summary>
  /// Initializes a new instance of the <see cref="DiscStructure"/> class.
  /// </summary>
  /// <param name="kind">The kind of the disc.</param>
  /// <param name="rootPath">The folder the disc structure is rooted at.</param>
  /// <param name="totalSize">The total size of the disc content in bytes.</param>
  protected DiscStructure(DiscKind kind, string rootPath, long totalSize)
  {
    Kind = kind;
    RootPath = rootPath;
    TotalSize = totalSize;
  }

  /// <summary>
  /// Gets the kind of the disc.
  /// </summary>
  public DiscKind Kind { get; }

  /// <summary>
  /// Gets the folder the disc structure is rooted at.
  /// </summary>
  public string RootPath { get; }

  /// <summary>
  /// Gets the total size of the disc content in bytes.
  /// </summary>
  public long TotalSize { get; }

  /// <summary>
  /// Gets the titles of the disc.
  /// </summary>
  public abstract IReadOnlyList<IDiscTitle> Titles { get; }

  /// <summary>
  /// Gets the title that is most likely to be the main feature.
  /// </summary>
  /// <remarks>
  /// The longest title is selected. When no duration could be determined for any title, which happens when the media
  /// files could not be probed, the largest title is selected instead.
  /// </remarks>
  public IDiscTitle? MainTitle =>
    Titles.Count == 0
      ? null
      : Titles.Any(x => x.Duration > TimeSpan.Zero)
        ? Titles.OrderByDescending(x => x.Duration).First()
        : Titles.OrderByDescending(x => x.Size).First();
}
