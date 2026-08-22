#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Strategies;
using MediaInfo.Analysis.Tests.Fakes;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for how the duration of a media is read.</summary>
public class DurationReaderTests
{
  private const string Movie = @"D:\media\movie.mkv";

  [Fact]
  public async Task Duration_IsTakenFromTheContainerWhenItKnowsIt()
  {
    var result = await Analyze(profile => profile
      .WithStreams(StreamKind.Video, 1)
      .SetIndexed(StreamKind.General, 0, (int)NativeMethods.General.General_Duration, "5400000"));

    result.General.Duration.Should().Be(TimeSpan.FromMinutes(90));
  }

  [Fact]
  public async Task Duration_FallsBackToTheVideoStreamWhenTheContainerDoesNotKnowIt()
  {
    var result = await Analyze(profile => profile
      .WithStreams(StreamKind.Video, 1)
      .SetIndexed(StreamKind.Video, 0, (int)NativeMethods.Video.Video_Duration, "1800000"));

    result.General.Duration.Should().Be(TimeSpan.FromMinutes(30));
  }

  [Fact]
  public async Task Duration_FallsBackToTheAudioStreamWhenNoVideoStreamKnowsIt()
  {
    var result = await Analyze(profile => profile
      .WithStreams(StreamKind.Audio, 1)
      .SetIndexed(StreamKind.Audio, 0, (int)NativeMethods.Audio.Audio_Duration, "210000"));

    result.General.Duration.Should().Be(TimeSpan.FromMinutes(3.5));
  }

  [Fact]
  public async Task Duration_ThatDoesNotFitInATimeSpan_IsReportedAsUnknown()
  {
    // A transport stream parsed without being able to seek makes the library extrapolate a nonsensical length.
    var absurd = (TimeSpan.MaxValue.TotalMilliseconds * 10).ToString("F0", CultureInfo.InvariantCulture);

    var result = await Analyze(profile => profile
      .WithStreams(StreamKind.Video, 1)
      .SetIndexed(StreamKind.General, 0, (int)NativeMethods.General.General_Duration, absurd));

    result.Success.Should().BeTrue("an unusable duration must not fail the whole analysis");
    result.General.Duration.Should().Be(TimeSpan.Zero);
  }

  [Fact]
  public async Task Duration_ThatIsNotANumber_IsReportedAsUnknown()
  {
    var result = await Analyze(profile => profile
      .WithStreams(StreamKind.Video, 1)
      .SetIndexed(StreamKind.General, 0, (int)NativeMethods.General.General_Duration, "1 h 30 min"));

    result.General.Duration.Should().Be(TimeSpan.Zero);
  }

  [Fact]
  public async Task Duration_OfADiscTitleThatDoesNotFitInATimeSpan_IsReportedAsUnknown()
  {
    var absurd = (TimeSpan.MaxValue.TotalMilliseconds * 10).ToString("F0", CultureInfo.InvariantCulture);
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Movie).SetIndexed(StreamKind.General, 0, (int)NativeMethods.General.General_Duration, absurd);

    var probe = TestContext.Create(factory, new FakeFileSystem()).Probe;
    var duration = await probe.ProbeDurationAsync(
      Movie,
      new MediaAnalysisOptions { OffloadBlockingCalls = false },
      CancellationToken.None);

    duration.Should().Be(TimeSpan.Zero);
  }

  private static async Task<MediaAnalysisResult> Analyze(Func<FakeMediaProfile, FakeMediaProfile> configure)
  {
    var fileSystem = new FakeFileSystem().AddFile(Movie, 4096);
    var factory = new FakeNativeMediaInfoFactory();
    configure(factory.AddMedia(Movie));

    return await new SingleFileAnalysisStrategy().AnalyzeAsync(
      new FileMediaSource(Movie),
      TestContext.Create(factory, fileSystem),
      CancellationToken.None);
  }
}
