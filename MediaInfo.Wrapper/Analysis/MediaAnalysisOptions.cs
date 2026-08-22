#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;

namespace MediaInfo.Analysis;

/// <summary>
/// Describes the phase an analysis is currently in.
/// </summary>
public enum AnalysisPhase
{
  /// <summary>
  /// The media is being opened.
  /// </summary>
  Opening,

  /// <summary>
  /// The structure of a disc is being discovered.
  /// </summary>
  DiscoveringStructure,

  /// <summary>
  /// Media data is being parsed.
  /// </summary>
  Parsing,

  /// <summary>
  /// The streams of the media are being read.
  /// </summary>
  Collecting,

  /// <summary>
  /// The analysis is complete.
  /// </summary>
  Completed
}

/// <summary>
/// Describes the progress of an analysis.
/// </summary>
/// <param name="Phase">The phase the analysis is currently in.</param>
/// <param name="BytesProcessed">The number of bytes that have been parsed so far.</param>
/// <param name="TotalBytes">The total number of bytes to parse, or <see langword="null"/> when it is unknown.</param>
/// <param name="Detail">A description of what is currently being processed.</param>
public sealed record AnalysisProgress(
  AnalysisPhase Phase,
  long BytesProcessed = 0L,
  long? TotalBytes = null,
  string? Detail = null)
{
  /// <summary>
  /// Gets the fraction of the media that has been parsed, when the total size is known.
  /// </summary>
  public double? Fraction =>
    TotalBytes is > 0 ? Math.Min(1d, (double)BytesProcessed / TotalBytes.Value) : null;
}

/// <summary>
/// Describes how an analysis should be performed.
/// </summary>
public sealed class MediaAnalysisOptions
{
  /// <summary>
  /// Gets the default options.
  /// </summary>
  public static MediaAnalysisOptions Default { get; } = new();

  /// <summary>
  /// Gets or sets the size of the blocks media data is submitted to the library in.
  /// </summary>
  /// <value>The default value is 64 kilobytes.</value>
  public int BufferSize { get; set; } = 64 * 1024;

  /// <summary>
  /// Gets or sets a value indicating whether blocking calls into the native library are moved off the calling thread.
  /// </summary>
  /// <remarks>
  /// This keeps the calling thread responsive, but it does not make the call cancellable. Opening a file or a network
  /// media runs to completion inside the library even when the analysis has already been abandoned.
  /// </remarks>
  /// <value>The default value is <see langword="true"/>.</value>
  public bool OffloadBlockingCalls { get; set; } = true;

  /// <summary>
  /// Gets or sets the maximum number of disc titles that are probed to determine their duration.
  /// </summary>
  /// <remarks>
  /// Every probe opens the media file of a title through the native library. A disc with many title sets is therefore
  /// noticeably slower to describe than a single file. Set the value to 0 to skip probing entirely, in which case the
  /// main title is selected by size.
  /// </remarks>
  /// <value>The default value is 32.</value>
  public int MaxProbedDiscTitles { get; set; } = 32;

  /// <summary>
  /// Gets or sets the minimum size a disc media file must have to be considered a title.
  /// </summary>
  /// <remarks>
  /// Discs carry short navigation and menu clips next to the feature. Ignoring the smallest files keeps them out of
  /// the title list.
  /// </remarks>
  /// <value>The default value is 1 megabyte.</value>
  public long MinimumDiscTitleSize { get; set; } = 1024L * 1024L;
}
