#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using Xunit;
using Xunit.Abstractions;

namespace MediaInfo.Analysis.Integration.Tests;

/// <summary>
/// Builds a Blu-ray structure out of the transport streams of the repository and analyzes it with the real library.
/// </summary>
/// <remarks>
/// The corpus contains genuine BDAV transport streams, so a BDMV folder assembled from them is parsed for real rather
/// than being a stand-in. The clips are small, so the fixture lowers the minimum title size the analyzer applies.
/// </remarks>
[Collection(NativeMediaCollection.Name)]
public class BluRayDiscAnalysisTests(ITestOutputHelper output) : IDisposable
{
  private readonly string _root = Directory
    .CreateDirectory(Path.Combine(Path.GetTempPath(), "mp-mediainfo-bd-" + Guid.NewGuid().ToString("N")))
    .FullName;

  private static MediaAnalysisOptions SmallClipOptions => new() { MinimumDiscTitleSize = 1024L };

  public void Dispose()
  {
    try
    {
      Directory.Delete(_root, recursive: true);
    }
    catch (IOException)
    {
      // A file that is still held open by the native library must not fail the test run.
    }
  }

  private string BuildDisc()
  {
    var bdmv = Directory.CreateDirectory(Path.Combine(_root, "BDMV")).FullName;
    var stream = Directory.CreateDirectory(Path.Combine(bdmv, "STREAM")).FullName;
    Directory.CreateDirectory(Path.Combine(bdmv, "PLAYLIST"));
    Directory.CreateDirectory(Path.Combine(bdmv, "CLIPINF"));

    File.WriteAllBytes(Path.Combine(bdmv, "index.bdmv"), "INDX0200"u8.ToArray());
    File.WriteAllBytes(Path.Combine(bdmv, "PLAYLIST", "00000.mpls"), "MPLS0200"u8.ToArray());

    // The largest clip is copied last so that the reader cannot pick the feature by enumeration order alone.
    File.Copy(TestMedia.Path("Test_H264.m2ts"), Path.Combine(stream, "00001.m2ts"));
    File.Copy(TestMedia.Path("Test_H264_AC3.m2ts"), Path.Combine(stream, "00002.m2ts"));
    File.Copy(TestMedia.Path("Test_H264_DTS1.m2ts"), Path.Combine(stream, "00003.m2ts"));

    return bdmv;
  }

  [Fact]
  public async Task AnalyzeBluRay_DiscoversTheClipsAndReadsTheMainTitle()
  {
    var bdmv = BuildDisc();
    var analyzer = MediaInfoAnalyzer.CreateDefault(SmallClipOptions);

    var result = await analyzer.AnalyzeAsync(bdmv);

    output.WriteLine($"Main title: {result.AnalyzedPath}, duration {result.General.Duration}");

    result.Success.Should().BeTrue(result.Failure?.ToString() ?? string.Empty);
    result.IsBluRay.Should().BeTrue();
    result.IsDvd.Should().BeFalse();
    result.SourceKind.Should().Be(MediaSourceKind.Directory);
    result.HasVideo.Should().BeTrue("the clips carry a real H.264 elementary stream");

    var disc = result.Disc.Should().BeOfType<BluRayStructure>().Subject;
    disc.Kind.Should().Be(DiscKind.BluRay);
    disc.RootPath.Should().Be(bdmv);
    disc.Playlists.Should().HaveCount(3);
    disc.PlaylistFiles.Should().ContainSingle().Which.Should().EndWith("00000.mpls");
    disc.Playlists.Select(x => x.Name).Should().BeEquivalentTo("00001", "00002", "00003");
  }

  [Fact]
  public async Task AnalyzeBluRay_SelectsTheLongestClipAsTheMainTitle()
  {
    var bdmv = BuildDisc();

    var result = await MediaInfoAnalyzer.CreateDefault(SmallClipOptions).AnalyzeAsync(bdmv);
    var disc = (BluRayStructure)result.Disc!;

    disc.Playlists.Should().OnlyContain(x => x.Duration > TimeSpan.Zero, "every clip was probed");

    var longest = disc.Playlists.OrderByDescending(x => x.Duration).First();
    disc.MainTitle!.Number.Should().Be(longest.Number);
    result.AnalyzedPath.Should().Be(longest.PrimaryFile);
  }

  [Fact]
  public async Task AnalyzeBluRay_ReportsTheSizeOfTheWholeStructure()
  {
    var bdmv = BuildDisc();
    var expected = Directory
      .GetFiles(bdmv, "*", SearchOption.AllDirectories)
      .Sum(x => new FileInfo(x).Length);

    var result = await MediaInfoAnalyzer.CreateDefault(SmallClipOptions).AnalyzeAsync(bdmv);

    result.General.Size.Should().Be(expected);
    result.Disc!.TotalSize.Should().Be(expected);
  }

  [Fact]
  public async Task AnalyzeBluRay_FromTheDiscRootRatherThanTheBdmvFolder_FindsTheSameStructure()
  {
    var bdmv = BuildDisc();

    var fromRoot = await MediaInfoAnalyzer.CreateDefault(SmallClipOptions).AnalyzeAsync(_root);
    var fromBdmv = await MediaInfoAnalyzer.CreateDefault(SmallClipOptions).AnalyzeAsync(bdmv);

    fromRoot.Success.Should().BeTrue(fromRoot.Failure?.ToString() ?? string.Empty);
    fromRoot.Disc!.RootPath.Should().Be(bdmv);
    fromRoot.AnalyzedPath.Should().Be(fromBdmv.AnalyzedPath);
  }

