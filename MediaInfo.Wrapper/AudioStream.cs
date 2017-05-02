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

using JetBrains.Annotations;

namespace MediaInfo
{
  #region Audio codec

  /// <summary>
  /// Defines constants for different kind of audio codecs.
  /// </summary>
  public enum AudioCodec
  {
    /// <summary>
    /// The undefined audio codec
    /// </summary>
    Undefined,

    /// <summary>
    /// MPEG Layer 1
    /// </summary>
    MpegLayer1,

    /// <summary>
    /// MPEG Layer 2
    /// </summary>
    MpegLayer2,

    /// <summary>
    /// MPEG Layer 3
    /// </summary>
    MpegLayer3,

    /// <summary>
    /// PCM big-endian int 
    /// </summary>
    PcmIntBig,

    /// <summary>
    /// PCM little-endian int 
    /// </summary>
    PcmIntLit,

    /// <summary>
    /// PCM float IEEE
    /// </summary>
    PcmFloatIeee,

    /// <summary>
    /// Dolby Digital
    /// </summary>
    Ac3,

    /// <summary>
    /// Dolby Digital Atmos
    /// </summary>
    Ac3Atmos,

    /// <summary>
    /// DolbyNet
    /// </summary>
    Ac3Bsid9,

    /// <summary>
    /// DolbyNet
    /// </summary>
    Ac3Bsid10,

    /// <summary>
    /// Dolby Digital Plus
    /// </summary>
    Eac3,

    /// <summary>
    /// Dolby Digital Plus Atmos
    /// </summary>
    Eac3Atmos,

    /// <summary>
    /// Dolby TrueHD
    /// </summary>
    Truehd,

    /// <summary>
    /// Dolby TrueHD Atmos
    /// </summary>
    TruehdAtmos,

    /// <summary>
    /// DTS
    /// </summary>
    Dts,

    /// <summary>
    /// DTS-HD
    /// </summary>
    DtsHd,

    /// <summary>
    /// Free Lossless Audio Codec
    /// </summary>
    Flac,

    /// <summary>
    /// OPUS
    /// </summary>
    Opus,

    /// <summary>
    /// True Audio
    /// </summary>
    Tta1,

    /// <summary>
    /// VORBIS
    /// </summary>
    Vorbis,

    /// <summary>
    /// WavPack v4
    /// </summary>
    WavPack4,

    /// <summary>
    /// WavPack
    /// </summary>
    WavPack,

    /// <summary>
    /// Waveform Audio
    /// </summary>
    Wave,

    /// <summary>
    /// Waveform Audio
    /// </summary>
    Wave64,

    /// <summary>
    /// The Real Audio
    /// </summary>
    // ReSharper disable once InconsistentNaming
    Real14_4,

    /// <summary>
    /// The Real Audio
    /// </summary>
    // ReSharper disable once InconsistentNaming
    Real28_8,

    /// <summary>
    /// The Real Audio
    /// </summary>
    RealCook,

    /// <summary>
    /// The Real Audio
    /// </summary>
    RealSipr,

    /// <summary>
    /// The Real Audio
    /// </summary>
    RealRalf,

    /// <summary>
    /// The Real Audio
    /// </summary>
    RealAtrc,

    /// <summary>
    /// Meridian Lossless
    /// </summary>
    Mlp,

    /// <summary>
    /// Advanced Audio Coding
    /// </summary>
    Aac,

    /// <summary>
    /// Advanced Audio Coding
    /// </summary>
    AacMpeg2Main,

    /// <summary>
    /// Advanced Audio Coding
    /// </summary>
    AacMpeg2Lc,

    /// <summary>
    /// Advanced Audio Coding
    /// </summary>
    AacMpeg2LcSbr,

    /// <summary>
    /// Advanced Audio Coding
    /// </summary>
    AacMpeg2Ssr,

    /// <summary>
    /// Advanced Audio Coding
    /// </summary>
    AacMpeg4Main,

    /// <summary>
    /// Advanced Audio Coding
    /// </summary>
    AacMpeg4Lc,

    /// <summary>
    /// Advanced Audio Coding
    /// </summary>
    AacMpeg4LcSbr,

