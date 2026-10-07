#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;

namespace MediaInfo.Analysis.Discs.Parsers;

/// <summary>
/// Reads the big endian values that the DVD and Blu-ray navigation tables are written in.
/// </summary>
/// <remarks>
/// Every accessor returns a default instead of throwing when the requested bytes fall outside the buffer. A
/// navigation table that has been truncated or that points somewhere unexpected is a disc to give up on, not a
/// program to crash, and the callers turn a default into "this table could not be read".
/// </remarks>
internal static class BigEndian
{
  /// <summary>Reads an unsigned byte, or 0 when it lies outside the buffer.</summary>
  public static byte U8(ReadOnlySpan<byte> data, int offset) =>
    (uint)offset < (uint)data.Length ? data[offset] : (byte)0;

  /// <summary>Reads a 16 bit unsigned integer, or 0 when it lies outside the buffer.</summary>
  public static ushort U16(ReadOnlySpan<byte> data, int offset) =>
    offset >= 0 && offset + 2 <= data.Length
      ? (ushort)((data[offset] << 8) | data[offset + 1])
      : (ushort)0;

  /// <summary>Reads a 32 bit unsigned integer, or 0 when it lies outside the buffer.</summary>
  public static uint U32(ReadOnlySpan<byte> data, int offset) =>
    offset >= 0 && offset + 4 <= data.Length
      ? ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) | ((uint)data[offset + 2] << 8) | data[offset + 3]
      : 0u;

  /// <summary>Reads a fixed length ASCII string, or an empty string when it lies outside the buffer.</summary>
  public static string Ascii(ReadOnlySpan<byte> data, int offset, int length)
  {
    if (offset < 0 || length <= 0 || offset + length > data.Length)
    {
      return string.Empty;
    }

    Span<char> characters = stackalloc char[length];
    for (var i = 0; i < length; i++)
    {
      characters[i] = (char)data[offset + i];
    }

    return new string(characters);
  }

  /// <summary>Decodes one binary coded decimal byte.</summary>
  public static int Bcd(byte value) => ((value >> 4) * 10) + (value & 0x0F);
}
