#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using MediaInfo.Analysis.Results;

namespace MediaInfo.Analysis.Discs.Parsers;

/// <summary>
/// Describes one title as the video manager of a DVD declares it.
/// </summary>
/// <param name="Number">The ordinal of the title on the disc, starting at one.</param>
/// <param name="TitleSetNumber">The video title set that holds the title.</param>
/// <param name="TitleNumberInSet">The ordinal of the title within its title set, starting at one.</param>
/// <param name="ChapterCount">The number of chapters the title declares.</param>
/// <param name="AngleCount">The number of camera angles the title declares.</param>
internal readonly record struct VideoManagerTitle(
  int Number,
  int TitleSetNumber,
  int TitleNumberInSet,
  int ChapterCount,
  int AngleCount);

/// <summary>
/// Describes one program chain of a video title set.
/// </summary>
/// <param name="Duration">The playback time the chain declares.</param>
/// <param name="Chapters">The chapters of the chain.</param>
internal readonly record struct ProgramChain(TimeSpan Duration, IReadOnlyList<DiscChapter> Chapters);

/// <summary>
/// Reads the navigation tables of a DVD out of its IFO files.
/// </summary>
/// <remarks>
/// Two tables are enough to describe a disc. <c>VIDEO_TS.IFO</c> holds the title search pointer table, which lists
/// every title with the title set that holds it and the number of chapters and angles it has. Each
/// <c>VTS_nn_0.IFO</c> holds the program chain table, which gives the playback time of each chain and, through its
/// program map and cell playback table, where each chapter starts and how long it runs.
/// <para>
/// Nothing here throws for a malformed file. A table that cannot be made sense of yields an empty result, and the
/// reader above falls back to describing the disc from its folder layout.
/// </para>
/// </remarks>
internal static class IfoParser
{
  private const int SectorSize = 2048;

  private const string VideoManagerSignature = "DVDVIDEO-VMG";
  private const string TitleSetSignature = "DVDVIDEO-VTS";

  // Offsets of the sector pointers within the management tables, as the DVD-Video specification lays them out.
  private const int TitleSearchPointerTableSector = 0xC4;
  private const int PartOfTitleSearchPointerTableSector = 0xC8;
  private const int ProgramChainTableSector = 0xCC;

  private const int TitleEntrySize = 12;
  private const int ChainSearchPointerSize = 8;
  private const int PartOfTitleEntrySize = 4;
  private const int CellPlaybackEntrySize = 24;

  /// <summary>
  /// Reads the titles declared by the video manager.
  /// </summary>
  /// <param name="videoManager">The content of <c>VIDEO_TS.IFO</c>.</param>
  /// <returns>Returns the titles, or an empty list when the table could not be read.</returns>
  public static IReadOnlyList<VideoManagerTitle> ReadTitles(ReadOnlySpan<byte> videoManager)
  {
    if (!BigEndian.Ascii(videoManager, 0, VideoManagerSignature.Length).Equals(VideoManagerSignature, StringComparison.Ordinal))
    {
      return [];
    }

    var table = (int)BigEndian.U32(videoManager, TitleSearchPointerTableSector) * SectorSize;
    if (table <= 0 || table >= videoManager.Length)
    {
      return [];
    }

    var count = BigEndian.U16(videoManager, table);
    if (count == 0)
    {
      return [];
    }

    var titles = new List<VideoManagerTitle>(count);
    for (var i = 0; i < count; i++)
    {
      var entry = table + 8 + (i * TitleEntrySize);
      if (entry + TitleEntrySize > videoManager.Length)
      {
        break;
      }

      var titleSet = BigEndian.U8(videoManager, entry + 6);
      var titleInSet = BigEndian.U8(videoManager, entry + 7);
      if (titleSet == 0 || titleInSet == 0)
      {
        continue;
      }

      titles.Add(
        new VideoManagerTitle(
          titles.Count + 1,
          titleSet,
          titleInSet,
          BigEndian.U16(videoManager, entry + 2),
          Math.Max(1, (int)BigEndian.U8(videoManager, entry + 1))));
    }

    return titles;
  }

  /// <summary>
  /// Reads the program chains declared by a video title set, keyed by their chain number.
  /// </summary>
  /// <param name="titleSet">The content of a <c>VTS_nn_0.IFO</c>.</param>
  /// <returns>Returns the chains, or an empty dictionary when the table could not be read.</returns>
  public static IReadOnlyDictionary<int, ProgramChain> ReadProgramChains(ReadOnlySpan<byte> titleSet)
  {
    if (!BigEndian.Ascii(titleSet, 0, TitleSetSignature.Length).Equals(TitleSetSignature, StringComparison.Ordinal))
    {
      return new Dictionary<int, ProgramChain>();
    }

    var table = (int)BigEndian.U32(titleSet, ProgramChainTableSector) * SectorSize;
    if (table <= 0 || table >= titleSet.Length)
    {
      return new Dictionary<int, ProgramChain>();
    }

    var count = BigEndian.U16(titleSet, table);
    var chains = new Dictionary<int, ProgramChain>(count);

    for (var i = 0; i < count; i++)
    {
      var pointer = table + 8 + (i * ChainSearchPointerSize);
      var start = table + (int)BigEndian.U32(titleSet, pointer + 4);
      if (start <= table || start >= titleSet.Length)
      {
        continue;
      }

      chains[i + 1] = ReadChain(titleSet, start);
    }

    return chains;
  }

