#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using MediaInfo.Analysis.Native;

namespace MediaInfo.Analysis.Tests.Fakes;

/// <summary>
/// Describes what a fake media reports when it is opened.
/// </summary>
public sealed class FakeMediaProfile
{
  private readonly Dictionary<(StreamKind Kind, int Number, string Parameter), string> _named =
    new(TupleComparer<string>.Instance);

  private readonly Dictionary<(StreamKind Kind, int Number, int Parameter), string> _indexed =
    new(TupleComparer<int>.Instance);

  /// <summary>Gets or sets a value indicating whether the media can be opened.</summary>
  public bool CanOpen { get; set; } = true;

  /// <summary>Gets the number of streams of each kind the media reports.</summary>
  public Dictionary<StreamKind, int> Counts { get; } = [];

  /// <summary>Gets or sets the report the media produces.</summary>
  public string Inform { get; set; } = string.Empty;

  /// <summary>Sets the duration the media reports on its container, video and audio streams.</summary>
  public FakeMediaProfile WithDuration(TimeSpan duration)
  {
    var milliseconds = duration.TotalMilliseconds.ToString("0.###", CultureInfo.InvariantCulture);
    SetIndexed(StreamKind.General, 0, (int)NativeMethods.General.General_Duration, milliseconds);
    SetIndexed(StreamKind.Video, 0, (int)NativeMethods.Video.Video_Duration, milliseconds);
    SetIndexed(StreamKind.Audio, 0, (int)NativeMethods.Audio.Audio_Duration, milliseconds);
    return this;
  }

  /// <summary>Sets the number of streams of the given kind, so that the collector builds that many.</summary>
  public FakeMediaProfile WithStreams(StreamKind kind, int count)
  {
    Counts[kind] = count;
    return this;
  }

  /// <summary>Sets the value of a parameter addressed by name.</summary>
  public FakeMediaProfile Set(StreamKind kind, int number, string parameter, string value)
  {
    _named[(kind, number, parameter)] = value;
    return this;
  }

  /// <summary>Sets the value of a parameter addressed by index.</summary>
  public FakeMediaProfile SetIndexed(StreamKind kind, int number, int parameter, string value)
  {
    _indexed[(kind, number, parameter)] = value;
    return this;
  }

  internal string Get(StreamKind kind, int number, string parameter) =>
    _named.TryGetValue((kind, number, parameter), out var value) ? value : string.Empty;

  internal string Get(StreamKind kind, int number, int parameter) =>
    _indexed.TryGetValue((kind, number, parameter), out var value) ? value : string.Empty;

  private sealed class TupleComparer<TParameter> : IEqualityComparer<(StreamKind Kind, int Number, TParameter Parameter)>
  {
    public static TupleComparer<TParameter> Instance { get; } = new();

    public bool Equals(
      (StreamKind Kind, int Number, TParameter Parameter) x,
      (StreamKind Kind, int Number, TParameter Parameter) y) =>
      x.Kind == y.Kind && x.Number == y.Number && EqualityComparer<TParameter>.Default.Equals(x.Parameter, y.Parameter);

    public int GetHashCode((StreamKind Kind, int Number, TParameter Parameter) obj) =>
      HashCode.Combine(obj.Kind, obj.Number, obj.Parameter);
  }
}

/// <summary>
/// A media handle that answers from a <see cref="FakeMediaProfile"/> instead of the native library.
/// </summary>
public sealed class FakeNativeMediaInfo : INativeMediaInfo
{
  private readonly FakeNativeMediaInfoFactory _owner;
  private FakeMediaProfile? _profile;

  internal FakeNativeMediaInfo(FakeNativeMediaInfoFactory owner)
  {
    _owner = owner;
  }

  /// <summary>Gets a value indicating whether the handle has been disposed.</summary>
  public bool IsDisposed { get; private set; }

  /// <summary>Gets the number of times the handle has been disposed.</summary>
  public int DisposeCount { get; private set; }

  /// <inheritdoc />
  public bool IsAvailable => _owner.IsLibraryAvailable && !IsDisposed;

  /// <inheritdoc />
  public string? LibraryVersion => _owner.LibraryVersion;

  /// <inheritdoc />
  public bool Open(string fileName)
  {
    _owner.OpenedPaths.Add(fileName);
    if (!_owner.Media.TryGetValue(fileName, out var profile) || !profile.CanOpen)
    {
      return false;
    }

    _profile = profile;
    return true;
  }

  /// <inheritdoc />
  public void OpenBufferInit(long mediaSize, long mediaOffset) => _owner.BufferInitCalls.Add((mediaSize, mediaOffset));

