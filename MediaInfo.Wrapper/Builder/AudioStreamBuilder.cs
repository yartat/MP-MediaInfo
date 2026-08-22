#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using MediaInfo.Model;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Builds an audio stream representation from the provided media information and stream identifiers.
  /// </summary>
  /// <remarks>This builder is intended for internal use when parsing and constructing audio stream
  /// metadata. It supports a wide range of audio codecs and profiles, mapping them to standardized codec identifiers.
  /// The resulting audio stream object contains detailed properties such as codec, bitrate, channel count, sampling
  /// rate, and additional tags relevant to the audio stream.
  /// </remarks>
  /// <param name="info">The media information source used to extract audio stream metadata. Cannot be null.</param>
  /// <param name="number">The zero-based index of the audio stream within the media container.</param>
  /// <param name="position">The position of the stream in the underlying media information structure.</param>
  internal class AudioStreamBuilder(MediaInfo info, int number, int position) : LanguageMediaStreamBuilder<AudioStream>(info, number, position)
  {
    #region matching dictionaries

    private static readonly Dictionary<string, AudioCodec> CodecIds = new(StringComparer.OrdinalIgnoreCase)
    {
      { "A_MPEG/L1", AudioCodec.MpegLayer1 },
      { "A_MPEG/L2", AudioCodec.MpegLayer2 },
      { "A_MPEG/L3", AudioCodec.MpegLayer3 },
      { "A_PCM/INT/BIG", AudioCodec.PcmIntBig },
      { "A_PCM/INT/LIT", AudioCodec.PcmIntLit },
      { "A_PCM/FLOAT/IEEE", AudioCodec.PcmFloatIeee },
      { "A_AC3", AudioCodec.Ac3 },
      { "A_AC3/BSID9", AudioCodec.Ac3Bsid9 },
      { "A_AC3/BSID10", AudioCodec.Ac3Bsid10 },
      { "A_DTS", AudioCodec.Dts },
      { "A_DTS-HD", AudioCodec.DtsHd },
      { "A_EAC3", AudioCodec.Eac3 },
      { "A_FLAC", AudioCodec.Flac },
      { "A_OPUS", AudioCodec.Opus },
      { "A_TTA1", AudioCodec.Tta1 },
      { "A_VORBIS", AudioCodec.Vorbis },
      { "A_WAVPACK4", AudioCodec.WavPack4 },
      { "A_WAVPACK", AudioCodec.WavPack },
      { "A_REAL/14_4", AudioCodec.Real14_4 },
      { "A_REAL/28_8", AudioCodec.Real28_8 },
      { "A_REAL/COOK", AudioCodec.RealCook },
      { "A_REAL/SIPR", AudioCodec.RealSipr },
      { "A_REAL/RALF", AudioCodec.RealRalf },
      { "A_REAL/ATRC", AudioCodec.RealAtrc },
      { "A_TRUEHD", AudioCodec.Truehd },
      { "A_MLP", AudioCodec.Mlp },
      { "A_AAC", AudioCodec.Aac },
      { "A_AAC/MPEG2/MAIN", AudioCodec.AacMpeg2Main },
      { "A_AAC/MPEG2/LC", AudioCodec.AacMpeg2Lc },
      { "A_AAC/MPEG2/LC/SBR", AudioCodec.AacMpeg2LcSbr },
      { "A_AAC/MPEG2/SSR", AudioCodec.AacMpeg2Ssr },
      { "A_AAC/MPEG4/MAIN", AudioCodec.AacMpeg4Main },
      { "A_AAC/MPEG4/LC", AudioCodec.AacMpeg4Lc },
      { "A_AAC/MPEG4/LC/SBR", AudioCodec.AacMpeg4LcSbr },
      { "A_AAC/MPEG4/LC/SBR/PS", AudioCodec.AacMpeg4LcSbrPs },
      { "A_AAC/MPEG4/SSR", AudioCodec.AacMpeg4Ssr },
      { "A_AAC/MPEG4/LTP", AudioCodec.AacMpeg4Ltp },
      { "A_ALAC", AudioCodec.Alac },
      { "A_APE", AudioCodec.Ape },
      { "SAMR", AudioCodec.Amr },
      { "160", AudioCodec.Wma1 },
      { "161", AudioCodec.Wma2 },
      //{ "162", AudioCodec.WmaPro },
      //{ "163", AudioCodec.WmaLossless },
      { "MAC3", AudioCodec.Mac3 },
      { "MAC6", AudioCodec.Mac6 },
    };

    private static readonly Dictionary<string, AudioCodec> Codecs = new(StringComparer.OrdinalIgnoreCase)
    {
      { "MPA1L1", AudioCodec.MpegLayer1 },
      { "MPA1L2", AudioCodec.MpegLayer2 },
      { "MPA1L3", AudioCodec.MpegLayer3 },
      { "MPEG Audio", AudioCodec.MpegLayer3 },
      { "PCM BIG", AudioCodec.PcmIntBig },
      { "PCM LITTLE", AudioCodec.PcmIntLit },
      { "PCM", AudioCodec.PcmIntLit },
      { "PCM/FLOAT/IEEE", AudioCodec.PcmFloatIeee },
      { "AC3", AudioCodec.Ac3 },
      { "AC-3", AudioCodec.Ac3 },
      { "AC3/BSID9", AudioCodec.Ac3Bsid9 },
      { "AC3/BSID10", AudioCodec.Ac3Bsid10 },
      { "DTS", AudioCodec.Dts },
      { "DTS-HD", AudioCodec.DtsHd },
      { "EAC3", AudioCodec.Eac3 },
      { "E-AC-3+ATMOS", AudioCodec.Eac3Atmos },
      { "EAC-3", AudioCodec.Eac3 },
      { "E-AC-3", AudioCodec.Eac3 },
      { "AC-3+ATMOS", AudioCodec.Ac3Atmos },
      { "AC3+", AudioCodec.Eac3 },
      { "FLAC", AudioCodec.Flac },
      { "OPUS", AudioCodec.Opus },
      { "TTA1", AudioCodec.Tta1 },
      { "TTA", AudioCodec.Tta1 },
      { "VORBIS", AudioCodec.Vorbis },
      { "WAVPACK4", AudioCodec.WavPack4 },
      { "WAVPACK", AudioCodec.WavPack },
      { "WAVE", AudioCodec.Wave },
      { "WAVE64", AudioCodec.Wave64 },
      { "REAL/14_4", AudioCodec.Real14_4 },
      { "REAL/28_8", AudioCodec.Real28_8 },
      { "REAL/COOK", AudioCodec.RealCook },
      { "REAL/SIPR", AudioCodec.RealSipr },
      { "REAL/RALF", AudioCodec.RealRalf },
      { "REAL/ATRC", AudioCodec.RealAtrc },
      { "TRUEHD", AudioCodec.Truehd },
      { "TRUEHD / AC3", AudioCodec.Truehd },
      { "TRUEHD+ATMOS / TRUEHD", AudioCodec.TruehdAtmos },
      { "TRUEHD+ATMOS", AudioCodec.TruehdAtmos },
      { "MLP", AudioCodec.Mlp },
      { "AAC", AudioCodec.Aac },
      { "AAC LC", AudioCodec.AacMpeg4Lc },
      { "AAC LTP", AudioCodec.AacMpeg4Ltp },
      { "AAC MAIN", AudioCodec.AacMpeg4Main },
      { "AAC SSR", AudioCodec.AacMpeg4Ssr },
      { "AAC/MPEG2/MAIN", AudioCodec.AacMpeg2Main },
      { "AAC/MPEG2/LC", AudioCodec.AacMpeg2Lc },
      { "AAC/MPEG2/LC/SBR", AudioCodec.AacMpeg2LcSbr },
      { "AAC/MPEG2/SSR", AudioCodec.AacMpeg2Ssr },
      { "AAC/MPEG4/MAIN", AudioCodec.AacMpeg4Main },
      { "AAC/MPEG4/LC", AudioCodec.AacMpeg4Lc },
      { "AAC/MPEG4/LC/SBR", AudioCodec.AacMpeg4LcSbr },
      { "AAC/MPEG4/LC/SBR/PS", AudioCodec.AacMpeg4LcSbrPs },
      { "AAC/MPEG4/SSR", AudioCodec.AacMpeg4Ssr },
      { "AAC/MPEG4/LTP", AudioCodec.AacMpeg4Ltp },
      { "ALAC", AudioCodec.Alac },
      { "APE", AudioCodec.Ape },
      { "11", AudioCodec.Adpcm },
      { "AMR", AudioCodec.Amr },
      { "160", AudioCodec.Wma1 },
      { "161", AudioCodec.Wma2 },
      { "WMAPRO", AudioCodec.WmaPro },
      { "WMAVOICE", AudioCodec.WmaVoice },
      { "WMALossless", AudioCodec.WmaLossless },
      { "WMA3", AudioCodec.Wma3 },
      { "DSD", AudioCodec.Dsd },
      { "Atrac3", AudioCodec.Atrac3 },
      { "Atrac1", AudioCodec.Atrac1 },
      { "Atrac9", AudioCodec.Atrac9 },
      { "ADPCM", AudioCodec.Adpcm },
      { "G.723.1", AudioCodec.G_723_1 },
      { "Truespeech", AudioCodec.Truespeech },
      { "Monkey's Audio", AudioCodec.Ape },
      { "RK Audio", AudioCodec.RkAudio },
      { "RealAudio Lossless", AudioCodec.Real10 },
      { "ALS", AudioCodec.Als },
      { "IAC2", AudioCodec.Iac2 },
      { "MLP FBA", AudioCodec.Truehd },
      { "MPEG-H 3D Audio", AudioCodec.Mpeg3DAudio },
      { "USAC", AudioCodec.Usac },
      { "QDesign 1", AudioCodec.QDesignMusic1 },
      { "QDesign 2", AudioCodec.QDesignMusic2 },
      { "QCELP", AudioCodec.QualcommPureVoice },
      { "Ac4", AudioCodec.Ac4 },
      { "Ac-4", AudioCodec.Ac4 },
      { "APAC", AudioCodec.Apac },
      { "APT-X100", AudioCodec.Aptx100 },
      { "Auro-Cx", AudioCodec.AuroCx },
      { "Dolby ED2", AudioCodec.DolbyEd2 },
      { "Dolby E", AudioCodec.DolbyE },
      { "Dolby E-8", AudioCodec.DolbyE },
      { "DTS-UHD", AudioCodec.DtsUhd },
      { "Nellymoser", AudioCodec.Nellymoser },
      { "EVRC", AudioCodec.Evrc },
      { "IAMF", AudioCodec.Iamf },
      { "WMA Pro", AudioCodec.WmaPro },
    };

    private static readonly Dictionary<string, AudioCodec> MlpCodecsAdditionalFeatures = new(StringComparer.OrdinalIgnoreCase)
    {
      { "MLP FBA 16-ch", AudioCodec.TruehdAtmos },
      { "MLP FBA AC-3 16-ch", AudioCodec.TruehdAtmos },
      { "FBA 16-ch", AudioCodec.TruehdAtmos },
      { "FBA AC-3 16-ch", AudioCodec.TruehdAtmos },
      { "16-ch", AudioCodec.TruehdAtmos },
      { "AC-3 16-ch", AudioCodec.TruehdAtmos },
      { "MLP 16-ch", AudioCodec.TruehdAtmos },
      { "MLP AC-3 16-ch", AudioCodec.TruehdAtmos },
      { "Dep JOC", AudioCodec.Eac3Atmos },
      { "JOC", AudioCodec.Eac3Atmos },
      { "Dep", AudioCodec.Eac3 },
      { "XLL", AudioCodec.DtsHdMa },
      { "ES XXCH XLL", AudioCodec.DtsHdMa },
      { "ES XLL", AudioCodec.DtsHdMa },
      { "ES XXCH XLL X", AudioCodec.DtsX },
      { "XXCH XLL X", AudioCodec.DtsX },
      { "XLL X", AudioCodec.DtsX },
      { "ES XLL X", AudioCodec.DtsX },
      { "XLL X IMAX", AudioCodec.DtsHdMaImax },
      { "XBR", AudioCodec.DtsHdHra },
      { "ES XXCH XBR", AudioCodec.DtsHdHra },
      { "ES XCh XBR", AudioCodec.DtsHdHra },
      { "ES XCh XLL", AudioCodec.DtsHdMa },
      { "ES XBR", AudioCodec.DtsHdHra },
      { "ES XCh", AudioCodec.DtsEs },
      { "XXCH XBR", AudioCodec.DtsHdHra },
      { "XXCh", AudioCodec.DtsHd },
      { "XCh", AudioCodec.DtsEs },
      { "ES XXCH", AudioCodec.DtsEs },
      { "96/24", AudioCodec.DtsHdHra },
      { "x96", AudioCodec.DtsHdHra },
      { "X96 XXCH", AudioCodec.DtsHdHra },
      { "DTS-UHD", AudioCodec.DtsUhd },
      { "X", AudioCodec.DtsX },
      { "IMAX", AudioCodec.DtsXImax },
      { "LC", AudioCodec.AacMpeg4Lc },
      { "LC SBR", AudioCodec.AacMpeg4LcSbr },
      { "LC-SBR", AudioCodec.AacMpeg4LcSbr },
      { "LC SBR PS", AudioCodec.AacMpeg4LcSbrPs },
      { "LC-SBR PS", AudioCodec.AacMpeg4LcSbrPs },
      { "LC-SBR-PS", AudioCodec.AacMpeg4LcSbrPs },
      { "LTP", AudioCodec.AacMpeg4Ltp },
      { "SSR", AudioCodec.AacMpeg4Ssr },
      { "Express", AudioCodec.DtsExpress },
      { "ES Discrete", AudioCodec.DtsEs },
      { "ES Matrix", AudioCodec.DtsEs },
      { "HRA", AudioCodec.DtsHdHra },
    };

        #endregion

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Audio;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Audio;

    /// <inheritdoc />
    public override AudioStream Build()
    {
      var result = base.Build();
      var baseIndex = 0;
      result.Codec = GetCodecFromStream();

      switch (result.Codec)
      {
        case AudioCodec.MpegLayer3:
          ExtractMpegLayer3Audio(result);
          break;
        case AudioCodec.Dts:
          baseIndex = ExtractDtsAudio(result, baseIndex);
          break;
        case AudioCodec.Aac:
          baseIndex = ExtractAacAudio(result, baseIndex);
          break;
        case AudioCodec.DtsHd:
          baseIndex = 1;
          break;
        // Correction for Atmos audio
        case AudioCodec.Ac3:
        case AudioCodec.Ac3Bsid10:
        case AudioCodec.Ac3Bsid9:
        case AudioCodec.Eac3:
        case AudioCodec.Truehd:
          baseIndex = ExtractDolbyAudio(result, baseIndex);
          break;
      }

      result.Duration = TimeSpan.FromMilliseconds(Get<double>((int)NativeMethods.Audio.Audio_Duration, InfoKind.Text, TagBuilderHelper.TryGetDouble, x => ExtractInfo(x, 0)));
      result.FrameRate = Get<double>((int)NativeMethods.Audio.Audio_FrameRate, InfoKind.Text, TagBuilderHelper.TryGetDouble, x => ExtractInfo(x, baseIndex));
      result.FrameRateNumerator = Get<int>((int)NativeMethods.Audio.Audio_FrameRate_Num, InfoKind.Text, TagBuilderHelper.TryGetInt);
      result.FrameRateDenominator = Get<int>((int)NativeMethods.Audio.Audio_FrameRate_Den, InfoKind.Text, TagBuilderHelper.TryGetInt);
      result.TimeCodeFirstFrame = Get((int)NativeMethods.Audio.Audio_TimeCode_FirstFrame, InfoKind.Text);
      result.TimeCodeLastFrame = Get((int)NativeMethods.Audio.Audio_TimeCode_LastFrame, InfoKind.Text);
      result.TimeCodeDropFrame = Get<bool>((int)NativeMethods.Audio.Audio_TimeCode_DropFrame, InfoKind.Text, TagBuilderHelper.TryGetBool);
      result.TimeCodeSettings = Get((int)NativeMethods.Audio.Audio_TimeCode_Settings, InfoKind.Text);
      result.TimeCodeSource = Get((int)NativeMethods.Audio.Audio_TimeCode_Source, InfoKind.Text);
      result.Bitrate = Get<double>((int)NativeMethods.Audio.Audio_BitRate, InfoKind.Text, TagBuilderHelper.TryGetDouble, x => ExtractInfo(x, baseIndex));
      result.Channel = Get<int>((int)NativeMethods.Audio.Audio_Channel_s_, InfoKind.Text, TagBuilderHelper.TryGetInt, x => ExtractInfo(x, baseIndex));
      result.SamplingRate = Get<double>((int)NativeMethods.Audio.Audio_SamplingRate, InfoKind.Text, TagBuilderHelper.TryGetDouble, x => ExtractInfo(x, baseIndex));
      result.BitDepth = Get<int>((int)NativeMethods.Audio.Audio_BitDepth, InfoKind.Text, TagBuilderHelper.TryGetInt, x => ExtractInfo(x, baseIndex));
      result.BitrateMode = Get<BitrateMode>((int)NativeMethods.Audio.Audio_BitRate_Mode, InfoKind.Text, TagBuilderHelper.TryGetBitrateMode, x => ExtractInfo(x, baseIndex));
      switch (result.Codec)
      {
        case AudioCodec.Dsd:
          result.BitDepth = 1;
          break;
        case AudioCodec.Ac4:
          if (result.Channel == 0)
          {
            result.Channel = 2;
          }
          break;
      }

      result.Format = Get((int)NativeMethods.Audio.Audio_Format, InfoKind.Text, x => ExtractInfo(x, 0));
      result.CodecName = Get((int)NativeMethods.Audio.Audio_Format, InfoKind.Text).ToUpper();
      result.CodecDescription = Get((int)NativeMethods.Audio.Audio_Format_Commercial, InfoKind.Text);
      result.Tags = new AudioTagBuilder(Info, StreamPosition).Build();

      return result;
    }

    private AudioCodec GetCodecFromStream()
    {
      var result = Get<AudioCodec>((int)NativeMethods.Audio.Audio_CodecID, InfoKind.Text, TryGetCodecByCodecId);
      if (result == AudioCodec.Undefined)
      {
        var codecValue = Get((int)NativeMethods.Audio.Audio_Format, InfoKind.Text);
        if (codecValue.Equals("PCM", StringComparison.OrdinalIgnoreCase))
        {
          var endianness = Get((int)NativeMethods.Audio.Audio_Format_Settings_Endianness, InfoKind.Text);
          codecValue = $"{codecValue}{(string.IsNullOrEmpty(endianness) ? string.Empty : " " + endianness)}";
        }
        if (codecValue.Equals("WMA", StringComparison.OrdinalIgnoreCase))
        {
          var profile = Get((int)NativeMethods.Audio.Audio_Format_Profile, InfoKind.Text);
          codecValue = $"{codecValue}{(string.IsNullOrEmpty(profile) ? string.Empty : profile)}";
        }

        result = GetCodecIdByCodecName(codecValue);
      }

      return result;
    }

    private int ExtractDtsAudio(AudioStream result, int baseIndex)
    {
      var formatProfile = GetCodecIdByCodecName(Get((int)NativeMethods.Audio.Audio_Format_Profile, InfoKind.Text).Split('/')[0].Trim());
      if (formatProfile != AudioCodec.Undefined)
      {
        result.Codec = formatProfile;
        baseIndex = 1;
      }
      else
      {
        formatProfile = GetMlpCodecIdByAdditionalFeatures(Get((int)NativeMethods.Audio.Audio_Format_AdditionalFeatures, InfoKind.Text).Trim());
        if (formatProfile != AudioCodec.Undefined)
        {
          result.Codec = formatProfile;
          baseIndex = 1;
        }
        else
        {
          formatProfile = GetMlpCodecIdByAdditionalFeatures(Get((int)NativeMethods.Audio.Audio_Format_String, InfoKind.Text).Trim());
          if (formatProfile != AudioCodec.Undefined)
          {
            result.Codec = formatProfile;
            baseIndex = 1;
          }
        }
      }

      return baseIndex;
    }

    private void ExtractMpegLayer3Audio(AudioStream result)
    {
      var formatProfile = Get((int)NativeMethods.Audio.Audio_Format_Profile, InfoKind.Text)?.Trim();
      switch (formatProfile?.ToLower())
      {
        case "layer 2":
          result.Codec = AudioCodec.MpegLayer2;
          break;
        case "layer 1":
          result.Codec = AudioCodec.MpegLayer1;
          break;
      }
    }

    private int ExtractAacAudio(AudioStream result, int baseIndex)
    {
      var formatProfile = GetCodecIdByCodecName(Get((int)NativeMethods.Audio.Audio_Format_Profile, InfoKind.Text).Split('/')[0].Trim());
      if (formatProfile != AudioCodec.Undefined)
      {
        result.Codec = formatProfile;
        baseIndex = 1;
      }
      else
      {
        formatProfile = GetMlpCodecIdByAdditionalFeatures(Get((int)NativeMethods.Audio.Audio_Format_AdditionalFeatures, InfoKind.Text).Trim());
        if (formatProfile != AudioCodec.Undefined)
        {
            result.Codec = formatProfile;
            baseIndex = 1;
        }
        else
        {
          formatProfile = GetMlpCodecIdByAdditionalFeatures(Get((int)NativeMethods.Audio.Audio_Format_String, InfoKind.Text).Trim());
          if (formatProfile != AudioCodec.Undefined)
          {
            result.Codec = formatProfile;
            baseIndex = 1;
          }
        }
      }

      return baseIndex;
    }

    private int ExtractDolbyAudio(AudioStream result, int baseIndex)
    {
      var formatProfile = GetCodecIdByCodecName(Get((int)NativeMethods.Audio.Audio_Format_Profile, InfoKind.Text).Split('/')[0].Trim());
      if (formatProfile != AudioCodec.Undefined)
      {
        result.Codec = formatProfile;
        baseIndex = 1;
      }
      else
      {
        formatProfile = GetMlpCodecIdByAdditionalFeatures(Get((int)NativeMethods.Audio.Audio_Format_AdditionalFeatures, InfoKind.Text).Trim());
        if (formatProfile != AudioCodec.Undefined)
        {
          result.Codec = formatProfile;
          baseIndex = 1;
        }
        else
        {
          formatProfile = GetMlpCodecIdByAdditionalFeatures(Get((int)NativeMethods.Audio.Audio_Format_String, InfoKind.Text).Trim());
          if (formatProfile != AudioCodec.Undefined)
          {
            result.Codec = formatProfile;
            baseIndex = 1;
          }
        }
      }

      return baseIndex;
    }

    private static string? ExtractInfo(string source, int index) =>
      source.IndexOf("/", StringComparison.Ordinal) >= 0 ?
        source.Split('/').Skip(index).FirstOrDefault()?.Trim() :
        source;

    private static bool TryGetCodecByCodecId(string source, out AudioCodec result) =>
      CodecIds.TryGetValue(source, out result);

    private static AudioCodec GetCodecIdByCodecName(string source) =>
      Codecs.TryGetValue(source, out var result) ? result : AudioCodec.Undefined;

    private static AudioCodec GetMlpCodecIdByAdditionalFeatures(string source) =>
      MlpCodecsAdditionalFeatures.TryGetValue(source, out var result) ? result : AudioCodec.Undefined;
  }
}