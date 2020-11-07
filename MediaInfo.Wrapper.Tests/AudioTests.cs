#region Copyright (C) 2017-2020 Yaroslav Tatarenko

// Copyright (C) 2017-2020 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2020 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Linq;
using FluentAssertions;
using MediaInfo.Model;
using Xunit;
using Xunit.Abstractions;

namespace MediaInfo.Wrapper.Tests
{
  public class AudioTests
  {
    private readonly ILogger _logger;
    private MediaInfoWrapper _mediaInfoWrapper;

    public AudioTests(ITestOutputHelper testOutputHelper)
    {
      _logger = new TestLogger(testOutputHelper);
    }

    [Theory]
    [InlineData("./Data/Test_H264_DTS1.m2ts", 9, 1)]
    [InlineData("./Data/Test_H264_DTS2.m2ts", 6, 0)]
    public void LoadVideoWithDtsMa(string fileName, int audioStreamCount, int dtsIndex)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
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

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("../../../../HD Audio/2L-125_04_stereo.mqa.flac", 2, 24, 44100.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/2L-125_mch-96k-24b_04.flac", 6, 24, 96000.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/2L-125_stereo-44k-16b_04.flac", 2, 16, 44100.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/2L-125_stereo-88k-24b_04.flac", 2, 24, 88200.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/2L-125_stereo-176k-24b_04.flac", 2, 24, 176400.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/2L-125_stereo-352k-24b_04.flac", 2, 24, 352800.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/2L-125_mch-2822k-1b_04.dsf", 6, 1, 2822400.0, AudioCodec.Dsd)]
    [InlineData("../../../../HD Audio/2L-125_stereo-2822k-1b_04.dsf", 2, 1, 2822400.0, AudioCodec.Dsd)]
    [InlineData("../../../../HD Audio/2L-125_stereo-5644k-1b_04.dsf", 2, 1, 5644800.0, AudioCodec.Dsd)]
    [InlineData("../../../../HD Audio/2L-125_stereo-11289k-1b_04.dsf", 2, 1, 11289600.0, AudioCodec.Dsd)]
    [InlineData("../../../../HD Audio/2L-145_01_stereo.mqa.flac", 2, 24, 44100.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/2L-145_01_stereo.mqacd.mqa.flac", 2, 16, 44100.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/2L-145_mch_FLAC_96k_24b_01.flac", 6, 24, 96000.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/2L-45_stereo_01_DSF_11289k_1b.dsf", 2, 1, 11289600.0, AudioCodec.Dsd)]
    [InlineData("../../../../HD Audio/2L-45_stereo_01_FLAC_88k_24b.flac", 2, 24, 88200.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/2L-45_stereo_01_FLAC_176k_24b.flac", 2, 24, 176400.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/2L-45_stereo_01_FLAC_352k_24b.flac", 2, 24, 352800.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/03_NEUE_MUSIK_BERND_THEWESl_kleine_pingpark_wolke_DTS.wav", 6, 24, 44100.0, AudioCodec.Dts)]
    [InlineData("../../../../HD Audio/3-1.dts", 4, 24, 48000.0, AudioCodec.DtsHdMa)]
    [InlineData("../../../../HD Audio/5.1 24bit.dts", 6, 24, 48000.0, AudioCodec.Dts)]
    [InlineData("../../../../HD Audio/06_NEW_JAZZ_SSQ3_ENERGY_DTS.wav", 6, 24, 44100.0, AudioCodec.Dts)]
    [InlineData("../../../../HD Audio/7.1auditionOutLeader v2.wav", 8, 16, 48000.0, AudioCodec.PcmIntLit)]
    [InlineData("../../../../HD Audio/7_pt_1.eac3", 8, 0, 48000.0, AudioCodec.Eac3)]
    [InlineData("../../../../HD Audio/8_Channel_ID.m4a", 8, 0, 48000.0, AudioCodec.AacMpeg4Lc)]
    [InlineData("../../../../HD Audio/96-24.dts", 6, 24, 96000.0, AudioCodec.DtsHd)]
    [InlineData("../../../../HD Audio/Berlioz - Hungarian March.aac", 6, 0, 48000.0, AudioCodec.AacMpeg4LcSbr)]
    [InlineData("../../../../HD Audio/Berlioz - Hungarian March.wma", 6, 24, 96000.0, AudioCodec.WmaPro)]
    [InlineData("../../../../HD Audio/Broadway-5.1-48khz-448kbit.ac3", 6, 0, 48000.0, AudioCodec.Ac3)]
    [InlineData("../../../../HD Audio/csi_miami_5.1_256_spx.eac3", 6, 0, 48000.0, AudioCodec.Eac3)]
    [InlineData("../../../../HD Audio/csi_miami_5.1_256_spx_nero.flac", 6, 24, 48000.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/ct_faac-adts.aac", 2, 0, 44100.0, AudioCodec.AacMpeg4Lc)]
    [InlineData("../../../../HD Audio/Cybele_SACD_030802_24b96k_8ch_Aurophonie_11.flac", 8, 24, 96000.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/Cybele_SACD_030802_24b96k_Surround_11.flac", 5, 24, 96000.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/Cybele_SACD_030802_DSD_Surround_11.dsf", 5, 1, 2822400.0, AudioCodec.Dsd)]
    [InlineData("../../../../HD Audio/Cybele_SACD_051501_16b44k_3D-Binaural-Stereo_03.flac", 2, 16, 44100.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/Cybele_SACD_051501_24b96k_3D-Binaural-Stereo_03.flac", 2, 24, 96000.0, AudioCodec.Flac)]
    [InlineData("../../../../HD Audio/dtswavsample14.wav", 6, 24, 44100.0, AudioCodec.Dts)]
    [InlineData("../../../../HD Audio/ES 6.1 - 5.1 16bit.dts", 7, 16, 48000.0, AudioCodec.DtsEs)]
    [InlineData("../../../../HD Audio/ES 6.1 24bit.dts", 7, 24, 48000.0, AudioCodec.DtsEs)]
    [InlineData("../../../../HD Audio/faad_infinite_long.aacp", 2, 0, 44100.0, AudioCodec.AacMpeg4LcSbrPs)]
    [InlineData("../../../../HD Audio/Hi-Res 6.1 24bit.dts", 7, 24, 48000.0, AudioCodec.DtsHdHra)]
    [InlineData("../../../../HD Audio/lotr_5.1_768.dts", 7, 24, 48000.0, AudioCodec.DtsEs)]
    [InlineData("../../../../HD Audio/Major 06 kwestia 03.mka", 1, 0, 48000.0, AudioCodec.AacMpeg4LcSbrPs)]
    [InlineData("../../../../HD Audio/Master Audio 2.0 16bit.dts", 2, 16, 48000.0, AudioCodec.DtsHdMa)]
    [InlineData("../../../../HD Audio/Master Audio 5.0 96khz.dts", 5, 24, 96000.0, AudioCodec.DtsHdMa)]
    [InlineData("../../../../HD Audio/Master Audio 5.1 24bit.dts", 6, 24, 48000.0, AudioCodec.DtsHdMa)]
    [InlineData("../../../../HD Audio/Master Audio 7.1.dts", 8, 24, 48000.0, AudioCodec.DtsHdMa)]
    [InlineData("../../../../HD Audio/Master Audio 7.1 24bit.dts", 8, 24, 48000.0, AudioCodec.DtsHdMa)]
    [InlineData("../../../../HD Audio/monsters_inc_5.1_448.ac3", 6, 0, 48000.0, AudioCodec.Ac3)]
    [InlineData("../../../../HD Audio/SBR_LFETest5_1-441-16b.wav", 6, 16, 44100.0, AudioCodec.PcmIntLit)]
    [InlineData("../../../../HD Audio/stuttering_dts_truhd.dts", 6, 24, 48000.0, AudioCodec.Dts)]
    [InlineData("../../../../HD Audio/thx_2_0.ac3", 2, 0, 48000.0, AudioCodec.Ac3)]
    [InlineData("../../../../HD Audio/working_dts_with_dtscore.dts", 6, 16, 48000.0, AudioCodec.Dts)]
    public void LoadHdAudioFile(string fileName, int channels, int bitDepth, double samplingRate, AudioCodec codec)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      _mediaInfoWrapper.HasVideo.Should().BeFalse("Audio file");
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

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("../../../../Audio/8_Channel_ID.wav", 8, 24, 48000.0, AudioCodec.PcmIntLit)]
    [InlineData("../../../../Audio/8_Channel_ID.wma", 8, 24, 48000.0, AudioCodec.WmaPro)]
    [InlineData("../../../../Audio/16bit.wv", 2, 16, 44100.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/24bit.wv", 2, 24, 44100.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/24kbps.wvc", 1, 16, 8000.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/27 MC Solaar - Rmi.mp3", 2, 0, 22050.0, AudioCodec.MpegLayer3)]
    [InlineData("../../../../Audio/32bit_float.wv", 2, 32, 44100.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/32bit_int.wv", 2, 32, 44100.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/32bit_int_p.wv", 2, 32, 44100.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/320kbps.wv", 2, 16, 44100.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/320kbps.wvc", 2, 16, 44100.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/440hz.mlp", 2, 16, 44100.0, AudioCodec.Mlp)]
    [InlineData("../../../../Audio/440hz.wv", 2, 16, 44100.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/adpcmtst.wav", 2, 4, 11025.0, AudioCodec.Adpcm)]
    [InlineData("../../../../Audio/ALAC_6ch.mov", 6, 0, 48000.0, AudioCodec.Alac)]
    [InlineData("../../../../Audio/ALAC_24bits2.mov", 2, 0, 48000.0, AudioCodec.Alac)]
    [InlineData("../../../../Audio/alac_encoding_failes.wav", 2, 16, 44100.0, AudioCodec.PcmIntLit)]
    [InlineData("../../../../Audio/ambient4_192_mulitchannel.wma", 6, 24, 48000.0, AudioCodec.WmaPro)]
    [InlineData("../../../../Audio/at3p_sample1.oma", 2, 0, 44100.0, AudioCodec.Atrac3)]
    [InlineData("../../../../Audio/at3p_sample6.oma", 2, 0, 44100.0, AudioCodec.Atrac3)]
    [InlineData("../../../../Audio/Ayumi Hamasaki - ever free.wv", 2, 16, 44100.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/Bach1-1.aiff", 1, 8, 22255.0, AudioCodec.PcmIntBig)]
    [InlineData("../../../../Audio/basicinstinct.ogm", 2, 0, 44100.0, AudioCodec.Vorbis)]
    [InlineData("../../../../Audio/Beethovens nionde symfoni (Scherzo)-2.wma", 2, 24, 48000.0, AudioCodec.WmaPro)]
    [InlineData("../../../../Audio/Boys3-1.aiff", 1, 8, 22255.0, AudioCodec.Mac3)]
    [InlineData("../../../../Audio/Boys6-1.aiff", 1, 8, 22255.0, AudioCodec.Mac6)]
    [InlineData("../../../../Audio/chrisrock-nosex.asf", 1, 16, 16000.0, AudioCodec.Wma1)]
    [InlineData("../../../../Audio/classical_16_16_1_8000_off_0_off_1_29.wma", 1, 16, 16000.0, AudioCodec.WmaPro)]
    [InlineData("../../../../Audio/Classical_96_24_6_256000_1_20.wma", 6, 24, 96000.0, AudioCodec.WmaPro)]
    [InlineData("../../../../Audio/cristinreel.mov", 2, 8, 44100.0, AudioCodec.Mac6)]
    [InlineData("../../../../Audio/DG.wmv", 2, 16, 48000.0, AudioCodec.Wma2)]
    [InlineData("../../../../Audio/dune2.avi", 2, 0, 48000.0, AudioCodec.MpegLayer3)]
    [InlineData("../../../../Audio/edward_4.0_24bit.wv", 4, 24, 48000.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/eoa.wma", 2, 16, 44100.0, AudioCodec.Wma2)]
    [InlineData("../../../../Audio/farm.wav", 1, 8, 11025.0, AudioCodec.PcmIntLit)]
    [InlineData("../../../../Audio/ffvorbis_crash.ogm", 2, 0, 48000.0, AudioCodec.Vorbis)]
    [InlineData("../../../../Audio/format-0x42-speechtest-MSG5.3.asf", 1, 0, 8000.0, AudioCodec.G_723_1)]
    [InlineData("../../../../Audio/God Save The Queen.mlp", 6, 24, 96000.0, AudioCodec.Mlp)]
    [InlineData("../../../../Audio/H263_ALAC_24bits.mov", 2, 0, 48000.0, AudioCodec.Alac)]
    [InlineData("../../../../Audio/intro - Creative ADPCM.wav", 1, 4, 44100.0, AudioCodec.Adpcm)]
    [InlineData("../../../../Audio/jpg_in_mp3.mp3", 2, 0, 44100.0, AudioCodec.MpegLayer3)]
    [InlineData("../../../../Audio/large_superframe.wma", 2, 16, 44100.0, AudioCodec.Wma2)]
    [InlineData("../../../../Audio/lbd-ts.wav", 1, 1, 8000.0, AudioCodec.Truespeech)]
    [InlineData("../../../../Audio/luckynight.ape", 2, 16, 44100.0, AudioCodec.Ape)]
    [InlineData("../../../../Audio/luckynight.flac", 2, 16, 44100.0, AudioCodec.Flac)]
    [InlineData("../../../../Audio/luckynight.m4a", 2, 16, 44100.0, AudioCodec.Alac)]
    [InlineData("../../../../Audio/luckynight.rka", 2, 16, 44100.0, AudioCodec.RkAudio)]
    [InlineData("../../../../Audio/luckynight.rmvb", 0, 0, 0.0, AudioCodec.Real10)]
    [InlineData("../../../../Audio/luckynight.tta", 2, 16, 44100.0, AudioCodec.Tta1)]
    [InlineData("../../../../Audio/luckynight.wma", 2, 16, 44100.0, AudioCodec.WmaLossless)]
    [InlineData("../../../../Audio/luckynight.wv", 2, 16, 44100.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/luckynight-als.mp4", 2, 0, 44100.0, AudioCodec.Als)]
    [InlineData("../../../../Audio/Lumme-Badloop.ogg", 2, 0, 44100.0, AudioCodec.Vorbis)]
    [InlineData("../../../../Audio/mac3audio.mov", 2, 8, 22050.0, AudioCodec.Mac3)]
    [InlineData("../../../../Audio/mjpega.mov", 1, 8, 8000.0, AudioCodec.Mac6)]
    [InlineData("../../../../Audio/mp3_with_embedded_albumart.mp3", 2, 0, 44100.0, AudioCodec.MpegLayer3)]
    [InlineData("../../../../Audio/mpeg-in-ogm.ogm", 2, 0, 48000.0, AudioCodec.Vorbis)]
    [InlineData("../../../../Audio/mpeg_layer1_audio.mpg", 2, 0, 44100.0, AudioCodec.MpegLayer1)]
    [InlineData("../../../../Audio/mplayer_sample-audio_0x161.wmv", 2, 16, 44100.0, AudioCodec.Wma2)]
    [InlineData("../../../../Audio/newedition-coolitnow.24bit-lpcm.vob", 2, 24, 48000.0, AudioCodec.PcmIntBig)]
    [InlineData("../../../../Audio/newOrleans_192_mulitchannel.wma", 6, 24, 48000.0, AudioCodec.WmaPro)]
    [InlineData("../../../../Audio/old_midi_stuff.m4a", 2, 16, 44100.0, AudioCodec.Alac)]
    [InlineData("../../../../Audio/panslab_sample_5.1_16bit.wv", 6, 16, 48000.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/panslab_sample_5.1_16bit_lossy.wv", 6, 16, 48000.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/panslab_sample_7.1_16bit_lossy.wv", 8, 16, 48000.0, AudioCodec.WavPack)]
    [InlineData("../../../../Audio/piece.wmv", 6, 24, 48000.0, AudioCodec.WmaPro)]
    [InlineData("../../../../Audio/qanda_2008_ep10.wmv", 2, 16, 44100.0, AudioCodec.Wma2)]
    [InlineData("../../../../Audio/quicktime-newcodec-applelosslessaudiocodec.m4a", 2, 16, 44100.0, AudioCodec.Alac)]
    [InlineData("../../../../Audio/raam28mb.wmv", 2, 16, 22050.0, AudioCodec.Wma2)]
    [InlineData("../../../../Audio/rum.wma", 2, 16, 32000.0, AudioCodec.Wma2)]
    [InlineData("../../../../Audio/saltnpepa-doyouwantme.asf", 2, 16, 32000.0, AudioCodec.Wma2)]
    [InlineData("../../../../Audio/sonateno14op27-2.aa3", 2, 0, 44100.0, AudioCodec.Atrac3)]
    [InlineData("../../../../Audio/streaming.wina.com-live.asx", 1, 16, 22050.0, AudioCodec.Wma2)]
    [InlineData("../../../../Audio/streaming_CBR-19K-timecomp.wma", 1, 16, 16000.0, AudioCodec.WmaVoice)]
    [InlineData("../../../../Audio/surge-1-8-MAC3.mov", 1, 8, 44100.0, AudioCodec.Mac3)]
    [InlineData("../../../../Audio/surge-1-8-MAC3.wav", 1, 16, 44100.0, AudioCodec.PcmIntLit)]
    [InlineData("../../../../Audio/TAXI8BIT.wav", 1, 8, 11111.0, AudioCodec.PcmIntLit)]
    [InlineData("../../../../Audio/test-96.wav", 2, 24, 96000.0, AudioCodec.PcmIntLit)]
    [InlineData("../../../../Audio/testvector01.ogg", 2, 0, 44100.0, AudioCodec.Opus)]
    [InlineData("../../../../Audio/testvector07.ogg", 2, 0, 44100.0, AudioCodec.Opus)]
    [InlineData("../../../../Audio/tide_8-bit_22KHz.wav", 1, 8, 22050.0, AudioCodec.PcmIntLit)]
    [InlineData("../../../../Audio/tide_16-bit_44KHz_master.wav", 1, 16, 44100.0, AudioCodec.PcmIntLit)]
    [InlineData("../../../../Audio/toesidebackroll.AVI", 2, 2, 44100.0, AudioCodec.Iac2)]
    [InlineData("../../../../Audio/traciespencer-allaboutyou.asf", 2, 16, 22050.0, AudioCodec.Wma2)]
    [InlineData("../../../../Audio/TrueHD.raw", 6, 0, 48000.0, AudioCodec.Truehd)]
    [InlineData("../../../../Audio/truespeech_a5.wav", 1, 1, 8000.0, AudioCodec.Truespeech)]
    [InlineData("../../../../Audio/vc1-with-truehd.m2ts", 6, 0, 48000.0, AudioCodec.Ac3)]
    [InlineData("../../../../Audio/vorbis3plus_sample.avi", 2, 0, 48000.0, AudioCodec.Vorbis)]
    [InlineData("../../../../Audio/WMA_protected_napster.wma", 2, 16, 44100.0, AudioCodec.Wma2)]
    [InlineData("../../../../Audio/wmav_8.wma", 1, 16, 8000.0, AudioCodec.WmaVoice)]
    [InlineData("../../../../Audio/wmv-surroundtest_720p.wmv", 6, 24, 48000.0, AudioCodec.WmaPro)]
    [InlineData("../../../../Audio/wmv3-wmaspeeech.wmv", 1, 16, 8000.0, AudioCodec.WmaVoice)]
    public void LoadAudioFile(string fileName, int channels, int bitDepth, double samplingRate, AudioCodec codec)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
      _mediaInfoWrapper.MediaInfoNotloaded.Should().BeFalse("InfoWrapper should be loaded");
      var audio = _mediaInfoWrapper.AudioStreams[0];
      audio.Codec.Should().Be(codec);
      audio.Channel.Should().Be(channels);
      audio.BitDepth.Should().Be(bitDepth);
      audio.SamplingRate.Should().Be(samplingRate);
    }

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("../../../../HD Audio/7.1auditionOutLeader_v2_rtb.mp4", 8, 0, 48000.0, AudioCodec.AacMpeg4LcSbr, 0, 1)]
    [InlineData("../../../../HD Audio/7_pt_1_sample.evo", 8, 0, 48000.0, AudioCodec.Eac3, 0, 1)]
    [InlineData("../../../../HD Audio/12-10_19-18-52_BBC HD_Wild China.ts", 6, 0, 48000.0, AudioCodec.AacMpeg4Lc, 0, 2)]
    [InlineData("../../../../HD Audio/ac3-sound-sample.vob", 6, 0, 48000.0, AudioCodec.Ac3, 0, 1)]
    [InlineData("../../../../HD Audio/bond_sample_dtshdma.m2ts", 6, 24, 48000.0, AudioCodec.DtsHdMa, 0, 7)]
    [InlineData("../../../../HD Audio/ChID-BLITS-EBU.mp4", 6, 0, 44100.0, AudioCodec.AacMpeg4LcSbr, 0, 1)]
    [InlineData("../../../../HD Audio/ChID-BLITS-EBU-Narration.mp4", 6, 0, 44100.0, AudioCodec.AacMpeg4LcSbr, 0, 1)]
    [InlineData("../../../../HD Audio/danish-1.m2t", 2, 0, 48000.0, AudioCodec.AacMpeg4LcSbr, 0, 1)]
    [InlineData("../../../../HD Audio/DNCE 29.97 h264 1080i 26mbps DTS-HD MA 2.0 sample.mkv", 2, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1)]
    [InlineData("../../../../HD Audio/dolby-audiosphere-lossless-(www.demolandia.net).m2ts", 8, 0, 48000.0, AudioCodec.TruehdAtmos, 0, 2)]
    [InlineData("../../../../HD Audio/dolby-silent-lossless-(www.demolandia.net).m2ts", 8, 0, 48000.0, AudioCodec.TruehdAtmos, 0, 2)]
    [InlineData("../../../../HD Audio/Dolby ATMOS Helicopter.m2ts", 8, 0, 48000.0, AudioCodec.TruehdAtmos, 0, 2)]
    [InlineData("../../../../HD Audio/dolby_amaze_lossless-DWEU.m2ts", 8, 0, 48000.0, AudioCodec.TruehdAtmos, 0, 2)]
    [InlineData("../../../../HD Audio/dolby_digital_plus_channel_check_lossless-DWEU.mkv", 6, 0, 48000.0, AudioCodec.Eac3, 0, 1)]
    [InlineData("../../../../HD Audio/dolby_truehd_channel_check_lossless-DWEU.mkv", 8, 0, 48000.0, AudioCodec.Truehd, 0, 1)]
    [InlineData("../../../../HD Audio/Dredd – DTS Sound Check DTS-HD MA 7.1.m2ts", 8, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1)]
    [InlineData("../../../../HD Audio/DTS-HRA5.1_VC1-23.976.mkv", 6, 24, 96000.0, AudioCodec.DtsHdHra, 0, 1)]
    [InlineData("../../../../HD Audio/dts-listen-x-long-lossless-(www.demolandia.net).mkv", 8, 0, 48000.0, AudioCodec.DtsX, 0, 1)]
    [InlineData("../../../../HD Audio/dts-sound-unbound-callout-11.1-lossless-(www.demolandia.net).mkv", 8, 0, 48000.0, AudioCodec.DtsX, 0, 1)]
    [InlineData("../../../../HD Audio/DTS-X Gravity.mkv", 8, 24, 48000.0, AudioCodec.DtsX, 0, 1)]
    [InlineData("../../../../HD Audio/dts_hd_master_audio_sound_check_7_1_lossless-DWEU.mkv", 6, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1)]
    [InlineData("../../../../HD Audio/dts_MA_all_around_us_lossless-DWEU.mkv", 8, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1)]
    [InlineData("../../../../HD Audio/dts_orchestra_short_lossless-DWEU.mkv", 6, 24, 96000.0, AudioCodec.DtsHdHra, 0, 1)]
    [InlineData("../../../../HD Audio/dtsac3audiosample.avi", 6, 16, 48000.0, AudioCodec.Dts, 0, 1)]
    [InlineData("../../../../HD Audio/DTSX_Demo_2016.m2ts", 0, 0, 0.0, AudioCodec.DtsX, 0, 1)]
    [InlineData("../../../../HD Audio/dvb-aac-latm.m2t", 2, 0, 48000.0, AudioCodec.AacMpeg4LcSbr, 0, 1)]
    [InlineData("../../../../HD Audio/freetv_aac_latm.ts", 1, 0, 24000.0, AudioCodec.AacMpeg4Lc, 0, 1)]
    [InlineData("../../../../HD Audio/hd_dts_hd_master_audio_sound_check_5_1_lossless.m2ts", 6, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1)]
    [InlineData("../../../../HD Audio/lfe_is_sce.mp4", 6, 0, 44100.0, AudioCodec.AacMpeg4Lc, 0, 1)]
    [InlineData("../../../../HD Audio/opus.ts", 0, 0, 0.0, AudioCodec.Opus, 0, 1)]
    [InlineData("../../../../HD Audio/POCAWE_Sample.mkv", 6, 24, 48000.0, AudioCodec.PcmIntLit, 0, 1)]
    [InlineData("../../../../HD Audio/Safari_ Dolby_Digital_Plus.m2ts", 8, 0, 48000.0, AudioCodec.Eac3, 0, 1)]
    [InlineData("../../../../HD Audio/SBR_LFEtest5_1.mp4", 6, 0, 44100.0, AudioCodec.AacMpeg4LcSbr, 0, 1)]
    [InlineData("../../../../HD Audio/small-sample-file.ts", 6, 20, 48000.0, AudioCodec.Dts, 0, 2)]
    [InlineData("../../../../HD Audio/transf.m2ts", 6, 0, 48000.0, AudioCodec.Ac3, 0, 5)]
    [InlineData("../../../../HD Audio/VC1-1080p23.976-LPCM7.1.mkv", 8, 16, 48000.0, AudioCodec.PcmIntLit, 0, 1)]
    [InlineData("../../../../HD Audio/zodiac_audio.mov", 6, 0, 48000.0, AudioCodec.AacMpeg4Lc, 0, 1)]
    [InlineData("../../../../HD Audio/zx.eva.renewal.01.divx511.mkv", 6, 0, 44100.0, AudioCodec.AacMpeg4LcSbr, 0, 2)]
    public void LoadHdAudioFromVideoContainer(string fileName, int channels, int bitDepth, double samplingRate, AudioCodec codec, int index, int audioCount)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
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
    [InlineData("./Data/Test_MP3Tags.mp3", 74406L)]
    [InlineData("./Data/Test_MP3Tags_2.mp3", 212274L)]
    public void LoadMp3FileWithTags(string fileName, long size)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
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

    [Theory]
    [InlineData("./Data/Test_MP3Tags.mka")]
    public void LoadMultiStreamContainer(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
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