  [Fact]
  public async Task AnalyzeBluRay_WithoutStreams_ReportsAnUnreadableStructure()
  {
    var bdmv = Directory.CreateDirectory(Path.Combine(_root, "BDMV")).FullName;
    Directory.CreateDirectory(Path.Combine(bdmv, "STREAM"));
    File.WriteAllBytes(Path.Combine(bdmv, "index.bdmv"), "INDX0200"u8.ToArray());

    var result = await MediaInfoAnalyzer.CreateDefault(SmallClipOptions).AnalyzeAsync(bdmv);

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.DiscStructureUnreadable);
  }
}

/// <summary>
/// Analyzes a real DVD structure when one is available.
/// </summary>
/// <remarks>
/// No DVD sample is committed to the repository, because a renamed VOB or IFO would not be parsed by the library and
/// would prove nothing. These tests are therefore skipped unless a real structure is supplied. The discovery logic
/// itself is covered without a disc by the unit tests that run against an in-memory file system.
/// </remarks>
[Collection(NativeMediaCollection.Name)]
public class DvdDiscAnalysisTests(ITestOutputHelper output)
{
  [DvdFact]
  public async Task AnalyzeDvd_DiscoversTheTitleSetsAndReadsTheMainTitle()
  {
    var path = TestMedia.FindDvd()!;
    var result = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(path);

    var disc = result.Disc.Should().BeOfType<DvdStructure>().Subject;
    foreach (var title in disc.DvdTitles)
    {
      output.WriteLine(
        $"VTS_{title.TitleSetNumber:00}: {title.Duration}, {title.Size:N0} bytes, {title.VobFiles.Count} VOB(s)");
    }

    result.Success.Should().BeTrue(result.Failure?.ToString() ?? string.Empty);
    result.IsDvd.Should().BeTrue();
    result.SourceKind.Should().Be(MediaSourceKind.Directory);

    disc.Kind.Should().Be(DiscKind.Dvd);
    disc.DvdTitles.Should().NotBeEmpty();
    disc.DvdTitles.Should().OnlyContain(x => x.VobFiles.Count > 0);
    disc.MainTitle.Should().NotBeNull();
    result.AnalyzedPath.Should().Be(disc.MainTitle!.PrimaryFile);
  }

  [DvdFact]
  public async Task AnalyzeDvd_ReportsTheDurationOfTheWholeTitle()
  {
    var path = TestMedia.FindDvd()!;
    var result = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(path);
    var main = (DvdTitle)result.Disc!.MainTitle!;

    main.Duration.Should().BeGreaterThan(
      TimeSpan.Zero,
      "the information file of the title set was probed for the duration");

    result.General.Duration.Should().Be(main.Duration);

    if (main.VobFiles.Count > 1)
    {
      // The library recognises a VTS_nn_* sequence and reports the joined title, so opening the first part gives
      // the duration of the whole title rather than of that one gigabyte slice. The information file is probed
      // first regardless, because it is the navigation table and a fraction of the size.
      var firstVob = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(main.VobFiles[0]);
      output.WriteLine(
        $"Title {main.Duration} across {main.VobFiles.Count} VOBs; first VOB reports {firstVob.General.Duration}");

      main.Duration.Should().Be(
        firstVob.General.Duration,
        "the information file and the joined VOB sequence describe the same title");
    }
  }

  [DvdFact]
  public async Task AnalyzeDvd_OrdersTheVobsOfATitleSetByPart()
  {
    var path = TestMedia.FindDvd()!;
    var result = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(path);
    var disc = (DvdStructure)result.Disc!;

    foreach (var title in disc.DvdTitles)
    {
      var names = title.VobFiles.Select(Path.GetFileName).ToArray();
      names.Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase);
      names.Should().OnlyContain(x => !x!.EndsWith("_0.VOB", StringComparison.OrdinalIgnoreCase));
    }
  }

  [DvdFact]
  public async Task AnalyzeDvd_ReportsTheSizeOfTheWholeStructure()
  {
    var path = TestMedia.FindDvd()!;
    var result = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(path);
    var disc = (DvdStructure)result.Disc!;

    var expected = Directory
      .GetFiles(disc.RootPath, "*", SearchOption.TopDirectoryOnly)
      .Sum(x => new FileInfo(x).Length);

    disc.TotalSize.Should().Be(expected);
    result.General.Size.Should().Be(expected);
  }

  [DvdFact]
  public async Task AnalyzeDvd_FromAnIfoFile_AnalyzesTheWholeDisc()
  {
    var path = TestMedia.FindDvd()!;
    var ifo = Directory.GetFiles(path, "VTS_*_0.IFO").OrderBy(x => x).First();

    var result = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(ifo);

    result.SourceKind.Should().Be(MediaSourceKind.Directory, "a file inside a disc identifies the disc");
    result.IsDvd.Should().BeTrue();
  }
}
