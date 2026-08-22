#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MediaInfo.Analysis.Abstractions;

namespace MediaInfo.Analysis.Tests.Fakes;

/// <summary>
/// An in-memory file system that lets disc layouts be described without touching a disk.
/// </summary>
public sealed class FakeFileSystem : IFileSystem
{
  private readonly Dictionary<string, long> _files = new(StringComparer.OrdinalIgnoreCase);
  private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);

  /// <summary>Adds a file of the given size, creating every directory above it.</summary>
  public FakeFileSystem AddFile(string path, long size = 4096L)
  {
    var normalized = Normalize(path);
    _files[normalized] = size;
    AddDirectory(Path.GetDirectoryName(normalized) ?? string.Empty);
    return this;
  }

  /// <summary>Adds a directory, creating every directory above it.</summary>
  public FakeFileSystem AddDirectory(string path)
  {
    var current = Normalize(path);
    while (!string.IsNullOrEmpty(current))
    {
      if (!_directories.Add(current))
      {
        break;
      }

      current = Path.GetDirectoryName(current) ?? string.Empty;
    }

    return this;
  }

  /// <inheritdoc />
  public bool FileExists(string path) => !string.IsNullOrEmpty(path) && _files.ContainsKey(Normalize(path));

  /// <inheritdoc />
  public bool DirectoryExists(string path) => !string.IsNullOrEmpty(path) && _directories.Contains(Normalize(path));

  /// <inheritdoc />
  public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
  {
    if (!DirectoryExists(path))
    {
      return [];
    }

    var root = Normalize(path);
    var prefix = root + Path.DirectorySeparatorChar;
    var pattern = ToRegex(searchPattern);

    return _files.Keys
      .Where(file => searchOption == SearchOption.AllDirectories
        ? file.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
        : string.Equals(Path.GetDirectoryName(file), root, StringComparison.OrdinalIgnoreCase))
      .Where(file => pattern.IsMatch(Path.GetFileName(file)))
      .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
      .ToArray();
  }

  /// <inheritdoc />
  public string[] GetDirectories(string path)
  {
    if (!DirectoryExists(path))
    {
      return [];
    }

    var root = Normalize(path);
    return _directories
      .Where(directory => string.Equals(Path.GetDirectoryName(directory), root, StringComparison.OrdinalIgnoreCase))
      .OrderBy(directory => directory, StringComparer.OrdinalIgnoreCase)
      .ToArray();
  }

  /// <inheritdoc />
  public long GetFileLength(string path) => _files.TryGetValue(Normalize(path), out var size) ? size : 0L;

  /// <inheritdoc />
  public Stream OpenRead(string path) =>
    FileExists(path) ? new MemoryStream(new byte[GetFileLength(path)]) : throw new FileNotFoundException(path);

  private static string Normalize(string path) =>
    path
      .Replace('/', Path.DirectorySeparatorChar)
      .Replace('\\', Path.DirectorySeparatorChar)
      .TrimEnd(Path.DirectorySeparatorChar);

  private static Regex ToRegex(string searchPattern) =>
    new(
      "^" + Regex.Escape(searchPattern).Replace("\\*", ".*").Replace("\\?", ".") + "$",
      RegexOptions.IgnoreCase);
}