    /// <summary>
    /// Advanced Audio Coding
    /// </summary>
    AacMpeg4LcSbrPs,

    /// <summary>
    /// Advanced Audio Coding
    /// </summary>
    AacMpeg4Ssr,

    /// <summary>
    /// Advanced Audio Coding
    /// </summary>
    AacMpeg4Ltp,

    /// <summary>
    /// Apple Lossless
    /// </summary>
    Alac,

    /// <summary>
    /// Monkey's Audio
    /// </summary>
    Ape,

    /// <summary>
    /// Windows Media Audio
    /// </summary>
    Wma1,

    /// <summary>
    /// Windows Media Audio v2
    /// </summary>
    Wma2,

    /// <summary>
    /// Windows Media Audio 9
    /// </summary>
    Wma9,

    /// <summary>
    /// Adaptive differential pulse-code modulation
    /// </summary>
    Adpcm,

    /// <summary>
    /// Adaptive multi rate
    /// </summary>
    Amr,

    /// <summary>
    /// Adaptive Transform Acoustic Coding (SDDS)
    /// </summary>
    Atrac1,

    /// <summary>
    /// Adaptive Transform Acoustic Coding 3
    /// </summary>
    Atrac3,

    /// <summary>
    /// ATRAC3plus
    /// </summary>
    Atrac3Plus,

    /// <summary>
    /// ATRAC Advanced Lossless
    /// </summary>
    AtracLossless,

    /// <summary>
    /// ATRAC9
    /// </summary>
    Atrac9
  }

  #endregion

  /// <summary>
  /// Provides properties and overridden methods for the analyze audio stream 
  /// and contains information about audio stream.
  /// </summary>
  /// <seealso cref="LanguageMediaStream" />
  public class AudioStream : LanguageMediaStream
  {
    #region matching dictionaries

    private static readonly Dictionary<AudioCodec, string> CodecFrendlyNames = new Dictionary<AudioCodec, string>
    {
      { AudioCodec.Undefined, "" },
      { AudioCodec.MpegLayer1, "MPEG Layer 1" },
      { AudioCodec.MpegLayer2, "MPEG Layer 2" },
      { AudioCodec.MpegLayer3, "MPEG Layer 3" },
      { AudioCodec.PcmIntBig, "PCM" },
      { AudioCodec.PcmIntLit, "PCM" },
      { AudioCodec.PcmFloatIeee, "PCM" },
      { AudioCodec.Ac3, "Dolby Digital" },
      { AudioCodec.Ac3Atmos, "Dolby Atmos" },
      { AudioCodec.Ac3Bsid9, "DolbyNet" },
      { AudioCodec.Ac3Bsid10, "DolbyNet" },
      { AudioCodec.Dts, "DTS" },
      { AudioCodec.DtsHd, "DTS-HD" },
      { AudioCodec.Eac3, "Dolby Digital Plus" },
      { AudioCodec.Eac3Atmos, "Dolby Atmos" },
      { AudioCodec.Flac, "FLAC" },
      { AudioCodec.Opus, "OPUS" },
      { AudioCodec.Tta1, "True Audio" },
      { AudioCodec.Vorbis, "Vorbis" },
      { AudioCodec.WavPack4, "WavPack" },
      { AudioCodec.WavPack, "WavPack" },
      { AudioCodec.Wave, "Wave" },
      { AudioCodec.Wave64, "Wave" },
      { AudioCodec.Real14_4, "Real Audio" },
      { AudioCodec.Real28_8, "Real Audio" },
      { AudioCodec.RealCook, "Real Audio" },
      { AudioCodec.RealSipr, "Real Audio" },
      { AudioCodec.RealRalf, "Real Audio" },
      { AudioCodec.RealAtrc, "Real Audio" },
      { AudioCodec.Truehd, "Dolby TrueHD" },
      { AudioCodec.TruehdAtmos, "Dolby TrueHD Atmos" },
      { AudioCodec.Mlp, "Meridian Lossless" },
      { AudioCodec.Aac, "AAC" },
      { AudioCodec.AacMpeg2Main, "AAC" },
      { AudioCodec.AacMpeg2Lc, "AAC" },
      { AudioCodec.AacMpeg2LcSbr, "AAC" },
      { AudioCodec.AacMpeg2Ssr, "AAC" },
      { AudioCodec.AacMpeg4Main, "AAC" },
      { AudioCodec.AacMpeg4Lc, "AAC" },
      { AudioCodec.AacMpeg4LcSbr, "AAC" },
      { AudioCodec.AacMpeg4LcSbrPs, "AAC" },
      { AudioCodec.AacMpeg4Ssr, "AAC" },
      { AudioCodec.AacMpeg4Ltp, "AAC" },
      { AudioCodec.Alac, "Apple Lossless" },
      { AudioCodec.Ape, "Monkey's Audio" },
      { AudioCodec.Wma1, "Windows Audio" },
      { AudioCodec.Wma2, "Windows Audio" },
      { AudioCodec.Wma9, "Windows Audio Pro" },
      { AudioCodec.Adpcm, "ADPCM" },
      { AudioCodec.Amr, "Adaptive Multi-Rate" },
      { AudioCodec.Atrac1, "SDSS" },
      { AudioCodec.Atrac3, "ATRAC3" },
      { AudioCodec.Atrac3Plus, "ATRAC3plus" },
      { AudioCodec.AtracLossless, "ATRAC Advanced Lossless" },
      { AudioCodec.Atrac9, "ATRAC9" },
    };

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

