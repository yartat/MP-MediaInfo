#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using MediaInfo.Model;

namespace MediaInfo.Analysis.Strategies.Selection;

/// <summary>
/// Selects the streams that best represent a media.
/// </summary>
/// <remarks>
/// Which stream is the best one is a policy rather than a fact. A player that always wants the highest resolution and
/// a player that always wants a particular language need different answers from the same media.
/// </remarks>
public interface IStreamSelectionStrategy
{
  /// <summary>
  /// Selects the video stream that best represents the media.
  /// </summary>
  /// <param name="streams">The video streams of the media.</param>
  /// <returns>Returns the selected stream, or <see langword="null"/> when there is none.</returns>
  VideoStream? SelectBestVideo(IReadOnlyList<VideoStream> streams);

  /// <summary>
  /// Selects the audio stream that best represents the media.
  /// </summary>
  /// <param name="streams">The audio streams of the media.</param>
  /// <returns>Returns the selected stream, or <see langword="null"/> when there is none.</returns>
  AudioStream? SelectBestAudio(IReadOnlyList<AudioStream> streams);
}

/// <summary>
/// Selects the streams that best represent a media by weighing every property that contributes to quality.
/// </summary>
/// <remarks>
/// This reproduces the selection the wrapper has always performed. A video stream is ranked by the product of its
/// frame size, bit depth, frame rate, bitrate and whether it carries a 3D effect. An audio stream is ranked by its
/// channel count first and its bitrate second.
/// </remarks>
public sealed class DefaultStreamSelectionStrategy : IStreamSelectionStrategy
{
  /// <summary>
  /// Gets a shared instance of the strategy.
  /// </summary>
  public static DefaultStreamSelectionStrategy Instance { get; } = new();

  /// <inheritdoc />
  public VideoStream? SelectBestVideo(IReadOnlyList<VideoStream> streams) =>
    streams is null || streams.Count == 0
      ? null
      : streams
        .OrderByDescending(x =>
          (long)x.Width * x.Height * x.BitDepth *
          (x.Stereoscopic == StereoMode.Mono ? 1L : 2L) *
          x.FrameRate *
          (x.Bitrate <= 1e-7 ? 1 : x.Bitrate))
        .FirstOrDefault();

  /// <inheritdoc />
  public AudioStream? SelectBestAudio(IReadOnlyList<AudioStream> streams) =>
    streams is null || streams.Count == 0
      ? null
      : streams
        .OrderByDescending(x => (x.Channel * 10000000) + x.Bitrate)
        .FirstOrDefault();
}

/// <summary>
/// Selects the streams with the highest bitrate.
/// </summary>
public sealed class HighestBitrateStreamSelectionStrategy : IStreamSelectionStrategy
{
  /// <inheritdoc />
  public VideoStream? SelectBestVideo(IReadOnlyList<VideoStream> streams) =>
    streams is null || streams.Count == 0 ? null : streams.OrderByDescending(x => x.Bitrate).FirstOrDefault();

  /// <inheritdoc />
  public AudioStream? SelectBestAudio(IReadOnlyList<AudioStream> streams) =>
    streams is null || streams.Count == 0 ? null : streams.OrderByDescending(x => x.Bitrate).FirstOrDefault();
}

/// <summary>
/// Restricts the selection to the streams of preferred languages, and delegates the ranking to another strategy.
/// </summary>
/// <remarks>
/// When no stream matches any of the preferred languages the whole set is passed to the inner strategy, so that a
/// media that carries only unexpected languages still produces a selection.
/// </remarks>
public sealed class PreferredLanguageSelectionStrategy : IStreamSelectionStrategy
{
  private readonly IStreamSelectionStrategy _inner;
  private readonly string[] _languages;

  /// <summary>
  /// Initializes a new instance of the <see cref="PreferredLanguageSelectionStrategy"/> class.
  /// </summary>
  /// <param name="inner">The strategy that ranks the streams of the preferred languages.</param>
  /// <param name="languages">The preferred languages, most preferred first.</param>
  /// <exception cref="ArgumentNullException"><paramref name="inner"/> or <paramref name="languages"/> is <see langword="null"/>.</exception>
  public PreferredLanguageSelectionStrategy(IStreamSelectionStrategy inner, params string[] languages)
  {
    _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    _languages = languages ?? throw new ArgumentNullException(nameof(languages));
  }

  /// <inheritdoc />
  public VideoStream? SelectBestVideo(IReadOnlyList<VideoStream> streams) =>
    _inner.SelectBestVideo(Preferred(streams));

  /// <inheritdoc />
  public AudioStream? SelectBestAudio(IReadOnlyList<AudioStream> streams) =>
    _inner.SelectBestAudio(Preferred(streams));

  private IReadOnlyList<TStream> Preferred<TStream>(IReadOnlyList<TStream> streams)
    where TStream : LanguageMediaStream
  {
    if (streams is null || streams.Count == 0 || _languages.Length == 0)
    {
      return streams ?? [];
    }

    foreach (var language in _languages)
    {
      var matching = streams
        .Where(x =>
          string.Equals(x.Language, language, StringComparison.OrdinalIgnoreCase) ||
          string.Equals(x.LanguageIetf, language, StringComparison.OrdinalIgnoreCase))
        .ToArray();
      if (matching.Length > 0)
      {
        return matching;
      }
    }

    return streams;
  }
}
