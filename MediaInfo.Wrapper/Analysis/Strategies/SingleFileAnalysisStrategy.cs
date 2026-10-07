#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;

namespace MediaInfo.Analysis.Strategies;

/// <summary>
/// Analyzes a single media file by handing its path to the native library.
/// </summary>
public sealed class SingleFileAnalysisStrategy : IMediaAnalysisStrategy
{
  /// <inheritdoc />
  public int Priority => 30;

  /// <inheritdoc />
  public bool CanHandle(IMediaSource source) => source is FileMediaSource;

  /// <inheritdoc />
  public async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    MediaAnalysisContext context,
    CancellationToken cancellationToken)
  {
    var file = (FileMediaSource)source;
    if (!context.FileSystem.FileExists(file.Path))
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.SourceNotFound,
        $"The media file '{file.Path}' does not exist.");
    }

    var size = context.FileSystem.GetFileLength(file.Path);
    if (size == 0L)
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.SourceEmpty,
        $"The media file '{file.Path}' is empty.");
    }

    context.Report(new AnalysisProgress(AnalysisPhase.Opening, TotalBytes: size, Detail: file.Path));
    var result = await context.Probe
      .OpenAndCollectAsync(file.Path, context.Options, size, cancellationToken)
      .ConfigureAwait(false);

    context.Report(new AnalysisProgress(AnalysisPhase.Completed, size, size, file.Path));
    return result with { SourceKind = MediaSourceKind.File, SourcePath = file.Path };
  }
}

/// <summary>
/// Analyzes a media served over a network protocol by handing its location to the native library.
/// </summary>
public sealed class NetworkStreamAnalysisStrategy : IMediaAnalysisStrategy
{
  /// <inheritdoc />
  public int Priority => 20;

  /// <inheritdoc />
  public bool CanHandle(IMediaSource source) => source is NetworkMediaSource;

  /// <inheritdoc />
  public async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    MediaAnalysisContext context,
    CancellationToken cancellationToken)
  {
    var network = (NetworkMediaSource)source;
    context.Report(new AnalysisProgress(AnalysisPhase.Opening, Detail: network.Location));

    var result = await context.Probe
      .OpenAndCollectAsync(network.Location, context.Options, 0L, cancellationToken)
      .ConfigureAwait(false);

    context.Report(new AnalysisProgress(AnalysisPhase.Completed, Detail: network.Location));
    return result with { SourceKind = MediaSourceKind.Network, SourcePath = network.Location };
  }
}

/// <summary>
/// Rejects the kinds of media the native library cannot describe.
/// </summary>
/// <remarks>
/// Live streams carried over RTSP, RTMP and MMS are declined before any work is attempted, so that a caller is told
/// why nothing was produced instead of receiving an empty result.
/// </remarks>
public sealed class UnsupportedSourceStrategy : IMediaAnalysisStrategy
{
  /// <inheritdoc />
  public int Priority => 0;

  /// <inheritdoc />
  public bool CanHandle(IMediaSource source) =>
    source is NetworkMediaSource network &&
      (network.Location.IsRtsp() || network.Location.IsRtmp() || network.Location.IsMms());

  /// <inheritdoc />
  public Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    MediaAnalysisContext context,
    CancellationToken cancellationToken)
  {
    var network = (NetworkMediaSource)source;
    var protocol = network.Location.IsRtsp() ? "RTSP" : network.Location.IsRtmp() ? "RTMP" : "MMS";
    return Task.FromResult(
      MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.UnsupportedSource,
        $"The MediaInfo library does not support {protocol} live streams."));
  }
}
