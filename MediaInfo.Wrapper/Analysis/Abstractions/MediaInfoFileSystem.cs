#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.IO;

namespace MediaInfo.Analysis.Abstractions;

/// <summary>
/// Provides access to the local file system.
/// </summary>
/// <remarks>
/// Enumeration and size lookups return empty or zero results instead of throwing, because a disc that becomes
/// unavailable while it is being walked is an expected condition rather than an error.
/// <para>
/// Folders that are symbolic links or junctions are not listed and not walked into. A link can point at one of its own
/// parents, and a walk that follows it never ends. A path that is itself a link, handed in as the folder to walk, is
/// walked.
/// </para>
/// </remarks>
public sealed class MediaInfoFileSystem : IFileSystem
{
  private static readonly string[] EmptyPaths = [];

  /// <summary>
  /// Gets a shared instance of the local file system.
  /// </summary>
  public static MediaInfoFileSystem Instance { get; } = new();

  /// <inheritdoc />
  public bool FileExists(string path) => !string.IsNullOrEmpty(path) && File.Exists(path);

  /// <inheritdoc />
  public bool DirectoryExists(string path) => !string.IsNullOrEmpty(path) && Directory.Exists(path);

  /// <inheritdoc />
  public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
  {
    if (!DirectoryExists(path))
    {
      return EmptyPaths;
    }

    if (searchOption != SearchOption.AllDirectories)
    {
      return GetFilesOf(path, searchPattern);
    }

    // The walk is made here, because Directory.GetFiles with subfolders follows every link it meets.
    var files = new List<string>();
    var pending = new Stack<string>();
    pending.Push(path);
    while (pending.Count > 0)
    {
      var folder = pending.Pop();
      files.AddRange(GetFilesOf(folder, searchPattern));
      foreach (var child in GetDirectories(folder))
      {
        pending.Push(child);
      }
    }

    return [.. files];
  }

  /// <inheritdoc />
  public string[] GetDirectories(string path)
  {
    if (!DirectoryExists(path))
    {
      return EmptyPaths;
    }

    try
    {
      var directories = Directory.GetDirectories(path);
      return Array.Exists(directories, IsLink) ? Array.FindAll(directories, x => !IsLink(x)) : directories;
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
      return EmptyPaths;
    }
  }

  // A folder that cannot be read is skipped instead of making the whole search come back empty.
  private static string[] GetFilesOf(string folder, string searchPattern)
  {
    try
    {
      return Directory.GetFiles(
        folder,
        searchPattern,
        new EnumerationOptions
        {
          RecurseSubdirectories = false,
          IgnoreInaccessible = true,
          AttributesToSkip = 0,
          MatchType = MatchType.Win32
        });
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
    {
      return EmptyPaths;
    }
  }

  private static bool IsLink(string directory)
  {
    try
    {
      var info = new DirectoryInfo(directory);
#if NET6_0_OR_GREATER
      // Only a symbolic link or a junction has a target. Other reparse points, such as the placeholders of a cloud
      // storage client, are folders like any other.
      return info.LinkTarget is not null;
#else
      return (info.Attributes & FileAttributes.ReparsePoint) != 0;
#endif
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
    {
      return false;
    }
  }

  /// <inheritdoc />
  public long GetFileLength(string path)
  {
    try
    {
      var info = new FileInfo(path);
      return info.Exists ? info.Length : 0L;
    }
    catch (Exception exception) when (
      exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
    {
      return 0L;
    }
  }

  /// <inheritdoc />
  public DateTimeOffset GetLastWriteTimeUtc(string path)
  {
    try
    {
      if (File.Exists(path))
      {
        return new DateTimeOffset(File.GetLastWriteTimeUtc(path), TimeSpan.Zero);
      }

      return Directory.Exists(path)
        ? new DateTimeOffset(Directory.GetLastWriteTimeUtc(path), TimeSpan.Zero)
        : DateTimeOffset.MinValue;
    }
    catch (Exception exception) when (
      exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
    {
      return DateTimeOffset.MinValue;
    }
  }

  /// <inheritdoc />
  public Stream OpenRead(string path) =>
    new FileStream(
      path,
      FileMode.Open,
      FileAccess.Read,
      FileShare.Read,
      64 * 1024,
      FileOptions.Asynchronous | FileOptions.SequentialScan);
}
