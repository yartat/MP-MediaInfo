#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;

namespace MediaInfo.Analysis.Native;

/// <summary>
/// Adapts the handle based <see cref="MediaInfo"/> library binding to the <see cref="INativeMediaInfo"/> interface.
/// </summary>
/// <remarks>
/// The adapter does not reimplement any platform invoke declaration. It translates the pointer and bit flag results of
/// the binding into the types the analysis pipeline works with, so that the pipeline can be driven by a test double.
/// </remarks>
internal sealed class MediaInfoLibAdapter : INativeMediaInfo
{
  private readonly MediaInfo _mediaInfo;
  private bool _disposed;

  /// <summary>
  /// Initializes a new instance of the <see cref="MediaInfoLibAdapter"/> class.
  /// </summary>
  /// <param name="mediaInfo">The library binding to adapt.</param>
  /// <exception cref="ArgumentNullException"><paramref name="mediaInfo"/> is <see langword="null"/>.</exception>
  public MediaInfoLibAdapter(MediaInfo mediaInfo)
  {
    _mediaInfo = mediaInfo ?? throw new ArgumentNullException(nameof(mediaInfo));
  }

  /// <inheritdoc />
  public bool IsAvailable => !_disposed && _mediaInfo.Handle != IntPtr.Zero;

  /// <inheritdoc />
  public string? LibraryVersion => IsAvailable ? _mediaInfo.Option("Info_Version") : null;

  /// <inheritdoc />
  public bool Open(string fileName) => _mediaInfo.Open(fileName) != IntPtr.Zero;

  /// <inheritdoc />
  public void OpenBufferInit(long mediaSize, long mediaOffset) => _mediaInfo.OpenBufferInit(mediaSize, mediaOffset);

  /// <inheritdoc />
  public unsafe MediaInfoBufferStatus OpenBufferContinue(ReadOnlySpan<byte> buffer)
  {
    if (buffer.IsEmpty)
    {
      return MediaInfoBufferStatus.None;
    }

    fixed (byte* pBuffer = buffer)
    {
      return (MediaInfoBufferStatus)_mediaInfo.OpenBufferContinue(pBuffer, buffer.Length);
    }
  }

  /// <inheritdoc />
  public long OpenBufferContinueGoToGet() => _mediaInfo.OpenBufferContinueGoToGet();

  /// <inheritdoc />
  public void OpenBufferFinalize() => _mediaInfo.OpenBufferFinalize();

  /// <inheritdoc />
  public void Close() => _mediaInfo.Close();

  /// <inheritdoc />
  public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch) =>
    _mediaInfo.Get(streamKind, streamNumber, parameter, kindOfInfo, kindOfSearch);

  /// <inheritdoc />
  public string Get(StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo) =>
    _mediaInfo.Get(streamKind, streamNumber, parameter, kindOfInfo);

  /// <inheritdoc />
  public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo) =>
    _mediaInfo.Get(streamKind, streamNumber, parameter, kindOfInfo);

  /// <inheritdoc />
  public string Get(StreamKind streamKind, int streamNumber, string parameter) =>
    _mediaInfo.Get(streamKind, streamNumber, parameter);

  /// <inheritdoc />
  public string Get(StreamKind streamKind, int streamNumber, int parameter) =>
    _mediaInfo.Get(streamKind, streamNumber, parameter);

  /// <inheritdoc />
  public int CountGet(StreamKind streamKind, int streamNumber) => _mediaInfo.CountGet(streamKind, streamNumber);

  /// <inheritdoc />
  public int CountGet(StreamKind streamKind) => _mediaInfo.CountGet(streamKind);

  /// <inheritdoc />
  public string Option(string option, string value) => _mediaInfo.Option(option, value);

  /// <inheritdoc />
  public string Option(string option) => _mediaInfo.Option(option);

  /// <inheritdoc />
  public string Inform() => _mediaInfo.Inform();

  /// <inheritdoc />
  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    _mediaInfo.Dispose();
  }
}
