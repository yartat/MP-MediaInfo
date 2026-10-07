#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using MediaInfo.Model;

namespace MediaInfo.Analysis.Results;

/// <summary>
/// Presents a <see cref="MediaAnalysisResult"/> through the flat property surface of the original wrapper.
/// </summary>
/// <remarks>
/// Code written against <see cref="MediaInfoWrapper"/> reads about forty properties directly off the object that
/// performed the analysis. This adapter exposes the same names with the same meanings over an immutable result, so
/// that migrating means changing the one line that produces the object rather than every line that reads from it.
/// <para>
/// The values are identical to those the wrapper produced, which the parity tests assert file by file over the whole
/// media corpus. <see cref="Duration"/> is therefore in milliseconds, as it always was.
/// </para>
/// </remarks>
public sealed class LegacyResultAdapter
{
  private readonly MediaAnalysisResult _result;

  /// <summary>
  /// Initializes a new instance of the <see cref="LegacyResultAdapter"/> class.
  /// </summary>
  /// <param name="result">The outcome to present.</param>
  /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
  public LegacyResultAdapter(MediaAnalysisResult result)
  {
    _result = result ?? throw new ArgumentNullException(nameof(result));
  }

  /// <summary>Gets the outcome this adapter presents.</summary>
  public MediaAnalysisResult Result => _result;

  #region Video

  /// <summary>Gets a value indicating whether the media has at least one video stream.</summary>
  public bool HasVideo => _result.HasVideo;

  /// <summary>Gets a value indicating whether the media has at least one video stream with a 3D effect.</summary>
  public bool Is3D => _result.Is3D;

  /// <summary>Gets a value indicating whether the media has at least one video stream with a high dynamic range.</summary>
  public bool IsHdr => _result.IsHdr;

  /// <summary>Gets the video streams of the media.</summary>
  public IList<VideoStream> VideoStreams => new List<VideoStream>(_result.VideoStreams);

  /// <summary>Gets the video stream that best represents the media.</summary>
  public VideoStream? BestVideoStream => _result.BestVideoStream;

  /// <summary>Gets the codec name of the best video stream.</summary>
  public string VideoCodec => _result.BestVideoStream?.CodecName ?? string.Empty;

  /// <summary>Gets the frame rate of the best video stream.</summary>
  public double Framerate => _result.BestVideoStream?.FrameRate ?? 0;

  /// <summary>Gets the width of the best video stream.</summary>
  public int Width => _result.BestVideoStream?.Width ?? 0;

  /// <summary>Gets the height of the best video stream.</summary>
  public int Height => _result.BestVideoStream?.Height ?? 0;

  /// <summary>Gets the display aspect ratio of the best video stream as a friendly name.</summary>
  public string AspectRatio =>
    _result.BestVideoStream is null
      ? string.Empty
      : GetAspectRatioText(_result.BestVideoStream.DisplayAspectRatio ?? string.Empty);

  /// <summary>Gets the scan type of the best video stream, in lower case.</summary>
  public string ScanType => _result.BestVideoStream?.ScanType?.ToLowerInvariant() ?? string.Empty;

  /// <summary>Gets a value indicating whether the best video stream is interlaced.</summary>
  public bool IsInterlaced => _result.BestVideoStream?.Interlaced ?? false;

  /// <summary>Gets the resolution name of the best video stream.</summary>
  public string VideoResolution => _result.BestVideoStream?.Resolution ?? string.Empty;

  /// <summary>Gets the bitrate of the best video stream.</summary>
  public int VideoRate => (int?)_result.BestVideoStream?.Bitrate ?? 0;

  /// <summary>Gets the rotation of the best video stream.</summary>
  public int VideoRotation => (int?)_result.BestVideoStream?.Rotation ?? 0;

  #endregion

  #region Audio

  /// <summary>Gets the audio streams of the media.</summary>
  public IList<AudioStream> AudioStreams => new List<AudioStream>(_result.AudioStreams);

  /// <summary>Gets the audio stream that best represents the media.</summary>
  public AudioStream? BestAudioStream => _result.BestAudioStream;

  /// <summary>Gets the codec name of the best audio stream.</summary>
  public string AudioCodec => _result.BestAudioStream?.CodecName ?? string.Empty;

  /// <summary>Gets the bitrate of the best audio stream.</summary>
  public int AudioRate => (int?)_result.BestAudioStream?.Bitrate ?? 0;

  /// <summary>Gets the sampling rate of the best audio stream.</summary>
  public int AudioSampleRate => (int?)_result.BestAudioStream?.SamplingRate ?? 0;

