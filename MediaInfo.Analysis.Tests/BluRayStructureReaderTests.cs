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

/// <summary>Tests for the Blu-ray structure reader that discovers titles from the folder layout.</summary>
public class BluRayStructureReaderTests
{
  private const string DiscRoot = @"D:\media\bd";
  private const string Bdmv = @"D:\media\bd\BDMV";

  private const long Gigabyte = 1024L * 1024L * 1024L;
  private const long Megabyte = 1024L * 1024L;

  private static FakeFileSystem CreateDisc() =>
    new FakeFileSystem()
      .AddFile(Path.Combine(Bdmv, "index.bdmv"), 2048)
      .AddFile(Path.Combine(Bdmv, "MovieObject.bdmv"), 4096)
      .AddFile(Path.Combine(Bdmv, "PLAYLIST", "00000.mpls"), 1024)
      .AddFile(Path.Combine(Bdmv, "PLAYLIST", "00001.mpls"), 512)
      .AddFile(Path.Combine(Bdmv, "CLIPINF", "00001.clpi"), 256)
      .AddFile(Path.Combine(Bdmv, "STREAM", "00001.m2ts"), 20 * Gigabyte)
      .AddFile(Path.Combine(Bdmv, "STREAM", "00002.m2ts"), 2 * Gigabyte)
      .AddFile(Path.Combine(Bdmv, "STREAM", "00003.m2ts"), 200 * 1024);

  [Fact]
  public void CanRead_BdmvFolder_ReturnsTheFolderItself()
  {
    var reader = new BluRayStructureReader(CreateDisc());

    reader.CanRead(Bdmv, out var discRoot).Should().BeTrue();
    discRoot.Should().Be(Bdmv);
  }

  [Fact]
  public void CanRead_DiscRootFolder_ReturnsTheNestedBdmvFolder()
  {
    var reader = new BluRayStructureReader(CreateDisc());

    reader.CanRead(DiscRoot, out var discRoot).Should().BeTrue();
    discRoot.Should().Be(Bdmv);
  }

  [Fact]
  public void CanRead_FolderWithoutABdmvStructure_ReturnsFalse()
  {
    var reader = new BluRayStructureReader(new FakeFileSystem().AddDirectory(@"D:\media\movies"));

    reader.CanRead(@"D:\media\movies", out var discRoot).Should().BeFalse();
    discRoot.Should().BeNull();
  }

  [Fact]
  public async Task ReadAsync_ListsTheClipsAndPlaylists()
  {
    var fileSystem = CreateDisc();
    var reader = new BluRayStructureReader(fileSystem);

    var structure = (BluRayStructure)(await reader.ReadAsync(
      Bdmv,
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem),
      CancellationToken.None))!;

    structure.Kind.Should().Be(DiscKind.BluRay);
    structure.Playlists.Should().HaveCount(2, "the 200 KB navigation clip is below the minimum title size");
    structure.Playlists.Select(x => x.Name).Should().ContainInOrder("00001", "00002");
    structure.PlaylistFiles.Select(Path.GetFileName).Should().ContainInOrder("00000.mpls", "00001.mpls");
  }

  [Fact]
  public async Task ReadAsync_SumsTheSizeOfTheWholeStructure()
  {
    var fileSystem = CreateDisc();
    var reader = new BluRayStructureReader(fileSystem);

    var structure = (await reader.ReadAsync(
      Bdmv,
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem),
      CancellationToken.None))!;

    structure.TotalSize.Should().Be(
      fileSystem.GetFiles(Bdmv, "*", SearchOption.AllDirectories).Sum(fileSystem.GetFileLength));
  }

  [Fact]
  public async Task ReadAsync_ProbesEachClipAndSelectsTheLongestAsTheMainTitle()
  {
    var fileSystem = CreateDisc();
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Path.Combine(Bdmv, "STREAM", "00001.m2ts")).WithDuration(TimeSpan.FromMinutes(128));
    factory.AddMedia(Path.Combine(Bdmv, "STREAM", "00002.m2ts")).WithDuration(TimeSpan.FromMinutes(14));

    var reader = new BluRayStructureReader(fileSystem);
    var structure = (BluRayStructure)(await reader.ReadAsync(
      Bdmv,
      TestContext.Create(factory, fileSystem),
      CancellationToken.None))!;

    structure.MainTitle!.Duration.Should().Be(TimeSpan.FromMinutes(128));
    Path.GetFileName(structure.MainTitle.PrimaryFile).Should().Be("00001.m2ts");
    structure.Playlists.Single(x => x.Name == "00001").Clips.Should().ContainSingle()
      .Which.Size.Should().Be(20 * Gigabyte);
  }

  [Fact]
  public async Task ReadAsync_ProbesTheLargestClipsFirstWhenTheBudgetIsLimited()
  {
    var fileSystem = CreateDisc();
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Path.Combine(Bdmv, "STREAM", "00001.m2ts")).WithDuration(TimeSpan.FromMinutes(128));
    factory.AddMedia(Path.Combine(Bdmv, "STREAM", "00002.m2ts")).WithDuration(TimeSpan.FromMinutes(14));

    var options = new MediaAnalysisOptions { OffloadBlockingCalls = false, MaxProbedDiscTitles = 1 };
    var reader = new BluRayStructureReader(fileSystem);

    var structure = (BluRayStructure)(await reader.ReadAsync(
      Bdmv,
      TestContext.Create(factory, fileSystem, options),
      CancellationToken.None))!;

    factory.OpenedPaths.Should().ContainSingle()
      .Which.Should().EndWith("00001.m2ts");
    structure.Playlists.Single(x => x.Name == "00001").Duration.Should().Be(TimeSpan.FromMinutes(128));
    structure.Playlists.Single(x => x.Name == "00002").Duration.Should().Be(TimeSpan.Zero);
  }

  [Fact]
  public async Task ReadAsync_OnAStructureWithoutClips_ReturnsNull()
  {
    var fileSystem = new FakeFileSystem()
      .AddFile(Path.Combine(Bdmv, "index.bdmv"), 2048)
      .AddDirectory(Path.Combine(Bdmv, "STREAM"));

    var reader = new BluRayStructureReader(fileSystem);
    var structure = await reader.ReadAsync(
      Bdmv,
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem),
      CancellationToken.None);

    structure.Should().BeNull();
  }

  [Fact]
  public async Task ReadAsync_ReportsWhatItFound()
  {
    var fileSystem = CreateDisc();
    var progress = new ProgressRecorder();
    var reader = new BluRayStructureReader(fileSystem);

    await reader.ReadAsync(
      Bdmv,
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem, progress: progress),
      CancellationToken.None);

    progress.Reports.Should().Contain(x =>
      x.Phase == AnalysisPhase.DiscoveringStructure && x.Detail!.Contains("2 Blu-ray clip"));
  }
}
