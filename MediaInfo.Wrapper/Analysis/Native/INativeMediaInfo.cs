#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;

namespace MediaInfo.Analysis.Native;

/// <summary>
/// Describes a single media handle of the MediaInfo library.
/// </summary>
/// <remarks>
/// A handle is not thread safe. Create one per analysis, never share it between concurrent operations, and dispose it
/// as soon as the analysis is complete. This interface is the seam that allows the analysis pipeline to be tested
/// without the native library being present.
/// </remarks>
public interface INativeMediaInfo : IMediaInfoReader, IDisposable
{
  /// <summary>
  /// Gets a value indicating whether the native library has been loaded and the handle is usable.
  /// </summary>
  bool IsAvailable { get; }

  /// <summary>
  /// Gets the version of the loaded native library.
  /// </summary>
  string? LibraryVersion { get; }

  /// <summary>
  /// Opens the specified media for reading.
  /// </summary>
  /// <param name="fileName">The path or URL of the media to open.</param>
  /// <returns>Returns <see langword="true"/> when the media has been opened; otherwise, <see langword="false"/>.</returns>
  bool Open(string fileName);

  /// <summary>
  /// Prepares the library to receive media data as a sequence of buffers.
  /// </summary>
  /// <param name="mediaSize">The total size of the media, or -1 when it is unknown.</param>
  /// <param name="mediaOffset">The offset within the media the next buffer starts at.</param>
  void OpenBufferInit(long mediaSize, long mediaOffset);

  /// <summary>
  /// Submits a block of media data for parsing.
  /// </summary>
  /// <param name="buffer">The block of media data.</param>
  /// <returns>Returns the state reported by the library after the block has been parsed.</returns>
  MediaInfoBufferStatus OpenBufferContinue(ReadOnlySpan<byte> buffer);

  /// <summary>
  /// Gets the offset within the media the library wants to read next.
  /// </summary>
  /// <returns>Returns the requested offset, or -1 when the library wants to continue sequentially.</returns>
  long OpenBufferContinueGoToGet();

  /// <summary>
  /// Completes parsing of a media that has been submitted as a sequence of buffers.
  /// </summary>
  void OpenBufferFinalize();

  /// <summary>
  /// Closes the currently opened media, leaving the handle available for reuse.
  /// </summary>
  void Close();
}
