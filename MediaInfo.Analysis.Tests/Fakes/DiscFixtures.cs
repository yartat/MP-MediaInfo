#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaInfo.Analysis.Tests.Fakes;

/// <summary>
/// Builds the navigation tables of a DVD and a Blu-ray, byte for byte, so that the parsers can be tested against a
/// layout the test controls rather than against a disc it has to be given.
/// </summary>
internal static class DiscFixtures
{
  private const int SectorSize = 2048;

  /// <summary>
  /// Builds a VIDEO_TS.IFO holding the given titles.
  /// </summary>
  /// <param name="titles">For each title: the title set, the title number within it, its chapters and its angles.</param>
  public static byte[] VideoManager(params (int TitleSet, int TitleInSet, int Chapters, int Angles)[] titles)
  {
    const int TableSector = 1;
    var buffer = new byte[(TableSector * SectorSize) + 8 + (titles.Length * 12) + 16];

    WriteAscii(buffer, 0, "DVDVIDEO-VMG");
    WriteU32(buffer, 0xC4, TableSector);

    var table = TableSector * SectorSize;
    WriteU16(buffer, table, (ushort)titles.Length);
    WriteU32(buffer, table + 4, (uint)(8 + (titles.Length * 12)));

    for (var i = 0; i < titles.Length; i++)
    {
      var entry = table + 8 + (i * 12);
      buffer[entry] = 0x00;
      buffer[entry + 1] = (byte)titles[i].Angles;
      WriteU16(buffer, entry + 2, (ushort)titles[i].Chapters);
      buffer[entry + 6] = (byte)titles[i].TitleSet;
      buffer[entry + 7] = (byte)titles[i].TitleInSet;
    }

    return buffer;
  }

  /// <summary>
  /// Builds a VTS_nn_0.IFO holding one program chain, entered by title one of the set.
  /// </summary>
  /// <param name="duration">The playback time the chain declares.</param>
  /// <param name="chapterCellCounts">How many cells each chapter spans.</param>
  /// <param name="cellDuration">The playback time each cell declares.</param>
  public static byte[] TitleSet(TimeSpan duration, IReadOnlyList<int> chapterCellCounts, TimeSpan cellDuration)
  {
    const int PartOfTitleSector = 1;
    const int ChainTableSector = 2;

    var programs = chapterCellCounts.Count;
    var cells = chapterCellCounts.Sum();

    // The chain body has to be long enough for the fixed header, the program map and the cell playback table.
    var programMapOffset = 0x100;
    var cellPlaybackOffset = programMapOffset + programs + 8;
    var chainLength = cellPlaybackOffset + (cells * 24);

    var chainStart = (ChainTableSector * SectorSize) + 0x100;
    var buffer = new byte[chainStart + chainLength + 16];

    WriteAscii(buffer, 0, "DVDVIDEO-VTS");
    WriteU32(buffer, 0xC8, PartOfTitleSector);
    WriteU32(buffer, 0xCC, ChainTableSector);

    // The part of title table: one title unit, entering chain one.
    var partTable = PartOfTitleSector * SectorSize;
    WriteU16(buffer, partTable, 1);
    WriteU32(buffer, partTable + 8, 16);
    WriteU16(buffer, partTable + 16, 1);
    WriteU16(buffer, partTable + 18, 1);

    // The chain table: one search pointer, aiming at the chain body.
    var chainTable = ChainTableSector * SectorSize;
    WriteU16(buffer, chainTable, 1);
    WriteU32(buffer, chainTable + 4, (uint)(8 + 8));
    buffer[chainTable + 8] = 0x80;
    WriteU32(buffer, chainTable + 8 + 4, (uint)(chainStart - chainTable));

    buffer[chainStart + 2] = (byte)programs;
    buffer[chainStart + 3] = (byte)cells;
    WriteBcdTime(buffer, chainStart + 4, duration);
    WriteU16(buffer, chainStart + 0xE6, (ushort)programMapOffset);
    WriteU16(buffer, chainStart + 0xE8, (ushort)cellPlaybackOffset);

    var cell = 1;
    for (var program = 0; program < programs; program++)
    {
      buffer[chainStart + programMapOffset + program] = (byte)cell;
      cell += chapterCellCounts[program];
    }

    for (var i = 0; i < cells; i++)
    {
      WriteBcdTime(buffer, chainStart + cellPlaybackOffset + (i * 24) + 4, cellDuration);
    }

    return buffer;
  }

