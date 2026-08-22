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

/// <summary>Tests for the strategy that feeds a stream to the library block by block.</summary>
public class StreamAnalysisStrategyTests
{
  private static MemoryStream CreateMedia(int size = 200 * 1024) => new(new byte[size], writable: false);

  private static FakeNativeMediaInfoFactory CreateFactory(int finalizeAfterBlocks = int.MaxValue)
  {
    var factory = new FakeNativeMediaInfoFactory { FinalizeAfterBlocks = finalizeAfterBlocks };
    factory.StreamProfile = new FakeMediaProfile()
      .WithStreams(StreamKind.Video, 1)
      .WithStreams(StreamKind.Audio, 1);
    return factory;
  }

  [Fact]
  public async Task AnalyzeAsync_ReadsTheWholeSeekableStreamAndFinalizes()
  {
    var factory = CreateFactory();
    var stream = CreateMedia(200 * 1024);
    var options = new MediaAnalysisOptions { OffloadBlockingCalls = false, BufferSize = 64 * 1024 };

    var result = await new StreamAnalysisStrategy().AnalyzeAsync(
      new StreamMediaSource(stream),
      TestContext.Create(factory, new FakeFileSystem(), options),
      CancellationToken.None);

    result.Success.Should().BeTrue();
    result.SourceKind.Should().Be(MediaSourceKind.Stream);
    factory.BytesSubmitted.Should().Be(200 * 1024);
    factory.BufferFinalizeCalls.Should().Be(1);
    factory.BufferInitCalls.Should().ContainSingle().Which.MediaSize.Should().Be(200 * 1024);
  }

  [Fact]
  public async Task AnalyzeAsync_StopsSubmittingOnceTheLibraryHasWhatItNeeds()
  {
    var factory = CreateFactory(finalizeAfterBlocks: 2);
    var options = new MediaAnalysisOptions { OffloadBlockingCalls = false, BufferSize = 64 * 1024 };

    await new StreamAnalysisStrategy().AnalyzeAsync(
      new StreamMediaSource(CreateMedia(10 * 1024 * 1024)),
      TestContext.Create(factory, new FakeFileSystem(), options),
      CancellationToken.None);

    factory.BufferContinueCalls.Should().Be(2);
    factory.BytesSubmitted.Should().Be(2 * 64 * 1024);
  }

  [Fact]
  public async Task AnalyzeAsync_HonoursTheOffsetTheLibraryAsksFor()
  {
    var factory = CreateFactory(finalizeAfterBlocks: 3);
    factory.RequestedOffset = 128 * 1024;
    var options = new MediaAnalysisOptions { OffloadBlockingCalls = false, BufferSize = 64 * 1024 };

    await new StreamAnalysisStrategy().AnalyzeAsync(
      new StreamMediaSource(CreateMedia(1024 * 1024)),
      TestContext.Create(factory, new FakeFileSystem(), options),
      CancellationToken.None);

    factory.BufferInitCalls.Should().HaveCountGreaterThan(1);
    factory.BufferInitCalls.Skip(1).Should().OnlyContain(x => x.MediaOffset == 128 * 1024);
  }

  [Fact]
  public async Task AnalyzeAsync_OnANonSeekableStream_TellsTheLibraryAndStartsWithAnUnknownSize()
  {
    var factory = CreateFactory(finalizeAfterBlocks: 2);
    var options = new MediaAnalysisOptions { OffloadBlockingCalls = false, BufferSize = 32 * 1024 };

    var result = await new StreamAnalysisStrategy().AnalyzeAsync(
      new StreamMediaSource(new ForwardOnlyStream(CreateMedia(256 * 1024))),
      TestContext.Create(factory, new FakeFileSystem(), options),
      CancellationToken.None);

    result.Success.Should().BeTrue();
    factory.Options.Should().ContainKey("File_IsSeekable").WhoseValue.Should().Be("0");
    factory.BufferInitCalls.Should().ContainSingle().Which.MediaSize.Should().Be(-1L);
  }

