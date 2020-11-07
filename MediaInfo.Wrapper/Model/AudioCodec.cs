#region Copyright (C) 2017-2020 Yaroslav Tatarenko

// Copyright (C) 2017-2020 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2020 MediaArea.net SARL. 
// https://mediaarea.net

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
    /// The RealAudio Lossless (RealAudio 10)
    /// </summary>
    Real10,

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
    /// Windows Media Audio v3
    /// </summary>
    Wma3,

    /// <summary>
    /// Windows Media Audio Voice
    /// </summary>
    WmaVoice,

    /// <summary>
    /// Windows Media Audio Pro
    /// </summary>
    WmaPro,

    /// <summary>
    /// Windows Media Audio Lossless
    /// </summary>
    WmaLossless,

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
    Dsd,

    /// <summary>
    /// MAC3
    /// </summary>
    Mac3,

    /// <summary>
    /// MAC6
    /// </summary>
    Mac6,

    /// <summary>
    /// G.723.1
    /// </summary>
    G_723_1,

    /// <summary>
    /// Truespeech
    /// </summary>
    Truespeech,

    /// <summary>
    /// RK Audio
    /// </summary>
    RkAudio,

    /// <summary>
    /// MPEG-4 Audio Lossless Coding
    /// </summary>
    Als,

    /// <summary>
    /// Ligos IAC2
    /// </summary>
    Iac2,
  }
}