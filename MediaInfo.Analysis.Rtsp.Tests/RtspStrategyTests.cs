#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Abstractions;
using MediaInfo.Analysis.Native;
using MediaInfo.Analysis.Pipeline;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Rtsp.Rtsp;
using MediaInfo.Analysis.Sources;
using MediaInfo.Analysis.Strategies;
using MediaInfo.Analysis.Strategies.Selection;
using Xunit;
using Xunit.Abstractions;

namespace MediaInfo.Analysis.Rtsp.Tests;

/// <summary>Tests for answering an authentication challenge and for choosing the RTSP strategy.</summary>
public class RtspStrategyTests(ITestOutputHelper output)
{
  private static MediaAnalysisContext CreateContext() =>
    new(
      new MediaProbe(MediaInfoLibFactory.Instance, DefaultStreamSelectionStrategy.Instance),
      MediaInfoFileSystem.Instance);

  #region Authentication

  [Fact]
  public void Digest_AnswersAChallengeWithTheValueTheSpecificationDefines()
  {
    // The example credentials of RFC 2617, hashed for an RTSP request rather than an HTTP one.
    var authenticator = new RtspAuthenticator(new NetworkCredential("Mufasa", "Circle Of Life"));

    authenticator
      .AcceptChallenge("Digest realm=\"testrealm@host.com\", nonce=\"dcd98b7102dd2f0e8b11d0f600bfb0c093\"")
      .Should().BeTrue();

    var authorization = authenticator.CreateAuthorization("DESCRIBE", "rtsp://host/stream")!;

    output.WriteLine(authorization);

    authorization.Should().StartWith("Digest ");
    authorization.Should().Contain("username=\"Mufasa\"");
    authorization.Should().Contain("realm=\"testrealm@host.com\"");
    authorization.Should().Contain("nonce=\"dcd98b7102dd2f0e8b11d0f600bfb0c093\"");
    authorization.Should().Contain("uri=\"rtsp://host/stream\"");
    authorization.Should().Contain("response=\"86543b3cbadd277ce9e14fff2af997dd\"");
  }

  [Fact]
  public void Basic_AnswersWithTheEncodedCredentials()
  {
    var authenticator = new RtspAuthenticator(new NetworkCredential("admin", "secret"));

    authenticator.AcceptChallenge("Basic realm=\"camera\"").Should().BeTrue();

    authenticator.CreateAuthorization("DESCRIBE", "rtsp://host/stream")
      .Should().Be("Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("admin:secret")));
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("Negotiate something")]
  [InlineData("Digest realm=\"camera\"")]
  public void Challenge_ThatCannotBeAnsweredIsDeclined(string? challenge)
  {
    var authenticator = new RtspAuthenticator(new NetworkCredential("admin", "secret"));

    authenticator.AcceptChallenge(challenge).Should().BeFalse("a digest challenge without a nonce is unusable");
    authenticator.CanAuthenticate.Should().BeFalse();
    authenticator.CreateAuthorization("DESCRIBE", "rtsp://host/stream").Should().BeNull();
  }

  [Fact]
  public void Challenge_WithoutCredentialsIsDeclined()
  {
    var authenticator = new RtspAuthenticator(null);

    authenticator.AcceptChallenge("Digest realm=\"c\", nonce=\"n\"").Should().BeFalse();
  }

  [Theory]
  [InlineData("realm=\"a\", nonce=\"b\"", "a", "b")]
  [InlineData("realm=a,nonce=b", "a", "b")]
  [InlineData("  realm = \"a\" ,  nonce = \"b\"  ", "a", "b")]
  public void ChallengeParameters_AreReadWhicheverWayTheServerSpacedThem(string value, string realm, string nonce)
  {
    var parameters = RtspAuthenticator.ParseParameters(value);

    parameters["realm"].Should().Be(realm);
    parameters["nonce"].Should().Be(nonce);
  }

  #endregion

  #region Strategy selection

  [Theory]
  [InlineData("rtsp://camera/stream", true)]
  [InlineData("RTSP://camera/stream", true)]
  [InlineData("http://host/movie.mp4", false)]
  [InlineData("rtmp://host/live", false)]
  public void CanHandle_ClaimsAnRtspLocationAndNothingElse(string location, bool expected)
  {
    new RtspAnalysisStrategy().CanHandle(new NetworkMediaSource(location)).Should().Be(expected);
  }

  [Fact]
  public void CanHandle_IgnoresAFileEvenWhenItsNameLooksLikeAStream()
  {
    new RtspAnalysisStrategy().CanHandle(new FileMediaSource(@"D:\media\rtsp.mkv")).Should().BeFalse();
  }

  [Fact]
  public void Priority_OutranksTheStrategyThatDeclinesLiveStreams()
  {
    new RtspAnalysisStrategy().Priority.Should().BeLessThan(
      new UnsupportedSourceStrategy().Priority,
      "otherwise every RTSP location would be declined before this strategy is asked");
  }

  [Fact]
  public async Task Analyze_OfAServerThatIsNotThereFailsWithAReason()
  {
    var options = new RtspAnalysisOptions
    {
      ConnectTimeout = TimeSpan.FromMilliseconds(300),
      ReceiveTimeout = TimeSpan.FromMilliseconds(300)
    };

    // Port zero is never listening, so this exercises the failure path without waiting for a real timeout.
    var result = await new RtspAnalysisStrategy(options).AnalyzeAsync(
      new NetworkMediaSource("rtsp://127.0.0.1:1/stream"),
      CreateContext(),
      CancellationToken.None);

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.NativeOpenFailed);
    result.Failure.Message.Should().Contain("rtsp://127.0.0.1:1/stream");
  }