  /// <summary>
  /// Reads which program chain plays each title of a video title set, keyed by the title number within the set.
  /// </summary>
  /// <param name="titleSet">The content of a <c>VTS_nn_0.IFO</c>.</param>
  /// <returns>Returns the chain numbers, or an empty dictionary when the table could not be read.</returns>
  public static IReadOnlyDictionary<int, int> ReadTitleToChainMap(ReadOnlySpan<byte> titleSet)
  {
    var map = new Dictionary<int, int>();
    if (!BigEndian.Ascii(titleSet, 0, TitleSetSignature.Length).Equals(TitleSetSignature, StringComparison.Ordinal))
    {
      return map;
    }

    var table = (int)BigEndian.U32(titleSet, PartOfTitleSearchPointerTableSector) * SectorSize;
    if (table <= 0 || table >= titleSet.Length)
    {
      return map;
    }

    var count = BigEndian.U16(titleSet, table);
    for (var i = 0; i < count; i++)
    {
      // Each title unit begins with the part of title that enters it, and that part names the chain.
      var unit = table + (int)BigEndian.U32(titleSet, table + 8 + (i * 4));
      if (unit <= table || unit + PartOfTitleEntrySize > titleSet.Length)
      {
        continue;
      }

      var chain = BigEndian.U16(titleSet, unit);
      if (chain > 0)
      {
        map[i + 1] = chain;
      }
    }

    return map;
  }

  private static ProgramChain ReadChain(ReadOnlySpan<byte> titleSet, int start)
  {
    var duration = ReadPlaybackTime(titleSet, start + 4);

    var programs = BigEndian.U8(titleSet, start + 2);
    var cells = BigEndian.U8(titleSet, start + 3);
    var programMap = BigEndian.U16(titleSet, start + 0xE6);
    var cellPlayback = BigEndian.U16(titleSet, start + 0xE8);

    if (programs == 0 || cells == 0 || programMap == 0 || cellPlayback == 0)
    {
      return new ProgramChain(duration, []);
    }

    // Every cell has its own playback time; a chapter is the run of cells its program map entry opens.
    // A chain declares its cell count in one byte, so the largest possible table is small enough for the stack.
    Span<TimeSpan> cellTimes = stackalloc TimeSpan[cells];
    for (var i = 0; i < cells; i++)
    {
      cellTimes[i] = ReadPlaybackTime(titleSet, start + cellPlayback + (i * CellPlaybackEntrySize) + 4);
    }

    var chapters = new List<DiscChapter>(programs);
    var elapsed = TimeSpan.Zero;

    for (var program = 0; program < programs; program++)
    {
      var firstCell = BigEndian.U8(titleSet, start + programMap + program) - 1;
      var lastCell = program + 1 < programs
        ? BigEndian.U8(titleSet, start + programMap + program + 1) - 2
        : cells - 1;

      if (firstCell < 0 || firstCell >= cells)
      {
        continue;
      }

      var chapterLength = TimeSpan.Zero;
      for (var cell = firstCell; cell <= Math.Min(lastCell, cells - 1); cell++)
      {
        chapterLength += cellTimes[cell];
      }

      chapters.Add(new DiscChapter(chapters.Count + 1, elapsed, chapterLength));
      elapsed += chapterLength;
    }

    return new ProgramChain(duration, chapters);
  }

  private static TimeSpan ReadPlaybackTime(ReadOnlySpan<byte> data, int offset)
  {
    if (offset < 0 || offset + 4 > data.Length)
    {
      return TimeSpan.Zero;
    }

    var hours = BigEndian.Bcd(data[offset]);
    var minutes = BigEndian.Bcd(data[offset + 1]);
    var seconds = BigEndian.Bcd(data[offset + 2]);

    // The top two bits of the frame byte carry the frame rate the frame count is expressed in.
    var frameByte = data[offset + 3];
    var frames = BigEndian.Bcd((byte)(frameByte & 0x3F));
    var rate = ((frameByte & 0xC0) >> 6) switch
    {
      0b11 => 30000d / 1001d,
      0b01 => 25d,
      _ => 0d
    };

    if (minutes > 59 || seconds > 59)
    {
      return TimeSpan.Zero;
    }

    var total = (hours * 3600d) + (minutes * 60d) + seconds + (rate > 0 ? frames / rate : 0d);
    return TimeSpan.FromSeconds(total);
  }
}
