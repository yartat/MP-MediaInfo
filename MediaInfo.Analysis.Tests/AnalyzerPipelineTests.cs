#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Strategies;
using MediaInfo.Analysis.Strategies.Selection;
using MediaInfo.Analysis.Tests.Fakes;
using MediaInfo.Model;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for strategy selection and the analyzer that drives it.</summary>
public class AnalyzerPipelineTests
{
  private const string VideoTs = @"D:\media\disc\VIDEO_TS";
  private const string Movie = @"D:\media\movie.mkv";

  [Fact]
  public void Selector_ChoosesTheLowestPriorityStrategyThatCanHandleTheSource()
  {
    var selector = new PriorityStrategySelector(
    [
      new StubStrategy(30, _ => true),
      new StubStrategy(10, _ => true),
      new StubStrategy(20, _ => true)
    ]);

    selector.Select(new FileMediaSource(Movie))!.Priority.Should().Be(10);
    selector.Strategies.Select(x => x.Priority).Should().ContainInOrder(10, 20, 30);
  }

  [Fact]
  public void Selector_SkipsStrategiesThatCannotHandleTheSource()
  {
    var selector = new PriorityStrategySelector(
    [
      new StubStrategy(10, _ => false),
      new StubStrategy(20, source => source is FileMediaSource)
    ]);

    selector.Select(new FileMediaSource(Movie))!.Priority.Should().Be(20);
    selector.Select(new DirectoryMediaSource(VideoTs)).Should().BeNull();
  }

