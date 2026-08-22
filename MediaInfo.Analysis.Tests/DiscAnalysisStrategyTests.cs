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
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Strategies;
using MediaInfo.Analysis.Tests.Fakes;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for the strategies that analyze an optical disc as a whole.</summary>
public class DiscAnalysisStrategyTests
{
  private const string VideoTs = @"D:\media\disc\VIDEO_TS";
  private const string Bdmv = @"D:\media\bd\BDMV";

  private const long Gigabyte = 1024L * 1024L * 1024L;
  private const long Megabyte = 1024L * 1024L;

  private static FakeFileSystem CreateDvd() =>
    new FakeFileSystem()
      .AddFile(Path.Combine(VideoTs, "VIDEO_TS.IFO"), 12 * 1024)
      .AddFile(Path.Combine(VideoTs, "VTS_01_0.IFO"), 60 * 1024)
      .AddFile(Path.Combine(VideoTs, "VTS_01_1.VOB"), Gigabyte)
      .AddFile(Path.Combine(VideoTs, "VTS_02_0.IFO"), 30 * 1024)
      .AddFile(Path.Combine(VideoTs, "VTS_02_1.VOB"), 300 * Megabyte);

  private static FakeFileSystem CreateBluRay() =>
    new FakeFileSystem()
      .AddFile(Path.Combine(Bdmv, "index.bdmv"), 2048)
      .AddFile(Path.Combine(Bdmv, "PLAYLIST", "00000.mpls"), 1024)
      .AddFile(Path.Combine(Bdmv, "STREAM", "00001.m2ts"), 20 * Gigabyte)
      .AddFile(Path.Combine(Bdmv, "STREAM", "00002.m2ts"), 2 * Gigabyte);

  [Fact]
  public void CanHandle_OnlyAcceptsADirectoryHoldingAMatchingStructure()
  {
    var dvd = new DvdAnalysisStrategy(CreateDvd());
    var bluRay = new BluRayAnalysisStrategy(CreateBluRay());

    dvd.CanHandle(new DirectoryMediaSource(VideoTs)).Should().BeTrue();
    dvd.CanHandle(new DirectoryMediaSource(Bdmv)).Should().BeFalse();
    dvd.CanHandle(new FileMediaSource(@"D:\media\movie.mkv")).Should().BeFalse();

    bluRay.CanHandle(new DirectoryMediaSource(Bdmv)).Should().BeTrue();
    bluRay.CanHandle(new DirectoryMediaSource(VideoTs)).Should().BeFalse();
  }

  [Fact]
  public void Priority_PrefersDvdOverBluRayAndBothOverPlainFiles()
  {
    new DvdAnalysisStrategy(CreateDvd()).Priority.Should().BeLessThan(
      new BluRayAnalysisStrategy(CreateBluRay()).Priority);

    new BluRayAnalysisStrategy(CreateBluRay()).Priority.Should().BeLessThan(
      new SingleFileAnalysisStrategy().Priority);
  }

  [Fact]
  public async Task AnalyzeAsync_Dvd_ReadsTheMainTitleAndAttachesTheStructure()
  {
    var fileSystem = CreateDvd();
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Path.Combine(VideoTs, "VTS_01_0.IFO")).WithDuration(TimeSpan.FromMinutes(94));
    factory.AddMedia(Path.Combine(VideoTs, "VTS_02_0.IFO")).WithDuration(TimeSpan.FromMinutes(6));
    factory
      .AddMedia(Path.Combine(VideoTs, "VTS_01_1.VOB"))
      .WithStreams(StreamKind.Video, 1)
      .WithStreams(StreamKind.Audio, 2)
      .WithDuration(TimeSpan.FromMinutes(30))
      .Set(StreamKind.General, 0, "Format", "MPEG-PS");

    var strategy = new DvdAnalysisStrategy(fileSystem);
    var result = await strategy.AnalyzeAsync(
      new DirectoryMediaSource(VideoTs),
      TestContext.Create(factory, fileSystem),
      CancellationToken.None);

    result.Success.Should().BeTrue();
    result.IsDvd.Should().BeTrue();
    result.IsBluRay.Should().BeFalse();
    result.SourceKind.Should().Be(MediaSourceKind.Directory);
    result.SourcePath.Should().Be(VideoTs);
    Path.GetFileName(result.AnalyzedPath!).Should().Be("VTS_01_1.VOB");
    result.VideoStreams.Should().HaveCount(1);
    result.AudioStreams.Should().HaveCount(2);
    result.General.Format.Should().Be("MPEG-PS");

