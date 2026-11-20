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

  // Every pointer of the chain table aims at one chain of 99 programs over 255 cells, and the file stays small.
  private static byte[] TitleSetWithSharedChain(int pointers, int programs)
  {
    const int table = 2048;
    var chainOffset = 8 + (pointers * 8);
    var start = table + chainOffset;
    var data = new byte[start + 0x200 + (255 * 24) + 16];
    "DVDVIDEO-VTS"u8.CopyTo(data);
    System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(0xCC), 1);
    System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(table), (ushort)pointers);
    for (var i = 0; i < pointers; i++)
    {
      System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(table + 8 + (i * 8) + 4), (uint)chainOffset);
    }

    data[start + 2] = (byte)programs;
    data[start + 3] = 255;
    System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(start + 0xE6), 0x100);
    System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(start + 0xE8), 0x200);
    for (var program = 0; program < 256; program++)
    {
      data[start + 0x100 + program] = (byte)(program % 2 == 0 ? 1 : 255);
    }

    return data;
  }

  [Fact]
  public void TitleSet_ManyPointersToOneChain_ReadsTheChainOnce()
  {
    var data = TitleSetWithSharedChain(pointers: 2000, programs: 50);

    var chains = IfoParser.ReadProgramChains(data);

    chains.Should().HaveCount(2000);
    chains.Values.Select(x => x.Chapters).Distinct().Should().ContainSingle("the same target is not read again");
    chains.Values.Sum(x => x.Chapters.Count).Should().BeGreaterThan(IfoParser.MaximumChaptersPerTitleSet,
      "the chapters are shared, and it is the work that is bounded");
  }

  [Fact]
  public void TitleSet_PointerCountsAndProgramCountsBeyondTheFormat_AreNotBelieved()
  {
    var many = IfoParser.ReadProgramChains(TitleSetWithSharedChain(pointers: 65535, programs: 50));
    many.Count.Should().BeLessThanOrEqualTo(IfoParser.MaximumChainPointers);

    var tooManyPrograms = IfoParser.ReadProgramChains(TitleSetWithSharedChain(pointers: 1, programs: 255));
    tooManyPrograms[1].Chapters.Should().BeEmpty("a chain has at most 99 programs");
  }

  [Fact]
  public void TitleSet_ChainsThatTogetherHoldMoreChaptersThanADiscCan_StopAtTheBudget()
  {
    const int table = 2048;
    const int pointers = 2000;
    var chainSize = 0x100 + 256 + (255 * 24);
    var data = new byte[table + 8 + (pointers * 8) + (pointers * 16) + chainSize + 16];
    "DVDVIDEO-VTS"u8.CopyTo(data);
    System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(0xCC), 1);
    System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(table), pointers);

    // The chains start a few bytes apart, so that they overlap and each one is a different target that reads the
    // same tables. No two pointers share a target here.
    var first = table + 8 + (pointers * 8);
    for (var i = 0; i < pointers; i++)
    {
      var start = first + (i * 16);
      System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(table + 8 + (i * 8) + 4), (uint)(start - table));
      data[start + 2] = 99;
      data[start + 3] = 255;
      System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(start + 0xE6), 0x100);
      System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(start + 0xE8), 0x200);

      // The program maps overlap too. They are shifted by an even number of bytes, so every chain reads the same pattern.
      for (var program = 0; program < 100; program++)
      {
        data[start + 0x100 + program] = (byte)(program % 2 == 0 ? 1 : 255);
      }
    }

    var chains = IfoParser.ReadProgramChains(data);
    var total = chains.Values.Sum(x => x.Chapters.Count);

    chains.Values.Select(x => x.Chapters).Distinct().Count().Should().BeGreaterThan(100, "every pointer has a target of its own");
    total.Should().BeGreaterThan(IfoParser.MaximumChaptersPerTitleSet - 99, "the budget is used up, not skipped");
    total.Should().BeLessThanOrEqualTo(IfoParser.MaximumChaptersPerTitleSet);
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
