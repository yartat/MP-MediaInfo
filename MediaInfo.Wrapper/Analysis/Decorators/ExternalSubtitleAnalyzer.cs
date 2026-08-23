#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;

namespace MediaInfo.Analysis.Decorators;

/// <summary>
/// Reports whether subtitle files sit next to the analyzed media.
/// </summary>
/// <remarks>
/// The scan looks for files that share the name of the media and carry a subtitle extension. It only applies to a
/// media file; a disc carries its subtitles inside the structure and a stream has no folder to look in.
/// </remarks>
public sealed class ExternalSubtitleAnalyzer : MediaInfoAnalyzerDecorator
{
  /// <summary>
  /// The file extensions the scan treats as subtitles.
  /// </summary>
  public static readonly IReadOnlyCollection<string> DefaultExtensions =
  [
    ".AQT", ".ASC", ".ASS", ".DAT", ".DKS", ".IDX", ".JS", ".JSS", ".LRC", ".MPL",
    ".OVR", ".PAN", ".PJS", ".PSB", ".RT", ".RTF", ".S2K", ".SBT", ".SCR", ".SMI",
    ".SON", ".SRT", ".SSA", ".SST", ".SSTS", ".STL", ".SUB", ".TXT", ".VKT", ".VSF",
    ".VTT", ".ZEG"
  ];

  private readonly IFileSystem _fileSystem;
  private readonly HashSet<string> _extensions;

  /// <summary>
  /// Initializes a new instance of the <see cref="ExternalSubtitleAnalyzer"/> class.
  /// </summary>
  /// <param name="inner">The analyzer to wrap.</param>
  /// <param name="fileSystem">The file system the scan reads from.</param>
  /// <param name="extensions">The extensions to treat as subtitles, or <see langword="null"/> for <see cref="DefaultExtensions"/>.</param>
  /// <exception cref="ArgumentNullException"><paramref name="fileSystem"/> is <see langword="null"/>.</exception>
  public ExternalSubtitleAnalyzer(
    IMediaInfoAnalyzer inner,
    IFileSystem fileSystem,
    IEnumerable<string>? extensions = null)
    : base(inner)
  {
    _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    _extensions = new HashSet<string>(extensions ?? DefaultExtensions, StringComparer.OrdinalIgnoreCase);
  }

  /// <inheritdoc />
  public override async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    CancellationToken cancellationToken = default)
  {
    var result = await Inner.AnalyzeAsync(source, cancellationToken).ConfigureAwait(false);

    return source is FileMediaSource file && !string.IsNullOrWhiteSpace(file.Path)
      ? result with { HasExternalSubtitles = HasSubtitlesBeside(file.Path) }
      : result;
  }

  private bool HasSubtitlesBeside(string path)
  {
    var folder = Path.GetDirectoryName(path);
    if (string.IsNullOrEmpty(folder))
    {
      return false;
    }

    var name = Path.GetFileNameWithoutExtension(path);
    return _fileSystem
      .GetFiles(folder, name + "*", SearchOption.TopDirectoryOnly)
      .Any(x =>
        !string.Equals(x, path, StringComparison.OrdinalIgnoreCase) &&
        _extensions.Contains(Path.GetExtension(x)));
  }
}
