#region Copyright (C) 2017-2020 Yaroslav Tatarenko

// Copyright (C) 2017-2020 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2020 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using FluentAssertions;
using MediaInfo.Model;
using Xunit;
using Xunit.Abstractions;

namespace MediaInfo.Wrapper.Tests
{
  /// <summary>A video tests.</summary>
  public class VideoTests
  {
    private MediaInfoWrapper _mediaInfoWrapper;
    private readonly ILogger _logger;

    public VideoTests(ITestOutputHelper testOutputHelper)
    {
      _logger = new TestLogger(testOutputHelper);
    }

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("rtsp://wowzaec2demo.streamlock.net/vod/mp4:BigBuckBunny_115k.mov")]
    public void LoadSteamVideo(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.Size.Should().Be(1371743L);
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.VideoRate.Should().Be(310275);
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is progressive");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.Tags.EncodedDate.Should().NotBeNull();
      _mediaInfoWrapper.Tags.TaggedDate.Should().NotBeNull();
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(1);
      _mediaInfoWrapper.AudioStreams[0].Tags.Should().NotBeNull();
      _mediaInfoWrapper.AudioStreams[0].Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.AudioStreams[0].Tags.GeneralTags.Should().NotBeEmpty();
      var videoStream = _mediaInfoWrapper.VideoStreams[0];
      videoStream.Hdr.Should().Be(Hdr.None);
      videoStream.Codec.Should().Be(VideoCodec.H263);
      videoStream.Standard.Should().Be(VideoStandard.NTSC);
      videoStream.SubSampling.Should().Be(ChromaSubSampling.Sampling420);
      videoStream.Tags.GeneralTags.Should().NotBeNull();
      videoStream.Tags.GeneralTags.Should().NotBeEmpty();
      videoStream.Tags.EncodedLibrary.Should().NotBeNullOrEmpty();
    }

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("http://localhost/video/test_8.mp4", 2296357L, 117206, 1, VideoCodec.Mpeg4IsoAvc, ChromaSubSampling.Sampling420, 845642L)]
    [InlineData("http://localhost/video/Sisvel3DTile.ts", 50320644, 0, 1, VideoCodec.Mpeg4IsoAvc, ChromaSubSampling.Sampling420, 0L)]
    [InlineData("http://localhost/video/iphone6s_4k.mov", 118742364L, 51105477, 1, VideoCodec.Mpeg4IsoAvc, ChromaSubSampling.Sampling420, 118512869L)]
    public void LoadAVSteam(string fileName, long size, int rate, int audioStreams, VideoCodec codec, ChromaSubSampling sampling, long streamSize)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.Size.Should().Be(size);
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.VideoRate.Should().Be(rate);
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is progressive");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(audioStreams);
      var videoStream = _mediaInfoWrapper.VideoStreams[0];
      videoStream.Hdr.Should().Be(Hdr.None);
      videoStream.Codec.Should().Be(codec);
      videoStream.Standard.Should().Be(VideoStandard.NTSC);
      videoStream.SubSampling.Should().Be(sampling);
      videoStream.StreamSize.Should().Be(streamSize);
    }

    [Theory]
    [InlineData("./Data/RTL_7_Darts_WK_2014-2013-12-23_1_h263.3gp")]
    public void LoadSimpleVideo(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.Size.Should().Be(1371743L);
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.VideoRate.Should().Be(310275);
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is progressive");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
      _mediaInfoWrapper.Tags.EncodedDate.Should().NotBeNull();
      _mediaInfoWrapper.Tags.TaggedDate.Should().NotBeNull();
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(1);
      _mediaInfoWrapper.AudioStreams[0].Tags.Should().NotBeNull();
      _mediaInfoWrapper.AudioStreams[0].Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.AudioStreams[0].Tags.GeneralTags.Should().NotBeEmpty();
      var videoStream = _mediaInfoWrapper.VideoStreams[0];
      videoStream.Hdr.Should().Be(Hdr.None);
      videoStream.Codec.Should().Be(VideoCodec.H263);
      videoStream.Standard.Should().Be(VideoStandard.NTSC);
      videoStream.SubSampling.Should().Be(ChromaSubSampling.Sampling420);
      videoStream.Tags.GeneralTags.Should().NotBeNull();
      videoStream.Tags.GeneralTags.Should().NotBeEmpty();
      videoStream.Tags.EncodedLibrary.Should().NotBeNullOrEmpty();
      videoStream.StreamSize.Should().Be(1305431L);
    }

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("../../../../HD/[Kametsu] Princess Principal Picture Drama - 01 (BD 1080p Hi10 FLAC) [BBF1B4CE].mkv")]
    public void LoadVideoWithAttachments(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.Size.Should().Be(20105030);
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.VideoRate.Should().Be(4180953);
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is progressive");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.Tags.EncodedDate.Should().BeNull();
      _mediaInfoWrapper.Tags.TaggedDate.Should().BeNull();
      _mediaInfoWrapper.IsStreamable.Should().BeTrue();
      _mediaInfoWrapper.WritingApplication.Should().Be("Lavf58.77.100");
      _mediaInfoWrapper.WritingLibrary.Should().Be("Lavf58.77.100");
      _mediaInfoWrapper.Attachments.Should().Be("Amaranth-Italic.otf / Amaranth-Regular.otf");
      _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(2);
      _mediaInfoWrapper.AudioStreams[0].Tags.Should().NotBeNull();
      _mediaInfoWrapper.AudioStreams[0].Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.AudioStreams[0].Tags.GeneralTags.Should().NotBeEmpty();
      var videoStream = _mediaInfoWrapper.VideoStreams[0];
      videoStream.Hdr.Should().Be(Hdr.None);
      videoStream.Codec.Should().Be(VideoCodec.Mpeg4IsoAvc);
      videoStream.Standard.Should().Be(VideoStandard.NTSC);
      videoStream.SubSampling.Should().Be(ChromaSubSampling.Sampling420);
      videoStream.Tags.GeneralTags.Should().NotBeNull();
      videoStream.Tags.GeneralTags.Should().NotBeEmpty();
      videoStream.Tags.EncodedLibrary.Should().NotBeNullOrEmpty();
      videoStream.StreamSize.Should().Be(116137489L);
    }

    [Theory]
    [InlineData("./Data/Test_H264_Atmos.m2ts")]
    public void LoadVideoWithDolbyAtmos(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.Size.Should().Be(503808L);
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.VideoRate.Should().Be(24000000);
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is progressive");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(2);
      var atmosStream = _mediaInfoWrapper.AudioStreams[0];
      atmosStream.Codec.Should().Be(AudioCodec.TruehdAtmos, "First audio channel is Dolby TrueHD with Atmos");
      var dolbyAtmosStream = _mediaInfoWrapper.AudioStreams[1];
      dolbyAtmosStream.Codec.Should().Be(AudioCodec.Eac3Atmos, "First audio channel is Dolby Atmos");
      _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.Tags.GeneralTags.Should().BeEmpty();
      _mediaInfoWrapper.AudioStreams[0].Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.AudioStreams[0].Tags.GeneralTags.Should().BeEmpty();
      _mediaInfoWrapper.AudioStreams[1].Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.AudioStreams[1].Tags.GeneralTags.Should().BeEmpty();
      var videoStream = _mediaInfoWrapper.VideoStreams[0];
      videoStream.Hdr.Should().Be(Hdr.None);
      videoStream.Codec.Should().Be(VideoCodec.Mpeg4IsoAvc);
      videoStream.Standard.Should().Be(VideoStandard.NTSC);
      videoStream.ColorSpace.Should().Be(ColorSpace.BT709);
      videoStream.SubSampling.Should().Be(ChromaSubSampling.Sampling420);
      videoStream.Tags.GeneralTags.Should().NotBeNull();
      videoStream.Tags.GeneralTags.Should().BeEmpty();
      videoStream.StreamSize.Should().Be(0L);
    }

    [Theory]
    [InlineData("./Data/Test_H264_Ac3.m2ts")]
    public void LoadVideoWithDolbyDigital(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.Size.Should().Be(86016L);
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is progressive");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(1);
      _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
      var ac3 = _mediaInfoWrapper.AudioStreams[0];
      ac3.Codec.Should().Be(AudioCodec.Ac3);
      _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.Tags.GeneralTags.Should().BeEmpty();
      _mediaInfoWrapper.AudioStreams[0].Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.AudioStreams[0].Tags.GeneralTags.Should().BeEmpty();
      var videoStream = _mediaInfoWrapper.VideoStreams[0];
      videoStream.Hdr.Should().Be(Hdr.None);
      videoStream.Codec.Should().Be(VideoCodec.Mpeg4IsoAvc);
      videoStream.Standard.Should().Be(VideoStandard.NTSC);
      videoStream.SubSampling.Should().Be(ChromaSubSampling.Sampling420);
      videoStream.Tags.GeneralTags.Should().NotBeNull();
      videoStream.Tags.GeneralTags.Should().BeEmpty();
      videoStream.StreamSize.Should().Be(0L);
    }

    [Theory]
    [InlineData("./Data/Test_H264.m2ts")]
    public void LoadVideoWithoutAudio(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.Size.Should().Be(18432L);
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.VideoRate.Should().Be(5000000);
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is progressive");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.AudioStreams.Should().BeEmpty();
      _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.Tags.GeneralTags.Should().BeEmpty();
      _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
      var videoStream = _mediaInfoWrapper.VideoStreams[0];
      videoStream.Hdr.Should().Be(Hdr.None);
      videoStream.Codec.Should().Be(VideoCodec.Mpeg4IsoAvc);
      videoStream.Standard.Should().Be(VideoStandard.NTSC);
      videoStream.ColorSpace.Should().Be(ColorSpace.BT709);
      videoStream.SubSampling.Should().Be(ChromaSubSampling.Sampling420);
      videoStream.Tags.GeneralTags.Should().NotBeNull();
      videoStream.Tags.GeneralTags.Should().BeEmpty();
      videoStream.StreamSize.Should().Be(625625L);
    }

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("//192.168.50.31/Video_O/2016 DOLBY ATMOS DEMO DISC/BDMV/index.bdmv")]
    public void LoadBluRayWithMenuAndDolbyAtmos(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.Size.Should().Be(24716230397L);
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.VideoRate.Should().Be(32173617);
      _mediaInfoWrapper.IsBluRay.Should().BeTrue("Is BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is progressive");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(22);
      _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
      var atmos = _mediaInfoWrapper.AudioStreams[0];
      atmos.Codec.Should().Be(AudioCodec.TruehdAtmos);
      atmos.Channel.Should().Be(8);
      var video = _mediaInfoWrapper.VideoStreams[0];
      video.Hdr.Should().Be(Hdr.None);
      video.Codec.Should().Be(VideoCodec.Mpeg4IsoAvc);
      video.ColorSpace.Should().Be(ColorSpace.BT709);
      video.SubSampling.Should().Be(ChromaSubSampling.Sampling420);
      video.StreamSize.Should().Be(423954426L);
      _mediaInfoWrapper.MenuStreams.Count.Should().Be(1);
      _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.Tags.GeneralTags.Should().BeEmpty();
    }

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("../../../../HDR/PE2_Leopard_4K.mkv", VideoCodec.MpeghIsoHevc, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling420, AudioCodec.DtsHdMa, 6, 23358403L)]
    [InlineData("../../../../HDR/LaLaLand_cafe_4K.mkv", VideoCodec.MpeghIsoHevc, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling420, AudioCodec.TruehdAtmos, 8, 427862660L)]
    [InlineData("../../../../HDR/The Redwoods.mkv", VideoCodec.Vp9, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling332, AudioCodec.Vorbis, 2, 352373663L)]
    [InlineData("../../../../HDR/The World in HDR.mkv", VideoCodec.Vp9, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling332, AudioCodec.Vorbis, 2, 351675124L)]
    [InlineData("../../../../HDR/LG Demo DolbyVision Comparison.mkv", VideoCodec.MpeghIsoHevc, Hdr.DolbyVision, ColorSpace.Generic, ChromaSubSampling.Sampling420, AudioCodec.Eac3, 6, 281969991L)]
    [InlineData("../../../../HDR/LG Demo DolbyVision Trailer.mkv", VideoCodec.MpeghIsoHevc, Hdr.DolbyVision, ColorSpace.Generic, ChromaSubSampling.Sampling420, AudioCodec.Eac3, 6, 295907864L)]
    [InlineData("../../../../HDR/LG Amaze Dolby Vision UHD 4K Demo.ts", VideoCodec.MpeghIsoHevc, Hdr.DolbyVision, ColorSpace.Generic, ChromaSubSampling.Sampling420, AudioCodec.Eac3Atmos, 6, 194690298L)]
    [InlineData("../../../../HDR/LG Daylight 4K Demo.ts", VideoCodec.MpeghIsoHevc, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling420, AudioCodec.AacMpeg4Lc, 2, 0L)]
    [InlineData("../../../../HDR/LG Earth Dolby Vision UHD 4K Demo.ts", VideoCodec.MpeghIsoHevc, Hdr.DolbyVision, ColorSpace.Generic, ChromaSubSampling.Sampling420, AudioCodec.Eac3, 2, 236589530L)]
    [InlineData("../../../../HDR/LG New York HDR UHD 4K Demo.ts", VideoCodec.MpeghIsoHevc, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling420, AudioCodec.AacMpeg4Lc, 2, 0L)]
    [InlineData("../../../../HDR/Life Untouched 4K Demo.mp4", VideoCodec.MpeghIsoHevc, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling420, AudioCodec.AacMpeg4Lc, 2, 467882368L)]
    [InlineData("../../../../HDR/Sony Camp 4K Demo.mp4", VideoCodec.MpeghIsoHevc, Hdr.HDR10, ColorSpace.BT709, ChromaSubSampling.Sampling420, AudioCodec.AacMpeg4Lc, 2, 1254444952L)]
    [InlineData("../../../../HDR/Sony Whale in Tonga HDR UHD 4K Demo.mp4", VideoCodec.MpeghIsoHevc, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling420, AudioCodec.AacMpeg4Lc, 2, 852516785L)]
    [InlineData("../../../../HDR/TravelXP 4K HDR HLG Demo.ts", VideoCodec.MpeghIsoHevc, Hdr.HLG, ColorSpace.BT2020, ChromaSubSampling.Sampling420, AudioCodec.AacMpeg4Lc, 1, 0L)]
    public void LoadHdrDemo(
        string fileName,
        VideoCodec videoCodec,
        Hdr hdr,
        ColorSpace colorSpace,
        ChromaSubSampling subSampling,
        AudioCodec audioCodec,
        int channels,
        long streamSize)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is progressive");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(1);
      _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
      var audio = _mediaInfoWrapper.AudioStreams[0];
      audio.Codec.Should().Be(audioCodec);
      audio.Channel.Should().Be(channels);
      var video = _mediaInfoWrapper.VideoStreams[0];
      video.Hdr.Should().Be(hdr);
      video.Codec.Should().Be(videoCodec);
      video.ColorSpace.Should().Be(colorSpace);
      video.SubSampling.Should().Be(subSampling);
      video.StreamSize.Should().Be(streamSize);
    }

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("../../../../UHD/(HEVC 10-bit 25fps) Astra DVB Sample.ts", VideoCodec.MpeghIsoHevc, 2160, ColorSpace.Generic, ChromaSubSampling.Sampling420, 1070986515L)]
    [InlineData("../../../../UHD/4K HEVC 59.940 Broadcast Capture Sample.mkv", VideoCodec.MpeghIsoHevc, 2160, ColorSpace.Generic, ChromaSubSampling.Sampling420, 187427353L)]
    [InlineData("../../../../UHD/4K youtube.webm", VideoCodec.Vp9, 2160, ColorSpace.BT709, ChromaSubSampling.Sampling332, 503064736L)]
    [InlineData("../../../../UHD/8K youtube.mp4", VideoCodec.Av1, 4320, ColorSpace.BT709, ChromaSubSampling.Sampling420, 497122531L)]
    [InlineData("../../../../UHD/iphone6s_4k.mov", VideoCodec.Mpeg4IsoAvc, 2160, ColorSpace.BT709, ChromaSubSampling.Sampling420, 118512869L)]
    [InlineData("../../../../UHD/lg-uhd-spain-and-patagonia-(www.demolandia.net).mkv", VideoCodec.Mpeg4IsoAvc, 2160, ColorSpace.BT709, ChromaSubSampling.Sampling420, 379530039L)]
    [InlineData("../../../../UHD/samsung-uhd-dubai-(www.demolandia.net).mkv", VideoCodec.Mpeg4IsoAvc, 2160, ColorSpace.BT709, ChromaSubSampling.Sampling420, 570813864L)]
    [InlineData("../../../../UHD/samsung_seven_wonders_of_the_world_china_uhd-DWEU.mkv", VideoCodec.Mpeg4IsoAvc, 2160, ColorSpace.BT709, ChromaSubSampling.Sampling420, 771372882L)]
    [InlineData("../../../../UHD/4k_Rec709_ProResHQ.mov", VideoCodec.ProRes, 3072, ColorSpace.Generic, ChromaSubSampling.Sampling422, 16300923221L)]
    public void LoadUhdDemo(string fileName, VideoCodec videoCodec, int height, ColorSpace colorSpace, ChromaSubSampling subSampling, long streamSize)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is progressive");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
      var video = _mediaInfoWrapper.VideoStreams[0];
      video.Height.Should().Be(height);
      video.Codec.Should().Be(videoCodec);
      video.ColorSpace.Should().Be(colorSpace);
      video.SubSampling.Should().Be(subSampling);
      video.StreamSize.Should().Be(streamSize);
    }

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("../../../../3D/3D-full-MVC.mkv", VideoCodec.Mpeg4IsoAvc, Hdr.None, ColorSpace.Generic, StereoMode.Stereo, ChromaSubSampling.Sampling420, 252343376L)]
    [InlineData("../../../../3D/Guards 3D Half-OU.mk3d", VideoCodec.Mpeg4IsoAvc, Hdr.None, ColorSpace.Generic, StereoMode.TopBottomRight, ChromaSubSampling.Sampling420, 26987921L)]
    [InlineData("../../../../3D/BD3D/BDMV/index.bdmv", VideoCodec.Mpeg4IsoAvc, Hdr.None, ColorSpace.Generic, StereoMode.Stereo, ChromaSubSampling.Sampling420, 156852466L)]
    [InlineData("../../../../3D/Dracula 480p.wmv", VideoCodec.Vc1, Hdr.None, ColorSpace.Generic, StereoMode.SideBySideRight, ChromaSubSampling.Sampling420, 39549750L)]
    [InlineData("../../../../3D/small-00000.ssif", VideoCodec.Mpeg4IsoAvc, Hdr.None, ColorSpace.Generic, StereoMode.Stereo, ChromaSubSampling.Sampling420, 1963071L)]
    public void Load3dDemo(string fileName, VideoCodec videoCodec, Hdr hdr, ColorSpace colorSpace, StereoMode stereoMode, ChromaSubSampling subSampling, long streamSize)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.Is3D.Should().BeTrue("Video stream is 3D");
      var video = _mediaInfoWrapper.VideoStreams[0];
      video.Hdr.Should().Be(hdr);
      video.Codec.Should().Be(videoCodec);
      video.Stereoscopic.Should().Be(stereoMode);
      video.ColorSpace.Should().Be(colorSpace);
      video.SubSampling.Should().Be(subSampling);
      video.StreamSize.Should().Be(streamSize);
    }

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("../../../../HD/[Underwater] Another - sample H264 Hi10P 720p.avi", VideoCodec.Mpeg4IsoAvc, 720, ColorSpace.BT709, ChromaSubSampling.Sampling420, 16215354L)]
    [InlineData("../../../../HD/[Underwater] Another - sample H264 Hi10P 1080p.avi", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling420, 66332302L)]
    [InlineData("../../../../HD/1080i-25-H264.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420, 105016794L)]
    [InlineData("../../../../HD/Bieber Grammys.ts", VideoCodec.Mpeg2, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling422, 0L)]
    [InlineData("../../../../HD/Clannad After Story OPA - sample HEVC main10 1080p.mkv", VideoCodec.MpeghIsoHevc, 1088, ColorSpace.Generic, ChromaSubSampling.Sampling420, 24904367L)]
    [InlineData("../../../../HD/DSCF1912_parrot.AVI", VideoCodec.Mjpg, 480, ColorSpace.Generic, ChromaSubSampling.Sampling422, 38634896L)]
    [InlineData("../../../../HD/DSCF1928_fish.AVI", VideoCodec.Mjpg, 480, ColorSpace.Generic, ChromaSubSampling.Sampling422, 24045488L)]
    [InlineData("../../../../HD/DSCF1929_fish.AVI", VideoCodec.Mjpg, 480, ColorSpace.Generic, ChromaSubSampling.Sampling422, 7300512L)]
    [InlineData("../../../../HD/FPS_test_1080p23.976_L4.1.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420, 16105645L)]
    [InlineData("../../../../HD/FPS_test_1080p24_L4.1.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420, 16193499L)]
    [InlineData("../../../../HD/FPS_test_1080p25_L4.1.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420, 16391549L)]
    [InlineData("../../../../HD/FPS_test_1080p50_L4.2.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420, 23693962L)]
    [InlineData("../../../../HD/FPS_test_1080p59.94_L4.2.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420, 26734961L)]
    [InlineData("../../../../HD/FPS_test_1080p60_L4.2.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420, 26951049L)]
    [InlineData("../../../../HD/Grace Potter 29.97 Mpeg-2 1080i 35mbps DTS-HD MA 5.1 Sample.ts", VideoCodec.Mpeg2, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling420, 0L)]
    [InlineData("../../../../HD/H.265 HVEC Test 1.mkv", VideoCodec.MpeghIsoHevc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420, 223811006L)]
    [InlineData("../../../../HD/H.265 HVEC Test 2.mkv", VideoCodec.MpeghIsoHevc, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling420, 254607172L)]
    [InlineData("../../../../HD/Human Flight 3D - Andy carving_(FullHD).avi", VideoCodec.Mpeg4IsoAvc, 540, ColorSpace.Generic, ChromaSubSampling.Sampling420, 22228619L)]
    [InlineData("../../../../HD/Imagine Dragons 59.94 720p 20mbps Mpeg-2 MPA2.0 Sample.ts", VideoCodec.Mpeg2, 720, ColorSpace.BT709, ChromaSubSampling.Sampling420, 488454711L)]
    [InlineData("../../../../HD/issue1930.h264", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420, 0L)]
    [InlineData("../../../../HD/MPEG2_1080i_sample.mkv", VideoCodec.Mpeg2, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling420, 131848306L)]
    [InlineData("../../../../HD/PE2_Leopard_1080.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling420, 16231007L)]
    [InlineData("../../../../HD/San%20Francisco%20Time%20Lapse%20(Empty%20America).mp4", VideoCodec.Mpeg4IsoAvc, 720, ColorSpace.Generic, ChromaSubSampling.Sampling420, 45680378L)]
    [InlineData("../../../../HD/Sisvel3DTile.ts", VideoCodec.Mpeg4IsoAvc, 720, ColorSpace.Generic, ChromaSubSampling.Sampling420, 0L)]
    [InlineData("../../../../HD/Surfcup.mp4", VideoCodec.H263, 408, ColorSpace.Generic, ChromaSubSampling.Sampling420, 277157713L)]
    [InlineData("../../../../HD/test 8.mp4", VideoCodec.Mpeg4IsoAvc, 288, ColorSpace.Generic, ChromaSubSampling.Sampling420, 845642L)]
    [InlineData("../../../../HD/VC-1_23.976_sample.mkv", VideoCodec.Vc1, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling420, 100182531L)]
    [InlineData("../../../../HD/VC-1_29.970_sample.mkv", VideoCodec.Vc1, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling420, 105596312L)]
    public void LoadHdDemo(string fileName, VideoCodec videoCodec, int height, ColorSpace colorSpace, ChromaSubSampling chromaSubSampling, long streamSize)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
      var video = _mediaInfoWrapper.VideoStreams[0];
      video.Height.Should().Be(height);
      video.Codec.Should().Be(videoCodec);
      video.ColorSpace.Should().Be(colorSpace);
      video.SubSampling.Should().Be(chromaSubSampling);
      video.StreamSize.Should().Be(streamSize);
    }
  }
}
