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
using Xunit;

namespace MediaInfo.Analysis.Rtsp.Tests;

/// <summary>Tests for reading RTP packets and rebuilding an elementary stream out of them.</summary>
public class RtpTests
{
  private static readonly byte[] StartCode = [0x00, 0x00, 0x00, 0x01];

  private static byte[] Packet(
    byte payloadType,
    ushort sequence,
    byte[] payload,
    bool marker = false,
    int contributingSources = 0,
    bool extension = false,
    int padding = 0)
  {
    var header = new List<byte>
    {
      (byte)(0x80 | (padding > 0 ? 0x20 : 0) | (extension ? 0x10 : 0) | contributingSources),
      (byte)((marker ? 0x80 : 0) | payloadType),
      (byte)(sequence >> 8), (byte)sequence,
      0, 0, 0, 1,          // timestamp
      0, 0, 0, 2           // synchronisation source
    };

    header.AddRange(Enumerable.Repeat((byte)0, contributingSources * 4));

    if (extension)
    {
      header.AddRange([0xBE, 0xDE, 0x00, 0x01, 0, 0, 0, 0]);
    }

    header.AddRange(payload);

    if (padding > 0)
    {
      header.AddRange(Enumerable.Repeat((byte)0, padding - 1));
      header.Add((byte)padding);
    }

    return header.ToArray();
  }

  #region Packet parsing

  [Fact]
  public void Parse_ReadsTheHeaderAndTheWholePayload()
  {
    var data = Packet(96, 1234, [0x65, 0xAA, 0xBB], marker: true);

    RtpPacket.TryParse(data, out var packet).Should().BeTrue();
    packet.PayloadType.Should().Be(96);
    packet.SequenceNumber.Should().Be(1234);
    packet.Marker.Should().BeTrue();
    packet.Ssrc.Should().Be(2u);
    packet.Payload.ToArray().Should().Equal(0x65, 0xAA, 0xBB);
  }

  [Fact]
  public void Parse_SkipsContributingSourcesAndTheHeaderExtension()
  {
    var withSources = Packet(96, 1, [0x01, 0x02], contributingSources: 3);
    var withExtension = Packet(96, 1, [0x01, 0x02], extension: true);

    RtpPacket.TryParse(withSources, out var a).Should().BeTrue();
    a.Payload.ToArray().Should().Equal(0x01, 0x02);

    RtpPacket.TryParse(withExtension, out var b).Should().BeTrue();
    b.Payload.ToArray().Should().Equal(0x01, 0x02);
  }

  [Fact]
  public void Parse_LeavesPaddingOutOfThePayload()
  {
    var data = Packet(96, 1, [0x01, 0x02], padding: 4);

    RtpPacket.TryParse(data, out var packet).Should().BeTrue();
    packet.Payload.ToArray().Should().Equal(0x01, 0x02);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(11)]
  public void Parse_RefusesSomethingTooShortToBeAPacket(int length)
  {
    RtpPacket.TryParse(new byte[length], out _).Should().BeFalse();
  }

  [Fact]
  public void Parse_RefusesAVersionItDoesNotKnow()
  {
    var data = Packet(96, 1, [0x01]);
    data[0] = 0x40; // version one

    RtpPacket.TryParse(data, out _).Should().BeFalse();
  }

  [Fact]
  public void Parse_RefusesAPacketWhosePaddingIsLongerThanItself()
  {
    var data = Packet(96, 1, [0x01]);
    data[0] |= 0x20;
    data[^1] = 0xFF;

    RtpPacket.TryParse(data, out _).Should().BeFalse();
  }

  #endregion

  #region Depacketization

