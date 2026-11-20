#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using static MediaInfo.Analysis.Tests.Fakes.TestPaths;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Decorators;
using MediaInfo.Analysis.Discs;
using MediaInfo.Analysis.Discs.Parsers;
using MediaInfo.Analysis.Pipeline;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Strategies;
using MediaInfo.Analysis.Strategies.Selection;
using MediaInfo.Analysis.Tests.Fakes;
using MediaInfo.Model;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for the lesser findings of the review: cultures, discs, limits and patterns.</summary>
public class HardeningTests
{
  private static readonly string Movie = P("/media/movie.mkv");

  private static MediaAnalysisContext CreateContext(FakeFileSystem? fileSystem = null) =>
    new(
      new MediaProbe(new FakeNativeMediaInfoFactory(), DefaultStreamSelectionStrategy.Instance),
      fileSystem ?? new FakeFileSystem());

  private static async Task<T> InCultureAsync<T>(string culture, Func<Task<T>> action)
  {
    var previous = CultureInfo.CurrentCulture;
    CultureInfo.CurrentCulture = new CultureInfo(culture);
    try
    {
      return await action();
    }
    finally
    {
      CultureInfo.CurrentCulture = previous;
    }
  }

  #region Fields of newer libraries

  [Fact]
  public async Task Analyze_ReadsTheIptcMetadata()
  {
    var fileSystem = new FakeFileSystem().AddFile(P("/media/photo.jpg"));
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(P("/media/photo.jpg"))
      .WithStreams(StreamKind.Image, 1)
      .Set(StreamKind.General, 0, "Headline", "A headline")
      .Set(StreamKind.General, 0, "City", "Kyiv")
      .Set(StreamKind.General, 0, "CountryCode", "UA")
      .Set(StreamKind.General, 0, "SupplementalCategories", "one / two")
      .Set(StreamKind.General, 0, "ExpirationDate", "2030-01-01")
      .Set(StreamKind.General, 0, "DigitalSourceType", "http://cv.iptc.org/newscodes/digitalsourcetype/trainedAlgorithmicMedia")
      .Set(StreamKind.General, 0, "DigitalSourceType/String", "Created using Generative AI");

    var result = await MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false)
      .Build()
      .AnalyzeAsync(new FileMediaSource(P("/media/photo.jpg")));

