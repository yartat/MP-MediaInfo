#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System.IO;

namespace MediaInfo.Analysis.Abstractions;

/// <summary>
/// Describes the file system operations the analysis pipeline depends on.
/// </summary>
/// <remarks>
/// Disc structure discovery is almost entirely directory walking. Routing it through this interface allows the
/// discovery logic to be tested against an in-memory layout instead of a real disc.
/// </remarks>
public interface IFileSystem
{
  /// <summary>
  /// Determines whether the specified file exists.
  /// </summary>
  /// <param name="path">The path of the file.</param>
  /// <returns>Returns <see langword="true"/> when the file exists; otherwise, <see langword="false"/>.</returns>
  bool FileExists(string path);

  /// <summary>
  /// Determines whether the specified directory exists.
  /// </summary>
  /// <param name="path">The path of the directory.</param>
  /// <returns>Returns <see langword="true"/> when the directory exists; otherwise, <see langword="false"/>.</returns>
  bool DirectoryExists(string path);

  /// <summary>
  /// Gets the files of the specified directory that match the search pattern.
  /// </summary>
  /// <param name="path">The path of the directory.</param>
  /// <param name="searchPattern">The search pattern to match against the file names.</param>
  /// <param name="searchOption">A value that specifies whether to search subdirectories.</param>
  /// <returns>Returns the matching file paths, or an empty array when the directory does not exist.</returns>
  string[] GetFiles(string path, string searchPattern, SearchOption searchOption);

  /// <summary>
  /// Gets the subdirectories of the specified directory.
  /// </summary>
  /// <param name="path">The path of the directory.</param>
  /// <returns>Returns the subdirectory paths, or an empty array when the directory does not exist.</returns>
  string[] GetDirectories(string path);

  /// <summary>
  /// Gets the size of the specified file.
  /// </summary>
  /// <param name="path">The path of the file.</param>
  /// <returns>Returns the size in bytes, or 0 when the file does not exist.</returns>
  long GetFileLength(string path);

  /// <summary>
  /// Opens the specified file for asynchronous reading.
  /// </summary>
  /// <param name="path">The path of the file.</param>
  /// <returns>Returns a readable stream over the file.</returns>
  Stream OpenRead(string path);
}
