#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using MediaInfo.Analysis.Rtsp.Rtp;
using MediaInfo.Analysis.Rtsp.Rtsp;
using MediaInfo.Analysis.Rtsp.Sdp;
using Xunit;

namespace MediaInfo.Analysis.Rtsp.Tests;

/// <summary>Tests for rebuilding an H.265 elementary stream out of RTP payloads.</summary>
public class H265Tests
{
  private static readonly byte[] StartCode = [0x00, 0x00, 0x00, 0x01];

  /// <summary>Builds a two byte H.265 unit header for the given type.</summary>
  private static byte[] Header(int type, int layerId = 0, int temporalId = 1) =>
  [
    (byte)(((type & 0x3F) << 1) | ((layerId >> 5) & 0x01)),
    (byte)(((layerId & 0x1F) << 3) | (temporalId & 0x07))
  ];

  private static byte[] Packet(ushort sequence, params byte[][] parts)
  {
    var payload = parts.SelectMany(x => x).ToArray();
    var packet = new List<byte>
    {
      0x80, 96,
      (byte)(sequence >> 8), (byte)sequence,
      0, 0, 0, 1,
      0, 0, 0, 2
    };

    packet.AddRange(payload);
    return packet.ToArray();
  }

  private static byte[] Depacketize(params byte[][] packets)
  {
    var depacketizer = new H265Depacketizer();
    using var output = new MemoryStream();

    foreach (var data in packets)
    {
      RtpPacket.TryParse(data, out var packet).Should().BeTrue();
      depacketizer.Write(output, packet);
    }

    depacketizer.Reset();
    return output.ToArray();
  }

  [Fact]
  public void TypeIsReadFromTheSecondBitOfTheHeader()
  {
    // Type 32 is a video parameter set, 33 a sequence parameter set, 34 a picture parameter set.
    H265Depacketizer.TypeOf(Header(32)).Should().Be(32);
    H265Depacketizer.TypeOf(Header(33)).Should().Be(33);
    H265Depacketizer.TypeOf(Header(19)).Should().Be(19);
  }

  [Fact]
  public void Depacketize_WritesASingleUnitWithAStartCode()
  {
    var unit = Header(19).Concat(new byte[] { 0xAA, 0xBB }).ToArray();

    var result = Depacketize(Packet(1, unit));

    result.Should().Equal([.. StartCode, .. unit]);
  }

  [Fact]
  public void Depacketize_SplitsAnAggregationPacketIntoItsUnits()
  {
    var first = Header(32).Concat(new byte[] { 0x11 }).ToArray();
    var second = Header(33).Concat(new byte[] { 0x22, 0x33 }).ToArray();

    var payload = Header(48)
      .Concat(new byte[] { 0x00, (byte)first.Length }).Concat(first)
      .Concat(new byte[] { 0x00, (byte)second.Length }).Concat(second)
      .ToArray();

    var result = Depacketize(Packet(1, payload));

    result.Should().Equal([.. StartCode, .. first, .. StartCode, .. second]);
  }

  [Fact]
  public void Depacketize_RebuildsAUnitThatWasSplitAcrossPackets()
  {
    // The payload header says type 49, and the fragment header carries the type of the unit being carried.
    var payloadHeader = Header(49, layerId: 0, temporalId: 1);

    var start = payloadHeader.Concat(new byte[] { 0x80 | 19, 0xAA }).ToArray();
    var middle = payloadHeader.Concat(new byte[] { 19, 0xBB }).ToArray();
    var end = payloadHeader.Concat(new byte[] { 0x40 | 19, 0xCC }).ToArray();

    var result = Depacketize(Packet(1, start), Packet(2, middle), Packet(3, end));

    // The rebuilt header carries the type of the fragment with the layer and temporal identifier of the packet.
    result.Should().Equal([.. StartCode, .. Header(19), 0xAA, 0xBB, 0xCC]);
  }

