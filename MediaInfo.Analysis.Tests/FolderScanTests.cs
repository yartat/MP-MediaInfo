#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Tests.Fakes;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for walking a folder of media.</summary>
public class FolderScanTests
{
  private const string Library = @"D:\library";

  private static FakeFileSystem CreateLibrary() =>
    new FakeFileSystem()
      .AddFile(Path.Combine(Library, "a.mkv"))
      .AddFile(Path.Combine(Library, "b.mp3"))
      .AddFile(Path.Combine(Library, "poster.jpg"))
      .AddFile(Path.Combine(Library, "notes.txt"))
      .AddFile(Path.Combine(Library, "series", "s01e01.mp4"))
      .AddFile(Path.Combine(Library, "film", "VIDEO_TS", "VTS_01_0.IFO"))
      .AddFile(Path.Combine(Library, "film", "VIDEO_TS", "VTS_01_1.VOB"))
      .AddFile(Path.Combine(Library, "concert", "BDMV", "index.bdmv"))
      .AddFile(Path.Combine(Library, "concert", "BDMV", "STREAM", "00001.m2ts"));

  private static async Task<List<MediaAnalysisResult>> ScanAsync(
    IMediaInfoAnalyzer analyzer,
    FakeFileSystem fileSystem,
    MediaFolderScanOptions? options = null,
    string folder = Library,
    CancellationToken cancellationToken = default)
  {
    var results = new List<MediaAnalysisResult>();
    await foreach (var result in analyzer.AnalyzeFolderAsync(folder, options, fileSystem, cancellationToken))
    {
      results.Add(result);
    }

    return results;
  }

  [Fact]
  public async Task Scan_SkipsFilesThatAreNotMedia()
  {
    var inner = new StubAnalyzer();

    await ScanAsync(inner, CreateLibrary());

    var names = inner.Calls.OfType<FileMediaSource>().Select(x => Path.GetFileName(x.Path)).ToArray();
    names.Should().BeEquivalentTo("a.mkv", "b.mp3", "s01e01.mp4");
    names.Should().NotContain("poster.jpg").And.NotContain("notes.txt");
  }

  [Fact]
  public async Task Scan_IncludesEveryFileWhenAskedTo()
  {
    var inner = new StubAnalyzer();

    await ScanAsync(inner, CreateLibrary(), new MediaFolderScanOptions { MediaFilesOnly = false });

    inner.Calls.OfType<FileMediaSource>().Select(x => Path.GetFileName(x.Path))
      .Should().Contain("poster.jpg");
  }

  [Fact]
  public async Task Scan_ReportsADiscFolderAsOneDiscRatherThanAsItsFiles()
  {
    var inner = new StubAnalyzer();

    await ScanAsync(inner, CreateLibrary());

    inner.Calls.OfType<DirectoryMediaSource>().Select(x => Path.GetFileName(x.Path))
      .Should().BeEquivalentTo("film", "concert");

    inner.Calls.OfType<FileMediaSource>().Should().NotContain(x =>
      x.Path.Contains("VIDEO_TS", StringComparison.OrdinalIgnoreCase) ||
      x.Path.Contains("BDMV", StringComparison.OrdinalIgnoreCase));
  }

  [Fact]
  public async Task Scan_OfADiscFolderItselfProducesOneResult()
  {
    var inner = new StubAnalyzer();

    var results = await ScanAsync(inner, CreateLibrary(), folder: Path.Combine(Library, "film"));

    results.Should().ContainSingle();
    inner.Calls.Should().ContainSingle().Which.Should().BeOfType<DirectoryMediaSource>();
  }

  [Fact]
  public async Task Scan_TreatsADiscFolderAsFilesWhenAskedTo()
  {
    var inner = new StubAnalyzer();
    var options = new MediaFolderScanOptions { TreatDiscFoldersAsDiscs = false, MediaFilesOnly = false };

    await ScanAsync(inner, CreateLibrary(), options);

    inner.Calls.Should().NotContain(x => x is DirectoryMediaSource);
    inner.Calls.OfType<FileMediaSource>().Should().Contain(x => x.Path.EndsWith("VTS_01_1.VOB"));
  }

  [Fact]
  public async Task Scan_StaysInTheTopFolderWhenNotRecursive()
  {
    var inner = new StubAnalyzer();

    await ScanAsync(inner, CreateLibrary(), new MediaFolderScanOptions { Recursive = false });

    inner.Calls.OfType<FileMediaSource>().Select(x => Path.GetFileName(x.Path))
      .Should().BeEquivalentTo("a.mkv", "b.mp3");
  }

  [Fact]
  public async Task Scan_OfAMissingFolderYieldsOneFailure()
  {
    var results = await ScanAsync(new StubAnalyzer(), CreateLibrary(), folder: @"D:\nothing");

    results.Should().ContainSingle()
      .Which.Failure!.Reason.Should().Be(AnalysisFailureReason.SourceNotFound);
  }

  [Fact]
  public async Task Scan_KeepsGoingWhenAMediaCannotBeRead()
  {
    var inner = new StubAnalyzer((source, ordinal, _) => Task.FromResult(
      ordinal == 2
        ? MediaAnalysisResult.Failed(source, AnalysisFailureReason.NativeOpenFailed, "nope")
        : new MediaAnalysisResult { Success = true }));

    var results = await ScanAsync(inner, CreateLibrary());

    results.Should().HaveCount(5, "two discs and three media files");
    results.Count(x => !x.Success).Should().Be(1);
  }

  [Fact]
  public async Task Scan_StopsWhenCancelled()
  {
    using var cancellation = new CancellationTokenSource();
    var inner = new StubAnalyzer((_, ordinal, _) =>
    {
      if (ordinal == 2)
      {
        cancellation.Cancel();
      }

      return Task.FromResult(new MediaAnalysisResult { Success = true });
    });

    var scan = () => ScanAsync(inner, CreateLibrary(), cancellationToken: cancellation.Token);

    await scan.Should().ThrowAsync<OperationCanceledException>();
    inner.CallCount.Should().BeLessThan(5);
  }

  [Fact]
  public async Task Scan_ProducesResultsAsTheyAreReadyRatherThanAtTheEnd()
  {
    var seen = 0;
    var inner = new StubAnalyzer();

    await foreach (var _ in inner.AnalyzeFolderAsync(Library, null, CreateLibrary()))
    {
      seen++;
      if (seen == 1)
      {
        // The walk has produced a result while the analyzer has been called only once.
        inner.CallCount.Should().Be(1);
      }
    }

    seen.Should().Be(5);
  }
}
