#region Copyright (C) 2005-2019 Team MediaPortal

// Copyright (C) 2005-2017 Team MediaPortal
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

using System.Linq;
using FluentAssertions;
using MediaInfo.Model;
using Xunit;

namespace MediaInfo.Wrapper.Tests
{
  public class AudioTests
  {
    private MediaInfoWrapper _mediaInfoWrapper;

    [Theory]
    [InlineData(@".\Data\Test_H264_DTS1.m2ts", 9, 1)]
    [InlineData(@".\Data\Test_H264_DTS2.m2ts", 6, 0)]
    public void LoadVideoWithDtsMa(string fileName, int audioStreamCount, int dtsIndex)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is progressive");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(audioStreamCount);
      var dts = _mediaInfoWrapper.AudioStreams[dtsIndex];
      dts.Codec.Should().Be(AudioCodec.DtsHdMa);
      _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.Tags.GeneralTags.Should().BeEmpty();
      _mediaInfoWrapper.VideoStreams[0].Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.VideoStreams[0].Tags.GeneralTags.Should().BeEmpty();
    }

    [TheoryInDebugOnly]
    [InlineData(@"..\..\..\..\HD Audio\2L-125_04_stereo.mqa.flac", 2, 24, 44100.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\2L-125_mch-96k-24b_04.flac", 6, 24, 96000.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\2L-125_stereo-44k-16b_04.flac", 2, 16, 44100.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\2L-125_stereo-88k-24b_04.flac", 2, 24, 88200.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\2L-125_stereo-176k-24b_04.flac", 2, 24, 176400.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\2L-125_stereo-352k-24b_04.flac", 2, 24, 352800.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\2L-125_mch-2822k-1b_04.dsf", 6, 1, 2822400.0, AudioCodec.Dsd)]
    [InlineData(@"..\..\..\..\HD Audio\2L-125_stereo-2822k-1b_04.dsf", 2, 1, 2822400.0, AudioCodec.Dsd)]
    [InlineData(@"..\..\..\..\HD Audio\2L-125_stereo-5644k-1b_04.dsf", 2, 1, 5644800.0, AudioCodec.Dsd)]
    [InlineData(@"..\..\..\..\HD Audio\2L-125_stereo-11289k-1b_04.dsf", 2, 1, 11289600.0, AudioCodec.Dsd)]
    [InlineData(@"..\..\..\..\HD Audio\2L-145_01_stereo.mqa.flac", 2, 24, 44100.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\2L-145_01_stereo.mqacd.mqa.flac", 2, 16, 44100.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\2L-145_mch_FLAC_96k_24b_01.flac", 6, 24, 96000.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\2L-45_stereo_01_DSF_11289k_1b.dsf", 2, 1, 11289600.0, AudioCodec.Dsd)]
    [InlineData(@"..\..\..\..\HD Audio\2L-45_stereo_01_FLAC_88k_24b.flac", 2, 24, 88200.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\2L-45_stereo_01_FLAC_176k_24b.flac", 2, 24, 176400.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\2L-45_stereo_01_FLAC_352k_24b.flac", 2, 24, 352800.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\03_NEUE_MUSIK_BERND_THEWESl_kleine_pingpark_wolke_DTS.wav", 6, 24, 44100.0, AudioCodec.Dts)]
    [InlineData(@"..\..\..\..\HD Audio\3-1.dts", 4, 24, 48000.0, AudioCodec.DtsHdMa)]
    [InlineData(@"..\..\..\..\HD Audio\5.1 24bit.dts", 6, 24, 48000.0, AudioCodec.Dts)]
    [InlineData(@"..\..\..\..\HD Audio\06_NEW_JAZZ_SSQ3_ENERGY_DTS.wav", 6, 24, 44100.0, AudioCodec.Dts)]
    [InlineData(@"..\..\..\..\HD Audio\7.1auditionOutLeader v2.wav", 8, 16, 48000.0, AudioCodec.PcmIntLit)]
    [InlineData(@"..\..\..\..\HD Audio\7_pt_1.eac3", 8, 0, 48000.0, AudioCodec.Eac3)]
    [InlineData(@"..\..\..\..\HD Audio\8_Channel_ID.m4a", 8, 0, 48000.0, AudioCodec.AacMpeg4Lc)]
    [InlineData(@"..\..\..\..\HD Audio\96-24.dts", 6, 24, 96000.0, AudioCodec.DtsHd)]
    [InlineData(@"..\..\..\..\HD Audio\Berlioz - Hungarian March.aac", 6, 0, 48000.0, AudioCodec.AacMpeg4LcSbr)]
    [InlineData(@"..\..\..\..\HD Audio\Berlioz - Hungarian March.wma", 6, 24, 96000.0, AudioCodec.Wma9)]
    [InlineData(@"..\..\..\..\HD Audio\Broadway-5.1-48khz-448kbit.ac3", 6, 16, 48000.0, AudioCodec.Ac3)]
    [InlineData(@"..\..\..\..\HD Audio\csi_miami_5.1_256_spx.eac3", 6, 0, 48000.0, AudioCodec.Eac3)]
    [InlineData(@"..\..\..\..\HD Audio\csi_miami_5.1_256_spx_nero.flac", 6, 24, 48000.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\ct_faac-adts.aac", 2, 0, 44100.0, AudioCodec.AacMpeg4Lc)]
    [InlineData(@"..\..\..\..\HD Audio\Cybele_SACD_030802_24b96k_8ch_Aurophonie_11.flac", 8, 24, 96000.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\Cybele_SACD_030802_24b96k_Surround_11.flac", 5, 24, 96000.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\Cybele_SACD_030802_DSD_Surround_11.dsf", 5, 1, 2822400.0, AudioCodec.Dsd)]
    [InlineData(@"..\..\..\..\HD Audio\Cybele_SACD_051501_16b44k_3D-Binaural-Stereo_03.flac", 2, 16, 44100.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\Cybele_SACD_051501_24b96k_3D-Binaural-Stereo_03.flac", 2, 24, 96000.0, AudioCodec.Flac)]
    [InlineData(@"..\..\..\..\HD Audio\dtswavsample14.wav", 6, 24, 44100.0, AudioCodec.Dts)]
    [InlineData(@"..\..\..\..\HD Audio\ES 6.1 - 5.1 16bit.dts", 7, 16, 48000.0, AudioCodec.DtsEs)]
    [InlineData(@"..\..\..\..\HD Audio\ES 6.1 24bit.dts", 7, 24, 48000.0, AudioCodec.DtsEs)]
    [InlineData(@"..\..\..\..\HD Audio\faad_infinite_long.aacp", 2, 0, 44100.0, AudioCodec.AacMpeg4LcSbrPs)]
    [InlineData(@"..\..\..\..\HD Audio\Hi-Res 6.1 24bit.dts", 7, 24, 48000.0, AudioCodec.DtsHdHra)]
    [InlineData(@"..\..\..\..\HD Audio\lotr_5.1_768.dts", 7, 24, 48000.0, AudioCodec.DtsEs)]
    [InlineData(@"..\..\..\..\HD Audio\Major 06 kwestia 03.mka", 1, 0, 48000.0, AudioCodec.AacMpeg4LcSbrPs)]
    [InlineData(@"..\..\..\..\HD Audio\Master Audio 2.0 16bit.dts", 2, 16, 48000.0, AudioCodec.DtsHdMa)]
    [InlineData(@"..\..\..\..\HD Audio\Master Audio 5.0 96khz.dts", 5, 24, 96000.0, AudioCodec.DtsHdMa)]
    [InlineData(@"..\..\..\..\HD Audio\Master Audio 5.1 24bit.dts", 6, 24, 48000.0, AudioCodec.DtsHdMa)]
    [InlineData(@"..\..\..\..\HD Audio\Master Audio 7.1.dts", 8, 24, 48000.0, AudioCodec.DtsHdMa)]
    [InlineData(@"..\..\..\..\HD Audio\Master Audio 7.1 24bit.dts", 8, 24, 48000.0, AudioCodec.DtsHdMa)]
    [InlineData(@"..\..\..\..\HD Audio\monsters_inc_5.1_448.ac3", 6, 16, 48000.0, AudioCodec.Ac3)]
    [InlineData(@"..\..\..\..\HD Audio\SBR_LFETest5_1-441-16b.wav", 6, 16, 44100.0, AudioCodec.PcmIntLit)]
    [InlineData(@"..\..\..\..\HD Audio\stuttering_dts_truhd.dts", 6, 24, 48000.0, AudioCodec.Dts)]
    [InlineData(@"..\..\..\..\HD Audio\thx_2_0.ac3", 2, 16, 48000.0, AudioCodec.Ac3)]
    [InlineData(@"..\..\..\..\HD Audio\working_dts_with_dtscore.dts", 6, 16, 48000.0, AudioCodec.Dts)]
    public void LoadHdAudioFile(string fileName, int channels, int bitDepth, double samplingRate, AudioCodec codec)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.HasVideo.Should().BeFalse("FLAC file");
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream does not exist");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream does not exist");
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(1);
      var audio = _mediaInfoWrapper.AudioStreams[0];
      audio.Codec.Should().Be(codec);
      audio.Channel.Should().Be(channels);
      audio.BitDepth.Should().Be(bitDepth);
      audio.SamplingRate.Should().Be(samplingRate);
    }

    [TheoryInDebugOnly]
    [InlineData(@"..\..\..\..\HD Audio\7.1auditionOutLeader_v2_rtb.mp4", 8, 0, 48000.0, AudioCodec.AacMpeg4LcSbr, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\7_pt_1_sample.evo", 8, 0, 48000.0, AudioCodec.Eac3, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\12-10_19-18-52_BBC HD_Wild China.ts", 6, 0, 48000.0, AudioCodec.AacMpeg4Lc, 0, 2)]
    [InlineData(@"..\..\..\..\HD Audio\ac3-sound-sample.vob", 6, 16, 48000.0, AudioCodec.Ac3, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\bond_sample_dtshdma.m2ts", 6, 24, 48000.0, AudioCodec.DtsHdMa, 0, 7)]
    [InlineData(@"..\..\..\..\HD Audio\ChID-BLITS-EBU.mp4", 6, 0, 44100.0, AudioCodec.AacMpeg4LcSbr, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\ChID-BLITS-EBU-Narration.mp4", 6, 0, 44100.0, AudioCodec.AacMpeg4LcSbr, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\danish-1.m2t", 2, 0, 48000.0, AudioCodec.AacMpeg4LcSbr, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\DNCE 29.97 h264 1080i 26mbps DTS-HD MA 2.0 sample.mkv", 2, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\dolby-audiosphere-lossless-(www.demolandia.net).m2ts", 8, 16, 48000.0, AudioCodec.TruehdAtmos, 0, 2)]
    [InlineData(@"..\..\..\..\HD Audio\dolby-silent-lossless-(www.demolandia.net).m2ts", 8, 16, 48000.0, AudioCodec.TruehdAtmos, 0, 2)]
    [InlineData(@"..\..\..\..\HD Audio\Dolby ATMOS Helicopter.m2ts", 8, 16, 48000.0, AudioCodec.TruehdAtmos, 0, 2)]
    [InlineData(@"..\..\..\..\HD Audio\dolby_amaze_lossless-DWEU.m2ts", 8, 16, 48000.0, AudioCodec.TruehdAtmos, 0, 2)]
    [InlineData(@"..\..\..\..\HD Audio\dolby_digital_plus_channel_check_lossless-DWEU.mkv", 6, 0, 48000.0, AudioCodec.Eac3, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\dolby_truehd_channel_check_lossless-DWEU.mkv", 8, 0, 48000.0, AudioCodec.Truehd, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\Dredd – DTS Sound Check DTS-HD MA 7.1.m2ts", 8, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\DTS-HRA5.1_VC1-23.976.mkv", 6, 24, 96000.0, AudioCodec.DtsHdHra, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\dts-listen-x-long-lossless-(www.demolandia.net).mkv", 8, 0, 48000.0, AudioCodec.DtsX, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\dts-sound-unbound-callout-11.1-lossless-(www.demolandia.net).mkv", 8, 0, 48000.0, AudioCodec.DtsX, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\DTS-X Gravity.mkv", 8, 24, 48000.0, AudioCodec.DtsX, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\dts_hd_master_audio_sound_check_7_1_lossless-DWEU.mkv", 6, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\dts_MA_all_around_us_lossless-DWEU.mkv", 8, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\dts_orchestra_short_lossless-DWEU.mkv", 6, 24, 96000.0, AudioCodec.DtsHdHra, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\dtsac3audiosample.avi", 6, 16, 48000.0, AudioCodec.Dts, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\DTSX_Demo_2016.m2ts", 0, 0, 0.0, AudioCodec.DtsX, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\dvb-aac-latm.m2t", 2, 0, 48000.0, AudioCodec.AacMpeg4LcSbr, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\freetv_aac_latm.ts", 1, 0, 24000.0, AudioCodec.AacMpeg4Lc, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\hd_dts_hd_master_audio_sound_check_5_1_lossless.m2ts", 6, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\lfe_is_sce.mp4", 6, 0, 44100.0, AudioCodec.AacMpeg4Lc, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\opus.ts", 0, 0, 0.0, AudioCodec.Opus, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\POCAWE_Sample.mkv", 6, 24, 48000.0, AudioCodec.PcmIntLit, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\Safari_ Dolby_Digital_Plus.m2ts", 8, 0, 48000.0, AudioCodec.Eac3, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\SBR_LFEtest5_1.mp4", 6, 0, 44100.0, AudioCodec.AacMpeg4LcSbr, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\small-sample-file.ts", 6, 20, 48000.0, AudioCodec.Dts, 0, 2)]
    [InlineData(@"..\..\..\..\HD Audio\transf.m2ts", 6, 16, 48000.0, AudioCodec.Ac3, 0, 5)]
    [InlineData(@"..\..\..\..\HD Audio\VC1-1080p23.976-LPCM7.1.mkv", 8, 16, 48000.0, AudioCodec.PcmIntLit, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\zodiac_audio.mov", 6, 0, 48000.0, AudioCodec.AacMpeg4Lc, 0, 1)]
    [InlineData(@"..\..\..\..\HD Audio\zx.eva.renewal.01.divx511.mkv", 6, 0, 44100.0, AudioCodec.AacMpeg4LcSbr, 0, 2)]
    public void LoadHdAudioFromVideoContainer(string fileName, int channels, int bitDepth, double samplingRate, AudioCodec codec, int index, int audioCount)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(audioCount);
      var flac = _mediaInfoWrapper.AudioStreams[index];
      flac.Codec.Should().Be(codec);
      flac.Channel.Should().Be(channels);
      flac.BitDepth.Should().Be(bitDepth);
      flac.SamplingRate.Should().Be(samplingRate);
    }

    [Theory]
    [InlineData(@".\Data\Test_MP3Tags.mp3", 74406L)]
    [InlineData(@".\Data\Test_MP3Tags_2.mp3", 212274L)]
    public void LoadMp3FileWithTags(string fileName, long size)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.Size.Should().Be(size);
      _mediaInfoWrapper.HasVideo.Should().BeFalse("Video stream does not supported in MP3!");
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is missing");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(1);
      // MP3 file contains all tags in general stream
      _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeEmpty();
      _mediaInfoWrapper.Tags.Album.Should().NotBeNullOrEmpty();
      _mediaInfoWrapper.Tags.Track.Should().NotBeNullOrEmpty();
      _mediaInfoWrapper.Tags.TrackPosition.Should().NotBeNull();
      _mediaInfoWrapper.Tags.Artist.Should().NotBeNullOrEmpty();
      _mediaInfoWrapper.Tags.RecordedDate.Should().NotBeNull();
      _mediaInfoWrapper.Tags.Genre.Should().NotBeNullOrEmpty();
      var audio = _mediaInfoWrapper.AudioStreams[0];
      audio.Codec.Should().Be(AudioCodec.MpegLayer3);
      audio.Tags.GeneralTags.Should().NotBeNull();
      audio.Tags.GeneralTags.Should().NotBeEmpty();
    }

    [TheoryInDebugOnly]
    [InlineData(@"E:\Music\Anugama\Healing\01 - Healing Earth.flac")]
    public void LoadFlacWithCover(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.Size.Should().Be(141439565L);
      _mediaInfoWrapper.HasVideo.Should().BeFalse("Video stream does not supported in FLAC!");
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is missing");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(1);
      // FLAC file contains all tags in general stream
      _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeEmpty();
      _mediaInfoWrapper.Tags.Album.Should().NotBeNullOrEmpty();
      _mediaInfoWrapper.Tags.Track.Should().NotBeNullOrEmpty();
      _mediaInfoWrapper.Tags.TrackPosition.Should().NotBeNull();
      _mediaInfoWrapper.Tags.Artist.Should().NotBeNullOrEmpty();
      _mediaInfoWrapper.Tags.RecordedDate.Should().NotBeNull();
      _mediaInfoWrapper.Tags.Genre.Should().NotBeNullOrEmpty();
      _mediaInfoWrapper.Tags.Covers.Should().NotBeNullOrEmpty();
      _mediaInfoWrapper.Tags.Covers.Count().Should().Be(1);
      var cover = _mediaInfoWrapper.Tags.Covers.First();
      cover.Mime.Should().Be("image/jpeg");
      var audio = _mediaInfoWrapper.AudioStreams[0];
      audio.Codec.Should().Be(AudioCodec.Flac);
      audio.Tags.GeneralTags.Should().NotBeNull();
      audio.Tags.GeneralTags.Should().NotBeEmpty();
    }

    [TheoryInDebugOnly]
    [InlineData(@"E:\Music\01. HARLEYS&INDIANS [RIDERS IN THE SKY].mp3")]
    public void LoadFlacWithoutCovers(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.Size.Should().Be(4171517L);
      _mediaInfoWrapper.HasVideo.Should().BeFalse("Video stream does not supported in MP3!");
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is missing");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(1);
      // MP3 file contains all tags in general stream
      _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeEmpty();
      _mediaInfoWrapper.Tags.Album.Should().NotBeNullOrEmpty();
      _mediaInfoWrapper.Tags.Track.Should().NotBeNullOrEmpty();
      _mediaInfoWrapper.Tags.TrackPosition.Should().NotBeNull();
      _mediaInfoWrapper.Tags.Artist.Should().NotBeNullOrEmpty();
      _mediaInfoWrapper.Tags.RecordedDate.Should().NotBeNull();
      _mediaInfoWrapper.Tags.Genre.Should().NotBeNullOrEmpty();
      var audio = _mediaInfoWrapper.AudioStreams[0];
      audio.Codec.Should().Be(AudioCodec.MpegLayer3);
      audio.Tags.GeneralTags.Should().NotBeNull();
      audio.Tags.GeneralTags.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(@".\Data\Test_MP3Tags.mka")]
    public void LoadMultiStreamContainer(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.Size.Should().Be(135172L);
      _mediaInfoWrapper.HasVideo.Should().BeFalse("Video stream does not supported in MKA!");
      _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
      _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
      _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is missing");
      _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
      _mediaInfoWrapper.AudioStreams.Count.Should().Be(2);
      // MKA file contains all tags in general stream
      _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeNull();
      _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeEmpty();
      _mediaInfoWrapper.Tags.EncodedDate.Should().NotBeNull();
      var audio = _mediaInfoWrapper.AudioStreams[0];
      audio.Should().NotBeNull();
      audio.Tags.GeneralTags.Should().NotBeNull();
      audio.Tags.GeneralTags.Should().NotBeEmpty();
      audio.Tags.Album.Should().NotBeNullOrEmpty();
      audio.Tags.Track.Should().NotBeNullOrEmpty();
      audio.Tags.Artist.Should().NotBeNullOrEmpty();
      audio.Tags.ReleasedDate.Should().NotBeNull();
      audio.Codec.Should().Be(AudioCodec.MpegLayer3);
      audio = _mediaInfoWrapper.AudioStreams[1];
      audio.Should().NotBeNull();
      audio.Tags.GeneralTags.Should().NotBeNull();
      audio.Tags.GeneralTags.Should().NotBeEmpty();
      audio.Tags.Album.Should().NotBeNullOrEmpty();
      audio.Tags.Track.Should().NotBeNullOrEmpty();
      audio.Tags.Artist.Should().NotBeNullOrEmpty();
      audio.Codec.Should().Be(AudioCodec.MpegLayer3);
    }
  }
}