  [Fact]
  public void Depacketize_DropsAUnitWhoseFragmentWentMissing()
  {
    var payloadHeader = Header(49);
    var start = payloadHeader.Concat(new byte[] { 0x80 | 19, 0xAA }).ToArray();
    var end = payloadHeader.Concat(new byte[] { 0x40 | 19, 0xCC }).ToArray();

    var result = Depacketize(Packet(1, start), Packet(3, end));

    result.Should().BeEmpty("half a frame decodes to nothing and would only mislead");
  }

  [Fact]
  public void Depacketize_IgnoresAFragmentThatContinuesAUnitItNeverSawTheStartOf()
  {
    var payloadHeader = Header(49);

    var result = Depacketize(
      Packet(1, payloadHeader.Concat(new byte[] { 19, 0xBB }).ToArray()),
      Packet(2, payloadHeader.Concat(new byte[] { 0x40 | 19, 0xCC }).ToArray()));

    result.Should().BeEmpty();
  }

  [Fact]
  public void Depacketize_SkipsAPayloadShapeItDoesNotHandle()
  {
    // Type 50 wraps a unit together with extra information no parser of ours needs.
    var result = Depacketize(Packet(1, Header(50).Concat(new byte[] { 0x01, 0x02 }).ToArray()));

    result.Should().BeEmpty();
  }

  [Theory]
  [InlineData(1)]
  [InlineData(3)]
  public void Depacketize_RefusesAPayloadTooShortToCarryItsHeaders(int length)
  {
    var result = Depacketize(Packet(1, Header(49).Take(Math.Min(length, 2)).Concat(new byte[Math.Max(0, length - 2)]).ToArray()));

    result.Should().BeEmpty();
  }

  [Theory]
  [InlineData("H265")]
  [InlineData("HEVC")]
  [InlineData("h265")]
  public void CreateDepacketizer_ChoosesTheHevcOneForEitherName(string encoding)
  {
    RtspStreamCapture.CreateDepacketizer(encoding).Should().BeOfType<H265Depacketizer>();
  }

  [Fact]
  public void CreateDepacketizer_ChoosesTheAvcOneForH264AndNothingForAnythingElse()
  {
    RtspStreamCapture.CreateDepacketizer("H264").Should().BeOfType<H264Depacketizer>();
    RtspStreamCapture.CreateDepacketizer("JPEG").Should().BeNull();
    RtspStreamCapture.CreateDepacketizer(null).Should().BeNull();
  }

  [Fact]
  public void ParameterSets_OfAnHevcSessionComeFromThreeSeparateParameters()
  {
    var vps = Convert.ToBase64String(Header(32).Concat(new byte[] { 0x01 }).ToArray());
    var sps = Convert.ToBase64String(Header(33).Concat(new byte[] { 0x02 }).ToArray());
    var pps = Convert.ToBase64String(Header(34).Concat(new byte[] { 0x03 }).ToArray());

    var session = SessionDescription.Parse(
      $"m=video 0 RTP/AVP 96\na=rtpmap:96 H265/90000\na=fmtp:96 sprop-vps={vps};sprop-sps={sps};sprop-pps={pps}");

    var sets = RtspStreamCapture.ParameterSetsOf(session.Video!).ToArray();

    sets.Should().HaveCount(3);
    H265Depacketizer.TypeOf(sets[0]).Should().Be(32, "the video parameter set comes first");
    H265Depacketizer.TypeOf(sets[1]).Should().Be(33);
    H265Depacketizer.TypeOf(sets[2]).Should().Be(34);
  }

  [Fact]
  public void ParameterSets_OfAnHevcSessionThatDeclaresOnlySomeOfThemKeepWhatIsThere()
  {
    var sps = Convert.ToBase64String(Header(33).Concat(new byte[] { 0x02 }).ToArray());

    var session = SessionDescription.Parse(
      $"m=video 0 RTP/AVP 96\na=rtpmap:96 H265/90000\na=fmtp:96 sprop-sps={sps}");

    RtspStreamCapture.ParameterSetsOf(session.Video!).Should().ContainSingle();
  }
}
