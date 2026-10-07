#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MediaInfo.Analysis.Results;

namespace MediaInfo.Analysis.Decorators;

/// <summary>
/// Stores the outcome of an analysis so that the same media does not have to be read twice.
/// </summary>
/// <remarks>
/// A <see cref="MediaAnalysisResult"/> is immutable, so an entry can be handed to any number of callers on any number
/// of threads. An implementation must be safe to call concurrently.
/// </remarks>
public interface IAnalysisCache
{
  /// <summary>
  /// Gets the stored outcome for the specified key.
  /// </summary>
  /// <param name="key">The key the outcome was stored under.</param>
  /// <param name="result">When this method returns <see langword="true"/>, contains the stored outcome.</param>
  /// <returns>Returns <see langword="true"/> when an outcome was stored; otherwise, <see langword="false"/>.</returns>
  bool TryGet(string key, [NotNullWhen(true)] out MediaAnalysisResult? result);

  /// <summary>
  /// Stores the outcome for the specified key.
  /// </summary>
  /// <param name="key">The key to store the outcome under.</param>
  /// <param name="result">The outcome to store.</param>
  void Set(string key, MediaAnalysisResult result);

  /// <summary>
  /// Removes the outcome stored under the specified key.
  /// </summary>
  /// <param name="key">The key to remove.</param>
  void Remove(string key);

  /// <summary>
  /// Removes every stored outcome.
  /// </summary>
  void Clear();
}

/// <summary>
/// Keeps analysis outcomes in memory, for a limited time and up to a limited count.
/// </summary>
/// <remarks>
/// When the cache is full the oldest entry is evicted. This is deliberately simpler than a least recently used
/// policy: an analysis is cheap enough to repeat that the cost of tracking access order is not worth paying.
/// </remarks>
public sealed class MemoryAnalysisCache : IAnalysisCache
{
  private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.Ordinal);
  private readonly TimeSpan _timeToLive;
  private readonly int _capacity;

  /// <summary>
  /// Initializes a new instance of the <see cref="MemoryAnalysisCache"/> class.
  /// </summary>
  /// <param name="timeToLive">How long an entry stays valid, or <see langword="null"/> to keep entries indefinitely.</param>
  /// <param name="capacity">The greatest number of entries to keep.</param>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is not greater than zero.</exception>
  public MemoryAnalysisCache(TimeSpan? timeToLive = null, int capacity = 256)
  {
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "The capacity must be greater than zero.");
    }

    _timeToLive = timeToLive ?? TimeSpan.MaxValue;
    _capacity = capacity;
  }

  /// <summary>
  /// Gets the number of entries currently stored.
  /// </summary>
  public int Count => _entries.Count;

  /// <inheritdoc />
  public bool TryGet(string key, [NotNullWhen(true)] out MediaAnalysisResult? result)
  {
    result = null;
    if (key is null || !_entries.TryGetValue(key, out var entry))
    {
      return false;
    }

    if (IsExpired(entry))
    {
      _entries.TryRemove(key, out _);
      return false;
    }

    result = entry.Result;
    return true;
  }

  /// <inheritdoc />
  public void Set(string key, MediaAnalysisResult result)
  {
    if (key is null || result is null)
    {
      return;
    }

    _entries[key] = new Entry(result, DateTimeOffset.UtcNow);
    Trim();
  }

  /// <inheritdoc />
  public void Remove(string key)
  {
    if (key is not null)
    {
      _entries.TryRemove(key, out _);
    }
  }

  /// <inheritdoc />
  public void Clear() => _entries.Clear();

  private bool IsExpired(Entry entry) =>
    _timeToLive != TimeSpan.MaxValue && DateTimeOffset.UtcNow - entry.StoredAt > _timeToLive;

  private void Trim()
  {
    while (_entries.Count > _capacity)
    {
      var oldest = _entries.OrderBy(x => x.Value.StoredAt).Select(x => x.Key).FirstOrDefault();
      if (oldest is null || !_entries.TryRemove(oldest, out _))
      {
        // Another thread already evicted it; the count will be rechecked on the next pass.
        return;
      }
    }
  }

  private sealed record Entry(MediaAnalysisResult Result, DateTimeOffset StoredAt);
}