  private static byte[] Depacketize(params byte[][] packets)
  {
    var depacketizer = new H264Depacketizer();
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
  public void Depacketize_WritesASingleUnitWithAStartCode()
  {
    var result = Depacketize(Packet(96, 1, [0x65, 0x11, 0x22]));

    result.Should().Equal([.. StartCode, 0x65, 0x11, 0x22]);
  }

  [Fact]
  public void Depacketize_SplitsAnAggregationPacketIntoItsUnits()
  {
    // Type 24, then a two byte length in front of each unit.
    var payload = new byte[] { 24, 0x00, 0x02, 0x67, 0xAA, 0x00, 0x03, 0x68, 0xBB, 0xCC };

    var result = Depacketize(Packet(96, 1, payload));

    result.Should().Equal([.. StartCode, 0x67, 0xAA, .. StartCode, 0x68, 0xBB, 0xCC]);
  }

  [Fact]
  public void Depacketize_RebuildsAUnitThatWasSplitAcrossPackets()
  {
    // Type 28, an indicator byte, then a fragment header whose top bits mark the start and the end.
    var start = new byte[] { 0x7C, 0x85, 0xAA, 0xBB };
    var middle = new byte[] { 0x7C, 0x05, 0xCC };
    var end = new byte[] { 0x7C, 0x45, 0xDD };

    var result = Depacketize(Packet(96, 1, start), Packet(96, 2, middle), Packet(96, 3, end));

    // The unit header is rebuilt from the importance bits of the indicator and the type of the fragment header.
    result.Should().Equal([.. StartCode, 0x65, 0xAA, 0xBB, 0xCC, 0xDD]);
  }

  [Fact]
  public void Depacketize_DropsAUnitWhoseFragmentWentMissing()
  {
    var start = new byte[] { 0x7C, 0x85, 0xAA };
    var end = new byte[] { 0x7C, 0x45, 0xDD };

    // Sequence number three follows one, so the packet in between was lost.
    var result = Depacketize(Packet(96, 1, start), Packet(96, 3, end));

    result.Should().BeEmpty("half a frame decodes to nothing and would only mislead");
  }

  [Fact]
  public void Depacketize_IgnoresAFragmentThatContinuesAUnitItNeverSawTheStartOf()
  {
    var result = Depacketize(Packet(96, 1, [0x7C, 0x05, 0xCC]), Packet(96, 2, [0x7C, 0x45, 0xDD]));

    result.Should().BeEmpty();
  }

  [Fact]
  public void Depacketize_LeavesOutAUnitThatWasStillBeingRebuiltWhenTheCaptureEnded()
  {
    var depacketizer = new H264Depacketizer();
    using var output = new MemoryStream();

    RtpPacket.TryParse(Packet(96, 1, [0x7C, 0x85, 0xAA]), out var packet).Should().BeTrue();
    depacketizer.Write(output, packet);
    depacketizer.Reset();

    output.ToArray().Should().BeEmpty();
    depacketizer.UnitsDropped.Should().Be(1);
  }

  [Fact]
  public void Depacketize_SkipsAPacketizationModeItDoesNotHandle()
  {
    // Type 25 is a multi time aggregation packet, which no server sends in answer to a plain play request.
    var result = Depacketize(Packet(96, 1, [25, 0x00, 0x01, 0x65]));

    result.Should().BeEmpty();
  }

  [Fact]
  public void Depacketize_RefusesAnAggregationPacketWhoseLengthRunsOffTheEnd()
  {
    var result = Depacketize(Packet(96, 1, [24, 0x00, 0x40, 0x67]));

    result.Should().BeEmpty();
  }

  [Fact]
  public void WriteParameterSet_PutsTheSessionParameterSetsInFrontOfTheStream()
  {
    var depacketizer = new H264Depacketizer();
    using var output = new MemoryStream();

    depacketizer.WriteParameterSet(output, new byte[] { 0x67, 0x42 });
    depacketizer.WriteParameterSet(output, new byte[] { 0x68, 0xCE });
    depacketizer.WriteParameterSet(output, ReadOnlySpan<byte>.Empty);

    output.ToArray().Should().Equal([.. StartCode, 0x67, 0x42, .. StartCode, 0x68, 0xCE]);
    depacketizer.UnitsWritten.Should().Be(2);
  }

  #endregion
}
