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
using MediaInfo.Analysis.Decorators;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Tests.Fakes;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for the wrappers that add cross-cutting concerns to an analyzer.</summary>
public class DecoratorTests
{
  private const string Movie = @"D:\media\movie.mkv";

  private static MediaAnalysisResult Failure(AnalysisFailureReason reason) =>
    new() { Success = false, Failure = new AnalysisFailure(reason, reason.ToString()) };

  #region Validation

  [Fact]
  public async Task Validating_RejectsAMissingSourceWithoutCallingTheInnerAnalyzer()
  {
    var inner = new StubAnalyzer();

    var result = await new ValidatingAnalyzer(inner).AnalyzeAsync(null!);

    result.Failure!.Reason.Should().Be(AnalysisFailureReason.SourceNotSpecified);
    inner.CallCount.Should().Be(0);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public async Task Validating_RejectsAnEmptyPath(string path)
  {
    var inner = new StubAnalyzer();

    var result = await new ValidatingAnalyzer(inner).AnalyzeAsync(new FileMediaSource(path));

    result.Failure!.Reason.Should().Be(AnalysisFailureReason.SourceNotSpecified);
    inner.CallCount.Should().Be(0);
  }

  [Fact]
  public async Task Validating_RejectsAStreamThatCannotBeRead()
  {
    var inner = new StubAnalyzer();
    var closed = new MemoryStream();
    await closed.DisposeAsync();

    var result = await new ValidatingAnalyzer(inner).AnalyzeAsync(new StreamMediaSource(closed));

    result.Failure!.Reason.Should().Be(AnalysisFailureReason.StreamNotReadable);
    inner.CallCount.Should().Be(0);
  }

  [Fact]
  public async Task Validating_PassesAUsableSourceThrough()
  {
    var inner = new StubAnalyzer();

    var result = await new ValidatingAnalyzer(inner).AnalyzeAsync(new FileMediaSource(Movie));

    result.Success.Should().BeTrue();
    inner.CallCount.Should().Be(1);
  }

  #endregion

  #region Caching

  private static (CachingAnalyzer Analyzer, StubAnalyzer Inner, FakeFileSystem FileSystem) CreateCaching(
    MediaAnalysisResult? result = null)
  {
    var fileSystem = new FakeFileSystem()
      .AddFile(Movie, 1000)
      .SetLastWriteTimeUtc(Movie, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

    var inner = result is null ? new StubAnalyzer() : new StubAnalyzer(result);
    return (new CachingAnalyzer(inner, new MemoryAnalysisCache(), fileSystem), inner, fileSystem);
  }

  [Fact]
  public async Task Caching_ReadsTheSameFileOnlyOnce()
  {
    var (analyzer, inner, _) = CreateCaching();

    await analyzer.AnalyzeAsync(new FileMediaSource(Movie));
    await analyzer.AnalyzeAsync(new FileMediaSource(Movie));
    await analyzer.AnalyzeAsync(new FileMediaSource(Movie));

    inner.CallCount.Should().Be(1);
  }

  [Fact]
  public async Task Caching_ReadsAgainAfterTheFileIsEdited()
  {
    var (analyzer, inner, fileSystem) = CreateCaching();

    await analyzer.AnalyzeAsync(new FileMediaSource(Movie));
    fileSystem.SetLastWriteTimeUtc(Movie, new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero));
    await analyzer.AnalyzeAsync(new FileMediaSource(Movie));

    inner.CallCount.Should().Be(2, "the last write time is part of the key");
  }

  [Fact]
  public async Task Caching_ReadsAgainAfterTheFileChangesSize()
  {
    var (analyzer, inner, fileSystem) = CreateCaching();

    await analyzer.AnalyzeAsync(new FileMediaSource(Movie));
    fileSystem.AddFile(Movie, 2000);
    await analyzer.AnalyzeAsync(new FileMediaSource(Movie));

    inner.CallCount.Should().Be(2);
  }

  [Fact]
  public async Task Caching_DoesNotStoreAFailedAnalysis()
  {
    var (analyzer, inner, _) = CreateCaching(Failure(AnalysisFailureReason.NativeOpenFailed));

    await analyzer.AnalyzeAsync(new FileMediaSource(Movie));
    await analyzer.AnalyzeAsync(new FileMediaSource(Movie));

    inner.CallCount.Should().Be(2);
  }

  [Fact]
  public async Task Caching_PassesAStreamStraightThrough()
  {
    var (analyzer, inner, _) = CreateCaching();
    using var stream = new MemoryStream(new byte[16]);

    await analyzer.AnalyzeAsync(new StreamMediaSource(stream));
    await analyzer.AnalyzeAsync(new StreamMediaSource(stream));

    inner.CallCount.Should().Be(2, "a stream cannot be identified without consuming it");
  }

  [Fact]
  public async Task Caching_PassesANetworkMediaStraightThrough()
  {
    var (analyzer, inner, _) = CreateCaching();

    await analyzer.AnalyzeAsync(new NetworkMediaSource("https://localhost/movie.mp4"));
    await analyzer.AnalyzeAsync(new NetworkMediaSource("https://localhost/movie.mp4"));

    inner.CallCount.Should().Be(2);
  }

  [Fact]
  public async Task Caching_ReusesADiscFolderResult()
  {
    var fileSystem = new FakeFileSystem()
      .AddDirectory(@"D:\media\disc\VIDEO_TS")
      .SetLastWriteTimeUtc(@"D:\media\disc\VIDEO_TS", DateTimeOffset.UnixEpoch);
    var inner = new StubAnalyzer();
    var analyzer = new CachingAnalyzer(inner, new MemoryAnalysisCache(), fileSystem);

    await analyzer.AnalyzeAsync(new DirectoryMediaSource(@"D:\media\disc\VIDEO_TS"));
    await analyzer.AnalyzeAsync(new DirectoryMediaSource(@"D:\media\disc\VIDEO_TS"));

    inner.CallCount.Should().Be(1);
  }

  [Fact]
  public void MemoryCache_EvictsTheOldestEntryWhenItIsFull()
  {
    var cache = new MemoryAnalysisCache(capacity: 2);

    cache.Set("a", new MediaAnalysisResult { Success = true });
    Thread.Sleep(10);
    cache.Set("b", new MediaAnalysisResult { Success = true });
    Thread.Sleep(10);
    cache.Set("c", new MediaAnalysisResult { Success = true });

    cache.Count.Should().Be(2);
    cache.TryGet("a", out _).Should().BeFalse();
    cache.TryGet("c", out _).Should().BeTrue();
  }

  [Fact]
  public void MemoryCache_ForgetsAnEntryOnceItsTimeIsUp()
  {
    var cache = new MemoryAnalysisCache(TimeSpan.FromMilliseconds(20));
    cache.Set("a", new MediaAnalysisResult { Success = true });

    cache.TryGet("a", out _).Should().BeTrue();
    Thread.Sleep(60);
    cache.TryGet("a", out _).Should().BeFalse();
  }

  [Fact]
  public void MemoryCache_RemovesAndClears()
  {
    var cache = new MemoryAnalysisCache();
    cache.Set("a", new MediaAnalysisResult { Success = true });
    cache.Set("b", new MediaAnalysisResult { Success = true });

    cache.Remove("a");
    cache.TryGet("a", out _).Should().BeFalse();

    cache.Clear();
    cache.Count.Should().Be(0);
  }

  #endregion

  #region Timeout

  [Fact]
  public async Task Timeout_ReportsAFailureRatherThanThrowing()
  {
    var inner = new StubAnalyzer(async (_, _, token) =>
    {
      await Task.Delay(Timeout.Infinite, token);
      return new MediaAnalysisResult { Success = true };
    });

    var result = await new TimeoutAnalyzer(inner, TimeSpan.FromMilliseconds(40))
      .AnalyzeAsync(new FileMediaSource(Movie));

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.Timeout);
  }

