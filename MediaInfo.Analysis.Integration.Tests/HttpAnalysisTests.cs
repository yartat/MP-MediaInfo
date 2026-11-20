#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Tests.Fakes;
using Xunit;

namespace MediaInfo.Analysis.Integration.Tests;

/// <summary>
/// Reads the media corpus through a local HTTP server with the real native library and compares it with the file.
/// </summary>
[Collection(NativeMediaCollection.Name)]
public class HttpAnalysisTests
{
  public static TheoryData<string, bool> Cases()
  {
    var data = new TheoryData<string, bool>();
    foreach (var row in (System.Collections.Generic.IEnumerable<object[]>)FileAndStreamAnalysisTests.Corpus)
    {
      var name = (string)row[0];
      data.Add(name, false);
      data.Add(name, true);
    }

    return data;
  }

  [Theory]
  [MemberData(nameof(Cases))]
  public async Task AnalyzeUrl_AgreesWithTheFileAnalysis(string fileName, bool ignoreRange)
  {
    var path = TestMedia.Path(fileName);
    using var server = new TestHttpServer();
    server.Add("/" + fileName, await File.ReadAllBytesAsync(path)).IgnoreRange = ignoreRange;
    var analyzer = MediaInfoAnalyzer.CreateDefault();

    var fromFile = await analyzer.AnalyzeAsync(path);
    var fromUrl = await analyzer.AnalyzeAsync(new NetworkMediaSource(server.Url("/" + fileName)));

    fromUrl.Success.Should().BeTrue(fromUrl.Failure?.ToString() ?? string.Empty);
    fromUrl.SourceKind.Should().Be(MediaSourceKind.Network);
    fromUrl.General.Format.Should().Be(fromFile.General.Format);
    fromUrl.VideoStreams.Should().HaveCount(fromFile.VideoStreams.Count);
    fromUrl.AudioStreams.Should().HaveCount(fromFile.AudioStreams.Count);
    fromUrl.Subtitles.Should().HaveCount(fromFile.Subtitles.Count);

    // A body that is read as it comes has no length the library can see.
    if (!ignoreRange)
    {
      fromUrl.General.Size.Should().Be(fromFile.General.Size);
    }
  }
}
