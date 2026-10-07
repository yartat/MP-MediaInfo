#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;

namespace MediaInfo.Analysis.Decorators;

/// <summary>
/// Rejects a media that cannot be analyzed before any work is started on it.
/// </summary>
/// <remarks>
/// Only checks that cost nothing are performed here. Whether a file exists and how large it is are left to the
/// strategy, which needs the size anyway and would otherwise look at the file twice.
/// </remarks>
public sealed class ValidatingAnalyzer : MediaInfoAnalyzerDecorator
{
  /// <summary>
  /// Initializes a new instance of the <see cref="ValidatingAnalyzer"/> class.
  /// </summary>
  /// <param name="inner">The analyzer to wrap.</param>
  public ValidatingAnalyzer(IMediaInfoAnalyzer inner)
    : base(inner)
  {
  }

  /// <inheritdoc />
  public override Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    CancellationToken cancellationToken = default)
  {
    var failure = Validate(source);
    return failure is not null
      ? Task.FromResult(failure)
      : Inner.AnalyzeAsync(source, cancellationToken);
  }

  private static MediaAnalysisResult? Validate(IMediaSource source) =>
    source switch
    {
      null => MediaAnalysisResult.Failed(
        null,
        AnalysisFailureReason.SourceNotSpecified,
        "The media to analyze must be specified."),

      FileMediaSource file when string.IsNullOrWhiteSpace(file.Path) => MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.SourceNotSpecified,
        "The path of the media file must be specified."),

      DirectoryMediaSource directory when string.IsNullOrWhiteSpace(directory.Path) => MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.SourceNotSpecified,
        "The path of the media folder must be specified."),

      NetworkMediaSource network when string.IsNullOrWhiteSpace(network.Location) => MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.SourceNotSpecified,
        "The location of the media must be specified."),

      StreamMediaSource stream when !stream.Stream.CanRead => MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.StreamNotReadable,
        "The media stream cannot be read."),

      _ => null
    };
}
