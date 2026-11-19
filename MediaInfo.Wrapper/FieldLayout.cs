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
  /// Translates the field numbers of the <see cref="NativeMethods"/> enumerations into the numbers the loaded library uses.
  /// </summary>
  /// <remarks>
  /// A field is identified by its position in the table of its stream kind, and the library inserts new fields into the
  /// middle of the table from one version to the next. The wrapper is written against one table, so it asks the library
  /// for the name of every field once, and from then on the number of a field is looked up by its name. A field the
  /// library does not have reads as empty. The order of the fields is a property of the library, not of a media, so
  /// the table is kept after the first conclusive look at a stream kind.
  /// </remarks>
  internal sealed class FieldLayout
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

    private readonly Dictionary<StreamKind, int[]?> _tables = new();
    private readonly object _sync = new();

    /// <summary>
    /// Gets the layout of the process, which is the layout of the one native library that is loaded.
    /// </summary>
    public static FieldLayout Shared { get; } = new();

    /// <summary>
    /// Translates the number of a field with a table.
    /// </summary>
    /// <param name="table">The table of the stream kind, or <see langword="null"/> when the numbers are the same.</param>
    /// <param name="parameter">The number of the field in the enumeration of this wrapper.</param>
    /// <returns>
    /// Returns the number of the field in the library, or -1 when the library does not have it. A number that does not
    /// belong to the enumeration (a field added to a stream by the media itself) is returned as it is.
    /// </returns>
    public static int Apply(int[]? table, int parameter) =>
      table is null || parameter < 0 || parameter >= table.Length ? parameter : table[parameter];

    /// <summary>
    /// Gets the table of a stream kind when it is already known.
    /// </summary>
    /// <param name="kind">The kind of the stream.</param>
    /// <param name="table">The table, which is <see langword="null"/> when the numbers are the same.</param>
    /// <returns>Returns <see langword="true"/> when the layout of the library has been looked at.</returns>
    public bool TryGetKnown(StreamKind kind, out int[]? table)
    {
      lock (_sync)
      {
        return _tables.TryGetValue(kind, out table);
      }
    }

    /// <summary>
    /// Gets the translation table of a stream kind.
    /// </summary>
    /// <param name="kind">The kind of the stream.</param>
    /// <param name="nameOf">Gets the name of a field of the library by its number, or an empty string when there is none.</param>
    /// <returns>
    /// Returns the table, indexed by the number in the enumeration, or <see langword="null"/> when the numbers are the
    /// same or the library has not told its layout yet.
    /// </returns>
    public int[]? GetTable(StreamKind kind, Func<int, string> nameOf)
    {
      lock (_sync)
      {
        if (_tables.TryGetValue(kind, out var known))
        {
          return known;
        }
      }

      var table = Build(kind, nameOf, out var conclusive);
      if (conclusive)
      {
        lock (_sync)
        {
          _tables[kind] = table;
        }
      }

      return table;
    }

    private static int[]? Build(StreamKind kind, Func<int, string> nameOf, out bool conclusive)
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

      // A library that answers nothing for the first field has not told anything about its layout.
      if (nameOf(0).Length == 0)
      {
        return null;
      }

      var members = Enum.GetValues(type);
      var table = new int[members.Length == 0 ? 0 : Convert.ToInt32(members.GetValue(members.Length - 1), CultureInfo.InvariantCulture) + 1];
      var identity = true;

      // The names of the library, by the number the library gives them.
      var library = new Dictionary<string, int>(StringComparer.Ordinal);
      for (var index = 0; ; index++)
      {
        var name = nameOf(index);
        if (name.Length == 0)
        {
          break;
        }

        var key = Normalize(name);
        if (!library.ContainsKey(key))
        {
          library[key] = index;
        }
      }

      foreach (var member in members)
      {
        var number = Convert.ToInt32(member, CultureInfo.InvariantCulture);
        var name = Enum.GetName(type, member)!;
        var expected = Normalize(name.Substring(name.IndexOf('_') + 1));
        table[number] = library.TryGetValue(expected, out var found) ? found : -1;
        identity &= table[number] == number;
      }

      conclusive = true;
      return identity ? null : table;
    }

    // The library names fields like "Format/String", the enumerations like "Format_String".
    internal static string Normalize(string name)
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
