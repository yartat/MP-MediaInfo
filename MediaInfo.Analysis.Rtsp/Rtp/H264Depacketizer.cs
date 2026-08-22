#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;

namespace MediaInfo.Analysis.Rtsp.Rtp;

/// <summary>
/// Rebuilds an H.264 elementary stream out of RTP payloads, as laid out by RFC 6184.
/// </summary>
/// <remarks>
/// The unit header is one byte, and its low five bits say what the payload holds: a unit on its own, an aggregation
/// packet that holds several, or a fragmentation unit that carries one across several packets. The interleaved modes
/// are not handled, because no server sends them in answer to a plain play request.
/// </remarks>
public sealed class H264Depacketizer : VideoDepacketizer
{
  private const int SingleUnitLast = 23;
  private const int AggregationPacket = 24;
  private const int FragmentationUnit = 28;

  /// <inheritdoc />
  protected override void WritePayload(Stream output, ReadOnlySpan<byte> payload, bool contiguous)
  {
    var type = payload[0] & 0x1F;

    if (type >= 1 && type <= SingleUnitLast)
    {
      WriteUnit(output, payload);
      return;
    }

    switch (type)
    {
      case AggregationPacket:
        WriteAggregated(output, payload);
        break;

      case FragmentationUnit:
        WriteFragment(output, payload, contiguous);
        break;

      default:
        break;
    }
  }

  private void WriteAggregated(Stream output, ReadOnlySpan<byte> payload)
  {
    // One byte of aggregation header, then a two byte size in front of every unit.
    var offset = 1;
    while (offset + 2 <= payload.Length)
    {
      var size = (payload[offset] << 8) | payload[offset + 1];
      offset += 2;

      if (size <= 0 || offset + size > payload.Length)
      {
        return;
      }

      WriteUnit(output, payload.Slice(offset, size));
      offset += size;
    }
  }

  private void WriteFragment(Stream output, ReadOnlySpan<byte> payload, bool contiguous)
  {
    if (payload.Length < 3)
    {
      return;
    }

    var start = (payload[1] & 0x80) != 0;
    var end = (payload[1] & 0x40) != 0;

    if (start)
    {
      // The unit header is rebuilt from the importance bits of the indicator and the type of the fragment header.
      Span<byte> header = [(byte)((payload[0] & 0xE0) | (payload[1] & 0x1F))];
      BeginFragment(header);
    }
    else if (!HasFragment || !contiguous)
    {
      return;
    }

    AppendFragment(payload.Slice(2));

    if (end)
    {
      EndFragment(output);
    }
  }
}
