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
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for keeping the credentials of a location out of what is shown, logged or returned.</summary>
public class CredentialRedactionTests
{
  private const string Secret = "s3cret";

  [Theory]
  [InlineData("http://user:s3cret@host/video.mp4", "http://***@host/video.mp4")]
  [InlineData("rtsp://admin:s3cret@192.168.1.5:554/stream?x=1", "rtsp://***@192.168.1.5:554/stream?x=1")]
  [InlineData("https://user@host", "https://***@host")]
  [InlineData("http://user:pa@ss@host/a", "http://***@host/a")]
  [InlineData("http://host/path@with-at", "http://host/path@with-at")]
  [InlineData("http://host/video.mp4?token=a@b", "http://host/video.mp4?token=a@b")]
  [InlineData("/media/movie.mkv", "/media/movie.mkv")]
  [InlineData("user@example.com", "user@example.com")]
  [InlineData("http://", "http://")]
  [InlineData("", "")]
  public void Redact_RemovesTheUserInformationOnly(string location, string expected) =>
    UrlRedactor.Redact(location).Should().Be(expected);

  [Fact]
  public void Redact_Null_IsNull() => UrlRedactor.Redact(null).Should().BeNull();

  [Fact]
  public void NetworkMediaSource_KeepsTheLocationAndHidesTheCredentialsInTheDisplayName()
  {
    var source = new NetworkMediaSource($"http://user:{Secret}@host/video.mp4");

    source.Location.Should().Contain(Secret, "the location is what is connected to");
    source.DisplayName.Should().NotContain(Secret);
  }

  [Fact]
  public async Task Analyze_NetworkMedia_NeverReturnsTheCredentials()
  {
    var location = $"http://user:{Secret}@host/video.mp4";
    var factory = new FakeNativeMediaInfoFactory();
    factory.AddMedia(location).WithStreams(StreamKind.Video, 1);
    var reports = new List<AnalysisProgress>();
    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .WithHttp(http => http.Enabled = false)
      .UseProgress(new SynchronousProgress(reports))
      .Configure(options => options.OffloadBlockingCalls = false)
      .Build();

    var result = await analyzer.AnalyzeAsync(new NetworkMediaSource(location));

    factory.OpenedPaths.Should().ContainSingle().Which.Should().Be(location, "the library is given the real location");
    result.Success.Should().BeTrue();
    result.SourcePath.Should().NotContain(Secret);
    result.AnalyzedPath.Should().NotContain(Secret);
    reports.Should().NotBeEmpty().And.OnlyContain(x => x.Detail == null || !x.Detail.Contains(Secret));
  }

  [Fact]
  public async Task Analyze_NetworkMediaThatCannotBeOpened_DoesNotNameTheCredentialsInTheFailure()
  {
    var location = $"http://user:{Secret}@host/missing.mp4";
    var factory = new FakeNativeMediaInfoFactory();
    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(factory)
      .Configure(options => options.OffloadBlockingCalls = false)
      .Build();

    var result = await analyzer.AnalyzeAsync(new NetworkMediaSource(location));

    result.Success.Should().BeFalse();
    result.Failure!.Message.Should().NotContain(Secret).And.Contain("***@host");
    result.SourcePath.Should().NotContain(Secret);
  }

  [Fact]
  public async Task Analyze_HttpMediaThatCannotBeReached_DoesNotNameTheCredentialsInTheFailure()
  {
    // Nothing listens on the port of a listener that was just stopped.
    var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
    listener.Start();
    var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .UseNativeFactory(new FakeNativeMediaInfoFactory())
      .Build();

    var result = await analyzer.AnalyzeAsync(new NetworkMediaSource($"http://user:{Secret}@127.0.0.1:{port}/video.mp4"));

    result.Success.Should().BeFalse();
    result.Failure!.Message.Should().NotContain(Secret);
    result.Failure.Exception?.ToString().Should().NotContain(Secret);
    result.SourcePath.Should().NotContain(Secret);
  }

  private sealed class SynchronousProgress(List<AnalysisProgress> reports) : System.IProgress<AnalysisProgress>
  {
    public void Report(AnalysisProgress value) => reports.Add(value);
  }
}
