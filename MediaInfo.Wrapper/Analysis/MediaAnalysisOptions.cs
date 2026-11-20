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
  /// <exception cref="ArgumentOutOfRangeException">The value is not greater than zero.</exception>
  public int BufferSize
  {
    get => _bufferSize;
    set => _bufferSize = value > 0
      ? value
      : throw new ArgumentOutOfRangeException(nameof(value), value, "The buffer must hold at least one byte.");
  }

  private int _bufferSize = 64 * 1024;

  /// <summary>
  /// Gets or sets the greatest number of bytes read from a stream.
  /// </summary>
  /// <remarks>
  /// A stream that cannot seek may never end, and the library may never decide that it has seen enough. A stream that
  /// can seek is read where the library asks to, and a damaged or hostile file can make it ask for ever. When the limit
  /// is reached the analysis goes on with what has been read, and the result carries a warning.
  /// </remarks>
  /// <value>
  /// The default value is <see langword="null"/>. A stream that cannot seek is then read to its end or until the library
  /// is done. A stream that can seek is read up to four times its length plus 32 MiB, which no parse comes near.
  /// </value>
  public long? MaximumStreamBytes { get; set; }

  /// <summary>
  /// Gets or sets a value indicating whether blocking calls into the native library are moved off the calling thread.
  /// </summary>
  /// <remarks>
  /// This keeps the calling thread responsive and lets a cancellation or a timeout release the caller at once. It does
  /// not interrupt the call: opening a file or a network media still runs to completion inside the library, on its own
  /// thread, and holds the file or the connection until it does. When the value is <see langword="false"/> the call runs
  /// on the calling thread and nothing can release the caller before it returns.
  /// </remarks>
  /// <value>The default value is <see langword="true"/>.</value>
  public bool OffloadBlockingCalls { get; set; } = true;

  /// <summary>
  /// Gets or sets how many native calls may still be running after the caller gave up on them.
  /// </summary>
  /// <remarks>
  /// A timeout or a cancellation releases the caller, not the native call: it keeps its thread, and with a network
  /// media its connection, until the library returns, which for a server that never answers is never. The library
  /// offers no timeout of its own. Once this many such calls are outstanding, a new analysis that needs the library is
  /// refused at once with <see cref="Results.AnalysisFailureReason.Busy"/> instead of adding another thread and connection, and
  /// analyses are accepted again as the outstanding calls return. Counted per analyzer. Only meaningful while
  /// <see cref="OffloadBlockingCalls"/> is <see langword="true"/>.
  /// </remarks>
  /// <value>The default value is 16.</value>
  /// <exception cref="ArgumentOutOfRangeException">The value is not greater than zero.</exception>
  public int MaximumAbandonedNativeCalls
  {
    get => _maximumAbandonedNativeCalls;
    set => _maximumAbandonedNativeCalls = value > 0
      ? value
      : throw new ArgumentOutOfRangeException(nameof(value), value, "At least one abandoned call must be allowed.");
  }

  private int _maximumAbandonedNativeCalls = 16;

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
