#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Decorators;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Strategies;
using MediaInfo.Analysis.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for assembling an analyzer and registering it with a container.</summary>
public class BuilderTests
{
  private const string Movie = @"D:\media\movie.mkv";

  private static (FakeNativeMediaInfoFactory Factory, FakeFileSystem FileSystem) CreateMedia()
  {
    var fileSystem = new FakeFileSystem()
      .AddFile(Movie, 4096)
      .SetLastWriteTimeUtc(Movie, DateTimeOffset.UnixEpoch);

    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Movie).WithStreams(StreamKind.Video, 1).WithStreams(StreamKind.Audio, 1);
    return (factory, fileSystem);
  }

  private static MediaInfoAnalyzerBuilder CreateBuilder()
  {
    var (factory, fileSystem) = CreateMedia();
    return MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false);
  }

  [Fact]
  public async Task Build_WithNothingRequested_ProducesAWorkingAnalyzer()
  {
    var analyzer = CreateBuilder().Build();

    var result = await analyzer.AnalyzeAsync(new FileMediaSource(Movie));

    analyzer.Should().BeOfType<MediaInfoAnalyzer>("no concern was requested, so nothing wraps the core");
    result.Success.Should().BeTrue();
  }

  [Fact]
  public void Build_ComposesTheDocumentedOrderWhicheverOrderTheConcernsAreRequestedIn()
  {
    // Requested back to front on purpose.
    var analyzer = CreateBuilder()
      .WithExternalSubtitles()
      .WithConcurrencyLimit(2)
      .WithRetry(2, TimeSpan.FromMilliseconds(1))
      .WithTimeout(TimeSpan.FromMinutes(1))
      .WithCaching()
      .WithValidation()
      .UseLogger(new RecordingLogger())
      .Build();

    Unwrap(analyzer).Should().ContainInOrder(
      typeof(LoggingAnalyzer),
      typeof(ValidatingAnalyzer),
      typeof(CachingAnalyzer),
      typeof(TimeoutAnalyzer),
      typeof(RetryingAnalyzer),
      typeof(ThrottlingAnalyzer),
      typeof(ExternalSubtitleAnalyzer),
      typeof(MediaInfoAnalyzer));

    (analyzer as IDisposable)?.Dispose();
  }

  [Fact]
  public void Build_AddsOnlyTheConcernsThatWereRequested()
  {
    var analyzer = CreateBuilder().WithValidation().WithCaching().Build();

    Unwrap(analyzer).Should().ContainInOrder(
      typeof(ValidatingAnalyzer),
      typeof(CachingAnalyzer),
      typeof(MediaInfoAnalyzer));

    Unwrap(analyzer).Should().NotContain(typeof(ThrottlingAnalyzer));
  }

  [Fact]
  public async Task Build_WithCaching_ReadsTheSameFileOnlyOnce()
  {
    var (factory, fileSystem) = CreateMedia();
    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false)
      .WithCaching()
      .Build();

    await analyzer.AnalyzeAsync(new FileMediaSource(Movie));
    await analyzer.AnalyzeAsync(new FileMediaSource(Movie));

    factory.OpenedPaths.Should().ContainSingle();
  }

  [Fact]
  public async Task Build_WithExternalSubtitles_ReportsThem()
  {
    var (factory, fileSystem) = CreateMedia();
    fileSystem.AddFile(@"D:\media\movie.srt");

    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false)
      .WithExternalSubtitles()
      .Build();

    var result = await analyzer.AnalyzeAsync(new FileMediaSource(Movie));

    result.HasExternalSubtitles.Should().BeTrue();
  }

  [Fact]
  public async Task Build_WithValidation_RejectsAnEmptyPathBeforeCreatingAHandle()
  {
    var (factory, fileSystem) = CreateMedia();
    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .WithValidation()
      .Build();

    var result = await analyzer.AnalyzeAsync(new FileMediaSource("   "));

    result.Failure!.Reason.Should().Be(AnalysisFailureReason.SourceNotSpecified);
    factory.Created.Should().BeEmpty();
  }

  [Fact]
  public void Build_UsesOnlyTheStrategiesThatWereAdded()
  {
    var only = new SingleFileAnalysisStrategy();
    var analyzer = (MediaInfoAnalyzer)CreateBuilder().AddStrategy(only).Build();

    analyzer.Should().NotBeNull();
  }

  [Fact]
  public async Task Build_AppliesTheConfiguredOptions()
  {
    var (factory, fileSystem) = CreateMedia();
    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options =>
      {
        options.OffloadBlockingCalls = false;
        options.BufferSize = 4096;
      })
      .Build();

    (await analyzer.AnalyzeAsync(new FileMediaSource(Movie))).Success.Should().BeTrue();
  }

  [Fact]
  public async Task Build_ReportsProgressToTheConfiguredReceiver()
  {
    var (factory, fileSystem) = CreateMedia();
    var progress = new ProgressRecorder();

    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false)
      .UseProgress(progress)
      .Build();

    await analyzer.AnalyzeAsync(new FileMediaSource(Movie));

    progress.Reports.Should().Contain(x => x.Phase == AnalysisPhase.Completed);
  }

  [Fact]
  public void Build_RejectsANullDependency()
  {
    var builder = MediaInfoAnalyzerBuilder.Create();

    ((Action)(() => builder.UseFileSystem(null!))).Should().Throw<ArgumentNullException>();
    ((Action)(() => builder.UseNativeFactory(null!))).Should().Throw<ArgumentNullException>();
    ((Action)(() => builder.UseLogger(null!))).Should().Throw<ArgumentNullException>();
    ((Action)(() => builder.AddStrategy(null!))).Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public async Task AddMediaInfoAnalyzer_RegistersASingletonThatWorks()
  {
    var (factory, fileSystem) = CreateMedia();
    var services = new ServiceCollection();

    services.AddMediaInfoAnalyzer(builder => builder
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false));

    await using var provider = services.BuildServiceProvider();
    var analyzer = provider.GetRequiredService<IMediaInfoAnalyzer>();

    analyzer.Should().BeSameAs(provider.GetRequiredService<IMediaInfoAnalyzer>());
    (await analyzer.AnalyzeAsync(new FileMediaSource(Movie))).Success.Should().BeTrue();
  }

  [Fact]
  public void AddMediaInfoAnalyzer_UsesTheLoggerFromTheContainerWhenOneIsRegistered()
  {
    var (factory, fileSystem) = CreateMedia();
    var services = new ServiceCollection();

    services.AddLogging();
    services.AddMediaInfoAnalyzer(builder => builder
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem));

    using var provider = services.BuildServiceProvider();

    Unwrap(provider.GetRequiredService<IMediaInfoAnalyzer>()).Should().Contain(typeof(LoggingAnalyzer));
  }

  [Fact]
  public void AddMediaInfoAnalyzer_WithoutLogging_DoesNotWrapTheAnalyzer()
  {
    var (factory, fileSystem) = CreateMedia();
    var services = new ServiceCollection();

    services.AddMediaInfoAnalyzer(builder => builder
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem));

    using var provider = services.BuildServiceProvider();

    Unwrap(provider.GetRequiredService<IMediaInfoAnalyzer>()).Should().NotContain(typeof(LoggingAnalyzer));
  }

  [Fact]
  public void AddMediaInfoAnalyzer_DoesNotReplaceAnAnalyzerThatIsAlreadyRegistered()
  {
    var mine = new StubAnalyzer();
    var services = new ServiceCollection();

    services.AddSingleton<IMediaInfoAnalyzer>(mine);
    services.AddMediaInfoAnalyzer();

    using var provider = services.BuildServiceProvider();

    provider.GetRequiredService<IMediaInfoAnalyzer>().Should().BeSameAs(mine);
  }

  [Fact]
  public void AddMediaInfoAnalyzer_RejectsAMissingContainer()
  {
    var register = () => ((IServiceCollection)null!).AddMediaInfoAnalyzer();

    register.Should().Throw<ArgumentNullException>();
  }

  private static IReadOnlyList<Type> Unwrap(IMediaInfoAnalyzer analyzer)
  {
    var chain = new List<Type>();
    var current = analyzer;

    while (current is not null)
    {
      chain.Add(current.GetType());
      current = current is MediaInfoAnalyzerDecorator decorator ? InnerOf(decorator) : null;
    }

    return chain;

    static IMediaInfoAnalyzer? InnerOf(MediaInfoAnalyzerDecorator decorator) =>
      (IMediaInfoAnalyzer?)typeof(MediaInfoAnalyzerDecorator)
        .GetProperty("Inner", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
        .GetValue(decorator);
  }
}