  [Fact]
  public async Task Timeout_LetsTheCallersOwnCancellationSurfaceAsAnException()
  {
    var inner = new StubAnalyzer(async (_, _, token) =>
    {
      await Task.Delay(Timeout.Infinite, token);
      return new MediaAnalysisResult { Success = true };
    });

    using var cancellation = new CancellationTokenSource();
    cancellation.CancelAfter(TimeSpan.FromMilliseconds(30));

    var analyze = () => new TimeoutAnalyzer(inner, TimeSpan.FromMinutes(5))
      .AnalyzeAsync(new FileMediaSource(Movie), cancellation.Token);

    await analyze.Should().ThrowAsync<OperationCanceledException>();
  }

  [Fact]
  public async Task Timeout_DoesNotInterfereWithAFastAnalysis()
  {
    var result = await new TimeoutAnalyzer(new StubAnalyzer(), TimeSpan.FromMinutes(1))
      .AnalyzeAsync(new FileMediaSource(Movie));

    result.Success.Should().BeTrue();
  }

  [Fact]
  public void Timeout_RejectsANonPositiveBudget()
  {
    var create = () => new TimeoutAnalyzer(new StubAnalyzer(), TimeSpan.Zero);

    create.Should().Throw<ArgumentOutOfRangeException>();
  }

