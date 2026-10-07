#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Discs;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;

namespace MediaInfo.Analysis.Strategies;

/// <summary>
/// Analyzes an optical disc by discovering its structure and then reading the media of its main title.
/// </summary>
/// <remarks>
/// The result carries the whole structure of the disc, so that a caller can present every title, while the stream
/// information describes the title that is most likely to be the feature.
/// </remarks>
public abstract class DiscAnalysisStrategy : IMediaAnalysisStrategy
{
  private readonly IDiscStructureReader _reader;

  /// <summary>
  /// Initializes a new instance of the <see cref="DiscAnalysisStrategy"/> class.
  /// </summary>
  /// <param name="reader">The reader that discovers the structure of the disc.</param>
  /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <see langword="null"/>.</exception>
  protected DiscAnalysisStrategy(IDiscStructureReader reader)
  {
    _reader = reader ?? throw new ArgumentNullException(nameof(reader));
  }

  /// <inheritdoc />
  public abstract int Priority { get; }

  /// <summary>
  /// Gets the reader that discovers the structure of the disc.
  /// </summary>
  protected IDiscStructureReader Reader => _reader;

  /// <inheritdoc />
  public bool CanHandle(IMediaSource source) =>
    source is DirectoryMediaSource directory && _reader.CanRead(directory.Path, out _);

  /// <inheritdoc />
  public async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    MediaAnalysisContext context,
    CancellationToken cancellationToken)
  {
    var directory = (DirectoryMediaSource)source;
    if (!_reader.CanRead(directory.Path, out var discRoot))
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.SourceNotFound,
        $"The folder '{directory.Path}' does not hold a {_reader.Kind} structure.");
    }

    context.Report(new AnalysisProgress(AnalysisPhase.DiscoveringStructure, Detail: discRoot));
    var structure = await _reader.ReadAsync(discRoot, context, cancellationToken).ConfigureAwait(false);

    if (structure is null)
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.DiscStructureUnreadable,
        $"The {_reader.Kind} structure at '{discRoot}' holds no playable title.");
    }

    var mainTitle = structure.MainTitle;
    if (mainTitle is null || string.IsNullOrEmpty(mainTitle.PrimaryFile))
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.DiscStructureUnreadable,
        $"The {_reader.Kind} structure at '{discRoot}' holds no title with a readable media file.")
        with
      { Disc = structure };
    }

    context.Report(
      new AnalysisProgress(
        AnalysisPhase.Opening,
        TotalBytes: structure.TotalSize,
        Detail: mainTitle.PrimaryFile));

    var result = await context.Probe
      .OpenAndCollectAsync(mainTitle.PrimaryFile, context.Options, mainTitle.Size, cancellationToken)
      .ConfigureAwait(false);

    context.Report(new AnalysisProgress(AnalysisPhase.Completed, Detail: discRoot));

    return result with
    {
      SourceKind = MediaSourceKind.Directory,
      SourcePath = directory.Path,
      AnalyzedPath = mainTitle.PrimaryFile,
      Disc = structure,
      General = result.General with
      {
        Size = structure.TotalSize,
        // The duration discovered for the title wins, because a disc reader that parses the navigation
        // tables knows the length of a title that the media file it points at cannot describe on its own.
        Duration = mainTitle.Duration > TimeSpan.Zero ? mainTitle.Duration : result.General.Duration
      }
    };
  }
}

/// <summary>
/// Analyzes a DVD video disc.
/// </summary>
public sealed class DvdAnalysisStrategy : DiscAnalysisStrategy
{
  /// <summary>
  /// Initializes a new instance of the <see cref="DvdAnalysisStrategy"/> class.
  /// </summary>
  /// <param name="reader">The reader that discovers the structure of the disc.</param>
  public DvdAnalysisStrategy(IDiscStructureReader reader)
    : base(reader)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="DvdAnalysisStrategy"/> class that reads from the specified file system.
  /// </summary>
  /// <param name="fileSystem">The file system the disc is read from.</param>
  public DvdAnalysisStrategy(IFileSystem fileSystem)
    : this(new DvdStructureReader(fileSystem))
  {
  }

  /// <inheritdoc />
  public override int Priority => 10;
}

/// <summary>
/// Analyzes a Blu-ray disc.
/// </summary>
public sealed class BluRayAnalysisStrategy : DiscAnalysisStrategy
{
  /// <summary>
  /// Initializes a new instance of the <see cref="BluRayAnalysisStrategy"/> class.
  /// </summary>
  /// <param name="reader">The reader that discovers the structure of the disc.</param>
  public BluRayAnalysisStrategy(IDiscStructureReader reader)
    : base(reader)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="BluRayAnalysisStrategy"/> class that reads from the specified file system.
  /// </summary>
  /// <param name="fileSystem">The file system the disc is read from.</param>
  public BluRayAnalysisStrategy(IFileSystem fileSystem)
    : this(new BluRayStructureReader(fileSystem))
  {
  }

  /// <inheritdoc />
  public override int Priority => 11;
}
