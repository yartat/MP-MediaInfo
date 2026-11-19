#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Rtsp.Rtsp;
using Xunit;

namespace MediaInfo.Analysis.Rtsp.Tests;

/// <summary>Tests for how the client answers more than one challenge, and for what it refuses to answer.</summary>
public class RtspAuthenticationTests
{
  private static readonly NetworkCredential Mufasa = new("Mufasa", "Circle Of Life");

  private const string Challenge =
    "Digest realm=\"testrealm@host.com\", qop=\"auth,auth-int\", " +
    "nonce=\"dcd98b7102dd2f0e8b11d0f600bfb0c093\", opaque=\"5ccc069c403ebaf9f0171e9517f40e41\"";

  [Fact]
  public void Digest_WithQualityOfProtection_MatchesTheExampleOfRfc2617()
  {
    // The only change from the example of the RFC is that the method and the location are those of an HTTP request.
    var authenticator = new RtspAuthenticator(Mufasa, createClientNonce: () => "0a4f113b");
    authenticator.AcceptChallenge(Challenge).Should().BeTrue();

    var authorization = authenticator.CreateAuthorization("GET", "/dir/index.html");

    authorization.Should()
      .Contain("response=\"6629fae49393a05397450978507c4ef1\"")
      .And.Contain("qop=auth")
      .And.Contain("nc=00000001")
      .And.Contain("cnonce=\"0a4f113b\"")
      .And.Contain("opaque=\"5ccc069c403ebaf9f0171e9517f40e41\"");
  }

  [Fact]
  public void Digest_CountsTheRequests()
  {
    var authenticator = new RtspAuthenticator(Mufasa, createClientNonce: () => "0a4f113b");
    authenticator.AcceptChallenge(Challenge).Should().BeTrue();

    authenticator.CreateAuthorization("DESCRIBE", "rtsp://host/stream");
    var second = authenticator.CreateAuthorization("SETUP", "rtsp://host/stream/track1");

    second.Should().Contain("nc=00000002");
  }

  [Fact]
  public void Digest_WithoutQualityOfProtection_IsAnsweredTheOldWay()
  {
    var authenticator = new RtspAuthenticator(Mufasa);
    authenticator.AcceptChallenge("Digest realm=\"r\", nonce=\"n\"").Should().BeTrue();

    var authorization = authenticator.CreateAuthorization("DESCRIBE", "rtsp://host/stream");

    authorization.Should().NotContain("qop").And.NotContain("cnonce").And.NotContain("opaque");
  }

  [Theory]
  [InlineData("Digest realm=\"r\", nonce=\"n\", algorithm=SHA-256")]
  [InlineData("Digest realm=\"r\", nonce=\"n\", qop=\"auth-int\"")]
  public void Digest_ThatCannotBeAnswered_IsDeclined(string challenge) =>
    new RtspAuthenticator(Mufasa).AcceptChallenge(challenge).Should().BeFalse();

  [Fact]
  public void Digest_WithTheAlgorithmNamedMd5_IsAnswered() =>
    new RtspAuthenticator(Mufasa).AcceptChallenge("Digest realm=\"r\", nonce=\"n\", algorithm=MD5").Should().BeTrue();

  [Fact]
  public void Challenges_BasicBeforeDigest_DigestWins()
  {
    var authenticator = new RtspAuthenticator(Mufasa);

    authenticator.AcceptChallenge("Basic realm=\"r\"\nDigest realm=\"r\", nonce=\"n\"").Should().BeTrue();

    authenticator.CreateAuthorization("DESCRIBE", "rtsp://host/stream").Should().StartWith("Digest ");
  }

  [Fact]
  public void Challenge_BasicAfterDigest_IsNotAnsweredWithThePassword()
  {
    var authenticator = new RtspAuthenticator(Mufasa);
    authenticator.AcceptChallenge("Digest realm=\"r\", nonce=\"n\"").Should().BeTrue();

    authenticator.AcceptChallenge("Basic realm=\"r\"").Should().BeFalse();

    authenticator.CreateAuthorization("SETUP", "rtsp://host/stream").Should().StartWith("Digest ");
  }

  [Fact]
  public void Challenge_NewDigestNonceAfterDigest_IsAccepted()
  {
    var authenticator = new RtspAuthenticator(Mufasa);
    authenticator.AcceptChallenge("Digest realm=\"r\", nonce=\"old\"").Should().BeTrue();

    authenticator.AcceptChallenge("Digest realm=\"r\", nonce=\"new\", stale=true").Should().BeTrue();

    authenticator.CreateAuthorization("DESCRIBE", "rtsp://host/stream").Should().Contain("nonce=\"new\"");
  }

  [Fact]
  public void Basic_WhenNotAllowed_IsDeclined()
  {
    var authenticator = new RtspAuthenticator(Mufasa, allowBasic: false);

    authenticator.AcceptChallenge("Basic realm=\"r\"").Should().BeFalse();

    authenticator.CanAuthenticate.Should().BeFalse();
  }

