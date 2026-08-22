#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace MediaInfo.Analysis.Tests.Fakes;

/// <summary>
/// A logger that keeps what it was told so that a test can assert on it.
/// </summary>
public sealed class RecordingLogger : ILogger
{
  /// <summary>Gets the entries that were logged, in order.</summary>
  public List<LogEntry> Entries { get; } = [];

  /// <inheritdoc />
  public IDisposable BeginScope<TState>(TState state)
    where TState : notnull => NullScope.Instance;

  /// <inheritdoc />
  public bool IsEnabled(LogLevel logLevel) => true;

  /// <inheritdoc />
  public void Log<TState>(
    LogLevel logLevel,
    EventId eventId,
    TState state,
    Exception? exception,
    Func<TState, Exception?, string> formatter) =>
    Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));

  /// <summary>One logged entry.</summary>
  /// <param name="Level">The severity of the entry.</param>
  /// <param name="Message">The formatted message.</param>
  /// <param name="Exception">The exception the entry carried, when it carried one.</param>
  public sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);

  private sealed class NullScope : IDisposable
  {
    public static NullScope Instance { get; } = new();

    public void Dispose()
    {
    }
  }
}
