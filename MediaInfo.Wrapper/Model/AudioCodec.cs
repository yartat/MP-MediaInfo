#region Copyright (C) 2005-2019 Team MediaPortal

// Copyright (C) 2005-2019 Team MediaPortal
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

namespace MediaInfo.Model
{
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
    /// Dolby Digital with Dolby Atmos
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
    /// Dolby Digital Plus with Dolby Atmos
    /// </summary>
    Eac3Atmos,

    /// <summary>
    /// Dolby TrueHD
    /// </summary>
    Truehd,

    /// <summary>
    /// Dolby TrueHD with Dolby Atmos
    /// </summary>
    TruehdAtmos,

    /// <summary>
    /// DTS
    /// </summary>
    Dts,

    /// <summary>
    /// DTS:X
    /// </summary>
    DtsX,

    /// <summary>
    /// DTS-HD MA
    /// </summary>
    DtsHdMa,

    /// <summary>
    /// DTS Express
    /// </summary>
    DtsExpress,

    /// <summary>
    /// DTS-HD HRA
    /// </summary>
    DtsHdHra,

    /// <summary>
    /// DTS-HD 96/24
    /// </summary>
    DtsHd,

    /// <summary>
    /// DTS-ES
    /// </summary>
    DtsEs,

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
    Atrac9,

    /// <summary>
    /// Direct Stream Digital
    /// </summary>
    Dsd
  }
}