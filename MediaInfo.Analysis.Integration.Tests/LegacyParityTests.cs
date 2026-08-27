#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using MediaInfo.Analysis.Results;
using Xunit;
using Xunit.Abstractions;

namespace MediaInfo.Analysis.Integration.Tests;

/// <summary>
/// Asserts that the new pipeline describes a media exactly as <see cref="MediaInfoWrapper"/> always did.
/// </summary>
/// <remarks>
/// This is the safety net for the whole rewrite. Every file of the committed corpus is analyzed twice, once through
/// the wrapper and once through the analyzer, and every value the two share is compared. A green suite here is what
/// makes it safe to tell an existing consumer that migrating changes nothing about the answers they get.
/// </remarks>
[Collection(NativeMediaCollection.Name)]
public class LegacyParityTests(ITestOutputHelper output)
{
  [Theory]
  [MemberData(nameof(FileAndStreamAnalysisTests.Corpus), MemberType = typeof(FileAndStreamAnalysisTests))]
  public async Task Analyzer_DescribesAFileExactlyAsTheWrapperDid(string fileName)
  {
    var path = TestMedia.Path(fileName);

    var legacy = new MediaInfoWrapper(path);
    var modern = (await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(path)).AsLegacy();

    output.WriteLine($"{fileName}: {legacy.Format} / {legacy.VideoCodec} / {legacy.AudioCodec}, {legacy.Duration} ms");

    using var _ = new AssertionScope(fileName);

    modern.Success.Should().Be(legacy.Success);
    modern.Duration.Should().Be(legacy.Duration);
    modern.Size.Should().Be(legacy.Size);
    modern.Version.Should().Be(legacy.Version);

    modern.Format.Should().Be(legacy.Format);
    modern.FormatVersion.Should().Be(legacy.FormatVersion);
    modern.Profile.Should().Be(legacy.Profile);
    modern.Codec.Should().Be(legacy.Codec);
    modern.IsStreamable.Should().Be(legacy.IsStreamable);
    modern.WritingApplication.Should().Be(legacy.WritingApplication);
    modern.WritingLibrary.Should().Be(legacy.WritingLibrary);
    modern.Attachments.Should().Be(legacy.Attachments);

    modern.HasVideo.Should().Be(legacy.HasVideo);
    modern.VideoStreams.Should().HaveCount(legacy.VideoStreams.Count);
    modern.VideoCodec.Should().Be(legacy.VideoCodec);
    modern.Framerate.Should().Be(legacy.Framerate);
    modern.Width.Should().Be(legacy.Width);
    modern.Height.Should().Be(legacy.Height);
    modern.AspectRatio.Should().Be(legacy.AspectRatio);
    modern.ScanType.Should().Be(legacy.ScanType);
    modern.IsInterlaced.Should().Be(legacy.IsInterlaced);
    modern.VideoResolution.Should().Be(legacy.VideoResolution);
    modern.VideoRate.Should().Be(legacy.VideoRate);
    modern.VideoRotation.Should().Be(legacy.VideoRotation);
    modern.Is3D.Should().Be(legacy.Is3D);
    modern.IsHdr.Should().Be(legacy.IsHdr);

    modern.AudioStreams.Should().HaveCount(legacy.AudioStreams.Count);
    modern.AudioCodec.Should().Be(legacy.AudioCodec);
    modern.AudioRate.Should().Be(legacy.AudioRate);
    modern.AudioSampleRate.Should().Be(legacy.AudioSampleRate);
    modern.AudioChannels.Should().Be(legacy.AudioChannels);
    modern.AudioChannelsTotal.Should().Be(legacy.AudioChannelsTotal);
    modern.AudioChannelsFriendly.Should().Be(legacy.AudioChannelsFriendly);

    modern.HasSubtitles.Should().Be(legacy.HasSubtitles);
    modern.Subtitles.Should().HaveCount(legacy.Subtitles.Count);
    modern.HasChapters.Should().Be(legacy.HasChapters);
    modern.Chapters.Should().HaveCount(legacy.Chapters.Count);
    modern.MenuStreams.Should().HaveCount(legacy.MenuStreams.Count);

    modern.IsDvd.Should().Be(legacy.IsDvd);
    modern.IsBluRay.Should().Be(legacy.IsBluRay);
  }

  [Theory]
  [MemberData(nameof(FileAndStreamAnalysisTests.Corpus), MemberType = typeof(FileAndStreamAnalysisTests))]
  public async Task Analyzer_BuildsTheSameStreamsAsTheWrapperDid(string fileName)
  {
    var path = TestMedia.Path(fileName);

    var legacy = new MediaInfoWrapper(path);
    var modern = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(path);

    using var _ = new AssertionScope(fileName);

    foreach (var (expected, actual) in legacy.VideoStreams.Zip(modern.VideoStreams))
    {
      actual.Codec.Should().Be(expected.Codec);
      actual.CodecName.Should().Be(expected.CodecName);
      actual.Width.Should().Be(expected.Width);
      actual.Height.Should().Be(expected.Height);
      actual.FrameRate.Should().Be(expected.FrameRate);
      actual.BitDepth.Should().Be(expected.BitDepth);
      actual.Hdr.Should().Be(expected.Hdr);
      actual.Stereoscopic.Should().Be(expected.Stereoscopic);
      actual.ColorSpace.Should().Be(expected.ColorSpace);
      actual.SubSampling.Should().Be(expected.SubSampling);
      actual.Standard.Should().Be(expected.Standard);
      actual.Duration.Should().Be(expected.Duration);
      actual.StreamNumber.Should().Be(expected.StreamNumber);
    }

    foreach (var (expected, actual) in legacy.AudioStreams.Zip(modern.AudioStreams))
    {
      actual.Codec.Should().Be(expected.Codec);
      actual.CodecName.Should().Be(expected.CodecName);
      actual.Channel.Should().Be(expected.Channel);
      actual.SamplingRate.Should().Be(expected.SamplingRate);
      actual.BitDepth.Should().Be(expected.BitDepth);
      actual.Bitrate.Should().Be(expected.Bitrate);
      actual.Language.Should().Be(expected.Language);
      actual.StreamNumber.Should().Be(expected.StreamNumber);
    }

    foreach (var (expected, actual) in legacy.Subtitles.Zip(modern.Subtitles))
    {
      actual.Format.Should().Be(expected.Format);
      actual.Codec.Should().Be(expected.Codec);
      actual.Language.Should().Be(expected.Language);
      actual.StreamNumber.Should().Be(expected.StreamNumber);
    }
  }

