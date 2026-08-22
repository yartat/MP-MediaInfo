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
using MediaInfo.Analysis.Discs;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Tests.Fakes;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for the DVD structure reader that discovers titles from the folder layout.</summary>
public class DvdStructureReaderTests
{
  private const string DiscRoot = @"D:\media\disc";
  private const string VideoTs = @"D:\media\disc\VIDEO_TS";

  private const long Gigabyte = 1024L * 1024L * 1024L;
  private const long Megabyte = 1024L * 1024L;

  private static FakeFileSystem CreateDisc() =>
    new FakeFileSystem()
      .AddFile(Path.Combine(VideoTs, "VIDEO_TS.IFO"), 12 * 1024)
      .AddFile(Path.Combine(VideoTs, "VIDEO_TS.BUP"), 12 * 1024)
      .AddFile(Path.Combine(VideoTs, "VIDEO_TS.VOB"), 200 * 1024)
      .AddFile(Path.Combine(VideoTs, "VTS_01_0.IFO"), 60 * 1024)
      .AddFile(Path.Combine(VideoTs, "VTS_01_0.BUP"), 60 * 1024)
      .AddFile(Path.Combine(VideoTs, "VTS_01_0.VOB"), 20 * Megabyte)
      .AddFile(Path.Combine(VideoTs, "VTS_01_1.VOB"), Gigabyte)
      .AddFile(Path.Combine(VideoTs, "VTS_01_2.VOB"), Gigabyte / 2)
      .AddFile(Path.Combine(VideoTs, "VTS_02_0.IFO"), 30 * 1024)
      .AddFile(Path.Combine(VideoTs, "VTS_02_1.VOB"), 300 * Megabyte);

  [Fact]
  public void CanRead_VideoTsFolder_ReturnsTheFolderItself()
  {
    var reader = new DvdStructureReader(CreateDisc());

    reader.CanRead(VideoTs, out var discRoot).Should().BeTrue();
    discRoot.Should().Be(VideoTs);
  }

  [Fact]
  public void CanRead_DiscRootFolder_ReturnsTheNestedVideoTsFolder()
  {
    var reader = new DvdStructureReader(CreateDisc());

    reader.CanRead(DiscRoot, out var discRoot).Should().BeTrue();
    discRoot.Should().Be(VideoTs);
  }

  [Theory]
  [InlineData(@"D:\media\empty")]
  [InlineData(@"D:\media\missing")]
  public void CanRead_FolderWithoutTitleSets_ReturnsFalse(string path)
  {
    var fileSystem = CreateDisc().AddDirectory(@"D:\media\empty");
    var reader = new DvdStructureReader(fileSystem);

    reader.CanRead(path, out var discRoot).Should().BeFalse();
    discRoot.Should().BeNull();
  }

  [Fact]
  public async Task ReadAsync_GroupsVobsByTitleSetAndExcludesMenuVobs()
  {
    var reader = new DvdStructureReader(CreateDisc());
    var context = TestContext.Create(new FakeNativeMediaInfoFactory(), CreateDisc());

    var structure = (DvdStructure)(await reader.ReadAsync(VideoTs, context, CancellationToken.None))!;

    structure.Should().NotBeNull();
    structure.Kind.Should().Be(DiscKind.Dvd);
    structure.DvdTitles.Should().HaveCount(2);

    var feature = structure.DvdTitles.Single(x => x.TitleSetNumber == 1);
    feature.VobFiles.Should().HaveCount(2, "the menu VOB numbered zero is not part of the feature");
    feature.VobFiles.Select(Path.GetFileName).Should().ContainInOrder("VTS_01_1.VOB", "VTS_01_2.VOB");
    feature.Size.Should().Be(Gigabyte + (Gigabyte / 2));
    Path.GetFileName(feature.InformationFile!).Should().Be("VTS_01_0.IFO");
    Path.GetFileName(feature.PrimaryFile).Should().Be("VTS_01_1.VOB");
  }

  [Fact]
  public async Task ReadAsync_FindsTheVideoManagerAndTotalSize()
  {
    var fileSystem = CreateDisc();
    var reader = new DvdStructureReader(fileSystem);
    var context = TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem);

    var structure = (DvdStructure)(await reader.ReadAsync(VideoTs, context, CancellationToken.None))!;