    private static readonly Dictionary<int, string> Channels = new Dictionary<int, string>
    {
      { 1, "Mono" },
      { 2, "Stereo" },
      { 3, "2.1" },
      { 4, "4.0" },
      { 5, "5.0" },
      { 6, "5.1" },
      { 7, "6.1" },
      { 8, "7.1" },
      { 9, "7.2" },
      { 10, "7.2.1" },
    };

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioStream"/> class.
    /// </summary>
    /// <param name="info">The media information.</param>
    /// <param name="number">The stream number.</param>
    /// <param name="position">The stream position.</param>
    public AudioStream(MediaInfo info, int number, int position)
        : base(info, number, position)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioStream"/> class.
    /// </summary>
    /// <param name="number">The stream number.</param>
    /// <param name="position">The stream position.</param>
    public AudioStream(int number, int position)
        : base(null, number, position)
    {
    }

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Audio;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Audio;

    /// <summary>
    /// Gets the audio codec.
    /// </summary>
    /// <value>
    /// The audio codec.
    /// </value>
    [PublicAPI]
    public AudioCodec Codec { get; set; }

    /// <summary>
    /// Gets the codec friendly name.
    /// </summary>
    /// <value>
    /// The codec friendly name.
    /// </value>
    [PublicAPI]
    public string CodecFriendly
    {
      get
      {
        string result;
        return CodecFrendlyNames.TryGetValue(Codec, out result) ? result : string.Empty;
      }
    }

    /// <summary>
    /// Gets the stream duration.
    /// </summary>
    /// <value>
    /// The stream duration.
    /// </value>
    [PublicAPI]
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets the audio bitrate.
    /// </summary>
    /// <value>
    /// The audio bitrate.
    /// </value>
    [PublicAPI]
    public double Bitrate { get; set; }

    /// <summary>
    /// Gets the audio channel amount.
    /// </summary>
    /// <value>
    /// The audio channel amount.
    /// </value>
    [PublicAPI]
    public int Channel { get; set; }

    /// <summary>
    /// Gets the audio sampling rate.
    /// </summary>
    /// <value>
    /// The audio sampling rate.
    /// </value>
    [PublicAPI]
    public double SamplingRate { get; set; }

    /// <summary>
    /// Gets the bit depth of stream.
    /// </summary>
    /// <value>
    /// The bit depth of stream.
    /// </value>
    [PublicAPI]
    public int BitDepth { get; set; }

    /// <summary>
    /// Gets the audio format.
    /// </summary>
    /// <value>
    /// The audio format.
    /// </value>
    [PublicAPI]
    public string Format { get; set; }

    /// <summary>
    /// Gets the audio codec name.
    /// </summary>
    /// <value>
    /// The audio codec name.
    /// </value>
    [PublicAPI]
    public string CodecName { get; set; }