  [Fact]
  public void Basic_WhenNotAllowed_StillAnswersDigestOffered()
  {
    var authenticator = new RtspAuthenticator(Mufasa, allowBasic: false);

    authenticator.AcceptChallenge("Basic realm=\"r\"\nDigest realm=\"r\", nonce=\"n\"").Should().BeTrue();
  }

  [Fact]
  public void Digest_ValuesWithQuotesAndBackslashes_AreEscaped()
  {
    var authenticator = new RtspAuthenticator(new NetworkCredential("a\"b\\c", "secret"));
    authenticator.AcceptChallenge("Digest realm=\"r\", nonce=\"n\"").Should().BeTrue();

    authenticator.CreateAuthorization("DESCRIBE", "rtsp://host/stream")
      .Should().Contain("username=\"a\\\"b\\\\c\"");
  }

  [Theory]
  [InlineData("user\r\nX-Injected: yes")]
  [InlineData("user\nname")]
  [InlineData("user\0name")]
  public void Digest_UserNameWithControlCharacters_IsNotPutIntoAHeader(string userName)
  {
    var authenticator = new RtspAuthenticator(new NetworkCredential(userName, "secret"));
    authenticator.AcceptChallenge("Digest realm=\"r\", nonce=\"n\"").Should().BeTrue();

    authenticator.CreateAuthorization("DESCRIBE", "rtsp://host/stream").Should().BeNull();
  }

  [Fact]
  public void Digest_ClientNonce_IsDifferentEveryTime()
  {
    var first = new RtspAuthenticator(Mufasa);
    var second = new RtspAuthenticator(Mufasa);
    first.AcceptChallenge(Challenge);
    second.AcceptChallenge(Challenge);

    first.CreateAuthorization("GET", "/").Should().NotBe(second.CreateAuthorization("GET", "/"));
  }

  [Fact]
  public async Task Capture_ServerOffersBasicThenDigest_IsAnsweredWithDigest()
  {
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port;
    string? received = null;

    // The server lists basic last, which a client that keeps only the last header would answer with the password.
    var server = Task.Run(async () =>
    {
      using var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
      var stream = client.GetStream();
      await ReadRequestAsync(stream).ConfigureAwait(false);
      await SendAsync(
        stream,
        "RTSP/1.0 401 Unauthorized\r\nCSeq: 1\r\nWWW-Authenticate: Digest realm=\"r\", nonce=\"n\"\r\n" +
        "WWW-Authenticate: Basic realm=\"r\"\r\n\r\n").ConfigureAwait(false);

      received = await ReadRequestAsync(stream).ConfigureAwait(false);
      await SendAsync(stream, "RTSP/1.0 404 Not Found\r\nCSeq: 2\r\n\r\n").ConfigureAwait(false);
    });

    using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    var uri = new Uri($"rtsp://admin:secret@127.0.0.1:{port}/stream");
    var act = () => RtspStreamCapture.CaptureAsync(uri, new RtspAnalysisOptions(), cancellation.Token);

    await act.Should().ThrowAsync<RtspProtocolException>();
    await server.ConfigureAwait(false);
    listener.Stop();

    received.Should().Contain("Authorization: Digest ").And.NotContain("Authorization: Basic");
  }

  [Fact]
  public async Task Capture_BasicNotAllowed_NeverSendsThePassword()
  {
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port;
    var requests = 0;

    var server = Task.Run(async () =>
    {
      using var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
      var stream = client.GetStream();
      await ReadRequestAsync(stream).ConfigureAwait(false);
      requests++;
      await SendAsync(stream, "RTSP/1.0 401 Unauthorized\r\nCSeq: 1\r\nWWW-Authenticate: Basic realm=\"r\"\r\n\r\n")
        .ConfigureAwait(false);

      // A client that sent the password would send a second request.
      using var wait = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
      try
      {
        var second = await ReadRequestAsync(stream, wait.Token).ConfigureAwait(false);
        if (second.Length > 0)
        {
          requests++;
        }
      }
      catch (OperationCanceledException)
      {
      }
    });

    using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    var uri = new Uri($"rtsp://admin:secret@127.0.0.1:{port}/stream");
    var options = new RtspAnalysisOptions { AllowBasicAuthentication = false };
    var act = () => RtspStreamCapture.CaptureAsync(uri, options, cancellation.Token);

    await act.Should().ThrowAsync<RtspProtocolException>().WithMessage("*401*");
    await server.ConfigureAwait(false);
    listener.Stop();

    requests.Should().Be(1);
  }

  private static async Task<string> ReadRequestAsync(NetworkStream stream, CancellationToken cancellationToken = default)
  {
    var text = new StringBuilder();
    var buffer = new byte[1];
    while (!text.ToString().EndsWith("\r\n\r\n", StringComparison.Ordinal))
    {
      if (await stream.ReadAsync(buffer, 0, 1, cancellationToken).ConfigureAwait(false) == 0)
      {
        break;
      }

      text.Append((char)buffer[0]);
    }

    return text.ToString();
  }

  private static Task SendAsync(NetworkStream stream, string text)
  {
    var bytes = Encoding.ASCII.GetBytes(text);
    return stream.WriteAsync(bytes, 0, bytes.Length);
  }
}
