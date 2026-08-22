#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;

namespace MediaInfo.Analysis.Rtsp.Rtp;

/// <summary>
/// One RTP packet, as laid out by RFC 3550.
/// </summary>
/// <remarks>
/// The payload is a window onto the buffer the packet was read from rather than a copy, so it stays valid only for as
/// long as that buffer does.
/// </remarks>
public readonly ref struct RtpPacket
{
  private RtpPacket(int payloadType, ushort sequenceNumber, uint timestamp, uint ssrc, bool marker, ReadOnlySpan<byte> payload)
  {
    PayloadType = payloadType;
    SequenceNumber = sequenceNumber;
    Timestamp = timestamp;
    Ssrc = ssrc;
    Marker = marker;
    Payload = payload;
  }

  /// <summary>Gets the payload type, which the session description maps to an encoding.</summary>
  public int PayloadType { get; }

  /// <summary>Gets the sequence number, which increases by one per packet and wraps at 16 bits.</summary>
  public ushort SequenceNumber { get; }

  /// <summary>Gets the sampling instant of the first byte of the payload, on the clock of the track.</summary>
  public uint Timestamp { get; }

  /// <summary>Gets the synchronisation source that sent the packet.</summary>
  public uint Ssrc { get; }

  /// <summary>Gets a value indicating whether the marker bit is set, which ends an access unit for video.</summary>
  public bool Marker { get; }

  /// <summary>Gets the payload, which is a window onto the buffer the packet was read from.</summary>
  public ReadOnlySpan<byte> Payload { get; }

  /// <summary>
  /// Reads a packet.
  /// </summary>
  /// <param name="data">The bytes of one packet.</param>
  /// <param name="packet">When this method returns <see langword="true"/>, contains the packet.</param>
  /// <returns>Returns <see langword="true"/> when the bytes hold a version 2 packet; otherwise, <see langword="false"/>.</returns>
  public static bool TryParse(ReadOnlySpan<byte> data, out RtpPacket packet)
  {
    packet = default;

    // The fixed header is twelve bytes, then one four byte entry per contributing source.
    if (data.Length < 12 || (data[0] >> 6) != 2)
    {
      return false;
    }

    var hasPadding = (data[0] & 0x20) != 0;
    var hasExtension = (data[0] & 0x10) != 0;
    var contributingSources = data[0] & 0x0F;

    var offset = 12 + (contributingSources * 4);
    if (offset > data.Length)
    {
      return false;
    }

    if (hasExtension)
    {
      if (offset + 4 > data.Length)
      {
        return false;
      }

      // The extension header states its own length in four byte words, after its two byte profile and length.
      var words = (data[offset + 2] << 8) | data[offset + 3];
      offset += 4 + (words * 4);
      if (offset > data.Length)
      {
        return false;
      }
    }

    var end = data.Length;
    if (hasPadding)
    {
      // The last byte of a padded packet counts the padding, itself included.
      var padding = data[end - 1];
      if (padding == 0 || end - padding < offset)
      {
        return false;
      }

      end -= padding;
    }

    packet = new RtpPacket(
      data[1] & 0x7F,
      (ushort)((data[2] << 8) | data[3]),
      (uint)((data[4] << 24) | (data[5] << 16) | (data[6] << 8) | data[7]),
      (uint)((data[8] << 24) | (data[9] << 16) | (data[10] << 8) | data[11]),
      (data[1] & 0x80) != 0,
      data.Slice(offset, end - offset));

    return true;
  }
}
