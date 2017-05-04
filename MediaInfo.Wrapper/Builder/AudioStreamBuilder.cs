#region Copyright (C) 2005-2017 Team MediaPortal

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

using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Describes method to build audio stream.
  /// </summary>
  internal class AudioStreamBuilder : LanguageMediaStreamBuilder<AudioStream>
  {
    #region matching dictionaries

    private static readonly Dictionary<string, AudioCodec> CodecIds = new Dictionary<string, AudioCodec>(StringComparer.OrdinalIgnoreCase)
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
    };

    private static readonly Dictionary<string, AudioCodec> Codecs = new Dictionary<string, AudioCodec>(StringComparer.OrdinalIgnoreCase)
    {
      { "MPA1L1", AudioCodec.MpegLayer1 },
      { "MPA1L2", AudioCodec.MpegLayer2 },
      { "MPA1L3", AudioCodec.MpegLayer3 },
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
      { "162", AudioCodec.Wma9 },
    };

    #endregion

    public AudioStreamBuilder(MediaInfo info, int number, int position)
      : base(info, number, position)
    {
    }

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Audio;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Audio;

    public override AudioStream Build()
    {
      var result = base.Build();
      var baseIndex = 0;
      result.Codec = Get<AudioCodec>("CodecID", TryGetCodecByCodecId);
      if (result.Codec == AudioCodec.Undefined)
      {
        var codecValue = Get("Codec");
        if (codecValue.Equals("PCM", StringComparison.OrdinalIgnoreCase))
        {
          var endianness = Get("Codec_Settings_Endianness");
          codecValue = $"{codecValue}{(string.IsNullOrEmpty(endianness) ? string.Empty : " " + endianness)}";
        }

        result.Codec = GetCodecIdByCodecName(codecValue);
        // Correction for Atmos audio
        switch (result.Codec)
        {
          case AudioCodec.DtsHd:
            baseIndex = 1;
            break;
          case AudioCodec.Ac3:
          case AudioCodec.Ac3Bsid10:
          case AudioCodec.Ac3Bsid9:
          case AudioCodec.Eac3:
          case AudioCodec.Truehd:
            var formatProfile = GetCodecIdByCodecName(Get("Format_Profile").Split('/')[0].Trim());
            if (formatProfile != AudioCodec.Undefined)
            {
              result.Codec = formatProfile;
              baseIndex = 1;
            }

            break;
        }
      }

      result.Duration = TimeSpan.FromMilliseconds(Get<double>("Duration", double.TryParse, x => ExtractInfo(x, 0)));
      result.Bitrate = Get<double>("BitRate", double.TryParse, x => ExtractInfo(x, baseIndex));
      result.Channel = Get<int>("Channel(s)", int.TryParse, x => ExtractInfo(x, baseIndex));
      result.SamplingRate = Get<double>("SamplingRate", double.TryParse, x => ExtractInfo(x, baseIndex));
      result.BitDepth = Get<int>("BitDepth", int.TryParse, x => ExtractInfo(x, baseIndex));
      result.Format = Get("Format", x => ExtractInfo(x, 0));
      result.CodecName = GetFullCodecName();

      return result;
    }

    private static string ExtractInfo(string source, int index)
    {
      return source.IndexOf("/", StringComparison.Ordinal) >= 0 ?
               source.Split('/').Skip(index).FirstOrDefault()?.Trim() :
               source;
    }

    private string GetFullCodecName()
    {
      var strCodec = Get("Format").ToUpper();
      var strCodecVer = Get("Format_Version").ToUpper();
      if (strCodec == "MPEG-4 VISUAL")
      {
        strCodec = Get("CodecID").ToUpperInvariant();
      }
      else
      {
        if (!string.IsNullOrEmpty(strCodecVer))
        {
          strCodec = (strCodec + " " + strCodecVer).Trim();
        }
      }

      var formatName = Get("Format_Profile").Split('/')[0].Trim();
      if (formatName.IndexOf("ATMOS", StringComparison.OrdinalIgnoreCase) >= 0)
      {
        return formatName;
      }

      strCodec = (strCodec + " " + formatName).Trim();
      return strCodec.Replace("+", "PLUS");
    }

    private static bool TryGetCodecByCodecId(string source, out AudioCodec result)
    {
      return CodecIds.TryGetValue(source, out result);
    }

    private static AudioCodec GetCodecIdByCodecName(string source)
    {
      AudioCodec result;
      return Codecs.TryGetValue(source, out result) ? result : AudioCodec.Undefined;
    }
  }
}