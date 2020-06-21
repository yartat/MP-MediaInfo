#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

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
      var videoStream = _mediaInfoWrapper.VideoStreams[0];
      videoStream.Hdr.Should().Be(Hdr.None);
      videoStream.Codec.Should().Be(VideoCodec.Mpeg4IsoAvc);
      videoStream.Standard.Should().Be(VideoStandard.NTSC);
      videoStream.ColorSpace.Should().Be(ColorSpace.BT709);
      videoStream.SubSampling.Should().Be(ChromaSubSampling.Sampling420);
      videoStream.Tags.GeneralTags.Should().NotBeNull();
      videoStream.Tags.GeneralTags.Should().BeEmpty();
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
      var atmos = _mediaInfoWrapper.AudioStreams[0];
      atmos.Codec.Should().Be(AudioCodec.TruehdAtmos);
      atmos.Channel.Should().Be(8);
      var video = _mediaInfoWrapper.VideoStreams[0];
      video.Hdr.Should().Be(Hdr.None);
      video.Codec.Should().Be(VideoCodec.Mpeg4IsoAvc);
      video.ColorSpace.Should().Be(ColorSpace.BT709);
      video.SubSampling.Should().Be(ChromaSubSampling.Sampling420);
      _mediaInfoWrapper.MenuStreams.Count.Should().Be(1);
      _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.Tags.GeneralTags.Should().BeEmpty();
    }

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("../../../../HDR/PE2_Leopard_4K.mkv", VideoCodec.MpeghIsoHevc, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling420, AudioCodec.DtsHdMa, 6)]
    [InlineData("../../../../HDR/LaLaLand_cafe_4K.mkv", VideoCodec.MpeghIsoHevc, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling420, AudioCodec.TruehdAtmos, 8)]
    [InlineData("../../../../HDR/The Redwoods.mkv", VideoCodec.Vp9, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling332, AudioCodec.Vorbis, 2)]
    [InlineData("../../../../HDR/The World in HDR.mkv", VideoCodec.Vp9, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling332, AudioCodec.Vorbis, 2)]
    [InlineData("../../../../HDR/LG Demo DolbyVision Comparison.mkv", VideoCodec.MpeghIsoHevc, Hdr.DolbyVision, ColorSpace.Generic, ChromaSubSampling.Sampling420, AudioCodec.Eac3, 6)]
    [InlineData("../../../../HDR/LG Demo DolbyVision Trailer.mkv", VideoCodec.MpeghIsoHevc, Hdr.DolbyVision, ColorSpace.Generic, ChromaSubSampling.Sampling420, AudioCodec.Eac3, 6)]
    [InlineData("../../../../HDR/LG Amaze Dolby Vision UHD 4K Demo.ts", VideoCodec.MpeghIsoHevc, Hdr.DolbyVision, ColorSpace.Generic, ChromaSubSampling.Sampling420, AudioCodec.Eac3Atmos, 6)]
    [InlineData("../../../../HDR/LG Daylight 4K Demo.ts", VideoCodec.MpeghIsoHevc, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling420, AudioCodec.AacMpeg4Lc, 2)]
    [InlineData("../../../../HDR/LG Earth Dolby Vision UHD 4K Demo.ts", VideoCodec.MpeghIsoHevc, Hdr.DolbyVision, ColorSpace.Generic, ChromaSubSampling.Sampling420, AudioCodec.Eac3, 2)]
    [InlineData("../../../../HDR/LG New York HDR UHD 4K Demo.ts", VideoCodec.MpeghIsoHevc, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling420, AudioCodec.AacMpeg4Lc, 2)]
    [InlineData("../../../../HDR/Life Untouched 4K Demo.mp4", VideoCodec.MpeghIsoHevc, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling420, AudioCodec.AacMpeg4Lc, 2)]
    [InlineData("../../../../HDR/Sony Camp 4K Demo.mp4", VideoCodec.MpeghIsoHevc, Hdr.HDR10, ColorSpace.BT709, ChromaSubSampling.Sampling420, AudioCodec.AacMpeg4Lc, 2)]
    [InlineData("../../../../HDR/Sony Whale in Tonga HDR UHD 4K Demo.mp4", VideoCodec.MpeghIsoHevc, Hdr.HDR10, ColorSpace.BT2020, ChromaSubSampling.Sampling420, AudioCodec.AacMpeg4Lc, 2)]
    [InlineData("../../../../HDR/TravelXP 4K HDR HLG Demo.ts", VideoCodec.MpeghIsoHevc, Hdr.HLG, ColorSpace.BT2020, ChromaSubSampling.Sampling420, AudioCodec.AacMpeg4Lc, 1)]
    public void LoadHdrDemo(string fileName, VideoCodec videoCodec, Hdr hdr, ColorSpace colorSpace, ChromaSubSampling subSampling, AudioCodec audioCodec, int channels)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is progressive");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(1);
      var audio = _mediaInfoWrapper.AudioStreams[0];
      audio.Codec.Should().Be(audioCodec);
      audio.Channel.Should().Be(channels);
      var video = _mediaInfoWrapper.VideoStreams[0];
      video.Hdr.Should().Be(hdr);
      video.Codec.Should().Be(videoCodec);
      video.ColorSpace.Should().Be(colorSpace);
      video.SubSampling.Should().Be(subSampling);
    }

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("../../../../UHD/(HEVC 10-bit 25fps) Astra DVB Sample.ts", VideoCodec.MpeghIsoHevc, 2160, ColorSpace.Generic, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../UHD/4K HEVC 59.940 Broadcast Capture Sample.mkv", VideoCodec.MpeghIsoHevc, 2160, ColorSpace.Generic, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../UHD/4K youtube.webm", VideoCodec.Vp9, 2160, ColorSpace.BT709, ChromaSubSampling.Sampling332)]
    [InlineData("../../../../UHD/8K youtube.mp4", VideoCodec.Av1, 4320, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../UHD/iphone6s_4k.mov", VideoCodec.Mpeg4IsoAvc, 2160, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../UHD/lg-uhd-spain-and-patagonia-(www.demolandia.net).mkv", VideoCodec.Mpeg4IsoAvc, 2160, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../UHD/samsung-uhd-dubai-(www.demolandia.net).mkv", VideoCodec.Mpeg4IsoAvc, 2160, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../UHD/samsung_seven_wonders_of_the_world_china_uhd-DWEU.mkv", VideoCodec.Mpeg4IsoAvc, 2160, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../UHD/4k_Rec709_ProResHQ.mov", VideoCodec.ProRes, 3072, ColorSpace.Generic, ChromaSubSampling.Sampling422)]
    public void LoadUhdDemo(string fileName, VideoCodec videoCodec, int height, ColorSpace colorSpace, ChromaSubSampling subSampling)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is progressive");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      var video = _mediaInfoWrapper.VideoStreams[0];
      video.Height.Should().Be(height);
      video.Codec.Should().Be(videoCodec);
      video.ColorSpace.Should().Be(colorSpace);
      video.SubSampling.Should().Be(subSampling);
    }

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("../../../../3D/3D-full-MVC.mkv", VideoCodec.Mpeg4IsoAvc, Hdr.None, ColorSpace.Generic, StereoMode.Stereo, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../3D/Guards 3D Half-OU.mk3d", VideoCodec.Mpeg4IsoAvc, Hdr.None, ColorSpace.Generic, StereoMode.TopBottomRight, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../3D/BD3D/BDMV/index.bdmv", VideoCodec.Mpeg4IsoAvc, Hdr.None, ColorSpace.Generic, StereoMode.Stereo, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../3D/Dracula 480p.wmv", VideoCodec.Vc1, Hdr.None, ColorSpace.Generic, StereoMode.SideBySideRight, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../3D/small-00000.ssif", VideoCodec.Mpeg4IsoAvc, Hdr.None, ColorSpace.Generic, StereoMode.Stereo, ChromaSubSampling.Sampling420)]
    public void Load3dDemo(string fileName, VideoCodec videoCodec, Hdr hdr, ColorSpace colorSpace, StereoMode stereoMode, ChromaSubSampling subSampling)
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
    }

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("../../../../HD/[Underwater] Another - sample H264 Hi10P 720p.avi", VideoCodec.Mpeg4IsoAvc, 720, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/[Underwater] Another - sample H264 Hi10P 1080p.avi", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/1080i-25-H264.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/Bieber Grammys.ts", VideoCodec.Mpeg2, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling422)]
    [InlineData("../../../../HD/Clannad After Story OPA - sample HEVC main10 1080p.mkv", VideoCodec.MpeghIsoHevc, 1088, ColorSpace.Generic, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/DSCF1912_parrot.AVI", VideoCodec.Mjpg, 480, ColorSpace.Generic, ChromaSubSampling.Sampling422)]
    [InlineData("../../../../HD/DSCF1928_fish.AVI", VideoCodec.Mjpg, 480, ColorSpace.Generic, ChromaSubSampling.Sampling422)]
    [InlineData("../../../../HD/DSCF1929_fish.AVI", VideoCodec.Mjpg, 480, ColorSpace.Generic, ChromaSubSampling.Sampling422)]
    [InlineData("../../../../HD/FPS_test_1080p23.976_L4.1.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/FPS_test_1080p24_L4.1.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/FPS_test_1080p25_L4.1.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/FPS_test_1080p50_L4.2.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/FPS_test_1080p59.94_L4.2.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/FPS_test_1080p60_L4.2.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/Grace Potter 29.97 Mpeg-2 1080i 35mbps DTS-HD MA 5.1 Sample.ts", VideoCodec.Mpeg2, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/H.265 HVEC Test 1.mkv", VideoCodec.MpeghIsoHevc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/H.265 HVEC Test 2.mkv", VideoCodec.MpeghIsoHevc, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling332)]
    [InlineData("../../../../HD/Human Flight 3D - Andy carving_(FullHD).avi", VideoCodec.Mpeg4IsoAvc, 540, ColorSpace.Generic, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/Imagine Dragons 59.94 720p 20mbps Mpeg-2 MPA2.0 Sample.ts", VideoCodec.Mpeg2, 720, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/issue1930.h264", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.BT709, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/MPEG2_1080i_sample.mkv", VideoCodec.Mpeg2, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/PE2_Leopard_1080.mkv", VideoCodec.Mpeg4IsoAvc, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/San%20Francisco%20Time%20Lapse%20(Empty%20America).mp4", VideoCodec.Mpeg4IsoAvc, 720, ColorSpace.Generic, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/Sisvel3DTile.ts", VideoCodec.Mpeg4IsoAvc, 720, ColorSpace.Generic, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/Surfcup.mp4", VideoCodec.H263, 408, ColorSpace.Generic, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/test 8.mp4", VideoCodec.Mpeg4IsoAvc, 288, ColorSpace.Generic, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/VC-1_23.976_sample.mkv", VideoCodec.Vc1, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling420)]
    [InlineData("../../../../HD/VC-1_29.970_sample.mkv", VideoCodec.Vc1, 1080, ColorSpace.Generic, ChromaSubSampling.Sampling420)]
    public void LoadHdDemo(string fileName, VideoCodec videoCodec, int height, ColorSpace colorSpace, ChromaSubSampling chromaSubSampling)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      var video = _mediaInfoWrapper.VideoStreams[0];
      video.Height.Should().Be(height);
      video.Codec.Should().Be(videoCodec);
      video.ColorSpace.Should().Be(colorSpace);
      video.SubSampling.Should().Be(chromaSubSampling);
    }
  }
}
