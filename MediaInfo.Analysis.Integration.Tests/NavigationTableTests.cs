#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Discs;
using MediaInfo.Analysis.Pipeline;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Strategies;
using MediaInfo.Analysis.Strategies.Selection;
using Xunit;
using Xunit.Abstractions;

namespace MediaInfo.Analysis.Integration.Tests;

/// <summary>
/// Runs the navigation table readers against a real disc.
/// </summary>
/// <remarks>
/// The parsers are covered without a disc by unit tests over tables the test builds itself. What only a real disc can
/// show is whether the tables of a disc pressed in the wild are laid out the way the specification says, and whether
/// what the parser reads out of them agrees with what the native library reads out of the media.
/// </remarks>
[Collection(NativeMediaCollection.Name)]
public class NavigationTableTests(ITestOutputHelper output)
{
  private static MediaAnalysisContext CreateContext() =>
    new(
      new MediaProbe(MediaInfoLibFactoryAccessor.Instance, DefaultStreamSelectionStrategy.Instance),
      MediaInfoFileSystem.Instance);

  [DvdFact]
  public async Task IfoReader_AgreesWithTheLibraryOnTheDurationOfTheMainTitle()
  {
    var path = TestMedia.FindDvd()!;

    var fromTables = (DvdStructure)(await new IfoDvdStructureReader(MediaInfoFileSystem.Instance)
      .ReadAsync(path, CreateContext(), CancellationToken.None))!;

    // What the library reports for the same disc, which it derives from the media rather than from the tables.
    var fromLibrary = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(path);

    foreach (var title in fromTables.DvdTitles)
    {
      output.WriteLine(
        $"VTS_{title.TitleSetNumber:00} title {title.Number}: {title.Duration}, " +
        $"{title.Chapters.Count} chapter(s), {title.AngleCount} angle(s), {title.VobFiles.Count} VOB(s)");
    }

    using var _ = new AssertionScope();

    fromTables.DvdTitles.Should().NotBeEmpty();
    fromTables.MainTitle!.Duration.Should().BeCloseTo(
      fromLibrary.General.Duration,
      TimeSpan.FromSeconds(1),
      "the navigation tables and the media describe the same title");
  }

  [DvdFact]
  public async Task IfoReader_ReadsChaptersThatTheFolderReaderCannotSee()
  {
    var path = TestMedia.FindDvd()!;
    var context = CreateContext();

    var fromTables = (DvdStructure)(await new IfoDvdStructureReader(MediaInfoFileSystem.Instance)
      .ReadAsync(path, context, CancellationToken.None))!;
    var fromFolder = (DvdStructure)(await new DvdStructureReader(MediaInfoFileSystem.Instance)
      .ReadAsync(path, context, CancellationToken.None))!;

    var main = (DvdTitle)fromTables.MainTitle!;
    output.WriteLine($"tables: {fromTables.DvdTitles.Count} title(s), main has {main.Chapters.Count} chapter(s)");
    output.WriteLine($"folder: {fromFolder.DvdTitles.Count} title set(s), main has {fromFolder.MainTitle!.Duration}");

    foreach (var chapter in main.Chapters.Take(5))
    {
      output.WriteLine($"  chapter {chapter.Number,3} starts {chapter.Start:hh\\:mm\\:ss} runs {chapter.Duration:hh\\:mm\\:ss}");
    }

    using var _ = new AssertionScope();

    main.Chapters.Should().NotBeEmpty("the program map and cell table carry the chapters");
    main.Chapters.Select(x => x.Start).Should().BeInAscendingOrder();
    main.Chapters.Should().OnlyContain(x => x.Duration > TimeSpan.Zero);

    // The chapters have to add up to roughly the title, allowing for cells the map does not enter.
    var summed = main.Chapters.Aggregate(TimeSpan.Zero, (total, x) => total + x.Duration);
    summed.Should().BeCloseTo(main.Duration, TimeSpan.FromSeconds(5));

    fromFolder.DvdTitles.Should().OnlyContain(x => x.Chapters.Count == 0, "the folder reader cannot see chapters");
  }

  [DvdFact]
  public async Task IfoReader_DescribesTheDiscWithoutOpeningAnyMedia()
  {
    var path = TestMedia.FindDvd()!;
    var clock = System.Diagnostics.Stopwatch.StartNew();

    var structure = await new IfoDvdStructureReader(MediaInfoFileSystem.Instance)
      .ReadAsync(path, CreateContext(), CancellationToken.None);

    clock.Stop();
    output.WriteLine($"Described an {structure!.TotalSize / (1024d * 1024 * 1024):N1} GiB disc in {clock.ElapsedMilliseconds} ms");

    structure.Titles.Should().NotBeEmpty();
    structure.Titles.Should().OnlyContain(x => x.Duration > TimeSpan.Zero);
  }

  [DvdFact]
  public async Task Analyzer_UsesTheTablesForARealDisc()
  {
    var path = TestMedia.FindDvd()!;

    var result = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(path);
    var disc = result.Disc.Should().BeOfType<DvdStructure>().Subject;

    result.Success.Should().BeTrue(result.Failure?.ToString() ?? string.Empty);
    disc.DvdTitles.Should().NotBeEmpty();
    disc.DvdTitles.Should().Contain(
      x => x.Chapters.Count > 0,
      "the default registration prefers the navigation tables over the folder layout");
  }
}

/// <summary>
/// Exposes the native factory, which is not otherwise needed outside the analyzer.
/// </summary>
internal static class MediaInfoLibFactoryAccessor
{
  public static Native.INativeMediaInfoFactory Instance { get; } = Native.MediaInfoLibFactory.Instance;
}