  /// <summary>Gets the channel count of the best audio stream.</summary>
  public int AudioChannels => _result.BestAudioStream?.Channel ?? 0;

  /// <summary>Gets the total number of audio channels across every audio stream.</summary>
  public int AudioChannelsTotal => _result.General.AudioChannelsTotal;

  /// <summary>Gets the friendly channel description of the best audio stream.</summary>
  public string AudioChannelsFriendly => _result.BestAudioStream?.AudioChannelsFriendly ?? string.Empty;

  #endregion

  #region Subtitles, chapters and menus

  /// <summary>Gets the subtitle streams of the media.</summary>
  public IList<SubtitleStream> Subtitles => new List<SubtitleStream>(_result.Subtitles);

  /// <summary>Gets a value indicating whether the media has at least one subtitle stream.</summary>
  public bool HasSubtitles => _result.HasSubtitles;

  /// <summary>Gets a value indicating whether subtitle files sit next to the media.</summary>
  public bool HasExternalSubtitles => _result.HasExternalSubtitles;

  /// <summary>Gets the chapters of the media.</summary>
  public IList<ChapterStream> Chapters => new List<ChapterStream>(_result.Chapters);

  /// <summary>Gets a value indicating whether the media has at least one chapter.</summary>
  public bool HasChapters => _result.HasChapters;

  /// <summary>Gets the menu streams of the media.</summary>
  public IList<MenuStream> MenuStreams => new List<MenuStream>(_result.MenuStreams);

  #endregion

  #region General

  /// <summary>Gets a value indicating whether the media was analyzed successfully.</summary>
  public bool Success => _result.Success;

  /// <summary>Gets a value indicating whether the analyzed media is a DVD video structure.</summary>
  public bool IsDvd => _result.IsDvd;

  /// <summary>Gets a value indicating whether the analyzed media is a Blu-ray structure.</summary>
  public bool IsBluRay => _result.IsBluRay;

  /// <summary>Gets the container format name.</summary>
  public string Format => _result.General.Format;

  /// <summary>Gets the container format version.</summary>
  public string FormatVersion => _result.General.FormatVersion;

  /// <summary>Gets the container format profile.</summary>
  public string Profile => _result.General.Profile;

  /// <summary>Gets the container codec identifier.</summary>
  public string Codec => _result.General.Codec;

  /// <summary>Gets a value indicating whether the media can be played while it is still being received.</summary>
  public bool IsStreamable => _result.General.IsStreamable;

  /// <summary>Gets the name of the application that produced the media.</summary>
  public string WritingApplication => _result.General.WritingApplication;

  /// <summary>Gets the name of the library that produced the media.</summary>
  public string WritingLibrary => _result.General.WritingLibrary;

  /// <summary>Gets the description of the media attachments.</summary>
  public string Attachments => _result.General.Attachments;

  /// <summary>Gets the duration of the media in milliseconds.</summary>
  public int Duration => (int)_result.General.Duration.TotalMilliseconds;

  /// <summary>Gets the size of the media in bytes.</summary>
  public long Size => _result.General.Size;

  /// <summary>Gets the version of the native library that performed the analysis.</summary>
  public string? Version => _result.LibraryVersion;

  /// <summary>Gets the tags of the media.</summary>
  public AudioTags Tags => _result.General.Tags;

  /// <summary>Gets the full report produced by the library.</summary>
  public string Text => _result.General.Text;

  #endregion

  private static string GetAspectRatioText(string ratio) =>
    ratio switch
    {
      "4:3" or "1.333" => "fullscreen",
      "3:2" or "1.5" => "classic widescreen",
      "1:1" or "1.0" or "1" or "2.0" => "square",
      "5:4" or "1.25" => "classic",
      "4:5" or "0.8" or "9:16" => "vertical",
      "1.90:1" or "1.9" or "1.90" or "1.89" => "cinema",
      "2.39:1" or "2.39" or "2.40:1" or "2.40" or "2.35:1" or "2.35" => "scope",
      "2.76:1" or "2.76" => "ultra widescreen",
      _ => "widescreen"
    };
}

/// <summary>
/// Provides the legacy property surface for an analysis outcome.
/// </summary>
public static class LegacyResultAdapterExtensions
{
  /// <summary>
  /// Presents the outcome through the flat property surface of the original wrapper.
  /// </summary>
  /// <param name="result">The outcome to present.</param>
  /// <returns>Returns an adapter over the outcome.</returns>
  public static LegacyResultAdapter AsLegacy(this MediaAnalysisResult result) => new(result);
}