    var iptc = result.General.Iptc;
    iptc.Should().NotBeNull();
    iptc!.Headline.Should().Be("A headline");
    iptc.City.Should().Be("Kyiv");
    iptc.CountryCode.Should().Be("UA");
    iptc.SupplementalCategories.Should().Be("one / two");
    iptc.ExpirationDate.Should().Be("2030-01-01");
    iptc.DigitalSourceTypeName.Should().Be("Created using Generative AI");
    iptc.Credit.Should().BeEmpty();
    new LegacyResultAdapter(result).Iptc.Should().BeSameAs(iptc);
  }

  [Fact]
  public async Task Analyze_FileWithoutIptc_HasNone()
  {
    var fileSystem = new FakeFileSystem().AddFile(Movie);
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Movie).WithStreams(StreamKind.Video, 1);

    var result = await MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false)
      .Build()
      .AnalyzeAsync(new FileMediaSource(Movie));

    result.General.Iptc.Should().BeNull();
  }

  [Theory]
  [InlineData("Headline", "Headline")]
  [InlineData("SupplementalCategories", "SupplementalCategories")]
  [InlineData("DigitalSourceType", "DigitalSourceType")]
  [InlineData("DigitalSourceType_String", "DigitalSourceType/String")]
  public void IptcField_ParameterName_IsTheNameTheLibraryUses(string member, string expected) =>
    Enum.Parse<IptcField>(member).ToParameterName().Should().Be(expected);

  [Fact]
  public async Task Analyze_Ps2Audio_ReportsTheMuxingMode()
  {
    var fileSystem = new FakeFileSystem().AddFile(P("/media/song.ss2"));
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(P("/media/song.ss2"))
      .WithStreams(StreamKind.Audio, 1)
      .SetIndexed(StreamKind.Audio, 0, (int)NativeMethods.Audio.Audio_Format, "ADPCM")
      .SetIndexed(StreamKind.Audio, 0, (int)NativeMethods.Audio.Audio_MuxingMode, "PS2");

    var result = await MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false)
      .Build()
      .AnalyzeAsync(new FileMediaSource(P("/media/song.ss2")));

    result.AudioStreams[0].MuxingMode.Should().Be("PS2");
    result.AudioStreams[0].Codec.Should().Be(AudioCodec.Adpcm);
  }

  [Theory]
  [InlineData("/media/song.ss2", true)]
  [InlineData("/media/broadcast.bw64", true)]
  [InlineData("/media/track.cmfa", true)]
  [InlineData("/media/video.cmfv", false)]
  public void Extensions_NewerFormats_AreKnownAsMedia(string path, bool audio)
  {
    (audio ? path.IsAudio() : path.IsVideo()).Should().BeTrue();
  }


  [Fact]
  public async Task Analyze_ReadsTheFieldsAddedAfter2610()
  {
    var fileSystem = new FakeFileSystem().AddFile(Movie);
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Movie)
      .WithStreams(StreamKind.Video, 1)
      .SetIndexed(StreamKind.General, 0, (int)NativeMethods.General.General_CompliantWith, "SMPTE ST 2067-21")
      .Set(StreamKind.Video, 0, "Mirror", "Horizontal");

    var result = await MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false)
      .Build()
      .AnalyzeAsync(new FileMediaSource(Movie));

    result.General.CompliantWith.Should().Be("SMPTE ST 2067-21");
    result.VideoStreams[0].Mirror.Should().Be("Horizontal");
    new LegacyResultAdapter(result).CompliantWith.Should().Be("SMPTE ST 2067-21");
  }

  #endregion

  [Fact]
  public async Task Analyze_ChaptersAndMenus_HaveTheirOwnKinds()
  {
    var fileSystem = new FakeFileSystem().AddFile(Movie);
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Movie)
      .WithStreams(StreamKind.Video, 1)
      .WithStreams(StreamKind.Other, 1)
      .WithStreams(StreamKind.Menu, 1);

    var result = await MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false)
      .Build()
      .AnalyzeAsync(new FileMediaSource(Movie));

    result.Chapters.Should().ContainSingle().Which.Kind.Should().Be(MediaStreamKind.Chapter);
    result.MenuStreams.Should().ContainSingle().Which.Kind.Should().Be(MediaStreamKind.Menu);
  }

  [Fact]
  public void MediaStreamKind_NewMember_DoesNotChangeTheNumbersOfTheOthers()
  {
    // The numbers are part of the contract of anything that stores the enumeration as a number.
    ((int)MediaStreamKind.Video).Should().Be(0);
    ((int)MediaStreamKind.Audio).Should().Be(1);
    ((int)MediaStreamKind.Text).Should().Be(2);
    ((int)MediaStreamKind.Image).Should().Be(3);
    ((int)MediaStreamKind.Menu).Should().Be(4);
    ((int)MediaStreamKind.Chapter).Should().Be(5);
  }

  #region Culture

  [Fact]
  public async Task Analyze_UnderTheTurkishCulture_StillFindsTheLanguageAndTheSubtitleCodec()
  {
    var fileSystem = new FakeFileSystem().AddFile(Movie);
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Movie)
      .WithStreams(StreamKind.Audio, 1)
      .WithStreams(StreamKind.Text, 1)
      .Set(StreamKind.Audio, 0, "Language", "ICE")
      .SetIndexed(StreamKind.Text, 0, (int)NativeMethods.Text.Text_Format, "DVB Subtitle");

    // In Turkish the capital of "i" is a dotted "İ", so a conversion that follows the culture misses these keys.
    new CultureInfo("tr-TR").TextInfo.ToUpper("i").Should().Be("İ", "the test needs a culture that tells the two apart");
    var result = await InCultureAsync("tr-TR", () => MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false)
      .Build()
      .AnalyzeAsync(new FileMediaSource(Movie)));

    result.AudioStreams[0].Language.Should().Be("Icelandic");
    result.Subtitles[0].Codec.Should().Be(SubtitleCodec.DvbSubtitle);
  }

  #endregion

  #region Discs

  private sealed class FailingReader(DiscKind kind) : IDiscStructureReader
  {
    public DiscKind Kind => kind;

    public int Priority => 1;

    public bool CanRead(string path, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? discRoot)
    {
      discRoot = path;
      return true;
    }

    public Task<DiscStructure?> ReadAsync(string discRoot, MediaAnalysisContext context, CancellationToken cancellationToken) =>
      throw new InvalidOperationException("The table is damaged.");
  }

  private sealed class FixedReader(DiscStructure structure) : IDiscStructureReader
  {
    public DiscKind Kind => structure.Kind;

    public int Priority => 2;

    public bool CanRead(string path, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? discRoot)
    {
      discRoot = path;
      return true;
    }

    public Task<DiscStructure?> ReadAsync(string discRoot, MediaAnalysisContext context, CancellationToken cancellationToken) =>
      Task.FromResult<DiscStructure?>(structure);
  }

  [Fact]
  public async Task FallbackReader_PreferredReaderThrows_DescribesTheDiscFromTheFolders()
  {
    var structure = new DvdStructure(P("/disc/VIDEO_TS"), 0, []);
    var reader = new FallbackDiscStructureReader(new FailingReader(DiscKind.Dvd), new FixedReader(structure));
    var reports = new List<AnalysisProgress>();
    var context = new MediaAnalysisContext(
      new MediaProbe(new FakeNativeMediaInfoFactory(), DefaultStreamSelectionStrategy.Instance),
      new FakeFileSystem(),
      progress: new ListProgress(reports));

    var result = await reader.ReadAsync(P("/disc/VIDEO_TS"), context, CancellationToken.None);

    result.Should().BeSameAs(structure);
    reports.Should().Contain(x => x.Detail != null && x.Detail.Contains("could not be read"));
  }

  [Fact]
  public async Task FallbackReader_Cancelled_IsNotHiddenByTheFallback()
  {
    var cancelling = new CancellingReader();
    var reader = new FallbackDiscStructureReader(cancelling, new FixedReader(new DvdStructure("x", 0, [])));

    var act = () => reader.ReadAsync("x", CreateContext(), CancellationToken.None);

    await act.Should().ThrowAsync<OperationCanceledException>();
  }

  private sealed class CancellingReader : IDiscStructureReader
  {
    public DiscKind Kind => DiscKind.Dvd;

    public int Priority => 1;

    public bool CanRead(string path, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? discRoot)
    {
      discRoot = path;
      return true;
    }

    public Task<DiscStructure?> ReadAsync(string discRoot, MediaAnalysisContext context, CancellationToken cancellationToken) =>
      throw new OperationCanceledException();
  }

  private sealed class ListProgress(List<AnalysisProgress> reports) : IProgress<AnalysisProgress>
  {
    public void Report(AnalysisProgress value) => reports.Add(value);
  }

  #endregion

  #region Builder

  [Fact]
  public async Task Builder_WithStrategy_KeepsTheDefaultsWhateverTheOrderOfTheCalls()
  {
    var fileSystem = new FakeFileSystem().AddFile(Movie);
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Movie).WithStreams(StreamKind.Video, 1);
    var extra = new StubAnalyzerStrategy();

    // The strategy is added before the file system is chosen, which used to decide which file system the defaults read.
    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .WithStrategy(extra)
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false)
      .Build();

    var file = await analyzer.AnalyzeAsync(new FileMediaSource(Movie));
    var other = await analyzer.AnalyzeAsync(new NetworkMediaSource("custom://thing"));

    file.Success.Should().BeTrue("the default strategy for a file is still there");
    other.Failure!.Message.Should().Be("handled by the extra strategy");
  }

  private sealed class StubAnalyzerStrategy : IMediaAnalysisStrategy
  {
    public int Priority => -100;

    public bool CanHandle(IMediaSource source) => source is NetworkMediaSource { Location: "custom://thing" };

    public Task<MediaAnalysisResult> AnalyzeAsync(
      IMediaSource source,
      MediaAnalysisContext context,
      CancellationToken cancellationToken) =>
      Task.FromResult(MediaAnalysisResult.Failed(source, AnalysisFailureReason.Unknown, "handled by the extra strategy"));
  }

  #endregion

  #region Decorators and options

  [Fact]
  public async Task Throttle_DisposedWhileAnAnalysisRuns_DoesNotMakeThatAnalysisFail()
  {
    var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    var inner = new StubAnalyzer(async (_, _, _) =>
    {
      await gate.Task;
      return new MediaAnalysisResult { Success = true };
    });
    var throttle = new ThrottlingAnalyzer(inner, 1);
    var running = throttle.AnalyzeAsync(new FileMediaSource(Movie));

    throttle.Dispose();
    gate.SetResult(true);

    (await running).Success.Should().BeTrue();
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void BufferSize_NotPositive_IsRejected(int size)
  {
    var act = () => new MediaAnalysisOptions { BufferSize = size };

    act.Should().Throw<ArgumentOutOfRangeException>();
  }

  [Fact]
  public async Task Stream_ThatNeverEnds_IsReadNoFurtherThanTheLimit()
  {
    var factory = new FakeNativeMediaInfoFactory();
    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .Configure(options =>
      {
        options.MaximumStreamBytes = 1024 * 1024;
        options.OffloadBlockingCalls = false;
      })
      .Build();
    using var endless = new EndlessStream();
    var clock = Stopwatch.StartNew();

    await analyzer.AnalyzeAsync(new StreamMediaSource(endless));

    clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
    factory.BytesSubmitted.Should().BeInRange(1024 * 1024, (1024 * 1024) + (64 * 1024));
  }

  private sealed class EndlessStream : Stream
  {
    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count) => count;

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
  }

  #endregion

  #region Parsers

  private static byte[] Mpls(int itemCount, int markCount, int markedItem, uint markTimestamp)
  {
    const int ItemLength = 18;
    var playListStart = 20;
    var itemsSize = itemCount * (2 + ItemLength);
    var markStart = playListStart + 10 + itemsSize;
    var data = new byte[markStart + 6 + (markCount * 14)];

    "MPLS0200".Select((c, i) => (c, i)).ToList().ForEach(x => data[x.i] = (byte)x.c);
    WriteU32(data, 8, (uint)playListStart);
    WriteU32(data, 12, (uint)markStart);
    WriteU16(data, playListStart + 6, (ushort)itemCount);

    var offset = playListStart + 10;
    for (var i = 0; i < itemCount; i++)
    {
      WriteU16(data, offset, ItemLength);
      var body = offset + 2;
      "00001".Select((c, j) => (c, j)).ToList().ForEach(x => data[body + x.j] = (byte)x.c);
      WriteU32(data, body + 10, (uint)(1000 * (i + 1)));
      WriteU32(data, body + 14, (uint)((1000 * (i + 1)) + 450_000));
      offset = body + ItemLength;
    }

    WriteU16(data, markStart + 4, (ushort)markCount);
    for (var i = 0; i < markCount; i++)
    {
      var entry = markStart + 6 + (i * 14);
      data[entry + 1] = 0x01;
      WriteU16(data, entry + 2, (ushort)markedItem);
      WriteU32(data, entry + 4, markTimestamp);
    }

    return data;
  }

  private static void WriteU16(byte[] data, int offset, ushort value)
  {
    data[offset] = (byte)(value >> 8);
    data[offset + 1] = (byte)value;
  }

  private static void WriteU32(byte[] data, int offset, uint value)
  {
    data[offset] = (byte)(value >> 24);
    data[offset + 1] = (byte)(value >> 16);
    data[offset + 2] = (byte)(value >> 8);
    data[offset + 3] = (byte)value;
  }

  [Fact]
  public void Mpls_MarkOfALaterItem_IsPlacedOnThePlaylist()
  {
    // Item 1 starts on the playlist after item 0, which is ten seconds long; the mark is one second into item 1.
    var playlist = MplsParser.Parse(Mpls(itemCount: 3, markCount: 1, markedItem: 1, markTimestamp: 2000 + 45_000));

    playlist.Chapters.Should().ContainSingle().Which.Start.Should().Be(TimeSpan.FromSeconds(11));
  }

  [Fact]
  public void Mpls_ThousandsOfItemsAndMarks_AreParsedInLinearTime()
  {
    var data = Mpls(itemCount: 5000, markCount: 65_535, markedItem: 4999, markTimestamp: 5_000_000);
    var clock = Stopwatch.StartNew();

    var playlist = MplsParser.Parse(data);

    playlist.Items.Should().HaveCount(5000);
    playlist.Chapters.Should().HaveCount(65_535);
    clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
  }

  [Fact]
  public void Ifo_SectorNumberThatWrapsAroundWhenMultiplied_IsRejected()
  {
    var data = DiscFixtures.VideoManager((1, 1, 3, 1));
    IfoParser.ReadTitles(data).Should().HaveCount(1, "the table is where the file says it is");

    // 0x00200001 sectors of 2048 bytes is 2^32 + 2048: as an int that wraps around to a valid looking 2048.
    WriteU32(data, 0xC4, 0x00200001);

    IfoParser.ReadTitles(data).Should().BeEmpty();
  }

  [Fact]
  public async Task IfoReader_FileFarLargerThanAnIfoCanBe_IsNotReadIntoMemory()
  {
    var fileSystem = new FakeFileSystem().AddFile(P("/disc/VIDEO_TS/VIDEO_TS.IFO"), 2L * 1024 * 1024 * 1024);
    var reader = new IfoDvdStructureReader(fileSystem);

    var structure = await reader.ReadAsync(P("/disc/VIDEO_TS"), CreateContext(fileSystem), CancellationToken.None);

    structure.Should().BeNull();
  }

  #endregion

  #region Patterns

  [Fact]
  public async Task ExternalSubtitles_NameWithWildcards_DoesNotMatchOtherFiles()
  {
    var fileSystem = new FakeFileSystem()
      .AddFile(P("/media/a*b.mkv"))
      .AddFile(P("/media/axxb.srt"));
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(P("/media/a*b.mkv")).WithStreams(StreamKind.Video, 1);
    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false)
      .WithExternalSubtitles()
      .Build();

    (await analyzer.AnalyzeAsync(new FileMediaSource(P("/media/a*b.mkv")))).HasExternalSubtitles.Should().BeFalse();

    fileSystem.AddFile(P("/media/a*b.srt"));

    (await analyzer.AnalyzeAsync(new FileMediaSource(P("/media/a*b.mkv")))).HasExternalSubtitles.Should().BeTrue();
  }

  #endregion
}
