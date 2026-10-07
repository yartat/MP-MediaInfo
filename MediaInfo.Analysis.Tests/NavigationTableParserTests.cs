#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Discs;
using MediaInfo.Analysis.Discs.Parsers;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Tests.Fakes;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for reading the navigation tables of a DVD and a Blu-ray.</summary>
public class NavigationTableParserTests
{
  private const string VideoTs = @"D:\media\disc\VIDEO_TS";
  private const string Bdmv = @"D:\media\bd\BDMV";

  #region DVD tables

  [Fact]
  public void VideoManager_ListsEveryTitleWithItsSetAnglesAndChapters()
  {
    var data = DiscFixtures.VideoManager((1, 1, 24, 1), (2, 1, 3, 4));

    var titles = IfoParser.ReadTitles(data);

    titles.Should().HaveCount(2);
    titles[0].Should().BeEquivalentTo(new { Number = 1, TitleSetNumber = 1, ChapterCount = 24, AngleCount = 1 });
    titles[1].Should().BeEquivalentTo(new { Number = 2, TitleSetNumber = 2, ChapterCount = 3, AngleCount = 4 });
  }

  [Theory]
  [InlineData(0)]
  [InlineData(4)]
  [InlineData(64)]
  public void VideoManager_ThatIsTruncatedOrForeignYieldsNothing(int length)
  {
    IfoParser.ReadTitles(new byte[length]).Should().BeEmpty();
  }

  [Fact]
  public void VideoManager_WithoutTheSignatureYieldsNothing()
  {
    var data = DiscFixtures.VideoManager((1, 1, 5, 1));
    data[0] = (byte)'X';

    IfoParser.ReadTitles(data).Should().BeEmpty();
  }

  [Fact]
  public void TitleSet_ReadsThePlaybackTimeOfTheChain()
  {
    var data = DiscFixtures.TitleSet(TimeSpan.FromMinutes(94), [1], TimeSpan.FromMinutes(94));

    var chains = IfoParser.ReadProgramChains(data);

    chains.Should().ContainKey(1);
    chains[1].Duration.Should().Be(TimeSpan.FromMinutes(94));
  }

  [Fact]
  public void TitleSet_BuildsChaptersFromTheProgramMapAndTheCellTimes()
  {
    // Three chapters spanning one, two and one cells, every cell running ten minutes.
    var data = DiscFixtures.TitleSet(TimeSpan.FromMinutes(40), [1, 2, 1], TimeSpan.FromMinutes(10));

    var chapters = IfoParser.ReadProgramChains(data)[1].Chapters;

    chapters.Should().HaveCount(3);
    chapters[0].Start.Should().Be(TimeSpan.Zero);
    chapters[0].Duration.Should().Be(TimeSpan.FromMinutes(10));
    chapters[1].Start.Should().Be(TimeSpan.FromMinutes(10));
    chapters[1].Duration.Should().Be(TimeSpan.FromMinutes(20), "the second chapter spans two cells");
    chapters[2].Start.Should().Be(TimeSpan.FromMinutes(30));
    chapters[2].Duration.Should().Be(TimeSpan.FromMinutes(10));
  }

  [Fact]
  public void TitleSet_MapsTitlesToTheChainThatPlaysThem()
  {
    var data = DiscFixtures.TitleSet(TimeSpan.FromMinutes(5), [1], TimeSpan.FromMinutes(5));

    IfoParser.ReadTitleToChainMap(data).Should().Contain(new System.Collections.Generic.KeyValuePair<int, int>(1, 1));
  }

  [Fact]
  public void TitleSet_ThatIsNotATitleSetYieldsNothing()
  {
    IfoParser.ReadProgramChains(new byte[4096]).Should().BeEmpty();
    IfoParser.ReadTitleToChainMap(new byte[4096]).Should().BeEmpty();
  }

  #endregion

  #region Blu-ray playlists