    Path.GetFileName(structure.VideoManagerFile!).Should().Be("VIDEO_TS.IFO");
    structure.TotalSize.Should().Be(
      fileSystem.GetFiles(VideoTs, "*", SearchOption.TopDirectoryOnly).Sum(fileSystem.GetFileLength));
  }

  [Fact]
  public async Task ReadAsync_ProbesTheInformationFileOfEachTitleSet()
  {
    var fileSystem = CreateDisc();
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Path.Combine(VideoTs, "VTS_01_0.IFO")).WithDuration(TimeSpan.FromMinutes(94));
    factory.AddMedia(Path.Combine(VideoTs, "VTS_02_0.IFO")).WithDuration(TimeSpan.FromMinutes(6));

    var reader = new DvdStructureReader(fileSystem);
    var structure = (DvdStructure)(await reader.ReadAsync(
      VideoTs,
      TestContext.Create(factory, fileSystem),
      CancellationToken.None))!;

    structure.DvdTitles.Single(x => x.TitleSetNumber == 1).Duration.Should().Be(TimeSpan.FromMinutes(94));
    structure.DvdTitles.Single(x => x.TitleSetNumber == 2).Duration.Should().Be(TimeSpan.FromMinutes(6));
    structure.MainTitle!.Duration.Should().Be(TimeSpan.FromMinutes(94));
  }

  [Fact]
  public async Task ReadAsync_WhenTheInformationFileHasNoDuration_FallsBackToTheFirstVob()
  {
    var fileSystem = CreateDisc();
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Path.Combine(VideoTs, "VTS_01_0.IFO"));
    factory.AddMedia(Path.Combine(VideoTs, "VTS_01_1.VOB")).WithDuration(TimeSpan.FromMinutes(31));

    var reader = new DvdStructureReader(fileSystem);
    var structure = (DvdStructure)(await reader.ReadAsync(
      VideoTs,
      TestContext.Create(factory, fileSystem),
      CancellationToken.None))!;

    structure.DvdTitles.Single(x => x.TitleSetNumber == 1).Duration.Should().Be(TimeSpan.FromMinutes(31));
    factory.OpenedPaths.Should().Contain(x => x.EndsWith("VTS_01_1.VOB", StringComparison.OrdinalIgnoreCase));
  }

  [Fact]
  public async Task ReadAsync_WhenNoDurationIsKnown_SelectsTheLargestTitleAsTheMainOne()
  {
    var fileSystem = CreateDisc();
    var reader = new DvdStructureReader(fileSystem);

    var structure = (await reader.ReadAsync(
      VideoTs,
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem),
      CancellationToken.None))!;

    structure.Titles.Should().OnlyContain(x => x.Duration == TimeSpan.Zero);
    ((DvdTitle)structure.MainTitle!).TitleSetNumber.Should().Be(1);
  }

  [Fact]
  public async Task ReadAsync_WithoutAProbeBudget_DoesNotOpenAnyMedia()
  {
    var fileSystem = CreateDisc();
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Path.Combine(VideoTs, "VTS_01_0.IFO")).WithDuration(TimeSpan.FromMinutes(94));

    var options = new MediaAnalysisOptions { OffloadBlockingCalls = false, MaxProbedDiscTitles = 0 };
    var reader = new DvdStructureReader(fileSystem);

    var structure = (await reader.ReadAsync(
      VideoTs,
      TestContext.Create(factory, fileSystem, options),
      CancellationToken.None))!;

    factory.OpenedPaths.Should().BeEmpty();
    structure.Titles.Should().HaveCount(2);
  }

  [Fact]
  public async Task ReadAsync_IgnoresTitleSetsBelowTheMinimumSize()
  {
    var fileSystem = CreateDisc();
    var options = new MediaAnalysisOptions { OffloadBlockingCalls = false, MinimumDiscTitleSize = Gigabyte };
    var reader = new DvdStructureReader(fileSystem);

    var structure = (DvdStructure)(await reader.ReadAsync(
      VideoTs,
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem, options),
      CancellationToken.None))!;

    structure.DvdTitles.Should().ContainSingle().Which.TitleSetNumber.Should().Be(1);
  }

  [Fact]
  public async Task ReadAsync_WhenEveryTitleSetIsTooSmall_ReturnsNull()
  {
    var fileSystem = CreateDisc();
    var options = new MediaAnalysisOptions { OffloadBlockingCalls = false, MinimumDiscTitleSize = 100 * Gigabyte };
    var reader = new DvdStructureReader(fileSystem);

    var structure = await reader.ReadAsync(
      VideoTs,
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem, options),
      CancellationToken.None);

    structure.Should().BeNull();
  }

  [Fact]
  public async Task ReadAsync_OnAFolderWithOnlyInformationFiles_ReturnsNull()
  {
    var fileSystem = new FakeFileSystem()
      .AddFile(Path.Combine(VideoTs, "VIDEO_TS.IFO"), 12 * 1024)
      .AddFile(Path.Combine(VideoTs, "VTS_01_0.IFO"), 60 * 1024);

    var reader = new DvdStructureReader(fileSystem);
    var structure = await reader.ReadAsync(
      VideoTs,
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem),
      CancellationToken.None);

    structure.Should().BeNull("a title set without content VOBs holds nothing playable");
  }
}