  [Fact]
  public async Task Analyze_OfAMalformedLocationFailsWithoutTouchingTheNetwork()
  {
    var result = await new RtspAnalysisStrategy().AnalyzeAsync(
      new NetworkMediaSource("rtsp://"),
      CreateContext(),
      CancellationToken.None);

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.SourceNotSpecified);
  }

  [Fact]
  public async Task Analyze_ObservesCancellation()
  {
    using var cancellation = new CancellationTokenSource();
    await cancellation.CancelAsync();

    var analyze = () => new RtspAnalysisStrategy().AnalyzeAsync(
      new NetworkMediaSource("rtsp://127.0.0.1:1/stream"),
      CreateContext(),
      cancellation.Token);

    await analyze.Should().ThrowAsync<OperationCanceledException>();
  }

  [Fact]
  public void WithRtsp_KeepsTheDefaultStrategiesAlongsideTheRtspOne()
  {
    var analyzer = MediaInfoAnalyzerBuilder.Create().WithRtsp().Build();

    analyzer.Should().BeOfType<MediaInfoAnalyzer>();
  }

  [Fact]
  public async Task WithRtsp_MakesAnAnalyzerThatStillReadsOrdinarySources()
  {
    var analyzer = MediaInfoAnalyzerBuilder.Create().WithRtsp().Build();

    // A file that does not exist proves the file strategy is still registered: without it the answer would be
    // that no strategy handles the media at all.
    var result = await analyzer.AnalyzeAsync(@"D:\media\does-not-exist.mkv");

    result.Failure!.Reason.Should().Be(AnalysisFailureReason.SourceNotFound);
  }

  #endregion

  /// <summary>
  /// Determines whether a server is listening where the integration tests expect one.
  /// </summary>
  internal static bool IsServerListening(string host, int port)
  {
    try
    {
      using var client = new TcpClient();
      return client.ConnectAsync(host, port).Wait(TimeSpan.FromMilliseconds(500)) && client.Connected;
    }
    catch (Exception exception) when (exception is SocketException or AggregateException)
    {
      return false;
    }
  }
}
