#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Native;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;

namespace MediaInfo.Analysis.Strategies;

/// <summary>
/// Analyzes a media held by a stream by feeding it to the native library block by block.
/// </summary>
/// <remarks>
/// This is the only path of the pipeline that is asynchronous end to end. Every block is read with
/// <see cref="Stream.ReadAsync(Memory{byte}, CancellationToken)"/>, so the analysis releases its thread while it waits
/// for data and observes cancellation between blocks.
/// </remarks>
public sealed class StreamAnalysisStrategy : IMediaAnalysisStrategy
{
  /// <inheritdoc />
  public int Priority => 30;

  /// <inheritdoc />
  public bool CanHandle(IMediaSource source) => source is StreamMediaSource;

  /// <inheritdoc />
  public async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    MediaAnalysisContext context,
    CancellationToken cancellationToken)
  {
    var media = (StreamMediaSource)source;
    var stream = media.Stream;

    if (!stream.CanRead)
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.StreamNotReadable,
        "The media stream cannot be read.");
    }

    using var handle = context.Probe.CreateHandle();
    if (!handle.IsAvailable)
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.NativeLibraryUnavailable,
        "The MediaInfo native library could not be loaded.");
    }

    var version = handle.LibraryVersion;
    var totalBytes = stream.CanSeek ? stream.Length : (long?)null;
    context.Report(new AnalysisProgress(AnalysisPhase.Opening, 0L, totalBytes, media.DisplayName));

    try
    {
      var (processed, limited) = stream.CanSeek
        ? await PumpSeekableAsync(stream, handle, context, cancellationToken).ConfigureAwait(false)
        : await PumpForwardOnlyAsync(stream, handle, context, cancellationToken).ConfigureAwait(false);

      handle.OpenBufferFinalize();

      context.Report(new AnalysisProgress(AnalysisPhase.Collecting, processed, totalBytes, media.DisplayName));
      var result = context.Probe.Collect(handle, totalBytes ?? 0L);

      context.Report(new AnalysisProgress(AnalysisPhase.Completed, processed, totalBytes, media.DisplayName));
      return result with
      {
        SourceKind = MediaSourceKind.Stream,
        SourcePath = media.DisplayName,
        LibraryVersion = version,
        Warnings = limited
          ? [.. result.Warnings, $"The library did not finish within the read limit of {processed:N0} bytes; the result holds what it had by then."]
          : result.Warnings
      };
    }
    finally
    {
      if (!media.LeaveOpen)
      {
        stream.Dispose();
      }
    }
  }

  // The library asks for the parts of a seekable stream it needs, and a damaged or hostile file can make it ask for ever.
  // A parse reads a few blocks, or at worst the stream once, so a few times the length is far more than any parse needs.
  private const long SeekableSlack = 32L * 1024 * 1024;
  private const long SeekableLengthFactor = 4;

  private static long SeekableLimit(MediaAnalysisContext context, long length) =>
    context.Options.MaximumStreamBytes
    ?? (length > (long.MaxValue - SeekableSlack) / SeekableLengthFactor
      ? long.MaxValue
      : (length * SeekableLengthFactor) + SeekableSlack);

  private static async Task<(long Processed, bool Limited)> PumpSeekableAsync(
    Stream stream,
    INativeMediaInfo handle,
    MediaAnalysisContext context,
    CancellationToken cancellationToken)
  {
    var bufferSize = context.Options.BufferSize;
    var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
    var processed = 0L;
    var limited = false;
    var limit = SeekableLimit(context, stream.Length);

    try
    {
      handle.OpenBufferInit(stream.Length, stream.Position);
      while (true)
      {
        cancellationToken.ThrowIfCancellationRequested();

        if (processed >= limit)
        {
          limited = true;
          break;
        }

        var read = await stream.ReadAsync(buffer.AsMemory(0, bufferSize), cancellationToken).ConfigureAwait(false);
        if (read <= 0)
        {
          break;
        }

        processed += read;
        var status = handle.OpenBufferContinue(buffer.AsSpan(0, read));
        if ((status & MediaInfoBufferStatus.Finalized) == MediaInfoBufferStatus.Finalized)
        {
          break;
        }

        context.Report(new AnalysisProgress(AnalysisPhase.Parsing, processed, stream.Length));

        // The library asks to jump ahead once it knows where the next structure it needs lives.
        var requestedOffset = handle.OpenBufferContinueGoToGet();
        if (requestedOffset < 0L)
        {
          continue;
        }

        var offset = stream.Seek(requestedOffset, SeekOrigin.Begin);
        handle.OpenBufferInit(stream.Length, offset);
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(buffer);
    }

    return (processed, limited);
  }

  private static async Task<(long Processed, bool Limited)> PumpForwardOnlyAsync(
    Stream stream,
    INativeMediaInfo handle,
    MediaAnalysisContext context,
    CancellationToken cancellationToken)
  {
    var bufferSize = context.Options.BufferSize;
    var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
    var processed = 0L;
    var limited = false;

    try
    {
      handle.Option("File_IsSeekable", "0");
      handle.OpenBufferInit(-1L, 0L);
      while (true)
      {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.Options.MaximumStreamBytes is { } limit && processed >= limit)
        {
          limited = true;
          break;
        }

        var read = await stream.ReadAsync(buffer.AsMemory(0, bufferSize), cancellationToken).ConfigureAwait(false);
        if (read <= 0)
        {
          break;
        }

        processed += read;
        var status = handle.OpenBufferContinue(buffer.AsSpan(0, read));
        if ((status & MediaInfoBufferStatus.Finalized) == MediaInfoBufferStatus.Finalized)
        {
          break;
        }

        context.Report(new AnalysisProgress(AnalysisPhase.Parsing, processed));
      }
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(buffer);
    }

    return (processed, limited);
  }
}