    var disc = result.Disc.Should().BeOfType<DvdStructure>().Subject;
    disc.DvdTitles.Should().HaveCount(2);
    disc.MainTitle!.Duration.Should().Be(TimeSpan.FromMinutes(94));
  }

  [Fact]
  public async Task AnalyzeAsync_Dvd_ReportsTheSizeOfTheDiscAndTheDurationOfTheTitle()
  {
    var fileSystem = CreateDvd();
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Path.Combine(VideoTs, "VTS_01_0.IFO")).WithDuration(TimeSpan.FromMinutes(94));
    factory
      .AddMedia(Path.Combine(VideoTs, "VTS_01_1.VOB"))
      .WithStreams(StreamKind.Video, 1)
      .WithDuration(TimeSpan.FromMinutes(30));

    var strategy = new DvdAnalysisStrategy(fileSystem);
    var result = await strategy.AnalyzeAsync(
      new DirectoryMediaSource(VideoTs),
      TestContext.Create(factory, fileSystem),
      CancellationToken.None);

    result.General.Size.Should().Be(
      fileSystem.GetFiles(VideoTs, "*", SearchOption.TopDirectoryOnly).Sum(fileSystem.GetFileLength),
      "the reported size is that of the whole disc, not of the single VOB that was opened");

    result.General.Duration.Should().Be(
      TimeSpan.FromMinutes(94),
      "the title spans more than the single VOB that was opened");
  }

  [Fact]
  public async Task AnalyzeAsync_BluRay_ReadsTheLongestClipAndAttachesTheStructure()
  {
    var fileSystem = CreateBluRay();
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Path.Combine(Bdmv, "STREAM", "00001.m2ts"))
      .WithStreams(StreamKind.Video, 1)
      .WithStreams(StreamKind.Audio, 1)
      .WithStreams(StreamKind.Text, 3)
      .WithDuration(TimeSpan.FromMinutes(128))
      .Set(StreamKind.General, 0, "Format", "BDAV");
    factory.AddMedia(Path.Combine(Bdmv, "STREAM", "00002.m2ts")).WithDuration(TimeSpan.FromMinutes(14));

    var strategy = new BluRayAnalysisStrategy(fileSystem);
    var result = await strategy.AnalyzeAsync(
      new DirectoryMediaSource(Bdmv),
      TestContext.Create(factory, fileSystem),
      CancellationToken.None);

    result.Success.Should().BeTrue();
    result.IsBluRay.Should().BeTrue();
    result.Subtitles.Should().HaveCount(3);
    Path.GetFileName(result.AnalyzedPath!).Should().Be("00001.m2ts");

    var disc = result.Disc.Should().BeOfType<BluRayStructure>().Subject;
    disc.Playlists.Should().HaveCount(2);
    disc.PlaylistFiles.Should().ContainSingle();
  }

  [Fact]
  public async Task AnalyzeAsync_OnAFolderThatIsNotADisc_Fails()
  {
    var fileSystem = new FakeFileSystem().AddDirectory(@"D:\media\movies");
    var strategy = new DvdAnalysisStrategy(fileSystem);

    var result = await strategy.AnalyzeAsync(
      new DirectoryMediaSource(@"D:\media\movies"),
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem),
      CancellationToken.None);

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.SourceNotFound);
  }

  [Fact]
  public async Task AnalyzeAsync_WhenTheStructureHoldsNoTitle_ReportsTheStructureAsUnreadable()
  {
    var fileSystem = new FakeFileSystem()
      .AddFile(Path.Combine(Bdmv, "index.bdmv"), 2048)
      .AddFile(Path.Combine(Bdmv, "STREAM", "00001.m2ts"), 512);

    var strategy = new BluRayAnalysisStrategy(fileSystem);
    var result = await strategy.AnalyzeAsync(
      new DirectoryMediaSource(Bdmv),
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem),
      CancellationToken.None);

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.DiscStructureUnreadable);
  }

  [Fact]
  public async Task AnalyzeAsync_DisposesEveryHandleItOpens()
  {
    var fileSystem = CreateBluRay();
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Path.Combine(Bdmv, "STREAM", "00001.m2ts"))
      .WithStreams(StreamKind.Video, 1)
      .WithDuration(TimeSpan.FromMinutes(128));
    factory.AddMedia(Path.Combine(Bdmv, "STREAM", "00002.m2ts")).WithDuration(TimeSpan.FromMinutes(14));

    var strategy = new BluRayAnalysisStrategy(fileSystem);
    await strategy.AnalyzeAsync(
      new DirectoryMediaSource(Bdmv),
      TestContext.Create(factory, fileSystem),
      CancellationToken.None);

    factory.Created.Should().NotBeEmpty();
    factory.Created.Should().OnlyContain(x => x.IsDisposed);
    factory.Created.Should().OnlyContain(x => x.DisposeCount == 1);
  }

  [Fact]
  public async Task AnalyzeAsync_ObservesCancellationWhileWalkingTheDisc()
  {
    var fileSystem = CreateBluRay();
    using var cancellation = new CancellationTokenSource();
    cancellation.Cancel();

    var strategy = new BluRayAnalysisStrategy(fileSystem);
    var analyze = () => strategy.AnalyzeAsync(
      new DirectoryMediaSource(Bdmv),
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem),
      cancellation.Token);

    await analyze.Should().ThrowAsync<OperationCanceledException>();
  }
}