  [Fact]
  public void Playlist_ListsItsClipsInOrderAndSumsTheirDuration()
  {
    var data = DiscFixtures.Playlist(
    [
      ("00001", TimeSpan.FromMinutes(30)),
      ("00002", TimeSpan.FromMinutes(45))
    ]);

    var playlist = MplsParser.Parse(data);

    playlist.Items.Select(x => x.ClipId).Should().ContainInOrder("00001", "00002");
    playlist.Items[0].Duration.Should().Be(TimeSpan.FromMinutes(30));
    playlist.Duration.Should().Be(TimeSpan.FromMinutes(75));
    playlist.IsEmpty.Should().BeFalse();
  }

  [Fact]
  public void Playlist_TurnsItsMarksIntoChaptersOnThePlaylistTimeline()
  {
    var data = DiscFixtures.Playlist(
    [
      ("00001", TimeSpan.FromMinutes(30)),
      ("00002", TimeSpan.FromMinutes(45))
    ]);

    var chapters = MplsParser.Parse(data).Chapters;

    chapters.Should().HaveCount(2);
    chapters[0].Start.Should().Be(TimeSpan.Zero);
    chapters[0].Duration.Should().Be(TimeSpan.FromMinutes(30));
    chapters[1].Start.Should().Be(TimeSpan.FromMinutes(30), "a mark is rebased from its clip onto the playlist");
    chapters[1].Duration.Should().Be(TimeSpan.FromMinutes(45));
  }

  [Fact]
  public void Playlist_WithoutMarksStillListsItsClips()
  {
    var data = DiscFixtures.Playlist([("00001", TimeSpan.FromMinutes(10))], withMarks: false);

    var playlist = MplsParser.Parse(data);

    playlist.Items.Should().ContainSingle();
    playlist.Chapters.Should().BeEmpty();
  }

  [Theory]
  [InlineData(0)]
  [InlineData(8)]
  [InlineData(40)]
  public void Playlist_ThatIsTruncatedOrForeignIsEmpty(int length)
  {
    MplsParser.Parse(new byte[length]).IsEmpty.Should().BeTrue();
  }

  #endregion

  #region Readers

  private static FakeFileSystem CreateDvd(byte[] videoManager, byte[] titleSet) =>
    new FakeFileSystem()
      .AddFile(Path.Combine(VideoTs, "VIDEO_TS.IFO"), videoManager.Length)
      .AddFile(Path.Combine(VideoTs, "VTS_01_0.IFO"), titleSet.Length)
      .AddFile(Path.Combine(VideoTs, "VTS_01_0.VOB"), 20 * 1024 * 1024)
      .AddFile(Path.Combine(VideoTs, "VTS_01_1.VOB"), 1024L * 1024 * 1024)
      .AddFile(Path.Combine(VideoTs, "VTS_01_2.VOB"), 512L * 1024 * 1024)
      .SetContent(Path.Combine(VideoTs, "VIDEO_TS.IFO"), videoManager)
      .SetContent(Path.Combine(VideoTs, "VTS_01_0.IFO"), titleSet);

  [Fact]
  public async Task IfoReader_DescribesTheDiscWithoutOpeningAnyMedia()
  {
    var fileSystem = CreateDvd(
      DiscFixtures.VideoManager((1, 1, 3, 2)),
      DiscFixtures.TitleSet(TimeSpan.FromMinutes(40), [1, 2, 1], TimeSpan.FromMinutes(10)));

    var factory = new FakeNativeMediaInfoFactory();
    var structure = (DvdStructure)(await new IfoDvdStructureReader(fileSystem).ReadAsync(
      VideoTs,
      TestContext.Create(factory, fileSystem),
      CancellationToken.None))!;

    factory.OpenedPaths.Should().BeEmpty("the tables carry everything the folder reader had to probe for");

    var title = structure.DvdTitles.Should().ContainSingle().Subject;
    title.Duration.Should().Be(TimeSpan.FromMinutes(40));
    title.AngleCount.Should().Be(2);
    title.Chapters.Should().HaveCount(3);
    title.VobFiles.Select(Path.GetFileName).Should().ContainInOrder("VTS_01_1.VOB", "VTS_01_2.VOB");
    title.VobFiles.Should().NotContain(x => x.EndsWith("_0.VOB"), "the menu VOB is not part of the feature");
  }