  /// <inheritdoc />
  public MediaInfoBufferStatus OpenBufferContinue(ReadOnlySpan<byte> buffer)
  {
    _owner.BytesSubmitted += buffer.Length;
    _owner.BufferContinueCalls++;
    _profile ??= _owner.StreamProfile;
    return _owner.BufferContinueCalls >= _owner.FinalizeAfterBlocks
      ? MediaInfoBufferStatus.Accepted | MediaInfoBufferStatus.Finalized
      : MediaInfoBufferStatus.Accepted;
  }

  /// <inheritdoc />
  public long OpenBufferContinueGoToGet() => _owner.RequestedOffset;

  /// <inheritdoc />
  public void OpenBufferFinalize() => _owner.BufferFinalizeCalls++;

  /// <inheritdoc />
  public void Close() => _profile = null;

  /// <inheritdoc />
  public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch) =>
    Get(streamKind, streamNumber, parameter);

  /// <inheritdoc />
  public string Get(StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo) =>
    Get(streamKind, streamNumber, parameter);

  /// <inheritdoc />
  public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo) =>
    Get(streamKind, streamNumber, parameter);

  /// <inheritdoc />
  public string Get(StreamKind streamKind, int streamNumber, string parameter) =>
    _profile?.Get(streamKind, streamNumber, parameter) ?? string.Empty;

  /// <inheritdoc />
  public string Get(StreamKind streamKind, int streamNumber, int parameter) =>
    _profile?.Get(streamKind, streamNumber, parameter) ?? string.Empty;

  /// <inheritdoc />
  public int CountGet(StreamKind streamKind, int streamNumber) => CountGet(streamKind);

  /// <inheritdoc />
  public int CountGet(StreamKind streamKind) =>
    _profile is not null && _profile.Counts.TryGetValue(streamKind, out var count) ? count : 0;

  /// <inheritdoc />
  public string Option(string option, string value)
  {
    _owner.Options[option] = value;
    return string.Empty;
  }

  /// <inheritdoc />
  public string Option(string option) => Option(option, string.Empty);

  /// <inheritdoc />
  public string Inform() => _profile?.Inform ?? string.Empty;

  /// <inheritdoc />
  public void Dispose()
  {
    IsDisposed = true;
    DisposeCount++;
  }
}

/// <summary>
/// Creates <see cref="FakeNativeMediaInfo"/> handles and records what the pipeline did with them.
/// </summary>
public sealed class FakeNativeMediaInfoFactory : INativeMediaInfoFactory
{
  /// <summary>Gets the media the factory knows about, keyed by path.</summary>
  public Dictionary<string, FakeMediaProfile> Media { get; } = new(StringComparer.OrdinalIgnoreCase);

  /// <summary>Gets the handles the factory has created.</summary>
  public List<FakeNativeMediaInfo> Created { get; } = [];

  /// <summary>Gets the paths that have been opened, in order.</summary>
  public List<string> OpenedPaths { get; } = [];

  /// <summary>Gets the arguments of every buffer initialization.</summary>
  public List<(long MediaSize, long MediaOffset)> BufferInitCalls { get; } = [];

  /// <summary>Gets the options that have been set.</summary>
  public Dictionary<string, string> Options { get; } = new(StringComparer.OrdinalIgnoreCase);

  /// <summary>Gets or sets the profile a handle reports after media data has been submitted to it.</summary>
  public FakeMediaProfile? StreamProfile { get; set; }

  /// <summary>Gets or sets a value indicating whether the native library is available.</summary>
  public bool IsLibraryAvailable { get; set; } = true;

  /// <summary>Gets or sets the version the library reports.</summary>
  public string LibraryVersion { get; set; } = "MediaInfoLib - v99.0";

  /// <summary>Gets or sets the number of submitted blocks after which parsing reports that it is finished.</summary>
  public int FinalizeAfterBlocks { get; set; } = int.MaxValue;

  /// <summary>Gets or sets the offset the library asks to jump to, or -1 to continue sequentially.</summary>
  public long RequestedOffset { get; set; } = -1L;

  /// <summary>Gets the number of blocks that have been submitted.</summary>
  public int BufferContinueCalls { get; set; }

  /// <summary>Gets the number of times parsing has been completed.</summary>
  public int BufferFinalizeCalls { get; set; }

  /// <summary>Gets the total number of bytes that have been submitted.</summary>
  public long BytesSubmitted { get; set; }

  /// <summary>Registers a media at the given path.</summary>
  public FakeMediaProfile AddMedia(string path, FakeMediaProfile? profile = null)
  {
    var result = profile ?? new FakeMediaProfile();
    Media[path] = result;
    return result;
  }

  /// <inheritdoc />
  public INativeMediaInfo Create()
  {
    var handle = new FakeNativeMediaInfo(this);
    Created.Add(handle);
    return handle;
  }
}
