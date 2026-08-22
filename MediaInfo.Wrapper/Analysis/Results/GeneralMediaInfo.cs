#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using MediaInfo.Model;

namespace MediaInfo.Analysis.Results;

/// <summary>
/// Describes the container level properties of an analyzed media.
/// </summary>
public sealed record GeneralMediaInfo
{
  /// <summary>
  /// Gets an instance that describes a media that could not be read.
  /// </summary>
  public static GeneralMediaInfo Empty { get; } = new();

  /// <summary>
  /// Gets the container format name.
  /// </summary>
  public string Format { get; init; } = string.Empty;

  /// <summary>
  /// Gets the container format version.
  /// </summary>
  public string FormatVersion { get; init; } = string.Empty;

  /// <summary>
  /// Gets the container format profile.
  /// </summary>
  public string Profile { get; init; } = string.Empty;

  /// <summary>
  /// Gets the container codec identifier.
  /// </summary>
  public string Codec { get; init; } = string.Empty;

  /// <summary>
  /// Gets the name of the application that produced the media.
  /// </summary>
  public string WritingApplication { get; init; } = string.Empty;

  /// <summary>
  /// Gets the name of the library that produced the media.
  /// </summary>
  public string WritingLibrary { get; init; } = string.Empty;

  /// <summary>
  /// Gets the description of the media attachments.
  /// </summary>
  public string Attachments { get; init; } = string.Empty;

  /// <summary>
  /// Gets a value indicating whether the media can be played while it is still being received.
  /// </summary>
  public bool IsStreamable { get; init; }

  /// <summary>
  /// Gets the size of the media in bytes.
  /// </summary>
  public long Size { get; init; }

  /// <summary>
  /// Gets the duration of the media.
  /// </summary>
  public TimeSpan Duration { get; init; }

  /// <summary>
  /// Gets the total number of audio channels across every audio stream.
  /// </summary>
  public int AudioChannelsTotal { get; init; }

  /// <summary>
  /// Gets the tags of the media.
  /// </summary>
  public AudioTags Tags { get; init; } = new();

  /// <summary>
  /// Gets the full report produced by the library.
  /// </summary>
  public string Text { get; init; } = string.Empty;
}
