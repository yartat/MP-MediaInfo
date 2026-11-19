#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;
using FluentAssertions;
using MediaInfo.Analysis.Sources;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for recognising a disc folder without walking the folders around it.</summary>
public sealed class DiscFolderDetectionTests : IDisposable
{
  private readonly string _root = Path.Combine(Path.GetTempPath(), "mi-disc-" + Guid.NewGuid().ToString("N"));

  public DiscFolderDetectionTests() => Directory.CreateDirectory(_root);

  public void Dispose() => Directory.Delete(_root, recursive: true);

  private string Create(params string[] parts)
  {
    var path = Path.Combine([_root, .. parts]);
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllBytes(path, [0]);
    return path;
  }

  [Fact]
  public void From_DiscRootWithVideoTs_ReturnsTheVideoTsFolder()
  {
    Create("disc", "VIDEO_TS", "VIDEO_TS.IFO");

    var source = MediaSource.From(Path.Combine(_root, "disc"));

    source.Should().BeOfType<DirectoryMediaSource>()
      .Which.Path.Should().Be(Path.Combine(_root, "disc", "VIDEO_TS"));
  }

  [Fact]
  public void From_VideoTsFolderItself_ReturnsIt()
  {
    Create("disc", "VIDEO_TS", "VTS_01_0.IFO");

    var source = MediaSource.From(Path.Combine(_root, "disc", "VIDEO_TS"));

    source.Should().BeOfType<DirectoryMediaSource>()
      .Which.Path.Should().Be(Path.Combine(_root, "disc", "VIDEO_TS"));
  }

  [Fact]
  public void From_DiscRootWithBdmv_ReturnsTheBdmvFolder()
  {
    Create("disc", "BDMV", "index.bdmv");

    var source = MediaSource.From(Path.Combine(_root, "disc"));

    source.Should().BeOfType<DirectoryMediaSource>()
      .Which.Path.Should().Be(Path.Combine(_root, "disc", "BDMV"));
  }

  [Fact]
  public void From_FolderThatOnlyHoldsDiscs_IsNotTakenForOneOfThem()
  {
    Create("library", "first", "VIDEO_TS", "VIDEO_TS.IFO");
    Create("library", "second", "BDMV", "index.bdmv");
    Create("library", "deep", "a", "b", "VIDEO_TS", "VIDEO_TS.IFO");

    var source = MediaSource.From(Path.Combine(_root, "library"));

    source.Should().BeOfType<DirectoryMediaSource>()
      .Which.Path.Should().Be(Path.Combine(_root, "library"));
  }

  [Fact]
  public void From_ExtensionThatOnlyStartsLikeIfo_IsNotAnIfoFile()
  {
    Create("folder", "title.ifox");

    var source = MediaSource.From(Path.Combine(_root, "folder"));

    source.Should().BeOfType<DirectoryMediaSource>()
      .Which.Path.Should().Be(Path.Combine(_root, "folder"));
  }

  [Fact]
  public void From_IfoFile_ReturnsItsFolder()
  {
    var ifo = Create("disc", "VIDEO_TS", "VIDEO_TS.IFO");

    var source = MediaSource.From(ifo);

    source.Should().BeOfType<DirectoryMediaSource>()
      .Which.Path.Should().Be(Path.GetDirectoryName(ifo));
  }
}
