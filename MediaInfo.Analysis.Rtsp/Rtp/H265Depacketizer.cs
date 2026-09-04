#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;

namespace MediaInfo.Analysis.Rtsp.Rtp;

/// <summary>
/// Rebuilds an H.265 elementary stream out of RTP payloads, as laid out by RFC 7798.
/// </summary>
/// <remarks>
/// H.265 labels a unit with a two byte header rather than one, and its type is six bits starting at the second bit
/// of the first byte. The three shapes a payload takes are the same as for H.264: a unit on its own, an aggregation
/// packet, and a fragmentation unit.
/// <para>
/// Decoding order numbers are not read. They only appear when the session declares <c>sprop-max-don-diff</c> above
/// zero, which a sender uses to interleave units, and a sender that answers a plain play request does not.
/// </para>
/// </remarks>
public sealed class H265Depacketizer : VideoDepacketizer
{
  private const int SingleUnitLast = 47;
  private const int AggregationPacket = 48;
  private const int FragmentationUnit = 49;

  private const int UnitHeaderSize = 2;

  /// <summary>
  /// Reads the type out of a two byte unit header.
  /// </summary>
  /// <param name="header">The bytes the header starts at.</param>
  /// <returns>Returns the type.</returns>
  internal static int TypeOf(ReadOnlySpan<byte> header) => (header[0] >> 1) & 0x3F;

  /// <inheritdoc />
  protected override void WritePayload(Stream output, ReadOnlySpan<byte> payload, bool contiguous)
  {
    if (payload.Length < UnitHeaderSize)
    {
      return;
    }

    var type = TypeOf(payload);

    if (type <= SingleUnitLast)
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
        // Type 50 wraps a unit together with extra information no parser of ours needs.
        break;
    }
  }

  private void WriteAggregated(Stream output, ReadOnlySpan<byte> payload)
  {
    // Two bytes of payload header, then a two byte size in front of every unit.
    var offset = UnitHeaderSize;
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
    // Two bytes of payload header and one of fragmentation header, then at least one byte of the unit.
    if (payload.Length < 4)
    {
      return;
    }

    var fragmentHeader = payload[2];
    var start = (fragmentHeader & 0x80) != 0;
    var end = (fragmentHeader & 0x40) != 0;

    if (start)
    {
      // The type of the fragment replaces the type of the payload header; the importance bit, the layer and the
      // temporal identifier are what the payload header already carried.
      Span<byte> header =
      [
        (byte)((payload[0] & 0x81) | ((fragmentHeader & 0x3F) << 1)),
        payload[1]
      ];

      BeginFragment(header);
    }
    else if (!HasFragment || !contiguous)
    {
      return;
    }

    AppendFragment(payload.Slice(3));

    if (end)
    {
      EndFragment(output);
    }
  }
}
