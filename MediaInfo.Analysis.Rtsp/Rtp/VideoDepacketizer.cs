#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;

namespace MediaInfo.Analysis.Rtsp.Rtp;

/// <summary>
/// Rebuilds an Annex B elementary stream out of the RTP payloads of a video track.
/// </summary>
/// <remarks>
/// H.264 and H.265 packetize video the same way and differ only in how a unit is labelled: both send a unit on its
/// own, aggregate several small ones into a packet, and split a large one across packets. Everything that does not
/// depend on the labelling lives here.
/// <para>
/// A unit whose fragment arrived after a gap in the sequence numbers is dropped along with the gap, because a unit
/// reassembled from an incomplete set of fragments decodes to nothing useful and would only mislead whoever parses
/// the result.
/// </para>
/// </remarks>
public abstract class VideoDepacketizer
{
  private static readonly byte[] AnnexBStartCode = [0x00, 0x00, 0x00, 0x01];

  private readonly MemoryStream _fragment = new();

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
  /// Gets a value indicating whether a unit is currently being reassembled.
  /// </summary>
  protected bool HasFragment { get; private set; }

  /// <summary>
  /// Writes a parameter set that the session description carried, so that the stream starts with what decodes it.
  /// </summary>
  /// <param name="output">The stream the elementary stream is written to.</param>
  /// <param name="parameterSet">The bytes of one network abstraction layer unit.</param>
  public void WriteParameterSet(Stream output, ReadOnlySpan<byte> parameterSet)
  {
    if (!parameterSet.IsEmpty)
    {
      WriteUnit(output, parameterSet);
    }
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

    if (!contiguous && HasFragment)
    {
      DropFragment();
    }

    WritePayload(output, payload, contiguous);
  }

  /// <summary>
  /// Forgets a unit that is still being reassembled, which is what the end of a capture leaves behind.
  /// </summary>
  public void Reset()
  {
    if (HasFragment)
    {
      DropFragment();
    }

    _hasSequenceNumber = false;
  }

  /// <summary>
  /// Writes whatever one payload completes, knowing how this codec labels its units.
  /// </summary>
  /// <param name="output">The stream the elementary stream is written to.</param>
  /// <param name="payload">The payload of one packet.</param>
  /// <param name="contiguous">Whether the packet followed the previous one without a gap.</param>
  protected abstract void WritePayload(Stream output, ReadOnlySpan<byte> payload, bool contiguous);

  /// <summary>
  /// Writes one complete unit, preceded by a start code.
  /// </summary>
  /// <param name="output">The stream the elementary stream is written to.</param>
  /// <param name="unit">The bytes of the unit, header included.</param>
  protected void WriteUnit(Stream output, ReadOnlySpan<byte> unit)
  {
    output.Write(AnnexBStartCode, 0, AnnexBStartCode.Length);

#if NETSTANDARD2_1_OR_GREATER || NET
    output.Write(unit);
#else
    output.Write(unit.ToArray(), 0, unit.Length);
#endif

    UnitsWritten++;
  }

  /// <summary>
  /// Starts reassembling a unit, beginning with the header this codec rebuilt for it.
  /// </summary>
  /// <param name="header">The reconstructed unit header.</param>
  protected void BeginFragment(ReadOnlySpan<byte> header)
  {
    if (HasFragment)
    {
      DropFragment();
    }

    _fragment.SetLength(0);
    Append(header);
    HasFragment = true;
  }

  /// <summary>
  /// Adds the body of a fragment to the unit being reassembled.
  /// </summary>
  /// <param name="body">The fragment, without its headers.</param>
  protected void AppendFragment(ReadOnlySpan<byte> body) => Append(body);

  /// <summary>
  /// Writes the unit that has finished being reassembled.
  /// </summary>
  /// <param name="output">The stream the elementary stream is written to.</param>
  protected void EndFragment(Stream output)
  {
    var buffer = _fragment.GetBuffer();
    WriteUnit(output, new ReadOnlySpan<byte>(buffer, 0, (int)_fragment.Length));

    HasFragment = false;
    _fragment.SetLength(0);
  }

  /// <summary>
  /// Forgets the unit being reassembled and counts it as lost.
  /// </summary>
  protected void DropFragment()
  {
    HasFragment = false;
    _fragment.SetLength(0);
    UnitsDropped++;
  }

  private void Append(ReadOnlySpan<byte> bytes)
  {
#if NETSTANDARD2_1_OR_GREATER || NET
    _fragment.Write(bytes);
#else
    _fragment.Write(bytes.ToArray(), 0, bytes.Length);
#endif
  }
}