  [Fact]
  public async Task IfoReader_WithoutAVideoManagerYieldsNothing()
  {
    var fileSystem = new FakeFileSystem().AddFile(Path.Combine(VideoTs, "VTS_01_1.VOB"), 1024);

    var structure = await new IfoDvdStructureReader(fileSystem).ReadAsync(
      VideoTs,
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem),
      CancellationToken.None);

    structure.Should().BeNull("so that the folder reader can take over");
  }

  [Fact]
  public async Task FallbackReader_UsesTheFolderLayoutWhenTheTablesCannotBeRead()
  {
    // A disc whose VIDEO_TS.IFO is unreadable, but whose files are laid out as usual.
    var fileSystem = new FakeFileSystem()
      .AddFile(Path.Combine(VideoTs, "VIDEO_TS.IFO"), 128)
      .SetContent(Path.Combine(VideoTs, "VIDEO_TS.IFO"), new byte[128])
      .AddFile(Path.Combine(VideoTs, "VTS_01_0.IFO"), 128)
      .AddFile(Path.Combine(VideoTs, "VTS_01_1.VOB"), 1024L * 1024 * 1024);

    var reader = new FallbackDiscStructureReader(
      new IfoDvdStructureReader(fileSystem),
      new DvdStructureReader(fileSystem));

    var structure = await reader.ReadAsync(
      VideoTs,
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem),
      CancellationToken.None);

    structure.Should().NotBeNull();
    structure!.Titles.Should().ContainSingle();
  }

  [Fact]
  public async Task FallbackReader_PrefersTheTablesWhenTheyCanBeRead()
  {
    var fileSystem = CreateDvd(
      DiscFixtures.VideoManager((1, 1, 3, 5)),
      DiscFixtures.TitleSet(TimeSpan.FromMinutes(40), [1, 2, 1], TimeSpan.FromMinutes(10)));

    var reader = new FallbackDiscStructureReader(
      new IfoDvdStructureReader(fileSystem),
      new DvdStructureReader(fileSystem));

    var structure = (DvdStructure)(await reader.ReadAsync(
      VideoTs,
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem),
      CancellationToken.None))!;

    structure.DvdTitles[0].AngleCount.Should().Be(5, "only the tables know the angle count");
    structure.DvdTitles[0].Chapters.Should().HaveCount(3);
  }

  [Fact]
  public void FallbackReader_RefusesTwoReadersForDifferentKindsOfDisc()
  {
    var fileSystem = new FakeFileSystem();
    var create = () => new FallbackDiscStructureReader(
      new IfoDvdStructureReader(fileSystem),
      new BluRayStructureReader(fileSystem));

    create.Should().Throw<ArgumentException>();
  }

  [Fact]
  public async Task MplsReader_BuildsTitlesFromThePlaylistsRatherThanFromTheClips()
  {
    var feature = DiscFixtures.Playlist(
    [
      ("00001", TimeSpan.FromMinutes(50)),
      ("00002", TimeSpan.FromMinutes(70))
    ]);
    var extra = DiscFixtures.Playlist([("00003", TimeSpan.FromMinutes(5))]);

    var fileSystem = new FakeFileSystem()
      .AddFile(Path.Combine(Bdmv, "index.bdmv"), 32)
      .AddFile(Path.Combine(Bdmv, "PLAYLIST", "00000.mpls"), feature.Length)
      .SetContent(Path.Combine(Bdmv, "PLAYLIST", "00000.mpls"), feature)
      .AddFile(Path.Combine(Bdmv, "PLAYLIST", "00001.mpls"), extra.Length)
      .SetContent(Path.Combine(Bdmv, "PLAYLIST", "00001.mpls"), extra)
      .AddFile(Path.Combine(Bdmv, "STREAM", "00001.m2ts"), 8L * 1024 * 1024 * 1024)
      .AddFile(Path.Combine(Bdmv, "STREAM", "00002.m2ts"), 10L * 1024 * 1024 * 1024)
      .AddFile(Path.Combine(Bdmv, "STREAM", "00003.m2ts"), 512L * 1024 * 1024);

    var structure = (BluRayStructure)(await new MplsBluRayStructureReader(fileSystem).ReadAsync(
      Bdmv,
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem),
      CancellationToken.None))!;

    structure.Playlists.Should().HaveCount(2);

    var main = structure.MainTitle.Should().BeOfType<BluRayPlaylist>().Subject;
    main.Duration.Should().Be(TimeSpan.FromMinutes(120));
    main.Clips.Should().HaveCount(2, "the feature is assembled from two clips, which the folder reader would split");
    main.Size.Should().Be(18L * 1024 * 1024 * 1024);
  }

  [Fact]
  public async Task MplsReader_DropsDuplicatePlaylistsAndOnesTooShortToBeATitle()
  {
    var feature = DiscFixtures.Playlist([("00001", TimeSpan.FromMinutes(90))]);
    var duplicate = DiscFixtures.Playlist([("00001", TimeSpan.FromMinutes(90))]);
    var jingle = DiscFixtures.Playlist([("00002", TimeSpan.FromSeconds(20))]);

    var fileSystem = new FakeFileSystem()
      .AddFile(Path.Combine(Bdmv, "index.bdmv"), 32)
      .AddFile(Path.Combine(Bdmv, "PLAYLIST", "00000.mpls"), feature.Length)
      .SetContent(Path.Combine(Bdmv, "PLAYLIST", "00000.mpls"), feature)
      .AddFile(Path.Combine(Bdmv, "PLAYLIST", "00001.mpls"), duplicate.Length)
      .SetContent(Path.Combine(Bdmv, "PLAYLIST", "00001.mpls"), duplicate)
      .AddFile(Path.Combine(Bdmv, "PLAYLIST", "00002.mpls"), jingle.Length)
      .SetContent(Path.Combine(Bdmv, "PLAYLIST", "00002.mpls"), jingle)
      .AddFile(Path.Combine(Bdmv, "STREAM", "00001.m2ts"), 1024)
      .AddFile(Path.Combine(Bdmv, "STREAM", "00002.m2ts"), 1024);

    var structure = (BluRayStructure)(await new MplsBluRayStructureReader(fileSystem).ReadAsync(
      Bdmv,
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem),
      CancellationToken.None))!;

    structure.Playlists.Should().ContainSingle().Which.Name.Should().Be("00000");
  }

  [Fact]
  public async Task MplsReader_IgnoresAPlaylistNamingClipsThatAreNotOnTheDisc()
  {
    var orphan = DiscFixtures.Playlist([("09999", TimeSpan.FromMinutes(90))]);

    var fileSystem = new FakeFileSystem()
      .AddFile(Path.Combine(Bdmv, "index.bdmv"), 32)
      .AddFile(Path.Combine(Bdmv, "PLAYLIST", "00000.mpls"), orphan.Length)
      .SetContent(Path.Combine(Bdmv, "PLAYLIST", "00000.mpls"), orphan)
      .AddFile(Path.Combine(Bdmv, "STREAM", "00001.m2ts"), 1024);

    var structure = await new MplsBluRayStructureReader(fileSystem).ReadAsync(
      Bdmv,
      TestContext.Create(new FakeNativeMediaInfoFactory(), fileSystem),
      CancellationToken.None);

    structure.Should().BeNull();
  }

  #endregion
}
