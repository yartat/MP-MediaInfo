#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MediaInfo
{
  /// <summary>
  /// Verifies that the fields of a loaded library are at the indexes the <see cref="NativeMethods"/> enumerations expect.
  /// </summary>
  /// <remarks>
  /// The order of the fields is a property of the library, not of a media, so the outcome is kept after the first
  /// conclusive check of a stream kind.
  /// </remarks>
  internal sealed class FieldLayoutInspector
  {
    private static readonly KeyValuePair<StreamKind, Type>[] Layouts =
    [
      new(StreamKind.General, typeof(NativeMethods.General)),
      new(StreamKind.Video, typeof(NativeMethods.Video)),
      new(StreamKind.Audio, typeof(NativeMethods.Audio)),
      new(StreamKind.Text, typeof(NativeMethods.Text)),
      new(StreamKind.Other, typeof(NativeMethods.Other)),
      new(StreamKind.Image, typeof(NativeMethods.Image)),
      new(StreamKind.Menu, typeof(NativeMethods.Menu))
    ];

    private readonly Dictionary<StreamKind, string?> _outcomes = new();
    private readonly object _sync = new();

    /// <summary>
    /// Gets the stream kinds that are checked.
    /// </summary>
    public static IEnumerable<StreamKind> Kinds
    {
      get
      {
        foreach (var layout in Layouts)
        {
          yield return layout.Key;
        }
      }
    }

    /// <summary>
    /// Checks the field layout of a stream kind.
    /// </summary>
    /// <param name="reader">The opened media. It must hold at least one stream of the kind.</param>
    /// <param name="kind">The kind of the streams to check.</param>
    /// <returns>Returns a message that describes the difference, or <see langword="null"/> when the layout matches.</returns>
    public string? Inspect(IMediaInfoReader reader, StreamKind kind)
    {
      lock (_sync)
      {
        if (_outcomes.TryGetValue(kind, out var known))
        {
          return known;
        }
      }

      var outcome = Compute(reader, kind, out var conclusive);
      if (conclusive)
      {
        lock (_sync)
        {
          _outcomes[kind] = outcome;
        }
      }

      return outcome;
    }

    private static string? Compute(IMediaInfoReader reader, StreamKind kind, out bool conclusive)
    {
      conclusive = false;
      Type? type = null;
      foreach (var layout in Layouts)
      {
        if (layout.Key == kind)
        {
          type = layout.Value;
        }
      }

      if (type is null)
      {
        return null;
      }

      var mismatches = 0;
      var firstIndex = -1;
      string? firstExpected = null;
      string? firstActual = null;
      var first = true;

      foreach (var item in Enum.GetValues(type))
      {
        var index = Convert.ToInt32(item, CultureInfo.InvariantCulture);
        var name = Enum.GetName(type, item)!;
        var expected = name.Substring(name.IndexOf('_') + 1);
        var actual = reader.Get(kind, 0, index, InfoKind.Name) ?? string.Empty;

        // A library that answers nothing for the first field has not told anything about its layout.
        if (first && actual.Length == 0)
        {
          return null;
        }

        first = false;
        if (Normalize(actual) == Normalize(expected))
        {
          continue;
        }

        if (mismatches++ == 0)
        {
          firstIndex = index;
          firstExpected = expected;
          firstActual = actual;
        }
      }

      conclusive = true;
      if (mismatches == 0)
      {
        return null;
      }

      return string.Format(
        CultureInfo.InvariantCulture,
        "The loaded MediaInfo library has a different {0} field layout than this wrapper expects: {1} field(s) differ, " +
        "the first is #{2}, where the library has '{3}' and the wrapper expects '{4}'. " +
        "Values of {0} streams read by field index are unreliable. Use the native package of version {5}.",
        kind,
        mismatches,
        firstIndex,
        firstActual,
        firstExpected,
        LibraryCompatibility.ExpectedVersion);
    }

    // The library names fields like "Format/String", the enumerations like "Format_String".
    private static string Normalize(string name)
    {
      var result = new StringBuilder(name.Length);
      foreach (var character in name)
      {
        result.Append(char.IsLetterOrDigit(character) ? character : '_');
      }

      return result.ToString();
    }
  }
}
