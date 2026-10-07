#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Rtsp.Rtsp;
using MediaInfo.Analysis.Rtsp.Sdp;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Strategies;
using MediaInfo.Model;

namespace MediaInfo.Analysis.Rtsp;

/// <summary>
/// Describes a media served over RTSP.
/// </summary>
/// <remarks>
/// The native library cannot open an RTSP location, so this strategy speaks the protocol itself: it describes the
/// session, plays the video track briefly, rebuilds the elementary stream the packets carried and hands that to the
/// ordinary stream analysis. The result is therefore as detailed as it is for a file, because the same parser reads
/// the same bitstream.
/// <para>
/// Audio is taken from the session description rather than played. What the session states about it, the encoding,
/// the sample rate and the channel count, is what an audio track of a live stream can be described by without a
/// second capture and a container to put both tracks in.
/// </para>
/// <para>
/// Its priority is below the strategy that declines live streams, so registering this one is what makes an RTSP
/// location analyzable at all.
/// </para>
/// </remarks>
public sealed class RtspAnalysisStrategy : IMediaAnalysisStrategy
{
  private readonly RtspAnalysisOptions _options;
  private readonly IMediaAnalysisStrategy _streamStrategy;

  /// <summary>
  /// Initializes a new instance of the <see cref="RtspAnalysisStrategy"/> class.
  /// </summary>
  /// <param name="options">How much of a stream to capture, or <see langword="null"/> for the defaults.</param>
  public RtspAnalysisStrategy(RtspAnalysisOptions? options = null)
    : this(options, new StreamAnalysisStrategy())
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="RtspAnalysisStrategy"/> class that hands the captured stream to
  /// the specified strategy.
  /// </summary>
  /// <param name="options">How much of a stream to capture, or <see langword="null"/> for the defaults.</param>
  /// <param name="streamStrategy">The strategy that reads the captured elementary stream.</param>
  /// <exception cref="ArgumentNullException"><paramref name="streamStrategy"/> is <see langword="null"/>.</exception>
  public RtspAnalysisStrategy(RtspAnalysisOptions? options, IMediaAnalysisStrategy streamStrategy)
  {
    _options = options ?? RtspAnalysisOptions.Default;
    _streamStrategy = streamStrategy ?? throw new ArgumentNullException(nameof(streamStrategy));
  }

  /// <summary>
  /// Gets the order the strategy is considered in.
  /// </summary>
  /// <remarks>
  /// The strategy that declines live streams sits at zero and would otherwise claim every RTSP location first, so
  /// this one deliberately outranks it.
  /// </remarks>
  public int Priority => -10;

  /// <inheritdoc />
  public bool CanHandle(IMediaSource source) =>
    source is NetworkMediaSource network && network.Location.IsRtsp();

  /// <inheritdoc />
  public async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    MediaAnalysisContext context,
    CancellationToken cancellationToken)
  {
    var network = (NetworkMediaSource)source;

    // A location without a host parses as an absolute URI but names nothing to connect to.
    if (!Uri.TryCreate(network.Location, UriKind.Absolute, out var uri) || string.IsNullOrEmpty(uri.Host))
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.SourceNotSpecified,
        $"'{network.Location}' is not a usable RTSP location.");
    }

    context.Report(new AnalysisProgress(AnalysisPhase.Opening, Detail: network.Location));

    RtspCapture capture;
    try
    {
      capture = await RtspStreamCapture.CaptureAsync(uri, _options, cancellationToken).ConfigureAwait(false);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      throw;
    }
    catch (Exception exception) when (exception is RtspProtocolException or IOException or SocketException)
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.NativeOpenFailed,
        $"Could not capture '{network.Location}': {exception.Message}",
        exception);
    }

    using (capture)
    {
      if (capture.ElementaryStream.Length == 0)
      {
        return MediaAnalysisResult.Failed(
          source,
          AnalysisFailureReason.NoStreamsFound,
          $"'{network.Location}' sent nothing that could be rebuilt within {_options.CaptureDuration}.");
      }

      context.Report(
        new AnalysisProgress(
          AnalysisPhase.Parsing,
          capture.ElementaryStream.Length,
          Detail: $"Captured {capture.FramesRebuilt} frame(s) in {capture.Elapsed.TotalSeconds:0.0} s."));

      var result = await _streamStrategy
        .AnalyzeAsync(
          new StreamMediaSource(capture.ElementaryStream, leaveOpen: true, network.Location),
          context,
          cancellationToken)
        .ConfigureAwait(false);

      context.Report(new AnalysisProgress(AnalysisPhase.Completed, Detail: network.Location));

      return Describe(result, capture, network);
    }
  }

  private static MediaAnalysisResult Describe(
    MediaAnalysisResult result,
    RtspCapture capture,
    NetworkMediaSource network)
  {
    var audio = new List<AudioStream>(result.AudioStreams);
    if (capture.Session.Audio is { } declared && DescribeAudio(declared, result.VideoStreams.Count) is { } track)
    {
      audio.Add(track);
    }

    return result with
    {
      SourceKind = MediaSourceKind.Network,
      SourcePath = network.Location,
      AnalyzedPath = network.Location,
      AudioStreams = audio,
      BestAudioStream = result.BestAudioStream ?? (audio.Count > 0 ? audio[0] : null),
      General = result.General with
      {
        // A live stream has no length, and the size is what the capture happened to take rather than a property
        // of the media, so neither is reported as if it described the stream.
        Duration = TimeSpan.Zero,
        Size = 0L,
        Format = string.IsNullOrEmpty(result.General.Format) ? "RTP" : result.General.Format
      }
    };
  }

  private static AudioStream? DescribeAudio(SdpMedia declared, int streamNumber)
  {
    if (declared.PrimaryRtpMap is not { } map)
    {
      return null;
    }

    return new AudioStream
    {
      Id = declared.PrimaryPayloadType,
      Name = map.Encoding,
      StreamNumber = streamNumber,
      StreamPosition = 0,
      Format = map.Encoding,
      CodecName = map.Encoding,
      Codec = CodecOf(map.Encoding),
      SamplingRate = map.ClockRate,
      Channel = map.Channels > 0 ? map.Channels : 1
    };
  }

  private static AudioCodec CodecOf(string encoding) =>
    encoding.ToUpperInvariant() switch
    {
      "MPEG4-GENERIC" or "MP4A-LATM" => AudioCodec.Aac,
      // The model has no member for either flavour of G.711, so the encoding survives as the codec name only.
      "PCMA" or "PCMU" => AudioCodec.Undefined,
      "OPUS" => AudioCodec.Opus,
      "MPA" or "MPEG" => AudioCodec.MpegLayer3,
      "L16" => AudioCodec.PcmIntBig,
      _ => AudioCodec.Undefined
    };
}
