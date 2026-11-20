#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Http;
using MediaInfo.Analysis.Pipeline;
using MediaInfo.Analysis.Strategies.Selection;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Strategies;
using MediaInfo.Analysis.Tests.Fakes;
using Xunit;

namespace MediaInfo.Analysis.Tests;

public sealed class HttpAnalysisStrategyTests
{
  private static readonly byte[] Data = Enumerable.Range(0, 100).Select(x => (byte)x).ToArray();

  private static MediaAnalysisContext CreateContext() =>
    new(new MediaProbe(new FakeNativeMediaInfoFactory(), DefaultStreamSelectionStrategy.Instance), new FakeFileSystem());

  private static HttpAnalysisOptions Options(int block = 10) => new()
  {
    BlockSize = block,
    RequestTimeout = TimeSpan.FromSeconds(5),
    ReceiveTimeout = TimeSpan.FromSeconds(5)
  };

  private static Task<MediaAnalysisResult> RunAsync(
    string url,
    HttpAnalysisOptions options,
    StreamReader reader,
    CancellationToken cancellationToken = default) =>
    new HttpAnalysisStrategy(options, reader).AnalyzeAsync(new NetworkMediaSource(url), CreateContext(), cancellationToken);

  /// <summary>Stands in for the strategy that gives the stream to the native library.</summary>
  private sealed class StreamReader(Action<Stream>? read = null) : IMediaAnalysisStrategy
  {
    public bool? Seekable { get; private set; }

    public byte[] Head { get; private set; } = [];

    public byte[] Tail { get; private set; } = [];

    public int Priority => 0;

    public bool CanHandle(IMediaSource source) => true;

    public async Task<MediaAnalysisResult> AnalyzeAsync(IMediaSource source, MediaAnalysisContext context, CancellationToken cancellationToken)
    {
      var stream = ((StreamMediaSource)source).Stream;
      Seekable = stream.CanSeek;
      var buffer = new byte[Data.Length];

      if (stream.CanSeek)
      {
        // The end first, as the library does for a file that has its index last.
        stream.Position = stream.Length - 5;
        var tail = await ReadFullyAsync(stream, buffer, 5, cancellationToken);
        Tail = buffer.AsSpan(0, tail).ToArray();
        stream.Position = 0;
      }

      var count = await ReadFullyAsync(stream, buffer, 5, cancellationToken);
      Head = buffer.AsSpan(0, count).ToArray();
      read?.Invoke(stream);
      return new MediaAnalysisResult { Success = true };
    }

    private static async Task<int> ReadFullyAsync(Stream stream, byte[] buffer, int count, CancellationToken cancellationToken)
    {
      var total = 0;
      while (total < count)
      {
        var read = await stream.ReadAsync(buffer.AsMemory(total, count - total), cancellationToken);
        if (read == 0)
        {
          break;
        }

        total += read;
      }

      return total;
    }
  }

  [Fact]
  public async Task Range_Server_GivesASeekableStreamReadInBlocks()
  {
    using var server = new TestHttpServer();
    server.Add("/a.mp4", Data);
    var reader = new StreamReader();

    var result = await RunAsync(server.Url("/a.mp4"), Options(), reader);

    result.Success.Should().BeTrue();
    result.SourceKind.Should().Be(MediaSourceKind.Network);
    reader.Seekable.Should().BeTrue();
    reader.Tail.Should().Equal(Data.Skip(95).ToArray());
    reader.Head.Should().Equal(Data.Take(5).ToArray());
    server.Requests.Should().Contain(x => x.Range == "bytes=95-99");
    server.Requests.Should().OnlyContain(x => x.Range != null);
  }

  [Fact]
  public async Task NoRangeServer_GivesTheBodyAsItComes()
  {
    using var server = new TestHttpServer();
    server.Add("/a.mp4", Data).IgnoreRange = true;
    var reader = new StreamReader();

    var result = await RunAsync(server.Url("/a.mp4"), Options(), reader);

    result.Success.Should().BeTrue();
    reader.Seekable.Should().BeFalse();
    reader.Head.Should().Equal(Data.Take(5).ToArray());
  }

