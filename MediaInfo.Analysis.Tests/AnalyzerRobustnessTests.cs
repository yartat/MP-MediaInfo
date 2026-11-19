#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Decorators;
using MediaInfo.Analysis.Native;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Tests.Fakes;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for what an analysis does with a media, a file system or a caller that does not behave.</summary>
public class AnalyzerRobustnessTests
{
  private const string Movie = @"D:\media\movie.mkv";

  private static (FakeNativeMediaInfoFactory Factory, FakeFileSystem FileSystem, FakeMediaProfile Profile) CreateMedia()
  {
    var fileSystem = new FakeFileSystem().AddFile(Movie, 4096);
    var factory = new FakeNativeMediaInfoFactory();
    var profile = factory.AddMedia(Movie).WithStreams(StreamKind.Video, 1).WithStreams(StreamKind.Audio, 1);
    return (factory, fileSystem, profile);
  }

  private static MediaInfoAnalyzerBuilder Builder(FakeNativeMediaInfoFactory factory, IFileSystem fileSystem) =>
    MediaInfoAnalyzerBuilder.Create().UseNativeFactory(factory).UseFileSystem(fileSystem);

  #region Timeout

  [Fact]
  public async Task Timeout_NativeCallDoesNotReturn_ReleasesTheCallerAtTheTimeout()
  {
    var (factory, fileSystem, profile) = CreateMedia();
    using var gate = new ManualResetEventSlim(false);
    profile.OpenGate = gate;
    var analyzer = Builder(factory, fileSystem).WithTimeout(TimeSpan.FromMilliseconds(300)).Build();
    var clock = Stopwatch.StartNew();

    var result = await analyzer.AnalyzeAsync(new FileMediaSource(Movie));

    result.Failure!.Reason.Should().Be(AnalysisFailureReason.Timeout);
    clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));

    // The abandoned call finishes on its own thread and gives the handle back.
    gate.Set();
    SpinWait.SpinUntil(() => factory.Created.Count > 0 && factory.Created[0].IsDisposed, TimeSpan.FromSeconds(5))
      .Should().BeTrue();
  }

  [Fact]
  public async Task Cancellation_NativeCallDoesNotReturn_ReleasesTheCaller()
  {
    var (factory, fileSystem, profile) = CreateMedia();
    using var gate = new ManualResetEventSlim(false);
    profile.OpenGate = gate;
    var analyzer = Builder(factory, fileSystem).Build();
    using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

    var act = () => analyzer.AnalyzeAsync(new FileMediaSource(Movie), cancellation.Token);

    await act.Should().ThrowAsync<OperationCanceledException>();
    gate.Set();
  }

  #endregion

  #region Retry

  [Fact]
  public async Task Retry_StreamSource_IsAnalyzedOnce()
  {
    var inner = new StubAnalyzer((source, _, _) => Task.FromResult(
      MediaAnalysisResult.Failed(source, AnalysisFailureReason.Unknown, "failed")));
    var retrying = new RetryingAnalyzer(inner, attempts: 3, delay: TimeSpan.FromMilliseconds(1));

    var result = await retrying.AnalyzeAsync(new StreamMediaSource(new MemoryStream([1, 2, 3])));

    result.Failure!.Reason.Should().Be(AnalysisFailureReason.Unknown);
    inner.CallCount.Should().Be(1, "a stream cannot be read twice");
  }

  [Fact]
  public async Task Retry_FileSource_IsStillRepeated()
  {
    var inner = new StubAnalyzer((source, _, _) => Task.FromResult(
      MediaAnalysisResult.Failed(source, AnalysisFailureReason.Unknown, "failed")));
    var retrying = new RetryingAnalyzer(inner, attempts: 3, delay: TimeSpan.FromMilliseconds(1));

    await retrying.AnalyzeAsync(new FileMediaSource(Movie));

    inner.CallCount.Should().Be(3);
  }

  [Theory]
  [InlineData(100, 200)]
  [InlineData(20_000, 40_000)]
  [InlineData(30_000, 60_000)]
  [InlineData(45_000, 60_000)]
  [InlineData(60_000, 60_000)]
  public void NextDelay_DoublesUpToTheLimit(int currentMilliseconds, int expectedMilliseconds) =>
    RetryingAnalyzer.NextDelay(TimeSpan.FromMilliseconds(currentMilliseconds))
      .Should().Be(TimeSpan.FromMilliseconds(expectedMilliseconds));

  [Fact]
  public void NextDelay_AfterAnyNumberOfAttempts_DoesNotOverflow()
  {
    var delay = TimeSpan.FromMilliseconds(200);

    for (var attempt = 0; attempt < 200; attempt++)
    {
      delay = RetryingAnalyzer.NextDelay(delay);
    }

    delay.Should().Be(RetryingAnalyzer.LongestDelay);
    RetryingAnalyzer.NextDelay(TimeSpan.MaxValue).Should().Be(RetryingAnalyzer.LongestDelay);
  }

  #endregion

  #region Durations

  [Fact]
  public async Task Analyze_DurationBeyondWhatATimeSpanHolds_DoesNotFailTheAnalysis()
  {
    var (factory, fileSystem, profile) = CreateMedia();
    profile
      .SetIndexed(StreamKind.Video, 0, (int)NativeMethods.Video.Video_Duration, "99999999999999999999999")
      .SetIndexed(StreamKind.Audio, 0, (int)NativeMethods.Audio.Audio_Duration, "99999999999999999999999");
    factory.Media[Movie].WithStreams(StreamKind.Text, 1)
      .SetIndexed(StreamKind.Text, 0, (int)NativeMethods.Text.Text_Duration, "99999999999999999999999");

    var result = await Builder(factory, fileSystem).Build().AnalyzeAsync(new FileMediaSource(Movie));

    result.Success.Should().BeTrue(result.Failure?.Message);
    result.VideoStreams[0].Duration.Should().Be(TimeSpan.Zero);
    result.AudioStreams[0].Duration.Should().Be(TimeSpan.Zero);
    result.Subtitles[0].Duration.Should().Be(TimeSpan.Zero);
  }

  [Fact]
  public async Task Analyze_NegativeRotation_IsKept()
  {
    var (factory, fileSystem, profile) = CreateMedia();
    profile.SetIndexed(StreamKind.Video, 0, (int)NativeMethods.Video.Video_Rotation, "-90.000");

    var result = await Builder(factory, fileSystem).Build().AnalyzeAsync(new FileMediaSource(Movie));

    result.VideoStreams[0].Rotation.Should().Be(-90d);
  }

  #endregion

  #region Cache

  [Fact]
  public async Task Cache_PathTheFileSystemRejects_IsAnalyzedWithoutCaching()
  {
    var inner = new StubAnalyzer();
    var cache = new MemoryAnalysisCache();
    var caching = new CachingAnalyzer(inner, cache, new RejectingFileSystem());

    var result = await caching.AnalyzeAsync(new FileMediaSource("bad\0path.mkv"));

    result.Success.Should().BeTrue();
    inner.CallCount.Should().Be(1);
    cache.Count.Should().Be(0);
  }

  [Fact]
  public async Task Cache_ExternalSubtitlesAddedAfterTheFirstAnalysis_AreNoticed()
  {
    var (factory, _, _) = CreateMedia();
    var fileSystem = new FakeFileSystem().AddFile(Movie, 4096).SetLastWriteTimeUtc(Movie, DateTimeOffset.UnixEpoch);
    var analyzer = Builder(factory, fileSystem).WithCaching().WithExternalSubtitles().Build();

    var first = await analyzer.AnalyzeAsync(new FileMediaSource(Movie));
    fileSystem.AddFile(@"D:\media\movie.srt", 100);
    var second = await analyzer.AnalyzeAsync(new FileMediaSource(Movie));

    first.HasExternalSubtitles.Should().BeFalse();
    second.HasExternalSubtitles.Should().BeTrue();
    factory.OpenedPaths.Should().ContainSingle("the second answer comes from the cache");
  }

  private static CachingAnalyzer CachingOf(StubAnalyzer inner, out MemoryAnalysisCache cache)
  {
    cache = new MemoryAnalysisCache();
    return new CachingAnalyzer(inner, cache, new FakeFileSystem().AddFile(Movie, 4096));
  }

  private static MediaAnalysisResult ResultWithStream(string title)
  {
    var video = new global::MediaInfo.Model.VideoStream { Format = title };
    return new MediaAnalysisResult { Success = true, VideoStreams = [video], BestVideoStream = video };
  }

  [Fact]
  public async Task Cache_ChangingAnAnswer_DoesNotChangeTheNextOne()
  {
    var inner = new StubAnalyzer((_, _, _) => Task.FromResult(ResultWithStream("original")));
    var caching = CachingOf(inner, out _);

    var first = await caching.AnalyzeAsync(new FileMediaSource(Movie));
    first.VideoStreams[0].Format = "changed";
    var second = await caching.AnalyzeAsync(new FileMediaSource(Movie));
    second.VideoStreams[0].Format = "changed too";
    var third = await caching.AnalyzeAsync(new FileMediaSource(Movie));

    inner.CallCount.Should().Be(1);
    third.VideoStreams[0].Format.Should().Be("original");
    third.Should().NotBeSameAs(second);
  }

  [Fact]
  public async Task Cache_Copy_KeepsTheBestStreamAsAnElementOfTheList()
  {
    var inner = new StubAnalyzer((_, _, _) => Task.FromResult(ResultWithStream("a")));
    var caching = CachingOf(inner, out _);

    await caching.AnalyzeAsync(new FileMediaSource(Movie));
    var cached = await caching.AnalyzeAsync(new FileMediaSource(Movie));

    cached.BestVideoStream.Should().BeSameAs(cached.VideoStreams[0]);
  }

  [Fact]
  public async Task Cache_ConcurrentMisses_ShareOneAnalysis()
  {
    var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    var inner = new StubAnalyzer(async (_, _, _) =>
    {
      await release.Task;
      return ResultWithStream("shared");
    });
    var caching = CachingOf(inner, out _);

    var calls = Enumerable.Range(0, 5).Select(_ => caching.AnalyzeAsync(new FileMediaSource(Movie))).ToArray();
    await Task.Delay(100);
    release.SetResult(true);
    var results = await Task.WhenAll(calls);

    inner.CallCount.Should().Be(1);
    results.Select(x => x.VideoStreams[0]).Distinct(ReferenceEqualityComparer.Instance).Should().HaveCount(5, "each caller gets its own copy");
  }

  [Fact]
  public async Task Cache_OneCallerGivingUp_DoesNotCancelTheOthers()
  {
    var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    CancellationToken seen = default;
    var inner = new StubAnalyzer(async (_, _, token) =>
    {
      seen = token;
      await release.Task.WaitAsync(token);
      return ResultWithStream("kept");
    });
    var caching = CachingOf(inner, out _);
    using var giveUp = new CancellationTokenSource();

    var impatient = caching.AnalyzeAsync(new FileMediaSource(Movie), giveUp.Token);
    var patient = caching.AnalyzeAsync(new FileMediaSource(Movie));
    await Task.Delay(100);
    giveUp.Cancel();
    await FluentActions.Awaiting(() => impatient).Should().ThrowAsync<OperationCanceledException>();
    release.SetResult(true);
    var result = await patient;

    result.Success.Should().BeTrue();
    seen.IsCancellationRequested.Should().BeFalse();
    inner.CallCount.Should().Be(1);
  }

  [Fact]
  public async Task Cache_EveryCallerGivingUp_CancelsTheAnalysis()
  {
    var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    CancellationToken seen = default;
    var inner = new StubAnalyzer(async (_, _, token) =>
    {
      seen = token;
      started.SetResult(true);
      await Task.Delay(Timeout.Infinite, token);
      return ResultWithStream("never");
    });
    var caching = CachingOf(inner, out var cache);
    using var giveUp = new CancellationTokenSource();

    var call = caching.AnalyzeAsync(new FileMediaSource(Movie), giveUp.Token);
    await started.Task;
    giveUp.Cancel();
    await FluentActions.Awaiting(() => call).Should().ThrowAsync<OperationCanceledException>();
    await Task.Delay(100);

    seen.IsCancellationRequested.Should().BeTrue();
    cache.Count.Should().Be(0);
  }

  [Fact]
  public async Task Cache_FailureOfTheSharedAnalysis_ReachesEveryCaller()
  {
    var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    var inner = new StubAnalyzer(async (_, _, _) =>
    {
      await release.Task;
      throw new InvalidOperationException("boom");
    });
    var caching = CachingOf(inner, out _);

    var calls = Enumerable.Range(0, 3).Select(_ => caching.AnalyzeAsync(new FileMediaSource(Movie))).ToArray();
    await Task.Delay(100);
    release.SetResult(true);

    foreach (var call in calls)
    {
      await FluentActions.Awaiting(() => call).Should().ThrowAsync<InvalidOperationException>();
    }

    inner.CallCount.Should().Be(1);
  }

  [Fact]
  public void FileSystem_PathItCannotRepresent_ReportsNothingInsteadOfThrowing()
  {
    var fileSystem = MediaInfoFileSystem.Instance;

    fileSystem.GetFileLength("bad\0path").Should().Be(0);
    fileSystem.GetLastWriteTimeUtc("bad\0path").Should().Be(DateTimeOffset.MinValue);
  }

  #endregion

  #region Folder scan

  private static async Task<List<IMediaSource>> ScanAsync(IFileSystem fileSystem, MediaFolderScanOptions? options = null)
  {
    var inner = new StubAnalyzer();
    await foreach (var _ in inner.AnalyzeFolderAsync(@"D:\library", options, fileSystem))
    {
    }

    return inner.Calls;
  }

  [Fact]
  public async Task Scan_DiscsAtAnyDepth_AreReportedAsDiscs()
  {
    var fileSystem = new FakeFileSystem()
      .AddFile(@"D:\library\a.mkv")
      .AddFile(@"D:\library\collection\2001\film\VIDEO_TS\VTS_01_0.IFO")
      .AddFile(@"D:\library\collection\2001\film\VIDEO_TS\VTS_01_1.VOB")
      .AddFile(@"D:\library\collection\2002\show\BDMV\index.bdmv")
      .AddFile(@"D:\library\collection\2002\show\BDMV\PLAYLIST\00000.mpls")
      .AddFile(@"D:\library\collection\2002\show\BDMV\STREAM\00001.m2ts");

    var calls = await ScanAsync(fileSystem);

    calls.OfType<DirectoryMediaSource>().Select(x => Path.GetFileName(x.Path))
      .Should().BeEquivalentTo("film", "show");
    calls.OfType<FileMediaSource>().Select(x => Path.GetFileName(x.Path))
      .Should().Equal(new[] { "a.mkv" }, "the service files of a disc are not media of their own");
  }

  [Fact]
  public async Task Scan_Cycle_StopsAtTheDepthLimit()
  {
    var fileSystem = new LinkedFileSystem();

    var calls = await ScanAsync(fileSystem);

    calls.Should().BeEmpty();
    fileSystem.Visited.Should().BeLessThan(200);
  }

  [Fact]
  public async Task Scan_FolderThatCannotBeRead_OnlyLosesItself()
  {
    var fileSystem = new FakeFileSystem()
      .AddFile(@"D:\library\a.mkv")
      .AddFile(@"D:\library\locked\b.mkv")
      .AddFile(@"D:\library\open\c.mkv");

    var calls = await ScanAsync(new UnreadableFolderFileSystem(fileSystem, @"D:\library\locked"));

    calls.OfType<FileMediaSource>().Select(x => Path.GetFileName(x.Path))
      .Should().Equal("a.mkv", "c.mkv");
  }

  #endregion

  #region Locale

  [Fact]
  public async Task NativeLocale_ManyHandlesAtOnce_SetsTheLocaleOnce()
  {
    var locale = new NativeLocale(applies: true);
    var readers = Enumerable.Range(0, 32).Select(_ => new OptionCountingReader()).ToArray();

    await Task.WhenAll(readers.Select(reader => Task.Run(() => locale.Ensure(reader))));

    readers.Sum(x => x.Calls).Should().Be(1);
    readers.SelectMany(x => x.Options).Should().ContainSingle().Which.Should().Be("setlocale_LC_CTYPE=C.UTF-8");
  }

  [Fact]
  public void NativeLocale_WhereItIsNotNeeded_DoesNothing()
  {
    var reader = new OptionCountingReader();

    new NativeLocale(applies: false).Ensure(reader);

    reader.Calls.Should().Be(0);
  }

  private sealed class OptionCountingReader : IMediaInfoReader
  {
    private int _calls;

    public int Calls => _calls;

    public List<string> Options { get; } = [];

    public string Option(string option, string value)
    {
      Interlocked.Increment(ref _calls);
      lock (Options)
      {
        Options.Add($"{option}={value}");
      }

      return string.Empty;
    }

    public string Option(string option) => string.Empty;

    public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch) => string.Empty;

    public string Get(StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo) => string.Empty;

    public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo) => string.Empty;

    public string Get(StreamKind streamKind, int streamNumber, string parameter) => string.Empty;

    public string Get(StreamKind streamKind, int streamNumber, int parameter) => string.Empty;

    public int CountGet(StreamKind streamKind, int streamNumber) => 0;

    public int CountGet(StreamKind streamKind) => 0;

    public string Inform() => string.Empty;
  }

  #endregion

  #region File system doubles

  private sealed class RejectingFileSystem : IFileSystem
  {
    public bool FileExists(string path) => true;

    public bool DirectoryExists(string path) => false;

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption) => [];

    public string[] GetDirectories(string path) => [];

    public long GetFileLength(string path) => throw new ArgumentException("Illegal characters in path.");

    public DateTimeOffset GetLastWriteTimeUtc(string path) => throw new ArgumentException("Illegal characters in path.");

    public Stream OpenRead(string path) => throw new ArgumentException("Illegal characters in path.");
  }

  /// <summary>A file system in which every folder holds one folder that leads back to it.</summary>
  private sealed class LinkedFileSystem : IFileSystem
  {
    public int Visited { get; private set; }

    public bool FileExists(string path) => false;

    public bool DirectoryExists(string path) => true;

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption) => [];

    public string[] GetDirectories(string path)
    {
      Visited++;
      return [path + @"\loop"];
    }

    public long GetFileLength(string path) => 0;

    public DateTimeOffset GetLastWriteTimeUtc(string path) => DateTimeOffset.MinValue;

    public Stream OpenRead(string path) => throw new FileNotFoundException(path);
  }

  /// <summary>A file system in which one folder is listed as empty, the way an unreadable one is.</summary>
  private sealed class UnreadableFolderFileSystem(FakeFileSystem inner, string unreadable) : IFileSystem
  {
    public bool FileExists(string path) => inner.FileExists(path);

    public bool DirectoryExists(string path) => inner.DirectoryExists(path);

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption) =>
      string.Equals(path, unreadable, StringComparison.OrdinalIgnoreCase)
        ? []
        : inner.GetFiles(path, searchPattern, searchOption);

    public string[] GetDirectories(string path) =>
      string.Equals(path, unreadable, StringComparison.OrdinalIgnoreCase) ? [] : inner.GetDirectories(path);

    public long GetFileLength(string path) => inner.GetFileLength(path);

    public DateTimeOffset GetLastWriteTimeUtc(string path) => inner.GetLastWriteTimeUtc(path);

    public Stream OpenRead(string path) => inner.OpenRead(path);
  }

  #endregion
}
