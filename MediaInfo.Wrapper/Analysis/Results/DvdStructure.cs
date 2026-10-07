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
/// Describes a single title set of a DVD video structure.
/// </summary>
/// <remarks>
/// One title set corresponds to one VTS_nn group of files. Chapters and angles are only populated when the disc reader
/// in use parses the IFO tables; the reader that discovers titles from the folder layout leaves them empty.
/// </remarks>
public sealed record DvdTitle : IDiscTitle
{
  /// <summary>
  /// Initializes a new instance of the <see cref="DvdTitle"/> class.
  /// </summary>
  /// <param name="number">The ordinal of the title on the disc, starting at one.</param>
  /// <param name="titleSetNumber">The number of the video title set the title belongs to.</param>
  /// <param name="duration">The duration of the title.</param>
  /// <param name="size">The total size of the media files of the title, in bytes.</param>
  /// <param name="vobFiles">The VOB files that make up the title, in playback order.</param>
  /// <param name="informationFile">The IFO file of the title set, when the disc declares one.</param>
  /// <param name="chapters">The chapters of the title.</param>
  /// <param name="angleCount">The number of camera angles of the title.</param>
  public DvdTitle(
    int number,
    int titleSetNumber,
    TimeSpan duration,
    long size,
    IReadOnlyList<string> vobFiles,
    string? informationFile = null,
    IReadOnlyList<DiscChapter>? chapters = null,
    int angleCount = 1)
  {
    Number = number;
    TitleSetNumber = titleSetNumber;
    Duration = duration;
    Size = size;
    VobFiles = vobFiles;
    InformationFile = informationFile;
    Chapters = chapters ?? [];
    AngleCount = angleCount;
  }

  /// <inheritdoc />
  public int Number { get; }

  /// <summary>
  /// Gets the number of the video title set the title belongs to.
  /// </summary>
  public int TitleSetNumber { get; }

  /// <inheritdoc />
  public TimeSpan Duration { get; }

  /// <inheritdoc />
  public long Size { get; }

  /// <summary>
  /// Gets the VOB files that make up the title, in playback order.
  /// </summary>
  public IReadOnlyList<string> VobFiles { get; }

  /// <summary>
  /// Gets the IFO file of the title set, when the disc declares one.
  /// </summary>
  public string? InformationFile { get; }

  /// <summary>
  /// Gets the chapters of the title.
  /// </summary>
  public IReadOnlyList<DiscChapter> Chapters { get; }

  /// <summary>
  /// Gets the number of camera angles of the title.
  /// </summary>
  public int AngleCount { get; }

  /// <inheritdoc />
  public string PrimaryFile => VobFiles.Count > 0 ? VobFiles[0] : InformationFile ?? string.Empty;

  /// <inheritdoc />
  public IReadOnlyList<string> Files => VobFiles;
}

/// <summary>
/// Describes the structure of a DVD video disc.
/// </summary>
public sealed record DvdStructure : DiscStructure
{
  /// <summary>
  /// Initializes a new instance of the <see cref="DvdStructure"/> class.
  /// </summary>
  /// <param name="rootPath">The VIDEO_TS folder of the disc.</param>
  /// <param name="totalSize">The total size of the disc content in bytes.</param>
  /// <param name="titles">The titles of the disc.</param>
  /// <param name="videoManagerFile">The VIDEO_TS.IFO file of the disc, when it is present.</param>
  public DvdStructure(
    string rootPath,
    long totalSize,
    IReadOnlyList<DvdTitle> titles,
    string? videoManagerFile = null)
    : base(DiscKind.Dvd, rootPath, totalSize)
  {
    DvdTitles = titles;
    VideoManagerFile = videoManagerFile;
  }

  /// <summary>
  /// Gets the titles of the disc.
  /// </summary>
  public IReadOnlyList<DvdTitle> DvdTitles { get; }

  /// <summary>
  /// Gets the VIDEO_TS.IFO file of the disc, when it is present.
  /// </summary>
  public string? VideoManagerFile { get; }

  /// <inheritdoc />
  public override IReadOnlyList<IDiscTitle> Titles => DvdTitles.Cast<IDiscTitle>().ToArray();
}