    /// <summary>
    /// Gets the audio channels friendly.
    /// </summary>
    /// <value>
    /// The audio channels friendly.
    /// </value>
    [PublicAPI]
    public string AudioChannelsFriendly => ConvertAudioChannels(Channel);

    /// <inheritdoc />
    protected override void AnalyzeStreamInternal(MediaInfo info)
    {
      base.AnalyzeStreamInternal(info);
      var baseIndex = 0;
      Codec = GetCodecByCodecId(Get(info, "CodecID"));
      if (Codec == AudioCodec.Undefined)
      {
        var codecValue = Get(info, "Codec");
        if (codecValue.Equals("PCM", StringComparison.OrdinalIgnoreCase))
        {
          var endianness = Get(info, "Codec_Settings_Endianness");
          codecValue = $"{codecValue}{(string.IsNullOrEmpty(endianness) ? string.Empty : " " + endianness)}";
        }

        Codec = GetCodecIdByCodecName(codecValue);
        // Correction for Atmos audio
        switch (Codec)
        {
          case AudioCodec.DtsHd:
            baseIndex = 1;
            break;
          case AudioCodec.Ac3:
          case AudioCodec.Ac3Bsid10:
          case AudioCodec.Ac3Bsid9:
          case AudioCodec.Eac3:
          case AudioCodec.Truehd:
            var formatProfile = GetCodecIdByCodecName(info.Get(StreamKind.Audio, StreamPosition, "Format_Profile").Split('/')[0].Trim());
            if (formatProfile != AudioCodec.Undefined)
            {
              Codec = formatProfile;
              baseIndex = 1;
            }

            break;
        }
      }

      Duration = TimeSpan.FromMilliseconds(Get<double>(info, "Duration", double.TryParse, x => ExtractInfo(x, 0)));
      Bitrate = Get<double>(info, "BitRate", double.TryParse, x => ExtractInfo(x, baseIndex));
      Channel = Get<int>(info, "Channel(s)", int.TryParse, x => ExtractInfo(x, baseIndex));
      SamplingRate = Get<double>(info, "SamplingRate", double.TryParse, x => ExtractInfo(x, baseIndex));
      BitDepth = Get<int>(info, "BitDepth", int.TryParse, x => ExtractInfo(x, baseIndex));
      Format = Get(info, "Format", x => ExtractInfo(x, 0));
      CodecName = GetFullCodecName(info);
    }

    private static string ExtractInfo(string source, int index)
    {
      return source.IndexOf("/", StringComparison.Ordinal) >= 0 ? 
          source.Split('/').Skip(index).FirstOrDefault()?.Trim() : 
          source;
    }

    private static AudioCodec GetCodecByCodecId(string source)
    {
      AudioCodec result;
      return CodecIds.TryGetValue(source, out result) ? result : AudioCodec.Undefined;
    }

    private static AudioCodec GetCodecIdByCodecName(string source)
    {
      AudioCodec result;
      return Codecs.TryGetValue(source, out result) ? result : AudioCodec.Undefined;
    }

    private static string ConvertAudioChannels(int channels)
    {
      string result;
      return Channels.TryGetValue(channels, out result) ? result : "Unknown";
    }

    private string GetFullCodecName(MediaInfo mediaInfo)
    {
      var strCodec = mediaInfo.Get(StreamKind.Audio, StreamPosition, "Format").ToUpper();
      var strCodecVer = mediaInfo.Get(StreamKind.Audio, StreamPosition, "Format_Version").ToUpper();
      if (strCodec == "MPEG-4 VISUAL")
      {
        strCodec = mediaInfo.Get(StreamKind.Audio, StreamPosition, "CodecID").ToUpperInvariant();
      }
      else
      {
        if (!string.IsNullOrEmpty(strCodecVer))
        {
          strCodec = (strCodec + " " + strCodecVer).Trim();
        }
      }

      var formatName = mediaInfo.Get(StreamKind.Audio, StreamPosition, "Format_Profile").Split('/')[0].Trim();
      if (formatName.IndexOf("ATMOS", StringComparison.OrdinalIgnoreCase) >= 0)
      {
        return formatName;
      }

      strCodec = (strCodec + " " + formatName).Trim();
      return strCodec.Replace("+", "PLUS");
    }
  }
}