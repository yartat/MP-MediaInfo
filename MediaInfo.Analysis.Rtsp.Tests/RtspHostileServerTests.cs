#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Rtsp.Rtsp;
using Xunit;

namespace MediaInfo.Analysis.Rtsp.Tests;

/// <summary>Tests for a server that sends what no well behaved server sends.</summary>
public class RtspHostileServerTests
{
  [Theory]
  [InlineData("0", 0)]
  [InlineData("128", 128)]
  [InlineData(" 42 ", 42)]
  [InlineData("1048576", RtspConnection.MaximumBodySize)]
  [InlineData("", 0)]
  [InlineData("abc", 0)]
  [InlineData("-5", 0)]
  public void ParseContentLength_UsableValue_IsReturned(string text, int expected) =>
    RtspConnection.ParseContentLength(text).Should().Be(expected);

  [Theory]
  [InlineData("1048577")]
  [InlineData("2147483647")]
  [InlineData("2147483648")]
  [InlineData("99999999999999999999")]
  public void ParseContentLength_MoreThanTheClientAccepts_IsAProtocolError(string text)
  {
    var act = () => RtspConnection.ParseContentLength(text);

    act.Should().Throw<RtspProtocolException>().WithMessage("*larger than*");
  }

  [Fact]
  public async Task Capture_ServerAnnouncesAnEnormousBody_FailsWithoutAllocatingIt()
  {
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port;

    // The server answers DESCRIBE with a body of two gigabytes, and never sends a byte of it.
    var server = Task.Run(async () =>
    {
      using var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
      var stream = client.GetStream();
      var request = new byte[4096];
      _ = await stream.ReadAsync(request, 0, request.Length).ConfigureAwait(false);
      var answer = Encoding.ASCII.GetBytes("RTSP/1.0 200 OK\r\nCSeq: 1\r\nContent-Length: 2147483647\r\n\r\n");
      await stream.WriteAsync(answer, 0, answer.Length).ConfigureAwait(false);
      await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
    });

    using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    var uri = new Uri($"rtsp://127.0.0.1:{port}/stream");
    var before = GC.GetTotalMemory(forceFullCollection: true);

    var act = () => RtspStreamCapture.CaptureAsync(uri, new RtspAnalysisOptions(), cancellation.Token);

    await act.Should().ThrowAsync<RtspProtocolException>().WithMessage("*larger than*");
    (GC.GetTotalMemory(forceFullCollection: false) - before).Should().BeLessThan(64 * 1024 * 1024);

    listener.Stop();
    await server.ConfigureAwait(false);
  }
}
