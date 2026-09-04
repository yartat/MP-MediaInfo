#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Rtsp.Rtp;
using MediaInfo.Analysis.Rtsp.Sdp;

namespace MediaInfo.Analysis.Rtsp.Rtsp;

/// <summary>
/// What a capture of an RTSP stream produced.
/// </summary>
internal sealed class RtspCapture : IDisposable
{
  /// <summary>Gets or sets the session the server described.</summary>
  public SessionDescription Session { get; set; } = new();

  /// <summary>Gets or sets the elementary stream that was rebuilt from the media packets.</summary>
  public MemoryStream ElementaryStream { get; set; } = new();

  /// <summary>Gets or sets the encoding of the video track, as the session named it.</summary>
  public string? VideoEncoding { get; set; }

  /// <summary>Gets or sets how long media was actually captured for.</summary>
  public TimeSpan Elapsed { get; set; }

  /// <summary>Gets or sets the number of media packets that were read.</summary>
  public int PacketsRead { get; set; }

  /// <summary>Gets or sets the number of complete access units that were rebuilt.</summary>
  public int FramesRebuilt { get; set; }

  /// <summary>Gets or sets the number of access units that were dropped because a fragment went missing.</summary>
  public int FramesDropped { get; set; }

  /// <inheritdoc />
  public void Dispose() => ElementaryStream.Dispose();
}

/// <summary>
/// Connects to an RTSP stream, plays it briefly and rebuilds what it carried into an elementary stream.
/// </summary>
/// <remarks>
/// Only the video track is set up. The audio track is described from the session description rather than played,
/// because an elementary audio stream would need a second capture and a container to be described alongside video,
/// and the session already states its encoding, sample rate and channel count.
/// </remarks>
internal static class RtspStreamCapture
{
  private const byte MediaChannel = 0;

  /// <summary>
  /// Captures a stream.
  /// </summary>
  /// <param name="uri">The location of the stream.</param>
  /// <param name="options">How much to capture.</param>
  /// <param name="cancellationToken">The token that abandons the capture.</param>
  /// <returns>Returns what the capture produced.</returns>
  /// <exception cref="RtspProtocolException">The server refused a request or does not offer a usable track.</exception>
  public static async Task<RtspCapture> CaptureAsync(
    Uri uri,
    RtspAnalysisOptions options,
    CancellationToken cancellationToken)
  {
    var credentials = options.Credentials ?? CredentialsFrom(uri);
    using var connection = new RtspConnection(uri, credentials, options.UserAgent, options.ReceiveTimeout);

    await connection.ConnectAsync(options.ConnectTimeout, cancellationToken).ConfigureAwait(false);

    var describe = await connection
      .SendAsync("DESCRIBE", connection.ControlUri, [new("Accept", "application/sdp")], cancellationToken)
      .ConfigureAwait(false);

    if (!describe.IsSuccess)
    {
      throw new RtspProtocolException(
        $"The server answered DESCRIBE with {describe.StatusCode} {describe.ReasonPhrase}.");
    }

    var session = SessionDescription.Parse(describe.Body);
    var video = session.Video
      ?? throw new RtspProtocolException("The session offers no video track to describe.");

    var capture = new RtspCapture
    {
      Session = session,
      VideoEncoding = video.PrimaryRtpMap?.Encoding
    };

    var baseUri = describe.Header("Content-Base") ?? describe.Header("Content-Location") ?? connection.ControlUri;
    var trackUri = ResolveControlUri(baseUri, video.Control);

    var setup = await connection
      .SendAsync(
        "SETUP",
        trackUri,
        [new("Transport", $"RTP/AVP/TCP;unicast;interleaved={MediaChannel}-{MediaChannel + 1}")],
        cancellationToken)
      .ConfigureAwait(false);

    if (!setup.IsSuccess)
    {
      throw new RtspProtocolException($"The server answered SETUP with {setup.StatusCode} {setup.ReasonPhrase}.");
    }

    var play = await connection
      .SendAsync("PLAY", connection.ControlUri, [new("Range", "npt=0.000-")], cancellationToken)
      .ConfigureAwait(false);

    if (!play.IsSuccess)
    {
      throw new RtspProtocolException($"The server answered PLAY with {play.StatusCode} {play.ReasonPhrase}.");
    }

    try
    {
      await ReadAsync(connection, video, capture, options, cancellationToken).ConfigureAwait(false);
    }
    finally
    {
      await TearDownAsync(connection, cancellationToken).ConfigureAwait(false);
    }

    capture.ElementaryStream.Position = 0;
    return capture;
  }

