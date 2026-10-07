#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System.Linq;
using FluentAssertions;
using MediaInfo.Analysis.Rtsp.Rtsp;
using MediaInfo.Analysis.Rtsp.Sdp;
using Xunit;

namespace MediaInfo.Analysis.Rtsp.Tests;

/// <summary>Tests for reading the session description a server answers DESCRIBE with.</summary>
public class SessionDescriptionTests
{
  private const string CameraSession = """
    v=0
    o=- 1234567890 1 IN IP4 192.168.1.10
    s=Media Presentation
    c=IN IP4 0.0.0.0
    t=0 0
    a=control:*
    m=video 0 RTP/AVP 96
    a=rtpmap:96 H264/90000
    a=fmtp:96 packetization-mode=1; profile-level-id=64001F; sprop-parameter-sets=Z2QAH6zZQKA9oQAAAwABAAADADyPFCKg,aOvjyyLA
    a=control:trackID=1
    m=audio 0 RTP/AVP 97
    a=rtpmap:97 mpeg4-generic/48000/2
    a=fmtp:97 profile-level-id=1;mode=AAC-hbr;sizelength=13;indexlength=3;config=1190
    a=control:trackID=2
    """;

  [Fact]
  public void Parse_ReadsBothTracksWithTheirControlAttributes()
  {
    var session = SessionDescription.Parse(CameraSession);

    session.Name.Should().Be("Media Presentation");
    session.Control.Should().Be("*");
    session.Media.Should().HaveCount(2);

    session.Video!.Kind.Should().Be("video");
    session.Video.Protocol.Should().Be("RTP/AVP");
    session.Video.Control.Should().Be("trackID=1");
    session.Audio!.Control.Should().Be("trackID=2");
  }

  [Fact]
  public void Parse_ReadsTheEncodingOfEachTrack()
  {
    var session = SessionDescription.Parse(CameraSession);

    var video = session.Video!.PrimaryRtpMap!;
    video.PayloadType.Should().Be(96);
    video.Encoding.Should().Be("H264");
    video.ClockRate.Should().Be(90000);
    video.Channels.Should().Be(0, "a video mapping states no channel count");

    var audio = session.Audio!.PrimaryRtpMap!;
    audio.Encoding.Should().Be("mpeg4-generic");
    audio.ClockRate.Should().Be(48000);
    audio.Channels.Should().Be(2);
  }

  [Theory]
  [InlineData("packetization-mode", "1")]
  [InlineData("profile-level-id", "64001F")]
  [InlineData("PROFILE-LEVEL-ID", "64001F")]
  public void GetFormatParameter_FindsAParameterWhateverItsCase(string name, string expected)
  {
    SessionDescription.Parse(CameraSession).Video!.GetFormatParameter(name).Should().Be(expected);
  }

  [Fact]
  public void GetFormatParameter_ReturnsNothingForAParameterThatIsNotDeclared()
  {
    SessionDescription.Parse(CameraSession).Video!.GetFormatParameter("sprop-max-don-diff").Should().BeNull();
  }

  [Fact]
  public void ParameterSets_AreDecodedFromTheSessionSoTheStreamStartsWithWhatDecodesIt()
  {
    var sets = RtspStreamCapture.ParameterSetsOf(SessionDescription.Parse(CameraSession).Video!).ToArray();

    sets.Should().HaveCount(2, "the session carries a sequence and a picture parameter set");
    (sets[0][0] & 0x1F).Should().Be(7, "the first is a sequence parameter set");
    (sets[1][0] & 0x1F).Should().Be(8, "the second is a picture parameter set");
  }

  [Fact]
  public void ParameterSets_SkipAnEntryThatIsNotBase64()
  {
    var session = SessionDescription.Parse(
      "m=video 0 RTP/AVP 96\na=rtpmap:96 H264/90000\na=fmtp:96 sprop-parameter-sets=not base64!,aOvjyyLA");

    RtspStreamCapture.ParameterSetsOf(session.Video!).Should().ContainSingle();
  }

  [Theory]
  [InlineData("rtsp://host/stream/", "trackID=1", "rtsp://host/stream/trackID=1")]
  [InlineData("rtsp://host/stream", "trackID=1", "rtsp://host/stream/trackID=1")]
  [InlineData("rtsp://host/stream", "rtsp://other/track", "rtsp://other/track")]
  [InlineData("rtsp://host/stream", "*", "rtsp://host/stream")]
  [InlineData("rtsp://host/stream", null, "rtsp://host/stream")]
  public void ResolveControlUri_HandlesEveryShapeAControlAttributeTakes(string baseUri, string? control, string expected)
  {
    RtspStreamCapture.ResolveControlUri(baseUri, control).Should().Be(expected);
  }

  [Fact]
  public void Parse_OfSomethingThatIsNotASessionYieldsNoMedia()
  {
    SessionDescription.Parse("<html>not a session</html>").Media.Should().BeEmpty();
    SessionDescription.Parse(string.Empty).Media.Should().BeEmpty();
  }

  [Fact]
  public void Parse_SkipsALineItCannotMakeSenseOfRatherThanFailing()
  {
    var session = SessionDescription.Parse(
      "v=0\ngarbage without an equals sign\nm=video 0 RTP/AVP 96\na=rtpmap:96 H264/90000");

    session.Video!.PrimaryRtpMap!.Encoding.Should().Be("H264");
  }

  [Fact]
  public void Parse_KeepsEveryPayloadTypeTheSectionOffers()
  {
    var session = SessionDescription.Parse("m=video 0 RTP/AVP 96 97 98\na=rtpmap:97 H264/90000");

    session.Video!.PayloadTypes.Should().ContainInOrder(96, 97, 98);
    session.Video.PrimaryPayloadType.Should().Be(96, "the first offered type is the preferred one");
    session.Video.PrimaryRtpMap.Should().BeNull("the preferred type carries no mapping here");
  }
}
