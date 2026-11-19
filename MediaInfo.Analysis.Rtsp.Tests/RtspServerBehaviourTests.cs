#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Native;
using MediaInfo.Analysis.Pipeline;
using MediaInfo.Analysis.Rtsp.Rtp;
using MediaInfo.Analysis.Rtsp.Rtsp;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Strategies;
using MediaInfo.Analysis.Strategies.Selection;
using Xunit;

namespace MediaInfo.Analysis.Rtsp.Tests;

/// <summary>Tests for a server that is slow, silent, verbose or leaves in the middle of a capture.</summary>
public sealed class RtspServerBehaviourTests : IDisposable
{
  private const string Description =
    "v=0\r\no=- 0 0 IN IP4 127.0.0.1\r\ns=test\r\nt=0 0\r\n" +
    "m=video 0 RTP/AVP 96\r\na=rtpmap:96 H264/90000\r\na=control:track1\r\n";

  private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
  private readonly CancellationTokenSource _lifetime = new(TimeSpan.FromSeconds(20));
  private Task _server = Task.CompletedTask;

  public RtspServerBehaviourTests() => _listener.Start();

  private Uri Location => new($"rtsp://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/stream");

  public void Dispose()
  {
    _lifetime.Cancel();
    _listener.Stop();
    try
    {
      _server.Wait(TimeSpan.FromSeconds(5));
    }
    catch (AggregateException)
    {
    }

    _lifetime.Dispose();
  }

  private void Serve(Func<NetworkStream, Task> script) =>
    _server = Task.Run(async () =>
    {
      using var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
      await script(client.GetStream()).ConfigureAwait(false);
    });

  private static async Task<string> ReadRequestAsync(NetworkStream stream)
  {
    var text = new StringBuilder();
    var buffer = new byte[1];
    while (!text.ToString().EndsWith("\r\n\r\n", StringComparison.Ordinal))
    {
      if (await stream.ReadAsync(buffer, 0, 1).ConfigureAwait(false) == 0)
      {
        break;
      }

      text.Append((char)buffer[0]);
    }

    return text.ToString();
  }

  private static Task SendAsync(NetworkStream stream, string text)
  {
    var bytes = Encoding.ASCII.GetBytes(text);
    return stream.WriteAsync(bytes, 0, bytes.Length);
  }

  private static byte[] Interleaved(ushort sequence, params byte[] payload)
  {
    var packet = new byte[12 + payload.Length];
    packet[0] = 0x80;
    packet[1] = 0x80 | 96;
    packet[2] = (byte)(sequence >> 8);
    packet[3] = (byte)sequence;
    Array.Copy(payload, 0, packet, 12, payload.Length);

    var framed = new byte[4 + packet.Length];
    framed[0] = (byte)'$';
    framed[1] = 0;
    framed[2] = (byte)(packet.Length >> 8);
    framed[3] = (byte)packet.Length;
    Array.Copy(packet, 0, framed, 4, packet.Length);
    return framed;
  }

  private static async Task AnswerSessionSetupAsync(NetworkStream stream)
  {
    await ReadRequestAsync(stream).ConfigureAwait(false);
    await SendAsync(
      stream,
      $"RTSP/1.0 200 OK\r\nCSeq: 1\r\nContent-Type: application/sdp\r\nContent-Length: {Description.Length}\r\n\r\n{Description}")
      .ConfigureAwait(false);

    await ReadRequestAsync(stream).ConfigureAwait(false);
    await SendAsync(stream, "RTSP/1.0 200 OK\r\nCSeq: 2\r\nSession: 1;timeout=60\r\n\r\n").ConfigureAwait(false);

    await ReadRequestAsync(stream).ConfigureAwait(false);
    await SendAsync(stream, "RTSP/1.0 200 OK\r\nCSeq: 3\r\nSession: 1\r\n\r\n").ConfigureAwait(false);
  }

  private Task<RtspCapture> CaptureAsync(TimeSpan receiveTimeout) =>
    RtspStreamCapture.CaptureAsync(
      Location,
      new RtspAnalysisOptions { ReceiveTimeout = receiveTimeout },
      _lifetime.Token);

  [Fact]
  public async Task Capture_ServerStaysSilent_FailsAfterTheReceiveTimeout()
  {
    Serve(async stream =>
    {
      await ReadRequestAsync(stream).ConfigureAwait(false);
      await Task.Delay(TimeSpan.FromSeconds(10), _lifetime.Token).ConfigureAwait(false);
    });
    var clock = Stopwatch.StartNew();

    var act = () => CaptureAsync(TimeSpan.FromMilliseconds(300));

    await act.Should().ThrowAsync<RtspProtocolException>().WithMessage("*did not answer DESCRIBE*");
    clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
  }

  [Fact]
  public async Task Capture_ServerAnswersOneByteAtATime_FailsWhenTheResponseTakesTooLong()
  {
    Serve(async stream =>
    {
      await ReadRequestAsync(stream).ConfigureAwait(false);

      // Every byte arrives well inside the receive timeout, so no single read is slow.
      foreach (var character in "RTSP/1.0 200 OK\r\nCSeq: 1\r\n\r\n")
      {
        await SendAsync(stream, character.ToString()).ConfigureAwait(false);
        await Task.Delay(150, _lifetime.Token).ConfigureAwait(false);
      }
    });
    var clock = Stopwatch.StartNew();

    var act = () => CaptureAsync(TimeSpan.FromMilliseconds(300));

    await act.Should().ThrowAsync<RtspProtocolException>().WithMessage("*did not finish answering*");
    clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
  }

