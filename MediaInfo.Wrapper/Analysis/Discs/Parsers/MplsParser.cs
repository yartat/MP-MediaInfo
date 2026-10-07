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
/// Describes one item of a Blu-ray playlist.
/// </summary>
/// <param name="ClipId">The name of the stream file the item plays, without its extension.</param>
/// <param name="Duration">How much of the clip the item plays.</param>
public readonly record struct PlaylistItem(string ClipId, TimeSpan Duration);

/// <summary>
/// Describes a Blu-ray playlist.
/// </summary>
/// <param name="Items">The clips the playlist plays, in order.</param>
/// <param name="Chapters">The chapters the playlist declares.</param>
public readonly record struct Playlist(IReadOnlyList<PlaylistItem> Items, IReadOnlyList<DiscChapter> Chapters)
{
  /// <summary>
  /// Gets the total duration of the playlist.
  /// </summary>
  public TimeSpan Duration
  {
    get
    {
      var total = TimeSpan.Zero;
      foreach (var item in Items)
      {
        total += item.Duration;
      }

      return total;
    }
  }

  /// <summary>
  /// Gets a value indicating whether the playlist holds anything worth playing.
  /// </summary>
  public bool IsEmpty => Items.Count == 0;
}

/// <summary>
/// Reads a Blu-ray playlist out of an MPLS file.
/// </summary>
/// <remarks>
/// A playlist names the clips that make up a title and the part of each one it plays, which is what turns a folder of
/// interchangeable stream files into an ordered title. Times are expressed in the 45 kHz clock of the transport
/// stream. The marks table carries the chapters.
/// <para>
/// Nothing here throws for a malformed file: a playlist that cannot be made sense of comes back empty, and the reader
/// above falls back to describing the disc from its folder layout.
/// </para>
/// </remarks>
public static class MplsParser
{
  private const string Signature = "MPLS";
  private const double TimeScale = 45000d;

  private const int PlayItemPrefix = 2;
  private const int MarkEntrySize = 14;
  private const byte EntryMark = 0x01;

  /// <summary>
  /// Reads the playlist held by the specified file content.
  /// </summary>
  /// <param name="data">The content of an MPLS file.</param>
  /// <returns>Returns the playlist, which is empty when the file could not be read.</returns>
  public static Playlist Parse(ReadOnlySpan<byte> data)
  {
    var empty = new Playlist([], []);
    if (!BigEndian.Ascii(data, 0, Signature.Length).Equals(Signature, StringComparison.Ordinal))
    {
      return empty;
    }

    var playListStart = (int)BigEndian.U32(data, 8);
    var markStart = (int)BigEndian.U32(data, 12);
    if (playListStart <= 0 || playListStart >= data.Length)
    {
      return empty;
    }

    var itemCount = BigEndian.U16(data, playListStart + 6);
    if (itemCount == 0)
    {
      return empty;
    }

    var items = new List<PlaylistItem>(itemCount);
    var itemStarts = new List<TimeSpan>(itemCount);
    var elapsed = TimeSpan.Zero;

    // The play items are packed one after another, each prefixed with its own length, so the length is what walks
    // the list. That keeps multi angle items, which carry extra entries, from throwing the walk off.
    var offset = playListStart + 10;
    for (var i = 0; i < itemCount; i++)
    {
      var length = BigEndian.U16(data, offset);
      if (length == 0 || offset + PlayItemPrefix + length > data.Length)
      {
        break;
      }

      var body = offset + PlayItemPrefix;
      var clipId = BigEndian.Ascii(data, body, 5);
      var inTime = BigEndian.U32(data, body + 10);
      var outTime = BigEndian.U32(data, body + 14);

      var duration = outTime > inTime
        ? TimeSpan.FromSeconds((outTime - inTime) / TimeScale)
        : TimeSpan.Zero;

      items.Add(new PlaylistItem(clipId, duration));
      itemStarts.Add(elapsed);
      elapsed += duration;

      offset = body + length;
    }

    return items.Count == 0
      ? empty
      : new Playlist(items, ReadChapters(data, markStart, items, itemStarts));
  }

  private static IReadOnlyList<DiscChapter> ReadChapters(
    ReadOnlySpan<byte> data,
    int markStart,
    IReadOnlyList<PlaylistItem> items,
    IReadOnlyList<TimeSpan> itemStarts)
  {
    if (markStart <= 0 || markStart >= data.Length)
    {
      return [];
    }

    var markCount = BigEndian.U16(data, markStart + 4);
    if (markCount == 0)
    {
      return [];
    }

    var starts = new List<TimeSpan>(markCount);
    for (var i = 0; i < markCount; i++)
    {
      var entry = markStart + 6 + (i * MarkEntrySize);
      if (entry + MarkEntrySize > data.Length || BigEndian.U8(data, entry + 1) != EntryMark)
      {
        continue;
      }

      var itemIndex = BigEndian.U16(data, entry + 2);
      if (itemIndex >= items.Count)
      {
        continue;
      }

      // A mark is a timestamp on the clock of the item it belongs to, so it has to be rebased onto the playlist.
      var itemStart = BigEndian.U32(data, MarkedItemInTimeOffset(data, itemIndex));
      var timestamp = BigEndian.U32(data, entry + 4);
      var withinItem = timestamp > itemStart
        ? TimeSpan.FromSeconds((timestamp - itemStart) / TimeScale)
        : TimeSpan.Zero;

      starts.Add(itemStarts[itemIndex] + withinItem);
    }

    if (starts.Count == 0)
    {
      return [];
    }

    starts.Sort();

    var total = TimeSpan.Zero;
    foreach (var item in items)
    {
      total += item.Duration;
    }

    var chapters = new List<DiscChapter>(starts.Count);
    for (var i = 0; i < starts.Count; i++)
    {
      var end = i + 1 < starts.Count ? starts[i + 1] : total;
      var length = end > starts[i] ? end - starts[i] : TimeSpan.Zero;
      chapters.Add(new DiscChapter(i + 1, starts[i], length));
    }

    return chapters;

    // Walks back to the play item the mark refers to and returns where its IN time is stored.
    static int MarkedItemInTimeOffset(ReadOnlySpan<byte> data, int itemIndex)
    {
      var playListStart = (int)BigEndian.U32(data, 8);
      var offset = playListStart + 10;
      for (var i = 0; i < itemIndex; i++)
      {
        var length = BigEndian.U16(data, offset);
        if (length == 0)
        {
          return -1;
        }

        offset += PlayItemPrefix + length;
      }

      return offset + PlayItemPrefix + 10;
    }
  }
}
