#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;

namespace MediaInfo.Analysis.Decorators;

/// <summary>
/// Returns the stored outcome of an earlier analysis of the same media instead of reading it again.
/// </summary>
/// <remarks>
/// Only a file and a disc folder are cached, and only when the analysis succeeded.
/// <list type="bullet">
///   <item>A file is keyed by its path, size and last write time, so an edit invalidates the entry.</item>
///   <item>
///     A disc folder is keyed by its path and its own last write time. The content of a disc does not change, so this
///     is sound for the case the cache exists to serve; a folder whose files are edited in place without the folder
///     itself being touched would keep a stale entry.
///   </item>
///   <item>A stream cannot be identified without consuming it, and a network media has no cheap validator, so both are passed straight through.</item>
///   <item>A failed analysis is never stored, because the condition that caused it is usually temporary.</item>
/// </list>
/// </remarks>
public sealed class CachingAnalyzer : MediaInfoAnalyzerDecorator
{
  private readonly IAnalysisCache _cache;
  private readonly IFileSystem _fileSystem;

  /// <summary>
  /// Initializes a new instance of the <see cref="CachingAnalyzer"/> class.
  /// </summary>
  /// <param name="inner">The analyzer to wrap.</param>
  /// <param name="cache">The store the outcomes are kept in.</param>
  /// <param name="fileSystem">The file system the keys are derived from.</param>
  /// <exception cref="ArgumentNullException"><paramref name="cache"/> or <paramref name="fileSystem"/> is <see langword="null"/>.</exception>
  public CachingAnalyzer(IMediaInfoAnalyzer inner, IAnalysisCache cache, IFileSystem fileSystem)
    : base(inner)
  {
    _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
  }

  /// <inheritdoc />
  public override async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    CancellationToken cancellationToken = default)
  {
    var key = CreateKey(source);
    if (key is null)
    {
      return await Inner.AnalyzeAsync(source, cancellationToken).ConfigureAwait(false);
    }

    if (_cache.TryGet(key, out var cached))
    {
      return cached;
    }

    var result = await Inner.AnalyzeAsync(source, cancellationToken).ConfigureAwait(false);
    if (result.Success)
    {
      _cache.Set(key, result);
    }

    return result;
  }

  private string? CreateKey(IMediaSource source)
  {
    switch (source)
    {
      case FileMediaSource file when !string.IsNullOrWhiteSpace(file.Path):
        return FormattableString.Invariant(
          $"file|{file.Path}|{_fileSystem.GetFileLength(file.Path)}|{_fileSystem.GetLastWriteTimeUtc(file.Path).UtcTicks}");

      case DirectoryMediaSource directory when !string.IsNullOrWhiteSpace(directory.Path):
        return FormattableString.Invariant(
          $"dir|{directory.Path}|{_fileSystem.GetLastWriteTimeUtc(directory.Path).UtcTicks}");

      default:
        return null;
    }
  }
}
