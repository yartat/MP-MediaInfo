#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MediaInfo
{
  /// <summary>
  /// Provides access to the compatibility warnings of an opened media library.
  /// </summary>
  internal interface ILibraryCompatibilityProvider
  {
    /// <summary>
    /// Checks the loaded native library against the version and the field layout this wrapper was built for.
    /// </summary>
    /// <returns>Returns the warnings, or an empty array when the library is compatible.</returns>
    string[] GetCompatibilityWarnings();
  }

  /// <summary>
  /// Describes the native library this wrapper was built for, and checks a loaded library against it.
  /// </summary>
  /// <remarks>
  /// The wrapper reads most values by the number of a field, and the numbers are those of <see cref="ExpectedVersion"/>.
  /// A library with other numbers is handled: the numbers are translated by the names of the fields, so a field that
  /// moved is still found, and a field the library does not have reads as empty. What is reported is the version, because
  /// a library of another version may also differ in what it reports and how.
  /// </remarks>
  public static class LibraryCompatibility
  {
    /// <summary>
    /// The version (major.minor) of MediaInfoLib the field indexes of this wrapper come from.
    /// </summary>
    public const string ExpectedVersion = "26.10";

    private static readonly Regex VersionPattern = new(@"(\d+)\.(\d+)", RegexOptions.CultureInvariant);

    /// <summary>
    /// Compares the version of a loaded library with <see cref="ExpectedVersion"/>.
    /// </summary>
    /// <param name="libraryVersion">The version reported by the library, for example "MediaInfoLib - v26.10".</param>
    /// <returns>
    /// Returns a message that describes the incompatibility, or <see langword="null"/> when the versions match or the
    /// version is unknown.
    /// </returns>
    public static string? CheckVersion(string? libraryVersion)
    {
      if (!TryParse(libraryVersion, out var found) || !TryParse(ExpectedVersion, out var expected))
      {
        return null;
      }

      if (found.Major == expected.Major && found.Minor == expected.Minor)
      {
        return null;
      }

      return string.Format(
        CultureInfo.InvariantCulture,
        "The loaded MediaInfo library is version {0}.{1:00}, but this wrapper was built for version {2}. " +
        "Some values may be missing or wrong. Use the native package of the same version.",
        found.Major,
        found.Minor,
        ExpectedVersion);
    }

    /// <summary>
    /// Checks an opened media library against the version and the field layout this wrapper was built for.
    /// </summary>
    /// <param name="reader">The opened media.</param>
    /// <returns>Returns the warnings, or an empty array when the library is compatible.</returns>
    internal static string[] Inspect(IMediaInfoReader reader)
    {
      var warnings = new List<string>();
      if (CheckVersion(reader.Option("Info_Version")) is { } version)
      {
        warnings.Add(version);
      }

      return warnings.ToArray();
    }

    private static bool TryParse(string? text, out (int Major, int Minor) version)
    {
      version = default;
      var match = text is null ? Match.Empty : VersionPattern.Match(text);
      if (!match.Success ||
          !int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var major) ||
          !int.TryParse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minor))
      {
        return false;
      }

      version = (major, minor);
      return true;
    }
  }
}