  #endregion

  #region Retry

  [Fact]
  public async Task Retry_RepeatsAFailureThatMayNotPersistAndGivesUpAfterTheLastAttempt()
  {
    var inner = new StubAnalyzer(Failure(AnalysisFailureReason.NativeOpenFailed));

    var result = await new RetryingAnalyzer(inner, attempts: 3, delay: TimeSpan.FromMilliseconds(1))
      .AnalyzeAsync(new FileMediaSource(Movie));

    inner.CallCount.Should().Be(3);
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.NativeOpenFailed);
  }

  [Fact]
  public async Task Retry_StopsAsSoonAsAnAttemptSucceeds()
  {
    var inner = new StubAnalyzer((_, ordinal, _) => Task.FromResult(
      ordinal < 2 ? Failure(AnalysisFailureReason.NativeOpenFailed) : new MediaAnalysisResult { Success = true }));

    var result = await new RetryingAnalyzer(inner, attempts: 5, delay: TimeSpan.FromMilliseconds(1))
      .AnalyzeAsync(new FileMediaSource(Movie));

    result.Success.Should().BeTrue();
    inner.CallCount.Should().Be(2);
  }

  [Theory]
  [InlineData(AnalysisFailureReason.SourceNotFound)]
  [InlineData(AnalysisFailureReason.SourceEmpty)]
  [InlineData(AnalysisFailureReason.UnsupportedSource)]
  [InlineData(AnalysisFailureReason.NoStreamsFound)]
  public async Task Retry_DoesNotRepeatAFailureThatWillNotChange(AnalysisFailureReason reason)
  {
    var inner = new StubAnalyzer(Failure(reason));

    await new RetryingAnalyzer(inner, attempts: 4, delay: TimeSpan.FromMilliseconds(1))
      .AnalyzeAsync(new FileMediaSource(Movie));

    inner.CallCount.Should().Be(1);
  }

  [Fact]
  public async Task Retry_DoesNotRepeatASuccess()
  {
    var inner = new StubAnalyzer();

    await new RetryingAnalyzer(inner, attempts: 4, delay: TimeSpan.FromMilliseconds(1))
      .AnalyzeAsync(new FileMediaSource(Movie));

    inner.CallCount.Should().Be(1);
  }

  [Fact]
  public void Retry_RejectsFewerThanOneAttempt()
  {
    var create = () => new RetryingAnalyzer(new StubAnalyzer(), attempts: 0);

    create.Should().Throw<ArgumentOutOfRangeException>();
  }

  #endregion

  #region Throttling

  [Fact]
  public async Task Throttling_NeverRunsMoreAnalysesThanItWasAllowed()
  {
    var inner = new StubAnalyzer(async (_, _, _) =>
    {
      await Task.Delay(25);
      return new MediaAnalysisResult { Success = true };
    });

    using var analyzer = new ThrottlingAnalyzer(inner, concurrency: 3);
    await Task.WhenAll(Enumerable
      .Range(0, 24)
      .Select(_ => analyzer.AnalyzeAsync(new FileMediaSource(Movie))));

    inner.CallCount.Should().Be(24);
    inner.PeakConcurrency.Should().BeLessThanOrEqualTo(3);
  }

  [Fact]
  public async Task Throttling_ReleasesItsSlotWhenAnAnalysisFails()
  {
    var inner = new StubAnalyzer((_, _, _) => throw new InvalidOperationException("boom"));
    using var analyzer = new ThrottlingAnalyzer(inner, concurrency: 1);

    for (var i = 0; i < 3; i++)
    {
      var analyze = () => analyzer.AnalyzeAsync(new FileMediaSource(Movie));
      await analyze.Should().ThrowAsync<InvalidOperationException>();
    }

    inner.CallCount.Should().Be(3, "a slot that was not released would deadlock the next call");
  }

  [Fact]
  public void Throttling_RejectsANonPositiveLimit()
  {
    var create = () => new ThrottlingAnalyzer(new StubAnalyzer(), concurrency: 0);

    create.Should().Throw<ArgumentOutOfRangeException>();
  }

  #endregion

  #region External subtitles

  [Fact]
  public async Task ExternalSubtitles_ReportsASubtitleFileBesideTheMedia()
  {
    var fileSystem = new FakeFileSystem()
      .AddFile(Movie)
      .AddFile(@"D:\media\movie.srt");

    var result = await new ExternalSubtitleAnalyzer(new StubAnalyzer(), fileSystem)
      .AnalyzeAsync(new FileMediaSource(Movie));

    result.HasExternalSubtitles.Should().BeTrue();
  }

  [Fact]
  public async Task ExternalSubtitles_IgnoresAFileThatIsNotASubtitle()
  {
    var fileSystem = new FakeFileSystem()
      .AddFile(Movie)
      .AddFile(@"D:\media\movie.jpg")
      .AddFile(@"D:\media\movie.nfo");

    var result = await new ExternalSubtitleAnalyzer(new StubAnalyzer(), fileSystem)
      .AnalyzeAsync(new FileMediaSource(Movie));

    result.HasExternalSubtitles.Should().BeFalse();
  }

  [Fact]
  public async Task ExternalSubtitles_IgnoresTheMediaItself()
  {
    var fileSystem = new FakeFileSystem().AddFile(@"D:\media\subtitles.txt");

    var result = await new ExternalSubtitleAnalyzer(new StubAnalyzer(), fileSystem)
      .AnalyzeAsync(new FileMediaSource(@"D:\media\subtitles.txt"));

    result.HasExternalSubtitles.Should().BeFalse();
  }

  [Fact]
  public async Task ExternalSubtitles_DoesNotScanForADiscOrAStream()
  {
    var fileSystem = new FakeFileSystem()
      .AddDirectory(@"D:\media\disc")
      .AddFile(@"D:\media\disc.srt");

    var result = await new ExternalSubtitleAnalyzer(new StubAnalyzer(), fileSystem)
      .AnalyzeAsync(new DirectoryMediaSource(@"D:\media\disc"));

    result.HasExternalSubtitles.Should().BeFalse();
  }

  #endregion

  #region Logging

  [Fact]
  public async Task Logging_DoesNotAlterTheOutcome()
  {
    var expected = new MediaAnalysisResult { Success = true, SourcePath = Movie };
    var logger = new RecordingLogger();

    var result = await new LoggingAnalyzer(new StubAnalyzer(expected), logger)
      .AnalyzeAsync(new FileMediaSource(Movie));

    result.Should().BeSameAs(expected);
    logger.Entries.Should().NotBeEmpty();
  }

  [Fact]
  public async Task Logging_RecordsAFailureAsAWarning()
  {
    var logger = new RecordingLogger();

    await new LoggingAnalyzer(new StubAnalyzer(Failure(AnalysisFailureReason.SourceNotFound)), logger)
      .AnalyzeAsync(new FileMediaSource(Movie));

    logger.Entries.Should().Contain(x => x.Level == Microsoft.Extensions.Logging.LogLevel.Warning);
  }

  [Fact]
  public async Task Logging_RecordsCancellationAndRethrows()
  {
    var logger = new RecordingLogger();
    var inner = new StubAnalyzer((_, _, _) => throw new OperationCanceledException());

    var analyze = () => new LoggingAnalyzer(inner, logger).AnalyzeAsync(new FileMediaSource(Movie));

    await analyze.Should().ThrowAsync<OperationCanceledException>();
    logger.Entries.Should().Contain(x => x.Message.Contains("cancelled"));
  }

  #endregion

  [Fact]
  public void Dispose_CascadesThroughTheWholeChain()
  {
    var inner = new StubAnalyzer();
    var analyzer = new ValidatingAnalyzer(new ThrottlingAnalyzer(inner, 2));

    analyzer.Dispose();

    inner.IsDisposed.Should().BeTrue();
  }
}