  /// <summary>
  /// Builds an MPLS playlist over the given clips, with an entry mark at the start of each one.
  /// </summary>
  /// <param name="clips">For each play item: the clip name and how much of it the item plays.</param>
  /// <param name="withMarks">Whether to write the chapter marks.</param>
  public static byte[] Playlist(IReadOnlyList<(string ClipId, TimeSpan Duration)> clips, bool withMarks = true)
  {
    const int PlayItemBody = 34;
    const int PlayListStart = 64;

    var playListLength = 10 + (clips.Count * (2 + PlayItemBody));
    var markStart = PlayListStart + playListLength;
    var markCount = withMarks ? clips.Count : 0;
    var buffer = new byte[markStart + 6 + (markCount * 14) + 16];

    WriteAscii(buffer, 0, "MPLS");
    WriteAscii(buffer, 4, "0200");
    WriteU32(buffer, 8, (uint)PlayListStart);
    WriteU32(buffer, 12, (uint)markStart);

    WriteU32(buffer, PlayListStart, (uint)(playListLength - 4));
    WriteU16(buffer, PlayListStart + 6, (ushort)clips.Count);

    var offset = PlayListStart + 10;
    var itemOffsets = new int[clips.Count];
    for (var i = 0; i < clips.Count; i++)
    {
      WriteU16(buffer, offset, PlayItemBody);
      var body = offset + 2;
      itemOffsets[i] = body;

      WriteAscii(buffer, body, clips[i].ClipId);
      WriteAscii(buffer, body + 5, "M2TS");
      WriteU32(buffer, body + 10, 45000);
      WriteU32(buffer, body + 14, (uint)(45000 + (clips[i].Duration.TotalSeconds * 45000)));

      offset = body + PlayItemBody;
    }

    if (!withMarks)
    {
      return buffer;
    }

    WriteU32(buffer, markStart, (uint)(2 + (markCount * 14)));
    WriteU16(buffer, markStart + 4, (ushort)markCount);
    for (var i = 0; i < markCount; i++)
    {
      var entry = markStart + 6 + (i * 14);
      buffer[entry + 1] = 0x01;
      WriteU16(buffer, entry + 2, (ushort)i);
      WriteU32(buffer, entry + 4, 45000);
    }

    return buffer;
  }

  private static void WriteAscii(byte[] buffer, int offset, string value)
  {
    for (var i = 0; i < value.Length; i++)
    {
      buffer[offset + i] = (byte)value[i];
    }
  }

  private static void WriteU16(byte[] buffer, int offset, ushort value)
  {
    buffer[offset] = (byte)(value >> 8);
    buffer[offset + 1] = (byte)value;
  }

  private static void WriteU32(byte[] buffer, int offset, uint value)
  {
    buffer[offset] = (byte)(value >> 24);
    buffer[offset + 1] = (byte)(value >> 16);
    buffer[offset + 2] = (byte)(value >> 8);
    buffer[offset + 3] = (byte)value;
  }

  private static void WriteBcdTime(byte[] buffer, int offset, TimeSpan value)
  {
    buffer[offset] = Bcd(value.Hours);
    buffer[offset + 1] = Bcd(value.Minutes);
    buffer[offset + 2] = Bcd(value.Seconds);
    // 0b11 in the top two bits marks the frame count as being expressed at the NTSC rate.
    buffer[offset + 3] = (byte)(0xC0 | Bcd(0));

    static byte Bcd(int value) => (byte)(((value / 10) << 4) | (value % 10));
  }
}
