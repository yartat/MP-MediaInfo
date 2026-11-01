#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Tests.Fakes;
using MediaInfo.Model;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for reading the HDR format details and the Dolby Vision metadata of a video stream.</summary>
public class DolbyVisionTests
{
  private const string Movie = @"D:\media\dv.mkv";

  private static async Task<VideoStream> AnalyzeAsync(System.Action<FakeMediaProfile> configure)
  {
    var fileSystem = new FakeFileSystem().AddFile(Movie, 4096);
    var factory = new FakeNativeMediaInfoFactory();
    var profile = factory.AddMedia(Movie).WithStreams(StreamKind.Video, 1);
    configure(profile);
    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false)
      .Build();

    var result = await analyzer.AnalyzeAsync(new FileMediaSource(Movie));
    return result.VideoStreams[0];
  }

  [Fact]
  public async Task Build_ReadsHdrFormatDetailsAndDolbyVision()
  {
    var video = await AnalyzeAsync(p => p
      .SetIndexed(StreamKind.Video, 0, (int)NativeMethods.Video.Video_HDR_Format, "Dolby Vision")
      .SetIndexed(StreamKind.Video, 0, (int)NativeMethods.Video.Video_HDR_Format_Version, "1.0")
      .SetIndexed(StreamKind.Video, 0, (int)NativeMethods.Video.Video_HDR_Format_Profile, "dvhe.08")
      .SetIndexed(StreamKind.Video, 0, (int)NativeMethods.Video.Video_HDR_Format_Level, "06")
      .SetIndexed(StreamKind.Video, 0, (int)NativeMethods.Video.Video_HDR_Format_Settings, "BL+RPU")
      .SetIndexed(StreamKind.Video, 0, (int)NativeMethods.Video.Video_HDR_Format_Compatibility, "HDR10")
      .Set(StreamKind.Video, 0, "DolbyVision", "Profile 8.1")
      .Set(StreamKind.Video, 0, "DolbyVision Profile", "8.1")
      .Set(StreamKind.Video, 0, "DolbyVision CMVersion", "4.0")
      .Set(StreamKind.Video, 0, "DolbyVision BaseLayer_ColorRange", "Limited")
      .Set(StreamKind.Video, 0, "DolbyVision MasteringDisplay_Luminance", "min: 0.0001 cd/m2, max: 4000 cd/m2"));

    video.Hdr.Should().Be(Hdr.DolbyVision);
    video.HdrInfo.Format.Should().Be(Hdr.DolbyVision);
    video.HdrInfo.Version.Should().Be("1.0");
    video.HdrInfo.Profile.Should().Be("dvhe.08");
    video.HdrInfo.Level.Should().Be("06");
    video.HdrInfo.Settings.Should().Be("BL+RPU");
    video.HdrInfo.Compatibility.Should().Be("HDR10");
    video.HdrInfo.DolbyVision.Should().NotBeNull();
    video.HdrInfo.DolbyVision!.Summary.Should().Be("Profile 8.1");
    video.HdrInfo.DolbyVision.Profile.Should().Be("8.1");
    video.HdrInfo.DolbyVision.CmVersion.Should().Be("4.0");
    video.HdrInfo.DolbyVision.BaseLayerColorRange.Should().Be("Limited");
    video.HdrInfo.DolbyVision.MasteringDisplayLuminance.Should().Be("min: 0.0001 cd/m2, max: 4000 cd/m2");
    video.HdrInfo.DolbyVision.EnhancementLayer.Should().BeEmpty();
  }

  [Fact]
  public async Task Build_WithoutDolbyVision_LeavesItNull()
  {
    var video = await AnalyzeAsync(_ => { });

    video.HdrInfo.DolbyVision.Should().BeNull();
    video.HdrInfo.Profile.Should().BeEmpty();
  }

  [Theory]
  [InlineData("Summary", "DolbyVision")]
  [InlineData("Profile", "DolbyVision Profile")]
  [InlineData("MasteringDisplay_Luminance", "DolbyVision MasteringDisplay_Luminance")]
  public void ToParameterName_PrefixesTheLibraryName(string field, string expected) =>
    System.Enum.Parse<DolbyVisionField>(field).ToParameterName().Should().Be(expected);
}
