#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using MediaInfo.Analysis.Pipeline;
using MediaInfo.Analysis.Strategies;
using MediaInfo.Analysis.Strategies.Selection;

namespace MediaInfo.Analysis.Tests.Fakes;

/// <summary>
/// Builds an analysis context whose native library and file system are fakes.
/// </summary>
public static class TestContext
{
  /// <summary>
  /// Creates a context that runs every step on the calling thread, so that tests stay deterministic.
  /// </summary>
  public static MediaAnalysisContext Create(
    FakeNativeMediaInfoFactory factory,
    FakeFileSystem fileSystem,
    MediaAnalysisOptions? options = null,
    IProgress<AnalysisProgress>? progress = null) =>
    new(
      new MediaProbe(factory, DefaultStreamSelectionStrategy.Instance),
      fileSystem,
      options ?? new MediaAnalysisOptions { OffloadBlockingCalls = false },
      progress);
}

/// <summary>
/// Collects the progress reports an analysis produces.
/// </summary>
public sealed class ProgressRecorder : IProgress<AnalysisProgress>
{
  /// <summary>Gets the reports that have been received, in order.</summary>
  public List<AnalysisProgress> Reports { get; } = [];

  /// <inheritdoc />
  public void Report(AnalysisProgress value) => Reports.Add(value);
}
