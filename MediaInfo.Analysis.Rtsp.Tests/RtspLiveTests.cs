#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using MediaInfo.Model;
using Xunit;
using Xunit.Abstractions;

namespace MediaInfo.Analysis.Rtsp.Tests;

/// <summary>
/// Names the stream the live tests run against.
/// </summary>
public static class LiveStream
{
  /// <summary>The environment variable that points the tests at a stream.</summary>
  public const string UrlVariable = "MP_MEDIAINFO_RTSP_URL";

  /// <summary>Gets the stream to analyze.</summary>
  public static string Url =>
    Environment.GetEnvironmentVariable(UrlVariable) is { Length: > 0 } configured
      ? configured
      : "rtsp://localhost:8554/test";

  /// <summary>Gets a value indicating whether something is listening where the stream should be.</summary>
  public static bool IsAvailable
  {
    get
    {
      if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
      {
        return false;
      }

      return RtspStrategyTests.IsServerListening(uri.Host, uri.IsDefaultPort || uri.Port <= 0 ? 554 : uri.Port);
    }
  }
}

/// <summary>
/// A fact that is skipped unless an RTSP stream is reachable.
/// </summary>
public sealed class RtspFactAttribute : FactAttribute
{
  /// <summary>Initializes a new instance of the <see cref="RtspFactAttribute"/> class.</summary>
  public RtspFactAttribute()
  {
    if (!LiveStream.IsAvailable)
    {
      Skip =
        $"No RTSP stream at {LiveStream.Url}. Start one and set {LiveStream.UrlVariable}, for example with " +
        "docker run --rm -p 8554:8554 bluenviron/mediamtx:latest-ffmpeg publishing a file of the test corpus.";
    }
  }
}

/// <summary>
/// Describes a real RTSP stream end to end.
/// </summary>
/// <remarks>
/// The protocol work is covered without a network by the unit tests. What only a live stream can show is whether a
/// server that was written by somebody else lays its session description out the way the specification says, and
/// whether the elementary stream rebuilt from its packets is one the native library can actually read.
/// </remarks>
[Collection("rtsp-live")]
public class RtspLiveTests(ITestOutputHelper output)
{
  private static IMediaInfoAnalyzer CreateAnalyzer(Action<RtspAnalysisOptions>? configure = null) =>
    MediaInfoAnalyzerBuilder.Create()
      .WithRtsp(options =>
      {
        options.CaptureDuration = TimeSpan.FromSeconds(5);
        configure?.Invoke(options);
      })
      .Build();

  [RtspFact]
  public async Task Analyze_DescribesTheVideoAsFullyAsItWouldAFile()
  {
    var result = await CreateAnalyzer().AnalyzeAsync(LiveStream.Url);

    output.WriteLine($"{result.General.Format} in {result.Elapsed.TotalMilliseconds:N0} ms");
    foreach (var video in result.VideoStreams)
    {
      output.WriteLine($"  video: {video.CodecName}, {video.Width}x{video.Height}, {video.FrameRate:0.###} fps, {video.BitDepth} bit");
    }

    foreach (var audio in result.AudioStreams)
    {
      output.WriteLine($"  audio: {audio.CodecName}, {audio.Channel} ch, {audio.SamplingRate} Hz");
    }

    using var _ = new AssertionScope();

    result.Success.Should().BeTrue(result.Failure?.ToString() ?? string.Empty);
    result.SourceKind.Should().Be(MediaSourceKind.Network);
    result.SourcePath.Should().Be(LiveStream.Url);

    var stream = result.VideoStreams.Should().ContainSingle().Subject;
    stream.Codec.Should().BeOneOf(
      [VideoCodec.Mpeg4IsoAvc, VideoCodec.MpeghIsoHevc],
      "the rebuilt elementary stream is read by the same parser a file is, whichever of the two it carries");
    stream.Width.Should().BeGreaterThan(0);
    stream.Height.Should().BeGreaterThan(0);
    stream.FrameRate.Should().BeGreaterThan(0);
  }

  [RtspFact]
  public async Task Analyze_ReportsNeitherALengthNorASizeForALiveStream()
  {
    var result = await CreateAnalyzer().AnalyzeAsync(LiveStream.Url);

    using var _ = new AssertionScope();

    result.General.Duration.Should().Be(TimeSpan.Zero, "a live stream has no length");
    result.General.Size.Should().Be(0L, "the capture size is not a property of the stream");
  }

  [RtspFact]
  public async Task Analyze_DescribesTheAudioTrackTheSessionDeclares()
  {
    var result = await CreateAnalyzer().AnalyzeAsync(LiveStream.Url);

    // The audio comes from the session description rather than from the packets, so it is present whenever the
    // stream declares a track, even though only the video track is played.
    if (result.AudioStreams.Count == 0)
    {
      output.WriteLine("The stream declares no audio track, so there is nothing to describe.");
      return;
    }

    var audio = result.AudioStreams[0];
    audio.CodecName.Should().NotBeNullOrEmpty();
    audio.SamplingRate.Should().BeGreaterThan(0);
    audio.Channel.Should().BeGreaterThan(0);
  }

  [RtspFact]
  public async Task Analyze_StopsEarlyOnceItHasSeenEnoughFrames()
  {
    // One capture, compared against its own window rather than against a second capture: a stream that is
    // published on demand does not always have a publisher ready for a connection that arrives right after a
    // teardown, and that has nothing to do with what this test is about.
    var result = await CreateAnalyzer(o => o.SufficientFrameCount = 5).AnalyzeAsync(LiveStream.Url);

    output.WriteLine($"five frames in {result.Elapsed.TotalMilliseconds:N0} ms of a 5,000 ms window");

    using var _ = new AssertionScope();

    result.Success.Should().BeTrue(result.Failure?.ToString() ?? string.Empty);
    result.Elapsed.Should().BeLessThan(
      TimeSpan.FromSeconds(4),
      "a five frame budget ends the capture well before the window does");
  }

  [RtspFact]
  public async Task Analyze_ObservesCancellationWhileCapturing()
  {
    using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

    var analyze = () => CreateAnalyzer(o => o.SufficientFrameCount = 0)
      .AnalyzeAsync(MediaSource.From(LiveStream.Url), cancellation.Token);

    await analyze.Should().ThrowAsync<OperationCanceledException>();
  }

  [RtspFact]
  public async Task Analyze_OfAPathTheServerDoesNotServeFailsWithTheAnswerItGave()
  {
    var uri = new Uri(LiveStream.Url);
    var missing = new UriBuilder(uri) { Path = "/no-such-path" }.Uri.ToString();

    var result = await CreateAnalyzer().AnalyzeAsync(missing);

    output.WriteLine(result.Failure?.Message);

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.NativeOpenFailed);
  }
}