  [Fact]
  public async Task EmptyMedia_IsReportedAsEmpty()
  {
    using var server = new TestHttpServer();
    server.Add("/empty", []);

    var result = await RunAsync(server.Url("/empty"), Options(), new StreamReader());

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.SourceEmpty);
  }

  [Theory]
  [InlineData(404)]
  [InlineData(410)]
  public async Task MissingMedia_IsReportedAsNotFound(int status)
  {
    using var server = new TestHttpServer();
    server.Add("/gone", Data).Status = status;

    var result = await RunAsync(server.Url("/gone"), Options(), new StreamReader());

    result.Failure!.Reason.Should().Be(AnalysisFailureReason.SourceNotFound);
  }

  [Fact]
  public async Task ServerError_IsReportedAsOpenFailure()
  {
    using var server = new TestHttpServer();
    server.Add("/broken", Data).Status = 500;

    var result = await RunAsync(server.Url("/broken"), Options(), new StreamReader());

    result.Failure!.Reason.Should().Be(AnalysisFailureReason.NativeOpenFailed);
  }

  [Fact]
  public async Task ServerThatNeverAnswers_TimesOutWithinTheLimit()
  {
    using var server = new TestHttpServer();
    server.Add("/silent", Data).Behaviour = HttpBehaviour.Silent;
    var options = Options();
    options.RequestTimeout = TimeSpan.FromMilliseconds(300);

    var watch = Stopwatch.StartNew();
    var result = await RunAsync(server.Url("/silent"), options, new StreamReader());

    result.Failure!.Reason.Should().Be(AnalysisFailureReason.Timeout);
    watch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
  }

  [Fact]
  public async Task ServerThatStopsSendingTheBody_TimesOutWithinTheLimit()
  {
    using var server = new TestHttpServer();
    server.Add("/stall", Data).Behaviour = HttpBehaviour.StallBody;
    var options = Options(block: 50);
    options.ReceiveTimeout = TimeSpan.FromMilliseconds(300);

    var watch = Stopwatch.StartNew();
    var result = await RunAsync(server.Url("/stall"), options, new StreamReader());

    result.Failure!.Reason.Should().Be(AnalysisFailureReason.Timeout);
    watch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
  }

  [Fact]
  public async Task Cancellation_EndsTheRequest()
  {
    using var server = new TestHttpServer();
    server.Add("/silent", Data).Behaviour = HttpBehaviour.Silent;
    using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

    var act = () => RunAsync(server.Url("/silent"), Options(), new StreamReader(), cancellation.Token);

    await act.Should().ThrowAsync<OperationCanceledException>();
  }

  [Fact]
  public async Task MediaThatChangesWhileItIsRead_IsReportedAndNotMixed()
  {
    using var server = new TestHttpServer();
    var resource = server.Add("/a.mp4", Data);
    var reader = new StreamReader();

    // The first block is read, the media is replaced, and the next block is asked for.
    var strategy = new HttpAnalysisStrategy(Options(), new ChangingReader(() => resource.ETag = "\"v2\""));
    var result = await strategy.AnalyzeAsync(new NetworkMediaSource(server.Url("/a.mp4")), CreateContext(), CancellationToken.None);

    result.Success.Should().BeFalse();
    result.Failure!.Message.Should().Contain("changed");
    reader.Head.Should().BeEmpty();
  }

  private sealed class ChangingReader(Action change) : IMediaAnalysisStrategy
  {
    public int Priority => 0;

    public bool CanHandle(IMediaSource source) => true;

    public async Task<MediaAnalysisResult> AnalyzeAsync(IMediaSource source, MediaAnalysisContext context, CancellationToken cancellationToken)
    {
      var stream = ((StreamMediaSource)source).Stream;
      var buffer = new byte[1];
      _ = await stream.ReadAsync(buffer.AsMemory(), cancellationToken);
      change();
      stream.Position = 50;
      _ = await stream.ReadAsync(buffer.AsMemory(), cancellationToken);
      return new MediaAnalysisResult { Success = true };
    }
  }

  [Fact]
  public async Task CredentialsOfTheLocation_AreSentToItsServer_AndNotToTheOneARedirectionPointsTo()
  {
    using var target = new TestHttpServer();
    target.Add("/real", Data);
    using var origin = new TestHttpServer();
    origin.Add("/a.mp4", Data).Basic = ("user", "p@ss");
    origin.Resources["/a.mp4"].RedirectTo = null;
    origin.Add("/moved", Data).RedirectTo = target.Url("/real");

    var withCredentials = new Uri(origin.Url("/a.mp4")).GetLeftPart(UriPartial.Authority).Replace("://", "://user:p%40ss@") + "/a.mp4";
    var ok = await RunAsync(withCredentials, Options(), new StreamReader());
    ok.Success.Should().BeTrue();
    origin.Requests.Should().Contain(x => x.Authorization != null);

    var redirected = new Uri(origin.Url("/moved")).GetLeftPart(UriPartial.Authority).Replace("://", "://user:p%40ss@") + "/moved";
    var moved = await RunAsync(redirected, Options(), new StreamReader());
    moved.Success.Should().BeTrue();
    target.Requests.Should().NotBeEmpty();
    target.Requests.Should().OnlyContain(x => x.Authorization == null);
  }

  [Fact]
  public async Task SelfSignedCertificate_IsRefusedUnlessAllowed()
  {
    using var server = new TestHttpServer(tls: true);
    server.Add("/a.mp4", Data);

    var refused = await RunAsync(server.Url("/a.mp4"), Options(), new StreamReader());
    refused.Success.Should().BeFalse();
    refused.Failure!.Reason.Should().Be(AnalysisFailureReason.NativeOpenFailed);

    var options = Options();
    options.AllowInvalidCertificates = true;
    var reader = new StreamReader();
    var accepted = await RunAsync(server.Url("/a.mp4"), options, reader);

    accepted.Success.Should().BeTrue();
    reader.Head.Should().Equal(Data.Take(5).ToArray());
  }

  [Fact]
  public void CanHandle_OnlyHttpLocations_WhenEnabled()
  {
    var strategy = new HttpAnalysisStrategy();
    strategy.CanHandle(new NetworkMediaSource("https://example.org/a.mp4")).Should().BeTrue();
    strategy.CanHandle(new NetworkMediaSource("HTTP://example.org/a.mp4")).Should().BeTrue();
    strategy.CanHandle(new NetworkMediaSource("rtsp://example.org/a")).Should().BeFalse();
    strategy.CanHandle(new FileMediaSource("a.mp4")).Should().BeFalse();
    new HttpAnalysisStrategy(new HttpAnalysisOptions { Enabled = false })
      .CanHandle(new NetworkMediaSource("http://example.org/a.mp4")).Should().BeFalse();
  }
}
