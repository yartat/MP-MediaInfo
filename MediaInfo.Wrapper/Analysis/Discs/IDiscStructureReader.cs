#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Strategies;

namespace MediaInfo.Analysis.Discs;

/// <summary>
/// Discovers the structure of an optical disc.
/// </summary>
/// <remarks>
/// Two kinds of reader can satisfy this interface. One discovers the titles from the folder layout and the media files
/// themselves, which needs no knowledge of the navigation tables of the disc. The other parses those tables and can
/// therefore also report chapters, angles and the mapping of a playlist to its clips. Both are selected through this
/// interface, so replacing one with the other changes nothing above it.
/// </remarks>
public interface IDiscStructureReader
{
  /// <summary>
  /// Gets the kind of disc the reader discovers.
  /// </summary>
  DiscKind Kind { get; }

  /// <summary>
  /// Gets the order the reader is considered in. The reader with the lowest value that can read a folder wins.
  /// </summary>
  int Priority { get; }

  /// <summary>
  /// Determines whether the specified folder holds a disc structure the reader understands.
  /// </summary>
  /// <param name="path">The folder to examine.</param>
  /// <param name="discRoot">
  /// When this method returns <see langword="true"/>, contains the folder the disc structure is rooted at, which is
  /// not necessarily the folder that was examined.
  /// </param>
  /// <returns>Returns <see langword="true"/> when the folder holds a disc structure; otherwise, <see langword="false"/>.</returns>
  bool CanRead(string path, [NotNullWhen(true)] out string? discRoot);

  /// <summary>
  /// Discovers the structure of the disc rooted at the specified folder.
  /// </summary>
  /// <param name="discRoot">The folder the disc structure is rooted at.</param>
  /// <param name="context">The services and settings the analysis needs.</param>
  /// <param name="cancellationToken">The token that cancels the discovery.</param>
  /// <returns>Returns the structure of the disc, or <see langword="null"/> when it holds no title.</returns>
  Task<DiscStructure?> ReadAsync(string discRoot, MediaAnalysisContext context, CancellationToken cancellationToken);
}