  private static async Task ReadAsync(
    RtspConnection connection,
    SdpMedia video,
    RtspCapture capture,
    RtspAnalysisOptions options,
    CancellationToken cancellationToken)
  {
    var depacketizer = new H264Depacketizer();
    var isH264 = string.Equals(capture.VideoEncoding, "H264", StringComparison.OrdinalIgnoreCase);

    if (!isH264)
    {
      throw new RtspProtocolException(
        $"The video track is {capture.VideoEncoding ?? "of an unnamed encoding"}, and only H264 is rebuilt today.");
    }

    // The session usually carries the parameter sets, so the stream starts with what decodes it even when the
    // capture happens to begin between key frames.
    foreach (var parameterSet in ParameterSetsOf(video))
    {
      depacketizer.WriteParameterSet(capture.ElementaryStream, parameterSet);
    }

    var payloadType = video.PrimaryPayloadType;
    var clock = Stopwatch.StartNew();

    using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    deadline.CancelAfter(options.CaptureDuration);

    try
    {
      while (true)
      {
        if (capture.ElementaryStream.Length >= options.MaximumCaptureBytes)
        {
          break;
        }

        if (options.SufficientFrameCount > 0 && capture.FramesRebuilt >= options.SufficientFrameCount)
        {
          break;
        }

        var block = await connection.ReadBlockAsync(deadline.Token).ConfigureAwait(false);
        if (block is not { } media)
        {
          break;
        }

        if (media.Channel != MediaChannel || !RtpPacket.TryParse(media.Data, out var packet))
        {
          continue;
        }

        if (payloadType >= 0 && packet.PayloadType != payloadType)
        {
          continue;
        }

        capture.PacketsRead++;
        depacketizer.Write(capture.ElementaryStream, packet);

        // The marker bit ends an access unit, which is the unit a frame count is meaningful in.
        if (packet.Marker)
        {
          capture.FramesRebuilt++;
        }
      }
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
      // The capture window closed, which is how a capture of a live stream normally ends.
    }

    depacketizer.Reset();

    clock.Stop();
    capture.Elapsed = clock.Elapsed;
    capture.FramesDropped = depacketizer.UnitsDropped;
  }

  private static async Task TearDownAsync(RtspConnection connection, CancellationToken cancellationToken)
  {
    try
    {
      // Best effort: a server that has already gone away does not make the capture any less valid.
      using var brief = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      brief.CancelAfter(TimeSpan.FromSeconds(1));
      await connection.SendAsync("TEARDOWN", connection.ControlUri, null, brief.Token).ConfigureAwait(false);
    }
    catch (Exception exception) when (exception is IOException or OperationCanceledException or SocketException)
    {
    }
  }

  internal static IEnumerable<byte[]> ParameterSetsOf(SdpMedia video)
  {
    var sets = video.GetFormatParameter("sprop-parameter-sets");
    if (string.IsNullOrEmpty(sets))
    {
      yield break;
    }

    foreach (var encoded in sets!.Split(','))
    {
      var trimmed = encoded.Trim();
      if (trimmed.Length == 0)
      {
        continue;
      }

      byte[] decoded;
      try
      {
        decoded = Convert.FromBase64String(trimmed);
      }
      catch (FormatException)
      {
        continue;
      }

      if (decoded.Length > 0)
      {
        yield return decoded;
      }
    }
  }

  internal static string ResolveControlUri(string baseUri, string? control)
  {
    if (string.IsNullOrEmpty(control) || control == "*")
    {
      return baseUri;
    }

    if (Uri.TryCreate(control, UriKind.Absolute, out var absolute))
    {
      return absolute.ToString();
    }

    return baseUri.EndsWith("/", StringComparison.Ordinal)
      ? baseUri + control
      : baseUri + "/" + control;
  }

  private static NetworkCredential? CredentialsFrom(Uri uri)
  {
    if (string.IsNullOrEmpty(uri.UserInfo))
    {
      return null;
    }

    var separator = uri.UserInfo.IndexOf(':');
    return separator < 0
      ? new NetworkCredential(Uri.UnescapeDataString(uri.UserInfo), string.Empty)
      : new NetworkCredential(
        Uri.UnescapeDataString(uri.UserInfo.Substring(0, separator)),
        Uri.UnescapeDataString(uri.UserInfo.Substring(separator + 1)));
  }
}