  [Fact]
  public async Task AnalyzeAsync_ToleratesShortReads()
  {
    var factory = CreateFactory();
    var options = new MediaAnalysisOptions { OffloadBlockingCalls = false, BufferSize = 64 * 1024 };

    await new StreamAnalysisStrategy().AnalyzeAsync(
      new StreamMediaSource(new ShortReadStream(CreateMedia(100 * 1024), maximumRead: 1000)),
      TestContext.Create(factory, new FakeFileSystem(), options),
      CancellationToken.None);

    factory.BytesSubmitted.Should().Be(100 * 1024);
    factory.BufferContinueCalls.Should().BeGreaterThan(1);
  }

  [Fact]
  public async Task AnalyzeAsync_OnAnEmptyStream_FinalizesWithoutSubmittingAnything()
  {
    var factory = CreateFactory();
    var options = new MediaAnalysisOptions { OffloadBlockingCalls = false };

    var result = await new StreamAnalysisStrategy().AnalyzeAsync(
      new StreamMediaSource(new MemoryStream([])),
      TestContext.Create(factory, new FakeFileSystem(), options),
      CancellationToken.None);

    factory.BytesSubmitted.Should().Be(0);
    factory.BufferFinalizeCalls.Should().Be(1);
    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.NoStreamsFound);
  }

  [Fact]
  public async Task AnalyzeAsync_OnAWriteOnlyStream_Fails()
  {
    var result = await new StreamAnalysisStrategy().AnalyzeAsync(
      new StreamMediaSource(new UnreadableStream()),
      TestContext.Create(CreateFactory(), new FakeFileSystem()),
      CancellationToken.None);

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.StreamNotReadable);
  }

  [Fact]
  public async Task AnalyzeAsync_ObservesCancellationBetweenBlocks()
  {
    var factory = CreateFactory();
    var options = new MediaAnalysisOptions { OffloadBlockingCalls = false, BufferSize = 4 * 1024 };
    using var cancellation = new CancellationTokenSource();

    var context = TestContext.Create(factory, new FakeFileSystem(), options);
    var progress = new CancelAfterBlocks(cancellation, blocks: 3);
    var contextWithProgress = TestContext.Create(factory, new FakeFileSystem(), options, progress);

    var analyze = () => new StreamAnalysisStrategy().AnalyzeAsync(
      new StreamMediaSource(CreateMedia(10 * 1024 * 1024)),
      contextWithProgress,
      cancellation.Token);

    await analyze.Should().ThrowAsync<OperationCanceledException>();
    factory.BufferContinueCalls.Should().BeLessThan(2560, "the pump stopped long before the end of the stream");
    context.Should().NotBeNull();
  }

  [Fact]
  public async Task AnalyzeAsync_LeavesTheStreamOpenOnlyWhenAsked()
  {
    var options = new MediaAnalysisOptions { OffloadBlockingCalls = false };

    var kept = CreateMedia(1024);
    await new StreamAnalysisStrategy().AnalyzeAsync(
      new StreamMediaSource(kept, leaveOpen: true),
      TestContext.Create(CreateFactory(), new FakeFileSystem(), options),
      CancellationToken.None);
    kept.CanRead.Should().BeTrue();

    var closed = CreateMedia(1024);
    await new StreamAnalysisStrategy().AnalyzeAsync(
      new StreamMediaSource(closed, leaveOpen: false),
      TestContext.Create(CreateFactory(), new FakeFileSystem(), options),
      CancellationToken.None);
    closed.CanRead.Should().BeFalse();
  }

  private sealed class CancelAfterBlocks(CancellationTokenSource cancellation, int blocks) : IProgress<AnalysisProgress>
  {
    private int _seen;

    public void Report(AnalysisProgress value)
    {
      if (value.Phase == AnalysisPhase.Parsing && ++_seen >= blocks)
      {
        cancellation.Cancel();
      }
    }
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

  private sealed class ShortReadStream(Stream inner, int maximumRead) : Stream
  {
    public override bool CanRead => true;

    public override bool CanSeek => inner.CanSeek;

    public override bool CanWrite => false;

    public override long Length => inner.Length;

    public override long Position
    {
      get => inner.Position;
      set => inner.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count) =>
      inner.Read(buffer, offset, Math.Min(count, maximumRead));

    public override void Flush() => inner.Flush();

    public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
  }

  private sealed class UnreadableStream : Stream
  {
    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
    }
  }
}
