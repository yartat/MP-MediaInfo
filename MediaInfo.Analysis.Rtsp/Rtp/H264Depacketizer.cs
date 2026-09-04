#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;

namespace MediaInfo.Analysis.Rtsp.Rtp;

/// <summary>
/// Rebuilds an Annex B elementary stream out of the RTP payloads of an H.264 track, as laid out by RFC 6184.
/// </summary>
/// <remarks>
/// Three packetization modes appear in the wild and all three are handled: a payload that holds one network
/// abstraction layer unit, an aggregation packet that holds several, and a fragmentation unit that carries one unit
/// across several packets. Anything else is skipped rather than corrupting the stream.
/// <para>
/// A fragment that arrives after a gap in the sequence numbers is dropped along with the unit it belongs to, because
/// a unit reassembled from an incomplete set of fragments decodes to nothing useful and would only mislead whoever
/// parses the result.
/// </para>
/// </remarks>
public sealed class H264Depacketizer
{
  private const int SingleNalUnitLast = 23;
  private const int AggregationPacket = 24;
  private const int FragmentationUnit = 28;

  private static readonly byte[] StartCode = [0x00, 0x00, 0x00, 0x01];

  private readonly MemoryStream _fragment = new();

  private bool _hasFragment;
  private byte _fragmentHeader;
  private ushort _expectedSequenceNumber;
  private bool _hasSequenceNumber;

  /// <summary>
  /// Gets the number of network abstraction layer units that were written.
  /// </summary>
  public int UnitsWritten { get; private set; }

  /// <summary>
  /// Gets the number of units that were dropped because a fragment of them went missing.
  /// </summary>
  public int UnitsDropped { get; private set; }

  /// <summary>
  /// Writes a parameter set that the session description carried, so that the stream starts with what decodes it.
  /// </summary>
  /// <param name="output">The stream the elementary stream is written to.</param>
  /// <param name="parameterSet">The bytes of one network abstraction layer unit.</param>
  public void WriteParameterSet(Stream output, ReadOnlySpan<byte> parameterSet)
  {
    if (parameterSet.IsEmpty)
    {
      return;
    }

    WriteUnit(output, parameterSet);
  }

  /// <summary>
  /// Writes whatever the payload of one packet completes.
  /// </summary>
  /// <param name="output">The stream the elementary stream is written to.</param>
  /// <param name="packet">The packet to take the payload from.</param>
  public void Write(Stream output, in RtpPacket packet)
  {
    var payload = packet.Payload;
    if (payload.IsEmpty)
    {
      return;
    }

    var contiguous = !_hasSequenceNumber || packet.SequenceNumber == _expectedSequenceNumber;
    _expectedSequenceNumber = (ushort)(packet.SequenceNumber + 1);
    _hasSequenceNumber = true;

    if (!contiguous && _hasFragment)
    {
      // A fragment of the unit being reassembled was lost, so the whole unit goes.
      DropFragment();
    }

    var type = payload[0] & 0x1F;

    if (type >= 1 && type <= SingleNalUnitLast)
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
        // Interleaved modes and the multi time aggregation packets are not used by any sender that answers a
        // plain PLAY, so they are skipped rather than guessed at.
        break;
    }
  }

  /// <summary>
  /// Forgets a unit that is still being reassembled, which is what the end of a capture leaves behind.
  /// </summary>
  public void Reset()
  {
    if (_hasFragment)
    {
      DropFragment();
    }

    _hasSequenceNumber = false;
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
    var body = payload.Slice(2);

    if (start)
    {
      if (_hasFragment)
      {
        DropFragment();
      }

      // The unit header is rebuilt from the importance bits of the indicator and the type of the fragment header.
      _fragmentHeader = (byte)((payload[0] & 0xE0) | (payload[1] & 0x1F));
      _fragment.SetLength(0);
      _fragment.WriteByte(_fragmentHeader);
      _hasFragment = true;
    }
    else if (!_hasFragment || !contiguous)
    {
      // A fragment that continues a unit we never saw the start of, or that follows a gap, is not usable.
      return;
    }

#if NETSTANDARD2_1_OR_GREATER || NET
    _fragment.Write(body);
#else
    _fragment.Write(body.ToArray(), 0, body.Length);
#endif

    if (!end)
    {
      return;
    }

    var buffer = _fragment.GetBuffer();
    WriteUnit(output, new ReadOnlySpan<byte>(buffer, 0, (int)_fragment.Length));

    _hasFragment = false;
    _fragment.SetLength(0);
  }

  private void DropFragment()
  {
    _hasFragment = false;
    _fragment.SetLength(0);
    UnitsDropped++;
  }

  private void WriteUnit(Stream output, ReadOnlySpan<byte> unit)
  {
    output.Write(StartCode, 0, StartCode.Length);

#if NETSTANDARD2_1_OR_GREATER || NET
    output.Write(unit);
#else
    output.Write(unit.ToArray(), 0, unit.Length);
#endif

    UnitsWritten++;
  }
}