  [Fact]
  public async Task Capture_ServerSendsFarTooManyHeaders_IsAProtocolError()
  {
    Serve(async stream =>
    {
      await ReadRequestAsync(stream).ConfigureAwait(false);
      var response = new StringBuilder("RTSP/1.0 200 OK\r\n");
      for (var i = 0; i < 500; i++)
      {
        response.Append("X-Header-").Append(i).Append(": value\r\n");
      }

      await SendAsync(stream, response.Append("\r\n").ToString()).ConfigureAwait(false);
    });

    var act = () => CaptureAsync(TimeSpan.FromSeconds(5));

    await act.Should().ThrowAsync<RtspProtocolException>().WithMessage("*more header data*");
  }

  [Fact]
  public async Task Capture_ServerLeavesAfterTheMedia_KeepsWhatWasCaptured()
  {
    Serve(async stream =>
    {
      await AnswerSessionSetupAsync(stream).ConfigureAwait(false);
      for (ushort i = 0; i < 3; i++)
      {
        var framed = Interleaved(i, 0x41, 0x9A, 0x00, 0x01);
        await stream.WriteAsync(framed, 0, framed.Length).ConfigureAwait(false);
      }

      // The connection closes, so the TEARDOWN that follows the capture has nobody to answer it.
    });

    using var capture = await CaptureAsync(TimeSpan.FromSeconds(5));

    capture.ElementaryStream.Length.Should().BeGreaterThan(0);
    capture.FramesRebuilt.Should().Be(3);
  }

  [Fact]
  public async Task Capture_ServerGoesSilentAfterSomeMedia_EndsTheCaptureWithoutFailing()
  {
    Serve(async stream =>
    {
      await AnswerSessionSetupAsync(stream).ConfigureAwait(false);
      var framed = Interleaved(0, 0x41, 0x9A, 0x00, 0x01);
      await stream.WriteAsync(framed, 0, framed.Length).ConfigureAwait(false);
      await Task.Delay(TimeSpan.FromSeconds(10), _lifetime.Token).ConfigureAwait(false);
    });
    var clock = Stopwatch.StartNew();

    using var capture = await CaptureAsync(TimeSpan.FromMilliseconds(300));

    capture.ElementaryStream.Length.Should().BeGreaterThan(0);
    clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
  }

  [Fact]
  public async Task Strategy_FailureDoesNotRevealTheCredentialsOfTheLocation()
  {
    Serve(async stream =>
    {
      await ReadRequestAsync(stream).ConfigureAwait(false);
      await SendAsync(stream, "RTSP/1.0 404 Not Found\r\nCSeq: 1\r\n\r\n").ConfigureAwait(false);
    });
    var location = $"rtsp://admin:s3cret@127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/stream";
    var context = new MediaAnalysisContext(
      new MediaProbe(MediaInfoLibFactory.Instance, DefaultStreamSelectionStrategy.Instance),
      MediaInfoFileSystem.Instance);

    var result = await new RtspAnalysisStrategy()
      .AnalyzeAsync(new NetworkMediaSource(location), context, _lifetime.Token);

    result.Success.Should().BeFalse();
    result.Failure!.Message.Should().NotContain("s3cret").And.Contain("***@127.0.0.1");
    result.SourcePath.Should().NotContain("s3cret");
  }

  #region Fragment limit

  private static void Send(VideoDepacketizer depacketizer, Stream output, ushort sequence, params byte[] payload)
  {
    var packet = new byte[12 + payload.Length];
    packet[0] = 0x80;
    packet[1] = 96;
    packet[2] = (byte)(sequence >> 8);
    packet[3] = (byte)sequence;
    Array.Copy(payload, 0, packet, 12, payload.Length);

    RtpPacket.TryParse(packet, out var parsed).Should().BeTrue();
    depacketizer.Write(output, in parsed);
  }

  private static byte[] FragmentUnit(byte fragmentHeader, int length)
  {
    var payload = new byte[2 + length];
    payload[0] = 0x7C;
    payload[1] = fragmentHeader;
    return payload;
  }

  [Fact]
  public void Fragment_ThatNeverEnds_IsDroppedOnceItIsTooLarge()
  {
    var depacketizer = new H264Depacketizer { MaximumFragmentSize = 100 };
    using var output = new MemoryStream();

    Send(depacketizer, output, 1, FragmentUnit(0x85, 60));
    Send(depacketizer, output, 2, FragmentUnit(0x05, 60));
    Send(depacketizer, output, 3, FragmentUnit(0x05, 60));
    Send(depacketizer, output, 4, FragmentUnit(0x45, 10));

    depacketizer.UnitsDropped.Should().Be(1);
    output.Length.Should().Be(0, "neither the dropped unit nor its end is written");
  }

  [Fact]
  public void Fragment_AfterADroppedOne_IsStillRebuilt()
  {
    var depacketizer = new H264Depacketizer { MaximumFragmentSize = 100 };
    using var output = new MemoryStream();
    Send(depacketizer, output, 1, FragmentUnit(0x85, 120));
    Send(depacketizer, output, 2, FragmentUnit(0x45, 10));

    Send(depacketizer, output, 3, FragmentUnit(0x85, 20));
    Send(depacketizer, output, 4, FragmentUnit(0x45, 20));

    depacketizer.UnitsWritten.Should().Be(1);
    output.ToArray().Should().StartWith(new byte[] { 0, 0, 0, 1, 0x65 });
  }

  [Fact]
  public void Fragment_WithinTheLimit_IsRebuilt()
  {
    var depacketizer = new H264Depacketizer { MaximumFragmentSize = 100 };
    using var output = new MemoryStream();

    Send(depacketizer, output, 1, FragmentUnit(0x85, 40));
    Send(depacketizer, output, 2, FragmentUnit(0x45, 40));

    depacketizer.UnitsDropped.Should().Be(0);
    depacketizer.UnitsWritten.Should().Be(1);
  }

  #endregion
}
