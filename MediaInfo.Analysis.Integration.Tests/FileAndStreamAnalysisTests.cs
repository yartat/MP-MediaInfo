#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using Xunit;
using Xunit.Abstractions;

namespace MediaInfo.Analysis.Integration.Tests;

/// <summary>
/// Runs the analysis pipeline against the media corpus of the repository using the real native library.
/// </summary>
[Collection(NativeMediaCollection.Name)]
public class FileAndStreamAnalysisTests(ITestOutputHelper output)
{
  public static TheoryData<string> Corpus =>
  [
    "Test_H264.m2ts",
    "Test_H264_AC3.m2ts",
    "Test_H264_Atmos.m2ts",
    "Test_H264_DTS1.m2ts",
    "Test_H264_DTS2.m2ts",
    "Test_MP3Tags.mka",
    "Test_MP3Tags.mp3",
    "Test_MP3Tags_2.mp3",
    "RTL_7_Darts_WK_2014-2013-12-23_1_h263.3gp"
  ];

  [Theory]
  [MemberData(nameof(Corpus))]
  public async Task AnalyzeFile_ReadsTheMedia(string fileName)
  {
    var path = TestMedia.Path(fileName);
    File.Exists(path).Should().BeTrue($"'{path}' must be copied to the test output");

    var result = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(path);

    output.WriteLine($"{fileName}: {result.General.Format}, {result.VideoStreams.Count}v " +
      $"{result.AudioStreams.Count}a {result.Subtitles.Count}s, {result.General.Duration}");

    result.Success.Should().BeTrue(result.Failure?.ToString() ?? string.Empty);
    result.Failure.Should().BeNull();
    result.SourceKind.Should().Be(MediaSourceKind.File);
    result.AnalyzedPath.Should().Be(path);
    result.LibraryVersion.Should().NotBeNullOrEmpty();
    result.General.Size.Should().Be(new FileInfo(path).Length);
    result.General.Format.Should().NotBeNullOrEmpty();
    result.Disc.Should().BeNull();
    (result.HasVideo || result.HasAudio).Should().BeTrue();
  }

  [Theory]
  [MemberData(nameof(Corpus))]
  public async Task AnalyzeSeekableStream_AgreesWithTheFileAnalysis(string fileName)
  {
    var path = TestMedia.Path(fileName);
    var analyzer = MediaInfoAnalyzer.CreateDefault();

    var fromPath = await analyzer.AnalyzeAsync(path);
    await using var stream = File.OpenRead(path);
    var fromStream = await analyzer.AnalyzeAsync(stream);

    fromStream.Success.Should().BeTrue(fromStream.Failure?.ToString() ?? string.Empty);
    fromStream.SourceKind.Should().Be(MediaSourceKind.Stream);
    fromStream.VideoStreams.Should().HaveCount(fromPath.VideoStreams.Count);
    fromStream.AudioStreams.Should().HaveCount(fromPath.AudioStreams.Count);
    fromStream.General.Format.Should().Be(fromPath.General.Format);
    fromStream.General.Size.Should().Be(fromPath.General.Size);
  }

  [Fact]
  public async Task AnalyzeForwardOnlyStream_StillReadsTheMedia()
  {
    var path = TestMedia.Path("Test_H264_DTS1.m2ts");

    await using var file = File.OpenRead(path);
    await using var forwardOnly = new ForwardOnlyStream(file);
    var result = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(forwardOnly);

    result.Success.Should().BeTrue(result.Failure?.ToString() ?? string.Empty);
    result.HasVideo.Should().BeTrue();
  }

  [Fact]
  public async Task AnalyzeStream_ReportsProgressAndClosesTheStreamWhenAsked()
  {
    var path = TestMedia.Path("Test_H264_DTS1.m2ts");
    var progress = new ProgressRecorder();
    var analyzer = MediaInfoAnalyzer.CreateDefault(progress: progress);

    var stream = File.OpenRead(path);
    var result = await analyzer.AnalyzeAsync(stream, leaveOpen: false);

    result.Success.Should().BeTrue();
    stream.CanRead.Should().BeFalse("the analyzer was asked to close the stream");
    progress.Reports.Should().Contain(x => x.Phase == AnalysisPhase.Completed);
  }

  [Fact]
  public async Task AnalyzeMissingFile_FailsWithoutThrowing()
  {
    var result = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(TestMedia.Path("does-not-exist.mkv"));

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.SourceNotFound);
  }

  [Fact]
  public async Task AnalyzeLiveStream_IsDeclinedWithAReason()
  {
    var result = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync("rtsp://localhost:8554/live");

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.UnsupportedSource);
    result.Failure.Message.Should().Contain("RTSP");
  }

  [Fact]
  public async Task AnalyzeStream_ObservesCancellation()
  {
    var path = TestMedia.Path("RTL_7_Darts_WK_2014-2013-12-23_1_h263.3gp");
    using var cancellation = new CancellationTokenSource();
    cancellation.Cancel();

    await using var stream = File.OpenRead(path);
    var analyze = () => MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(
      new StreamMediaSource(stream),
      cancellation.Token);

    await analyze.Should().ThrowAsync<OperationCanceledException>();
  }

  [Fact]
  public async Task RepeatedAnalysis_DoesNotAccumulateManagedMemory()
  {
    var path = TestMedia.Path("Test_H264_DTS1.m2ts");
    var analyzer = MediaInfoAnalyzer.CreateDefault();

    // Warm up so that the baseline is not measured against a cold JIT.
    for (var i = 0; i < 5; i++)
    {
      await analyzer.AnalyzeAsync(path);
    }

    Collect();
    var baseline = GC.GetTotalMemory(false);

    for (var i = 0; i < 100; i++)
    {
      (await analyzer.AnalyzeAsync(path)).Success.Should().BeTrue();
    }

    Collect();
    var growth = GC.GetTotalMemory(false) - baseline;
    output.WriteLine($"Managed growth over 100 analyses: {growth:N0} bytes");

    growth.Should().BeLessThan(
      20L * 1024 * 1024,
      "every handle is disposed as soon as its analysis completes");

    static void Collect()
    {
      for (var i = 0; i < 3; i++)
      {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
      }
    }
  }

  private sealed class ProgressRecorder : IProgress<AnalysisProgress>
  {
    public System.Collections.Generic.List<AnalysisProgress> Reports { get; } = [];

    public void Report(AnalysisProgress value) => Reports.Add(value);
  }

  private sealed class ForwardOnlyStream(Stream inner) : Stream
  {
    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

    public override void Flush() => inner.Flush();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
  }
}