  [Theory]
  [MemberData(nameof(FileAndStreamAnalysisTests.Corpus), MemberType = typeof(FileAndStreamAnalysisTests))]
  public async Task Analyzer_ReadsTheSameTagsAsTheWrapperDid(string fileName)
  {
    var path = TestMedia.Path(fileName);

    var legacy = new MediaInfoWrapper(path);
    var modern = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(path);

    using var _ = new AssertionScope(fileName);

    modern.General.Tags.GeneralTags.Should().HaveCount(legacy.Tags.GeneralTags.Count);
    modern.General.Tags.Title.Should().Be(legacy.Tags.Title);
    modern.General.Tags.EncodedDate.Should().Be(legacy.Tags.EncodedDate);
    modern.General.Tags.TaggedDate.Should().Be(legacy.Tags.TaggedDate);
  }

  /// <summary>
  /// A disc is where the two deliberately diverge, and this records exactly how far.
  /// </summary>
  /// <remarks>
  /// The wrapper reads the stream details out of a <c>.BUP</c> file, which is the backup copy of the navigation
  /// tables rather than the feature. The analyzer reads them out of the first content VOB of the main title. The two
  /// therefore agree on what the disc is and how long it runs, and disagree on the stream details, where the analyzer
  /// describes the media that actually plays.
  /// </remarks>
  [DvdFact]
  public async Task Analyzer_DescribesADvdAsTheWrapperDidExceptForTheStreamsItReadsFromTheFeature()
  {
    var path = TestMedia.FindDvd()!;

    var legacy = new MediaInfoWrapper(path);
    var result = await MediaInfoAnalyzer.CreateDefault().AnalyzeAsync(path);
    var modern = result.AsLegacy();

    output.WriteLine($"legacy  : {legacy.Duration} ms, {legacy.Size:N0} bytes, {legacy.VideoCodec} / {legacy.AudioCodec}");
    output.WriteLine($"modern  : {modern.Duration} ms, {modern.Size:N0} bytes, {modern.VideoCodec} / {modern.AudioCodec}");
    output.WriteLine($"read from {result.AnalyzedPath}");
    foreach (var audio in result.AudioStreams)
    {
      output.WriteLine($"  audio: {audio.CodecName}, {audio.Channel} ch, {audio.Bitrate} bps, {audio.Language}");
    }

    using var _ = new AssertionScope();

    // What the disc is, and how much of it there is, must agree exactly.
    modern.Success.Should().Be(legacy.Success);
    modern.IsDvd.Should().BeTrue();
    modern.IsDvd.Should().Be(legacy.IsDvd);
    modern.IsBluRay.Should().Be(legacy.IsBluRay);
    modern.Size.Should().Be(legacy.Size, "both report the size of the whole VIDEO_TS folder");
    modern.Duration.Should().Be(legacy.Duration, "both take the duration from the navigation tables of the title");

    // The stream details come from the feature rather than from a navigation file, so they are richer.
    result.AnalyzedPath.Should().EndWith(".VOB");
    modern.HasVideo.Should().BeTrue();
    modern.Width.Should().BeGreaterThan(0);
    modern.Height.Should().BeGreaterThan(0);
    modern.AudioStreams.Should().NotBeEmpty();
    modern.VideoCodec.Should().StartWith(legacy.VideoCodec.Split(' ')[0], "the same family, described more precisely");
  }

  [Fact]
  public async Task RepeatedAnalysis_DoesNotLeakNativeMemory()
  {
    var path = TestMedia.Path("Test_H264_DTS1.m2ts");
    var analyzer = MediaInfoAnalyzer.CreateDefault();

    for (var i = 0; i < 10; i++)
    {
      await analyzer.AnalyzeAsync(path);
    }

    Collect();
    var managedBefore = GC.GetTotalMemory(false);
    var privateBefore = Process.GetCurrentProcess().PrivateMemorySize64;

    for (var i = 0; i < 200; i++)
    {
      (await analyzer.AnalyzeAsync(path)).Success.Should().BeTrue();
    }

    Collect();
    var managedGrowth = GC.GetTotalMemory(false) - managedBefore;
    var privateGrowth = Process.GetCurrentProcess().PrivateMemorySize64 - privateBefore;

    output.WriteLine($"After 200 analyses: managed {managedGrowth:N0} bytes, private {privateGrowth:N0} bytes");

    using var _ = new AssertionScope();

    // The same thresholds the wrapper is held to, so that the guard against issue #52 also covers the new path.
    managedGrowth.Should().BeLessThan(20L * 1024 * 1024);
    privateGrowth.Should().BeLessThan(50L * 1024 * 1024);

    static void Collect()
    {
      for (var i = 0; i < 3; i++)
      {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
      }
    }
  }
}
