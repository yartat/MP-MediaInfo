#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using MediaInfo.Analysis.Sources;
using MediaInfo.Model;

namespace MediaInfo.Analysis.Results;

/// <summary>
/// Describes the outcome of a media analysis.
/// </summary>
/// <remarks>
/// The result is immutable, so it can be cached, shared between threads and compared without the analysis having to be
/// repeated.
/// </remarks>
public sealed record MediaAnalysisResult
{
  /// <summary>
  /// Gets a value indicating whether the media was analyzed successfully.
  /// </summary>
  public bool Success { get; init; }

  /// <summary>
  /// Gets the reason the analysis did not produce media information, when it did not.
  /// </summary>
  public AnalysisFailure? Failure { get; init; }

  /// <summary>
  /// Gets the kind of the analyzed media.
  /// </summary>
  public MediaSourceKind SourceKind { get; init; }

  /// <summary>
  /// Gets the path or URL of the analyzed media, when it has one.
  /// </summary>
  public string? SourcePath { get; init; }

  /// <summary>
  /// Gets the path of the media file the information was actually read from.
  /// </summary>
  /// <remarks>
  /// For a disc this is the media file of the main title rather than the folder that was requested.
  /// </remarks>
  public string? AnalyzedPath { get; init; }

  /// <summary>
  /// Gets the version of the native library that performed the analysis.
  /// </summary>
  public string? LibraryVersion { get; init; }

  /// <summary>
  /// Gets the time the analysis took.
  /// </summary>
  public TimeSpan Elapsed { get; init; }

  /// <summary>
  /// Gets the container level properties of the media.
  /// </summary>
  public GeneralMediaInfo General { get; init; } = GeneralMediaInfo.Empty;

  /// <summary>
  /// Gets the video streams of the media.
  /// </summary>
  public IReadOnlyList<VideoStream> VideoStreams { get; init; } = [];

  /// <summary>
  /// Gets the audio streams of the media.
  /// </summary>
  public IReadOnlyList<AudioStream> AudioStreams { get; init; } = [];

  /// <summary>
  /// Gets the subtitle streams of the media.
  /// </summary>
  public IReadOnlyList<SubtitleStream> Subtitles { get; init; } = [];

  /// <summary>
  /// Gets the chapters of the media.
  /// </summary>
  public IReadOnlyList<ChapterStream> Chapters { get; init; } = [];

  /// <summary>
  /// Gets the menu streams of the media.
  /// </summary>
  public IReadOnlyList<MenuStream> MenuStreams { get; init; } = [];

  /// <summary>
  /// Gets the video stream that best represents the media.
  /// </summary>
  public VideoStream? BestVideoStream { get; init; }

  /// <summary>
  /// Gets the audio stream that best represents the media.
  /// </summary>
  public AudioStream? BestAudioStream { get; init; }

  /// <summary>
  /// Gets the structure of the disc, when the analyzed media is an optical disc.
  /// </summary>
  public DiscStructure? Disc { get; init; }

  /// <summary>
  /// Gets a value indicating whether subtitle files were found next to the media.
  /// </summary>
  public bool HasExternalSubtitles { get; init; }

  /// <summary>
  /// Gets a value indicating whether the media has at least one video stream.
  /// </summary>
  public bool HasVideo => VideoStreams.Count > 0;

  /// <summary>
  /// Gets a value indicating whether the media has at least one audio stream.
  /// </summary>
  public bool HasAudio => AudioStreams.Count > 0;

  /// <summary>
  /// Gets a value indicating whether the media has at least one subtitle stream.
  /// </summary>
  public bool HasSubtitles => Subtitles.Count > 0;

  /// <summary>
  /// Gets a value indicating whether the media has at least one chapter.
  /// </summary>
  public bool HasChapters => Chapters.Count > 0;

  /// <summary>
  /// Gets a value indicating whether the media has at least one video stream with a 3D effect.
  /// </summary>
  public bool Is3D => VideoStreams.Any(x => x.Stereoscopic != StereoMode.Mono);

  /// <summary>
  /// Gets a value indicating whether the media has at least one video stream with a high dynamic range.
  /// </summary>
  public bool IsHdr => VideoStreams.Any(x => x.Hdr != Hdr.None);

  /// <summary>
  /// Gets a value indicating whether the analyzed media is a DVD video structure.
  /// </summary>
  public bool IsDvd => Disc?.Kind == DiscKind.Dvd;

  /// <summary>
  /// Gets a value indicating whether the analyzed media is a Blu-ray structure.
  /// </summary>
  public bool IsBluRay => Disc?.Kind == DiscKind.BluRay;

  /// <summary>
  /// Creates a result that describes a failed analysis.
  /// </summary>
  /// <param name="source">The media that was analyzed.</param>
  /// <param name="reason">The category of the failure.</param>
  /// <param name="message">A description of the failure.</param>
  /// <param name="exception">The exception that caused the failure, when the failure was caused by one.</param>
  /// <returns>Returns a result that describes the failure.</returns>
  public static MediaAnalysisResult Failed(
    IMediaSource? source,
    AnalysisFailureReason reason,
    string message,
    Exception? exception = null) =>
    new()
    {
      Success = false,
      Failure = new AnalysisFailure(reason, message, exception),
      SourceKind = source?.Kind ?? MediaSourceKind.Unknown,
      SourcePath = source?.DisplayName
    };
}
