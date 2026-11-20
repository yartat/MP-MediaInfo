#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests of the local file system against links that point back at their own parents.</summary>
public sealed class FileSystemLinkTests : IDisposable
{
  private readonly string _root = Path.Combine(Path.GetTempPath(), "mediainfo-links-" + Guid.NewGuid().ToString("n"));
  private readonly MediaInfoFileSystem _files = new();
  private readonly List<string> _links = [];

  public FileSystemLinkTests() => Directory.CreateDirectory(_root);

  public void Dispose()
  {
    // The links go first and on their own: deleting the folder would follow them.
    foreach (var link in _links)
    {
      Directory.Delete(link);
    }

    Directory.Delete(_root, recursive: true);
  }

  [Fact]
  public void GetFiles_LinkToItsOwnParent_ListsEveryFileOnce()
  {
    if (!TryLink(Path.Combine(_root, "up"), _root))
    {
      return;
    }

    File.WriteAllText(Path.Combine(_root, "a.mkv"), "x");
    Directory.CreateDirectory(Path.Combine(_root, "sub"));
    File.WriteAllText(Path.Combine(_root, "sub", "b.mkv"), "x");

    var files = RunWithin(() => _files.GetFiles(_root, "*", SearchOption.AllDirectories));

    files.Select(Path.GetFileName).Should().BeEquivalentTo("a.mkv", "b.mkv");
  }

  [Fact]
  public void GetDirectories_LeavesOutLinks()
  {
    var target = Path.Combine(_root, "target");
    Directory.CreateDirectory(target);
    if (!TryLink(Path.Combine(_root, "link"), target))
    {
      return;
    }

    _files.GetDirectories(_root).Select(Path.GetFileName).Should().Equal("target");
  }

  [Fact]
  public void GetFiles_FolderHandedInThatIsItselfALink_IsWalked()
  {
    var target = Path.Combine(_root, "target");
    Directory.CreateDirectory(target);
    File.WriteAllText(Path.Combine(target, "a.mkv"), "x");
    var link = Path.Combine(_root, "link");
    if (!TryLink(link, target))
    {
      return;
    }

    _files.GetFiles(link, "*", SearchOption.AllDirectories).Select(Path.GetFileName).Should().Equal("a.mkv");
  }

  [Fact]
  public async Task AnalyzeFolderAsync_TwoLinksToTheFolder_AnalyzesEachFileOnce()
  {
    if (!TryLink(Path.Combine(_root, "one"), _root) || !TryLink(Path.Combine(_root, "two"), _root))
    {
      return;
    }

    File.WriteAllText(Path.Combine(_root, "a.mkv"), "x");
    var analyzer = new RecordingAnalyzer();

    var results = await RunWithin(async () =>
    {
      var found = new List<MediaAnalysisResult>();
      await foreach (var result in analyzer.AnalyzeFolderAsync(_root, fileSystem: _files))
      {
        found.Add(result);
      }

      return found;
    });

    results.Should().ContainSingle();
    analyzer.Paths.Select(Path.GetFileName).Should().Equal("a.mkv");
  }

  // A walk that follows the links never ends, so the work is given a limit instead of being waited for.
  private static T RunWithin<T>(Func<T> work)
  {
    var task = Task.Run(work);
    task.Wait(TimeSpan.FromSeconds(20)).Should().BeTrue("a walk over links that point at their parents must end");
    return task.Result;
  }

  private bool TryLink(string link, string target)
  {
    if (CreateLink(link, target))
    {
      _links.Add(link);
      return true;
    }

    return false;
  }

  private static bool CreateLink(string link, string target)
  {
    try
    {
      Directory.CreateSymbolicLink(link, target);
      return true;
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
      // Windows lets only an administrator or a developer-mode user create a symbolic link, and a junction needs neither.
    }

    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      return false;
    }

    using var process = Process.Start(new ProcessStartInfo("cmd.exe", $"/c mklink /J \"{link}\" \"{target}\"")
    {
      CreateNoWindow = true,
      UseShellExecute = false,
      RedirectStandardOutput = true
    });
    process!.StandardOutput.ReadToEnd();
    process.WaitForExit();
    return process.ExitCode == 0;
  }

  private sealed class RecordingAnalyzer : IMediaInfoAnalyzer
  {
    public List<string> Paths { get; } = [];

    public Task<MediaAnalysisResult> AnalyzeAsync(IMediaSource source, CancellationToken cancellationToken = default)
    {
      Paths.Add(source.DisplayName);
      return Task.FromResult(new MediaAnalysisResult { Success = true });
    }
  }
}
