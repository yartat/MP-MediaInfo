#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;

namespace MediaInfo.Analysis.Abstractions;

/// <summary>
/// Provides access to the local file system.
/// </summary>
/// <remarks>
/// Enumeration and size lookups return empty or zero results instead of throwing, because a disc that becomes
/// unavailable while it is being walked is an expected condition rather than an error.
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

    try
    {
      return Directory.GetFiles(path, searchPattern, searchOption);
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
      return EmptyPaths;
    }
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
      return Directory.GetDirectories(path);
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
      return EmptyPaths;
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
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
      return 0L;
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