  [Fact]
  public async Task AnalyzeAsync_WithoutASource_FailsInsteadOfThrowing()
  {
    var analyzer = CreateAnalyzer(new FakeNativeMediaInfoFactory(), new FakeFileSystem());

    var result = await analyzer.AnalyzeAsync((IMediaSource)null!);

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.SourceNotSpecified);
  }

  [Fact]
  public async Task AnalyzeAsync_WithNoMatchingStrategy_ReportsAnUnsupportedSource()
  {
    var analyzer = new MediaInfoAnalyzer(
      new PriorityStrategySelector([]),
      TestContext.Create(new FakeNativeMediaInfoFactory(), new FakeFileSystem()));

    var result = await analyzer.AnalyzeAsync(new FileMediaSource(Movie));

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.UnsupportedSource);
  }

  [Fact]
  public async Task AnalyzeAsync_WhenAStrategyThrows_ReportsTheFailureAndKeepsTheException()
  {
    var boom = new InvalidOperationException("the disc is on fire");
    var analyzer = new MediaInfoAnalyzer(
      new PriorityStrategySelector([new ThrowingStrategy(boom)]),
      TestContext.Create(new FakeNativeMediaInfoFactory(), new FakeFileSystem()));

    var result = await analyzer.AnalyzeAsync(new FileMediaSource(Movie));

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.Unknown);
    result.Failure.Exception.Should().BeSameAs(boom);
  }

  [Fact]
  public async Task AnalyzeAsync_WhenCancelled_ThrowsInsteadOfReportingAFailure()
  {
    using var cancellation = new CancellationTokenSource();
    cancellation.Cancel();

    var analyzer = new MediaInfoAnalyzer(
      new PriorityStrategySelector([new ThrowingStrategy(new OperationCanceledException())]),
      TestContext.Create(new FakeNativeMediaInfoFactory(), new FakeFileSystem()));

    var analyze = () => analyzer.AnalyzeAsync(new FileMediaSource(Movie), cancellation.Token);

    await analyze.Should().ThrowAsync<OperationCanceledException>();
  }

  [Fact]
  public async Task AnalyzeAsync_OnAMissingFile_ReportsThatItWasNotFound()
  {
    var analyzer = CreateAnalyzer(new FakeNativeMediaInfoFactory(), new FakeFileSystem());

    var result = await analyzer.AnalyzeAsync(new FileMediaSource(Movie));

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.SourceNotFound);
  }

  [Fact]
  public async Task AnalyzeAsync_OnAnEmptyFile_ReportsThatItIsEmpty()
  {
    var fileSystem = new FakeFileSystem().AddFile(Movie, 0L);
    var analyzer = CreateAnalyzer(new FakeNativeMediaInfoFactory(), fileSystem);

    var result = await analyzer.AnalyzeAsync(new FileMediaSource(Movie));

    result.Failure!.Reason.Should().Be(AnalysisFailureReason.SourceEmpty);
  }

  [Fact]
  public async Task AnalyzeAsync_WhenTheLibraryIsMissing_SaysSo()
  {
    var fileSystem = new FakeFileSystem().AddFile(Movie, 1024);
    var factory = new FakeNativeMediaInfoFactory { IsLibraryAvailable = false };

    var result = await CreateAnalyzer(factory, fileSystem).AnalyzeAsync(new FileMediaSource(Movie));

    result.Failure!.Reason.Should().Be(AnalysisFailureReason.NativeLibraryUnavailable);
  }

  [Fact]
  public async Task AnalyzeAsync_WhenTheLibraryRefusesTheMedia_SaysSo()
  {
    var fileSystem = new FakeFileSystem().AddFile(Movie, 1024);
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Movie).CanOpen = false;

    var result = await CreateAnalyzer(factory, fileSystem).AnalyzeAsync(new FileMediaSource(Movie));

    result.Failure!.Reason.Should().Be(AnalysisFailureReason.NativeOpenFailed);
  }

  [Fact]
  public async Task AnalyzeAsync_OnAFile_ReportsTheStreamsAndTheLibraryVersion()
  {
    var fileSystem = new FakeFileSystem().AddFile(Movie, 4096);
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Movie)
      .WithStreams(StreamKind.Video, 1)
      .WithStreams(StreamKind.Audio, 2)
      .WithStreams(StreamKind.Text, 1)
      .WithDuration(TimeSpan.FromMinutes(42))
      .Set(StreamKind.General, 0, "Format", "Matroska");

    var result = await CreateAnalyzer(factory, fileSystem).AnalyzeAsync(new FileMediaSource(Movie));

    result.Success.Should().BeTrue();
    result.HasVideo.Should().BeTrue();
    result.AudioStreams.Should().HaveCount(2);
    result.HasSubtitles.Should().BeTrue();
    result.General.Format.Should().Be("Matroska");
    result.General.Duration.Should().Be(TimeSpan.FromMinutes(42));
    result.General.Size.Should().Be(4096);
    result.LibraryVersion.Should().Be(factory.LibraryVersion);
    result.AnalyzedPath.Should().Be(Movie);
  }

  [Theory]
  [InlineData("rtsp://localhost:8554/live")]
  [InlineData("rtmp://localhost/live")]
  [InlineData("mms://localhost/live")]
  public async Task AnalyzeAsync_OnALiveStream_IsDeclinedBeforeAnyWorkIsDone(string location)
  {
    var factory = new FakeNativeMediaInfoFactory();

    var result = await CreateAnalyzer(factory, new FakeFileSystem()).AnalyzeAsync(new NetworkMediaSource(location));

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.UnsupportedSource);
    factory.Created.Should().BeEmpty("no handle is created for a media that cannot be described");
  }

  [Fact]
  public async Task AnalyzeAsync_OnAnHttpSource_HandsTheLocationToTheLibrary()
  {
    const string Location = "https://localhost:8443/videos/test.mp4";
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Location).WithStreams(StreamKind.Video, 1);

    var result = await CreateAnalyzer(factory, new FakeFileSystem()).AnalyzeAsync(new NetworkMediaSource(Location));

    result.Success.Should().BeTrue();
    result.SourceKind.Should().Be(MediaSourceKind.Network);
    factory.OpenedPaths.Should().ContainSingle().Which.Should().Be(Location);
  }

  [Theory]
  [InlineData("http://localhost/movie.mp4", MediaSourceKind.Network)]
  [InlineData("rtsp://localhost/live", MediaSourceKind.Network)]
  [InlineData(@"D:\media\movie.mkv", MediaSourceKind.File)]
  public void MediaSource_ClassifiesAPathOrLocation(string input, MediaSourceKind expected)
  {
    MediaSource.From(input).Kind.Should().Be(expected);
  }

  [Fact]
  public void MediaSource_RejectsAnEmptyPath()
  {
    var create = () => MediaSource.From(string.Empty);

    create.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void DefaultSelection_RanksVideoByOverallQualityAndAudioByChannelsThenBitrate()
  {
    var streams = new[]
    {
      new VideoStream { Width = 1920, Height = 1080, BitDepth = 8, FrameRate = 25, Bitrate = 8_000_000 },
      new VideoStream { Width = 3840, Height = 2160, BitDepth = 10, FrameRate = 25, Bitrate = 40_000_000 },
      new VideoStream { Width = 720, Height = 576, BitDepth = 8, FrameRate = 25, Bitrate = 2_000_000 }
    };

    var audio = new[]
    {
      new AudioStream { Channel = 2, Bitrate = 320_000 },
      new AudioStream { Channel = 6, Bitrate = 448_000 },
      new AudioStream { Channel = 6, Bitrate = 1_500_000 }
    };

    DefaultStreamSelectionStrategy.Instance.SelectBestVideo(streams)!.Width.Should().Be(3840);
    DefaultStreamSelectionStrategy.Instance.SelectBestAudio(audio)!.Bitrate.Should().Be(1_500_000);
  }

  [Fact]
  public void DefaultSelection_OnAnEmptySet_SelectsNothing()
  {
    DefaultStreamSelectionStrategy.Instance.SelectBestVideo([]).Should().BeNull();
    DefaultStreamSelectionStrategy.Instance.SelectBestAudio([]).Should().BeNull();
  }

  [Fact]
  public void PreferredLanguageSelection_PrefersTheFirstLanguageThatIsPresent()
  {
    var streams = new[]
    {
      new AudioStream { Language = "English", Channel = 8, Bitrate = 3_000_000 },
      new AudioStream { Language = "Russian", Channel = 6, Bitrate = 448_000 },
      new AudioStream { Language = "Russian", Channel = 2, Bitrate = 192_000 }
    };

    var strategy = new PreferredLanguageSelectionStrategy(DefaultStreamSelectionStrategy.Instance, "Russian", "English");

    strategy.SelectBestAudio(streams)!.Channel.Should().Be(6, "the best Russian track wins over a better English one");
  }

  [Fact]
  public void PreferredLanguageSelection_WhenNoLanguageMatches_FallsBackToEveryStream()
  {
    var streams = new[]
    {
      new AudioStream { Language = "German", Channel = 6, Bitrate = 448_000 },
      new AudioStream { Language = "French", Channel = 2, Bitrate = 192_000 }
    };

    var strategy = new PreferredLanguageSelectionStrategy(DefaultStreamSelectionStrategy.Instance, "Japanese");

    strategy.SelectBestAudio(streams)!.Language.Should().Be("German");
  }

  private static MediaInfoAnalyzer CreateAnalyzer(FakeNativeMediaInfoFactory factory, FakeFileSystem fileSystem)
  {
    var context = TestContext.Create(factory, fileSystem);
    return new MediaInfoAnalyzer(
      new PriorityStrategySelector(MediaInfoAnalyzer.CreateDefaultStrategies(fileSystem)),
      context);
  }

  private sealed class StubStrategy(int priority, Func<IMediaSource, bool> canHandle) : IMediaAnalysisStrategy
  {
    public int Priority => priority;

    public bool CanHandle(IMediaSource source) => canHandle(source);

    public Task<MediaAnalysisResult> AnalyzeAsync(
      IMediaSource source,
      MediaAnalysisContext context,
      CancellationToken cancellationToken) =>
      Task.FromResult(new MediaAnalysisResult { Success = true });
  }

  private sealed class ThrowingStrategy(Exception exception) : IMediaAnalysisStrategy
  {
    public int Priority => 0;

    public bool CanHandle(IMediaSource source) => true;

    public Task<MediaAnalysisResult> AnalyzeAsync(
      IMediaSource source,
      MediaAnalysisContext context,
      CancellationToken cancellationToken) =>
      throw exception;
  }
}
