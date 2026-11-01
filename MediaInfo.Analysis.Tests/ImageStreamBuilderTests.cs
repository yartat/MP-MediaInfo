#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Tests.Fakes;
using MediaInfo.Builder;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for reading an image stream.</summary>
public class ImageStreamBuilderTests
{
  [Fact]
  public void Build_ReadsImageProperties()
  {
    var reader = new NamedReader
    {
      ["Type"] = "Gain map",
      ["Rotation"] = "90",
      ["Format_Settings_SliceCount"] = "6",
      ["Mirror"] = "Horizontal",
      ["Derived from"] = "Primary image",
      ["GainMap_BaseHeadroom"] = "0",
      ["GainMap_AlternateHeadroom"] = "2.5",
      ["GainMap_Max"] = "3 3 3",
      ["GainMap_UseBaseColorSpace"] = "Yes",
    };

    var stream = new ImageStreamBuilder(reader, 0, 0).Build();

    stream.Type.Should().Be("Gain map");
    stream.Rotation.Should().Be(90);
    stream.SliceCount.Should().Be(6);
    stream.Mirror.Should().Be("Horizontal");
    stream.DerivedFrom.Should().Be("Primary image");
    stream.GainMap.Should().NotBeNull();
    stream.GainMap!.AlternateHeadroom.Should().Be("2.5");
    stream.GainMap.Max.Should().Be("3 3 3");
    stream.GainMap.UseBaseColorSpace.Should().BeTrue();
  }

  [Fact]
  public void Build_WithoutGainMap_LeavesGainMapNull()
  {
    var stream = new ImageStreamBuilder(new NamedReader(), 0, 0).Build();

    stream.GainMap.Should().BeNull();
    stream.SliceCount.Should().Be(0);
  }

  [Fact]
  public async Task AnalyzeAsync_ImageOnlyMedia_ReportsImageStreams()
  {
    const string Photo = @"D:\media\photo.heic";
    var fileSystem = new FakeFileSystem().AddFile(Photo, 4096);
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(Photo)
      .WithStreams(StreamKind.Image, 2)
      .Set(StreamKind.Image, 0, "Rotation", "180")
      .Set(StreamKind.Image, 0, "Format_Settings_SliceCount", "12")
      .Set(StreamKind.Image, 1, "GainMap_BaseHeadroom", "0");
    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .UseFileSystem(fileSystem)
      .Configure(options => options.OffloadBlockingCalls = false)
      .Build();

    var result = await analyzer.AnalyzeAsync(new FileMediaSource(Photo));

    result.Success.Should().BeTrue();
    result.ImageStreams.Should().HaveCount(2);
    result.ImageStreams[0].Rotation.Should().Be(180);
    result.ImageStreams[0].SliceCount.Should().Be(12);
    result.ImageStreams[1].GainMap.Should().NotBeNull();
  }

  private sealed class NamedReader : Dictionary<string, string>, IMediaInfoReader
  {
    public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch) =>
      Get(streamKind, streamNumber, parameter);

    public string Get(StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo) => string.Empty;

    public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo) =>
      Get(streamKind, streamNumber, parameter);

    public string Get(StreamKind streamKind, int streamNumber, string parameter) =>
      TryGetValue(parameter, out var value) ? value : string.Empty;

    public string Get(StreamKind streamKind, int streamNumber, int parameter) => string.Empty;

    public int CountGet(StreamKind streamKind, int streamNumber) => 0;

    public int CountGet(StreamKind streamKind) => 0;

    public string Option(string option, string value) => string.Empty;

    public string Option(string option) => string.Empty;

    public string Inform() => string.Empty;
  }
}
