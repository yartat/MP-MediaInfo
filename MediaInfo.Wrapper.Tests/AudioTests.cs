#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2020 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Linq;
using FluentAssertions;
using MediaInfo.Model;
#if NET5_0_OR_GREATER
using Microsoft.Extensions.Logging;
#endif
using Xunit;
using Xunit.Abstractions;

namespace MediaInfo.Wrapper.Tests;

public class AudioTests
{
    private readonly ILogger? _logger;
    private MediaInfoWrapper? _mediaInfoWrapper;

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
        _mediaInfoWrapper.Success.Should().BeTrue("InfoWrapper should be loaded");
        _mediaInfoWrapper.HasVideo.Should().BeTrue("Video stream must be detected");
        _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
        _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
        _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is progressive");
        _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
        _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
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
        _mediaInfoWrapper.Success.Should().BeTrue("InfoWrapper should be loaded");
        _mediaInfoWrapper.HasVideo.Should().BeFalse("Audio file");
        _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
        _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
        _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream does not exist");
        _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream does not exist");
        _mediaInfoWrapper.AudioStreams.Count.Should().Be(1);
        _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
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
    [InlineData("../../../../Audio/Sample_Dolby_E.ts", 6, 20, 48000.0, AudioCodec.DolbyE, 2)]
    [InlineData("../../../../Audio/Samble_Dolby_E_2.ts", 6, 20, 48000.0, AudioCodec.DolbyE, 2)]
    [InlineData("../../../../HD/Living_Room_1080p_20_96k_25fps.mpd", 2, 0, 0, AudioCodec.Ac4, 0)]
    [InlineData("../../../../HD/Living_Room_1080p_51_192k_320k_2997fps.mpd", 2, 0, 0, AudioCodec.Ac4, 0)]
    [InlineData("../../../../HD/Living_Room_1080p_51_192k_2997fps.mpd", 2, 0, 0, AudioCodec.Ac4, 0)]
    public void LoadDolbyCodecsFile(string fileName, int channels, int bitDepth, double samplingRate, AudioCodec codec, int index)
    {
        _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
        _mediaInfoWrapper.Success.Should().BeTrue("InfoWrapper should be loaded");
        _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
        _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
        _mediaInfoWrapper.AudioStreams.Count.Should().BeGreaterThan(index);
        _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
        var audio = _mediaInfoWrapper.AudioStreams[index];
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
    [InlineData("../../../../Audio/8_Channel_ID.wav", 8, 24, 48000.0, AudioCodec.PcmIntLit, 16126056L)]
    [InlineData("../../../../Audio/8_Channel_ID.wma", 8, 24, 48000.0, AudioCodec.WmaPro, 671952L)]
    [InlineData("../../../../Audio/16bit.wv", 2, 16, 44100.0, AudioCodec.WavPack, 2902604L)]
    [InlineData("../../../../Audio/24bit.wv", 2, 24, 44100.0, AudioCodec.WavPack, 5518702L)]
    [InlineData("../../../../Audio/24kbps.wvc", 1, 16, 8000.0, AudioCodec.WavPack, 180558L)]
    [InlineData("../../../../Audio/27 MC Solaar - Rmi.mp3", 2, 0, 22050.0, AudioCodec.MpegLayer3, 2081228L)]
    [InlineData("../../../../Audio/32bit_float.wv", 2, 32, 44100.0, AudioCodec.WavPack, 7168512L)]
    [InlineData("../../../../Audio/32bit_int.wv", 2, 32, 44100.0, AudioCodec.WavPack, 8165754L)]
    [InlineData("../../../../Audio/32bit_int_p.wv", 2, 32, 44100.0, AudioCodec.WavPack, 5712194L)]
    [InlineData("../../../../Audio/320kbps.wv", 2, 16, 44100.0, AudioCodec.WavPack, 1254526L)]
    [InlineData("../../../../Audio/320kbps.wvc", 2, 16, 44100.0, AudioCodec.WavPack, 1607558L)]
    [InlineData("../../../../Audio/440hz.mlp", 2, 16, 44100.0, AudioCodec.Mlp, 0L)]
    [InlineData("../../../../Audio/440hz.wv", 2, 16, 44100.0, AudioCodec.WavPack, 325546L)]
    [InlineData("../../../../Audio/adpcmtst.wav", 2, 4, 11025.0, AudioCodec.Adpcm, 90892L)]
    [InlineData("../../../../Audio/ALAC_6ch.mov", 6, 0, 48000.0, AudioCodec.Alac, 4818053L)]
    [InlineData("../../../../Audio/ALAC_24bits2.mov", 2, 0, 48000.0, AudioCodec.Alac, 2150428L)]
    [InlineData("../../../../Audio/alac_encoding_failes.wav", 2, 16, 44100.0, AudioCodec.PcmIntLit, 1765376L)]
    [InlineData("../../../../Audio/ambient4_192_mulitchannel.wma", 6, 24, 48000.0, AudioCodec.WmaPro, 1539586L)]
    [InlineData("../../../../Audio/at3p_sample1.oma", 2, 0, 44100.0, AudioCodec.Atrac3, 605648L)]
    [InlineData("../../../../Audio/at3p_sample6.oma", 2, 0, 44100.0, AudioCodec.Atrac3, 260432L)]
    [InlineData("../../../../Audio/Ayumi Hamasaki - ever free.wv", 2, 16, 44100.0, AudioCodec.WavPack, 10403206L)]
    [InlineData("../../../../Audio/Bach1-1.aiff", 1, 8, 22255.0, AudioCodec.PcmIntBig, 257024L)]
    [InlineData("../../../../Audio/basicinstinct.ogm", 2, 0, 44100.0, AudioCodec.Vorbis, 4529696L)]
    [InlineData("../../../../Audio/Beethovens nionde symfoni (Scherzo)-2.wma", 2, 24, 48000.0, AudioCodec.WmaPro, 1200928L)]
    [InlineData("../../../../Audio/Boys3-1.aiff", 1, 8, 22255.0, AudioCodec.Mac3, 0L)]
    [InlineData("../../../../Audio/Boys6-1.aiff", 1, 8, 22255.0, AudioCodec.Mac6, 0L)]
    [InlineData("../../../../Audio/chrisrock-nosex.asf", 1, 16, 16000.0, AudioCodec.Wma1, 564868L)]
    [InlineData("../../../../Audio/classical_16_16_1_8000_off_0_off_1_29.wma", 1, 16, 16000.0, AudioCodec.WmaPro, 189705L)]
    [InlineData("../../../../Audio/Classical_96_24_6_256000_1_20.wma", 6, 24, 96000.0, AudioCodec.WmaPro, 6069726L)]
    [InlineData("../../../../Audio/cristinreel.mov", 2, 8, 44100.0, AudioCodec.Mac6, 3212222L)]
    [InlineData("../../../../Audio/DG.wmv", 2, 16, 48000.0, AudioCodec.Wma2, 10562512L)]
    [InlineData("../../../../Audio/dune2.avi", 2, 0, 48000.0, AudioCodec.MpegLayer3, 63320064L)]
    [InlineData("../../../../Audio/edward_4.0_24bit.wv", 4, 24, 48000.0, AudioCodec.WavPack, 32145718L)]
    [InlineData("../../../../Audio/eoa.wma", 2, 16, 44100.0, AudioCodec.Wma2, 923933L)]
    [InlineData("../../../../Audio/farm.wav", 1, 8, 11025.0, AudioCodec.PcmIntLit, 38912L)]
    [InlineData("../../../../Audio/ffvorbis_crash.ogm", 2, 0, 48000.0, AudioCodec.Vorbis, 69611L)]
    [InlineData("../../../../Audio/format-0x42-speechtest-MSG5.3.asf", 1, 0, 8000.0, AudioCodec.G_723_1, 7580L)]
    [InlineData("../../../../Audio/God Save The Queen.mlp", 6, 24, 96000.0, AudioCodec.Mlp, 0L)]
    [InlineData("../../../../Audio/H263_ALAC_24bits.mov", 2, 0, 48000.0, AudioCodec.Alac, 1132015L)]
    [InlineData("../../../../Audio/intro - Creative ADPCM.wav", 1, 4, 44100.0, AudioCodec.Adpcm, 2235116L)]
    [InlineData("../../../../Audio/jpg_in_mp3.mp3", 2, 0, 44100.0, AudioCodec.MpegLayer3, 4447654L)]
    [InlineData("../../../../Audio/large_superframe.wma", 2, 16, 44100.0, AudioCodec.Wma2, 6757355L)]
    [InlineData("../../../../Audio/lbd-ts.wav", 1, 1, 8000.0, AudioCodec.Truespeech, 95552L)]
    [InlineData("../../../../Audio/luckynight.ape", 2, 16, 44100.0, AudioCodec.Ape, 6510020L)]
    [InlineData("../../../../Audio/luckynight.flac", 2, 16, 44100.0, AudioCodec.Flac, 6659066L)]
    [InlineData("../../../../Audio/luckynight.m4a", 2, 16, 44100.0, AudioCodec.Alac, 6698970L)]
    [InlineData("../../../../Audio/luckynight.rka", 2, 16, 44100.0, AudioCodec.RkAudio, 6548495L)]
    [InlineData("../../../../Audio/luckynight.rmvb", 0, 0, 0.0, AudioCodec.Real10, 6572941L)]
    [InlineData("../../../../Audio/luckynight.tta", 2, 16, 44100.0, AudioCodec.Tta1, 6595089L)]
    [InlineData("../../../../Audio/luckynight.wma", 2, 16, 44100.0, AudioCodec.WmaLossless, 6567123L)]
    [InlineData("../../../../Audio/luckynight.wv", 2, 16, 44100.0, AudioCodec.WavPack, 6605816L)]
    [InlineData("../../../../Audio/luckynight-als.mp4", 2, 0, 44100.0, AudioCodec.Als, 6606757L)]
    [InlineData("../../../../Audio/Lumme-Badloop.ogg", 2, 0, 44100.0, AudioCodec.Vorbis, 6481032L)]
    [InlineData("../../../../Audio/mac3audio.mov", 2, 8, 22050.0, AudioCodec.Mac3, 630630L)]
    [InlineData("../../../../Audio/mjpega.mov", 1, 8, 8000.0, AudioCodec.Mac6, 17856L)]
    [InlineData("../../../../Audio/mp3_with_embedded_albumart.mp3", 2, 0, 44100.0, AudioCodec.MpegLayer3, 610740L)]
    [InlineData("../../../../Audio/mpeg-in-ogm.ogm", 2, 0, 48000.0, AudioCodec.Vorbis, 28014L)]
    [InlineData("../../../../Audio/mpeg_layer1_audio.mpg", 2, 0, 44100.0, AudioCodec.MpegLayer1, 880080L)]
    [InlineData("../../../../Audio/mplayer_sample-audio_0x161.wmv", 2, 16, 44100.0, AudioCodec.Wma2, 20270757L)]
    [InlineData("../../../../Audio/newedition-coolitnow.24bit-lpcm.vob", 2, 24, 48000.0, AudioCodec.PcmIntBig, 5798400L)]
    [InlineData("../../../../Audio/newOrleans_192_mulitchannel.wma", 6, 24, 48000.0, AudioCodec.WmaPro, 1408440L)]
    [InlineData("../../../../Audio/old_midi_stuff.m4a", 2, 16, 44100.0, AudioCodec.Alac, 12200326L)]
    [InlineData("../../../../Audio/panslab_sample_5.1_16bit.wv", 6, 16, 48000.0, AudioCodec.WavPack, 19797524L)]
    [InlineData("../../../../Audio/panslab_sample_5.1_16bit_lossy.wv", 6, 16, 48000.0, AudioCodec.WavPack, 7828154L)]
    [InlineData("../../../../Audio/panslab_sample_7.1_16bit_lossy.wv", 8, 16, 48000.0, AudioCodec.WavPack, 10972248L)]
    [InlineData("../../../../Audio/piece.wmv", 6, 24, 48000.0, AudioCodec.WmaPro, 608850971L)]
    [InlineData("../../../../Audio/qanda_2008_ep10.wmv", 2, 16, 44100.0, AudioCodec.Wma2, 27867086L)]
    [InlineData("../../../../Audio/quicktime-newcodec-applelosslessaudiocodec.m4a", 2, 16, 44100.0, AudioCodec.Alac, 46172332L)]
    [InlineData("../../../../Audio/raam28mb.wmv", 2, 16, 22050.0, AudioCodec.Wma2, 1988907L)]
    [InlineData("../../../../Audio/rum.wma", 2, 16, 32000.0, AudioCodec.Wma2, 245961L)]
    [InlineData("../../../../Audio/saltnpepa-doyouwantme.asf", 2, 16, 32000.0, AudioCodec.Wma2, 909710L)]
    [InlineData("../../../../Audio/sonateno14op27-2.aa3", 2, 0, 44100.0, AudioCodec.Atrac3, 1913504L)]
    [InlineData("../../../../Audio/streaming.wina.com-live.asx", 1, 16, 22050.0, AudioCodec.Wma2, 806911L)]
    [InlineData("../../../../Audio/streaming_CBR-19K-timecomp.wma", 1, 16, 16000.0, AudioCodec.WmaVoice, 29092L)]
    [InlineData("../../../../Audio/surge-1-8-MAC3.mov", 1, 8, 44100.0, AudioCodec.Mac3, 578298L)]
    [InlineData("../../../../Audio/surge-1-8-MAC3.wav", 1, 16, 44100.0, AudioCodec.PcmIntLit, 1156596L)]
    [InlineData("../../../../Audio/TAXI8BIT.wav", 1, 8, 11111.0, AudioCodec.PcmIntLit, 303756L)]
    [InlineData("../../../../Audio/test-96.wav", 2, 24, 96000.0, AudioCodec.PcmIntLit, 511956L)]
    [InlineData("../../../../Audio/testvector01.ogg", 2, 0, 44100.0, AudioCodec.Opus, 0L)]
    [InlineData("../../../../Audio/testvector07.ogg", 2, 0, 44100.0, AudioCodec.Opus, 0L)]
    [InlineData("../../../../Audio/tide_8-bit_22KHz.wav", 1, 8, 22050.0, AudioCodec.PcmIntLit, 1521278L)]
    [InlineData("../../../../Audio/tide_16-bit_44KHz_master.wav", 1, 16, 44100.0, AudioCodec.PcmIntLit, 6085112L)]
    [InlineData("../../../../Audio/toesidebackroll.AVI", 2, 2, 44100.0, AudioCodec.Iac2, 113152L)]
    [InlineData("../../../../Audio/traciespencer-allaboutyou.asf", 2, 16, 22050.0, AudioCodec.Wma2, 948508L)]
    [InlineData("../../../../Audio/TrueHD.raw", 6, 0, 48000.0, AudioCodec.Truehd, 0L)]
    [InlineData("../../../../Audio/truespeech_a5.wav", 1, 1, 8000.0, AudioCodec.Truespeech, 20320L)]
    [InlineData("../../../../Audio/vc1-with-truehd.m2ts", 6, 0, 48000.0, AudioCodec.Truehd, 414720L)]
    [InlineData("../../../../Audio/vorbis3plus_sample.avi", 2, 0, 48000.0, AudioCodec.Vorbis, 1576162L)]
    [InlineData("../../../../Audio/WMA_protected_napster.wma", 2, 16, 44100.0, AudioCodec.Wma2, 3349747L)]
    [InlineData("../../../../Audio/wmav_8.wma", 1, 16, 8000.0, AudioCodec.WmaVoice, 14439L)]
    [InlineData("../../../../Audio/wmv-surroundtest_720p.wmv", 6, 24, 48000.0, AudioCodec.WmaPro, 408000L)]
    [InlineData("../../../../Audio/wmv3-wmaspeeech.wmv", 1, 16, 8000.0, AudioCodec.WmaVoice, 6208L)]
    public void LoadAudioFile(string fileName, int channels, int bitDepth, double samplingRate, AudioCodec codec, long streamSize)
    {
        _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
        _mediaInfoWrapper.Success.Should().BeTrue("InfoWrapper should be loaded");
        _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
        var audio = _mediaInfoWrapper.AudioStreams[0];
        audio.Codec.Should().Be(codec);
        audio.Channel.Should().Be(channels);
        audio.BitDepth.Should().Be(bitDepth);
        audio.SamplingRate.Should().Be(samplingRate);
        audio.StreamSize.Should().Be(streamSize);
    }

#if DEBUG
    [Theory]
#else
    [Theory(Skip = "Test in development environment only")]
#endif
    [InlineData("../../../../HD Audio/7.1auditionOutLeader_v2_rtb.mp4", 8, 0, 48000.0, AudioCodec.AacMpeg4LcSbr, 0, 1, 887487L)]
    [InlineData("../../../../HD Audio/7_pt_1_sample.evo", 8, 0, 48000.0, AudioCodec.Eac3, 0, 1, 2314543L)]
    [InlineData("../../../../HD Audio/12-10_19-18-52_BBC HD_Wild China.ts", 6, 0, 48000.0, AudioCodec.AacMpeg4Lc, 0, 2, 0L)]
    [InlineData("../../../../HD Audio/ac3-sound-sample.vob", 6, 0, 48000.0, AudioCodec.Ac3, 0, 1, 1602048L)]
    [InlineData("../../../../HD Audio/bond_sample_dtshdma.m2ts", 6, 24, 48000.0, AudioCodec.DtsHdMa, 0, 7, 0L)]
    [InlineData("../../../../HD Audio/ChID-BLITS-EBU.mp4", 6, 0, 44100.0, AudioCodec.AacMpeg4LcSbr, 0, 1, 932513L)]
    [InlineData("../../../../HD Audio/ChID-BLITS-EBU-Narration.mp4", 6, 0, 44100.0, AudioCodec.AacMpeg4LcSbr, 0, 1, 930655L)]
    [InlineData("../../../../HD Audio/danish-1.m2t", 2, 0, 48000.0, AudioCodec.AacMpeg4LcSbr, 0, 1, 0L)]
    [InlineData("../../../../HD Audio/DNCE 29.97 h264 1080i 26mbps DTS-HD MA 2.0 sample.mkv", 2, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1, 70782196L)]
    [InlineData("../../../../HD Audio/dolby-audiosphere-lossless-(www.demolandia.net).m2ts", 8, 0, 48000.0, AudioCodec.TruehdAtmos, 0, 2, 4886640L)]
    [InlineData("../../../../HD Audio/dolby-silent-lossless-(www.demolandia.net).m2ts", 8, 0, 48000.0, AudioCodec.TruehdAtmos, 0, 2, 12723360L)]
    [InlineData("../../../../HD Audio/Dolby ATMOS Helicopter.m2ts", 8, 0, 48000.0, AudioCodec.TruehdAtmos, 0, 2, 4641040L)]
    [InlineData("../../../../HD Audio/dolby_amaze_lossless-DWEU.m2ts", 8, 0, 48000.0, AudioCodec.TruehdAtmos, 0, 2, 5080000L)]
    [InlineData("../../../../HD Audio/dolby_digital_plus_channel_check_lossless-DWEU.mkv", 6, 0, 48000.0, AudioCodec.Eac3, 0, 1, 19655168L)]
    [InlineData("../../../../HD Audio/dolby_truehd_channel_check_lossless-DWEU.mkv", 8, 0, 48000.0, AudioCodec.Truehd, 0, 1, 0L)]
    [InlineData("../../../../HD Audio/Dredd – DTS Sound Check DTS-HD MA 7.1.m2ts", 8, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1, 0L)]
    [InlineData("../../../../HD Audio/DTS-HRA5.1_VC1-23.976.mkv", 6, 24, 96000.0, AudioCodec.DtsHdHra, 0, 1, 16961160L)]
    [InlineData("../../../../HD Audio/dts-listen-x-long-lossless-(www.demolandia.net).mkv", 8, 0, 48000.0, AudioCodec.DtsX, 0, 1, 0L)]
    [InlineData("../../../../HD Audio/dts-sound-unbound-callout-11.1-lossless-(www.demolandia.net).mkv", 8, 0, 48000.0, AudioCodec.DtsX, 0, 1, 0L)]
    [InlineData("../../../../HD Audio/DTS-X Gravity.mkv", 8, 24, 48000.0, AudioCodec.DtsX, 0, 1, 67261000L)]
    [InlineData("../../../../HD Audio/dts_hd_master_audio_sound_check_7_1_lossless-DWEU.mkv", 6, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1, 0L)]
    [InlineData("../../../../HD Audio/dts_MA_all_around_us_lossless-DWEU.mkv", 8, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1, 0L)]
    [InlineData("../../../../HD Audio/dts_orchestra_short_lossless-DWEU.mkv", 6, 24, 96000.0, AudioCodec.DtsHdHra, 0, 1, 6128804L)]
    [InlineData("../../../../HD Audio/dtsac3audiosample.avi", 6, 16, 48000.0, AudioCodec.Dts, 0, 1, 299979392L)]
    [InlineData("../../../../HD Audio/DTSX_Demo_2016.m2ts", 0, 0, 0.0, AudioCodec.DtsX, 0, 1, 0L)]
    [InlineData("../../../../HD Audio/dvb-aac-latm.m2t", 2, 0, 48000.0, AudioCodec.AacMpeg4LcSbr, 0, 1, 0L)]
    [InlineData("../../../../HD Audio/freetv_aac_latm.ts", 1, 0, 24000.0, AudioCodec.AacMpeg4Lc, 0, 1, 0L)]
    [InlineData("../../../../HD Audio/hd_dts_hd_master_audio_sound_check_5_1_lossless.m2ts", 6, 24, 48000.0, AudioCodec.DtsHdMa, 0, 1, 0L)]
    [InlineData("../../../../HD Audio/lfe_is_sce.mp4", 6, 0, 44100.0, AudioCodec.AacMpeg4Lc, 0, 1, 43578L)]
    [InlineData("../../../../HD Audio/opus.ts", 0, 0, 0.0, AudioCodec.Opus, 0, 1, 0L)]
    [InlineData("../../../../HD Audio/POCAWE_Sample.mkv", 6, 24, 48000.0, AudioCodec.PcmIntLit, 0, 1, 51753600L)]
    [InlineData("../../../../HD Audio/Safari_ Dolby_Digital_Plus.m2ts", 8, 0, 48000.0, AudioCodec.Eac3, 0, 1, 19588608L)]
    [InlineData("../../../../HD Audio/SBR_LFEtest5_1.mp4", 6, 0, 44100.0, AudioCodec.AacMpeg4LcSbr, 0, 1, 903720L)]
    [InlineData("../../../../HD Audio/small-sample-file.ts", 6, 20, 48000.0, AudioCodec.Dts, 0, 2, 1620528L)]
    [InlineData("../../../../HD Audio/transf.m2ts", 6, 0, 48000.0, AudioCodec.Truehd, 0, 5, 896000L)]
    [InlineData("../../../../HD Audio/VC1-1080p23.976-LPCM7.1.mkv", 8, 16, 48000.0, AudioCodec.PcmIntLit, 0, 1, 45912000L)]
    [InlineData("../../../../HD Audio/zodiac_audio.mov", 6, 0, 48000.0, AudioCodec.AacMpeg4Lc, 0, 1, 3717917L)]
    [InlineData("../../../../HD Audio/zx.eva.renewal.01.divx511.mkv", 6, 0, 44100.0, AudioCodec.AacMpeg4LcSbr, 0, 2, 0L)]
    public void LoadHdAudioFromVideoContainer(
        string fileName,
        int channels,
        int bitDepth,
        double samplingRate,
        AudioCodec codec,
        int index,
        int audioCount,
        long streamSize)
    {
        _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
        _mediaInfoWrapper.Success.Should().BeTrue("InfoWrapper should be loaded");
        _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
        _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
        _mediaInfoWrapper.AudioStreams.Count.Should().Be(audioCount);
        _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
        var audio = _mediaInfoWrapper.AudioStreams[index];
        audio.Codec.Should().Be(codec);
        audio.Channel.Should().Be(channels);
        audio.BitDepth.Should().Be(bitDepth);
        audio.SamplingRate.Should().Be(samplingRate);
        audio.StreamSize.Should().Be(streamSize);
    }

    [Theory]
    [InlineData("./Data/Test_MP3Tags.mp3", 74406L)]
    [InlineData("./Data/Test_MP3Tags_2.mp3", 212274L)]
    public void LoadMp3FileWithTags(string fileName, long size)
    {
        _mediaInfoWrapper = new MediaInfoWrapper(fileName, _logger);
        _mediaInfoWrapper.Success.Should().BeTrue("InfoWrapper should be loaded");
        _mediaInfoWrapper.Size.Should().Be(size);
        _mediaInfoWrapper.HasVideo.Should().BeFalse("Video stream does not supported in MP3!");
        _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
        _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
        _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is missing");
        _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
        _mediaInfoWrapper.AudioStreams.Count.Should().Be(1);
        _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
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
        _mediaInfoWrapper.Success.Should().BeTrue("InfoWrapper should be loaded");
        _mediaInfoWrapper.Size.Should().Be(135172L);
        _mediaInfoWrapper.HasVideo.Should().BeFalse("Video stream does not supported in MKA!");
        _mediaInfoWrapper.IsBluRay.Should().BeFalse("Is not BluRay disk");
        _mediaInfoWrapper.IsDvd.Should().BeFalse("Is not DVD disk");
        _mediaInfoWrapper.IsInterlaced.Should().BeFalse("Video stream is missing");
        _mediaInfoWrapper.Is3D.Should().BeFalse("Video stream is not 3D");
        _mediaInfoWrapper.AudioStreams.Count.Should().Be(2);
        _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
        // MKA file contains all tags in general stream
        _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeNull();
        _mediaInfoWrapper.Tags.GeneralTags.Should().NotBeEmpty();
        _mediaInfoWrapper.Tags.EncodedDate.Should().NotBeNull();
        _mediaInfoWrapper.Text.Should().NotBeNullOrEmpty();
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