#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;

namespace MediaInfo.Analysis.Results;

/// <summary>
/// Describes why an analysis did not produce media information.
/// </summary>
public enum AnalysisFailureReason
{
  /// <summary>
  /// The analysis succeeded.
  /// </summary>
  None,

  /// <summary>
  /// The media was not specified.
  /// </summary>
  SourceNotSpecified,

  /// <summary>
  /// The specified file, directory or URL does not exist.
  /// </summary>
  SourceNotFound,

  /// <summary>
  /// The specified media contains no data.
  /// </summary>
  SourceEmpty,

  /// <summary>
  /// The kind of the specified media is not supported.
  /// </summary>
  UnsupportedSource,

  /// <summary>
  /// The native library could not be loaded.
  /// </summary>
  NativeLibraryUnavailable,

  /// <summary>
  /// The native library refused to open the media.
  /// </summary>
  NativeOpenFailed,

  /// <summary>
  /// The media was opened but contains no video, audio or subtitle stream.
  /// </summary>
  NoStreamsFound,

  /// <summary>
  /// The specified stream cannot be read.
  /// </summary>
  StreamNotReadable,

  /// <summary>
  /// The disc structure could not be discovered.
  /// </summary>
  DiscStructureUnreadable,

  /// <summary>
  /// The analysis was cancelled.
  /// </summary>
  Cancelled,

  /// <summary>
  /// The analysis did not complete within the allotted time.
  /// </summary>
  Timeout,

  /// <summary>
  /// The analysis failed for an unexpected reason.
  /// </summary>
  Unknown
}

/// <summary>
/// Describes the reason an analysis did not produce media information.
/// </summary>
/// <param name="Reason">The category of the failure.</param>
/// <param name="Message">A description of the failure.</param>
/// <param name="Exception">The exception that caused the failure, when the failure was caused by one.</param>
public sealed record AnalysisFailure(AnalysisFailureReason Reason, string Message, Exception? Exception = null)
{
  /// <inheritdoc />
  public override string ToString() => $"{Reason}: {Message}";
}
