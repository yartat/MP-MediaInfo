#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models
{
    /// <summary>
    /// Defines constants for different kind of audio codecs.
    /// </summary>
    [DataContract]
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum AudioCodec
    {
        /// <summary>
        /// The undefined audio codec
        /// </summary>
        [EnumMember(Value = "undefined")]
        Undefined,

        /// <summary>
        /// MPEG Layer 1
        /// </summary>
        [EnumMember(Value = "mpeg1")]
        MpegLayer1,

        /// <summary>
        /// MPEG Layer 2
        /// </summary>
        [EnumMember(Value = "mpeg2")]
        MpegLayer2,

        /// <summary>
        /// MPEG Layer 3
        /// </summary>
        [EnumMember(Value = "mpeg3")]
        MpegLayer3,

        /// <summary>
        /// PCM big-endian int
        /// </summary>
        [EnumMember(Value = "pcmBig")]
        PcmIntBig,

        /// <summary>
        /// PCM little-endian int
        /// </summary>
        [EnumMember(Value = "pcmLit")]
        PcmIntLit,

        /// <summary>
        /// PCM float IEEE
        /// </summary>
        [EnumMember(Value = "pcmIeee")]
        PcmFloatIeee,

        /// <summary>
        /// Dolby Digital
        /// </summary>
        [EnumMember(Value = "ac-3")]
        Ac3,

        /// <summary>
        /// Dolby Digital with Dolby Atmos
        /// </summary>
        [EnumMember(Value = "ac-3-atmos")]
        Ac3Atmos,

        /// <summary>
        /// DolbyNet
        /// </summary>
        [EnumMember(Value = "dolbyNet9")]
        Ac3Bsid9,

        /// <summary>
        /// DolbyNet
        /// </summary>
        [EnumMember(Value = "dolbyNet10")]
        Ac3Bsid10,

        /// <summary>
        /// Dolby Digital Plus
        /// </summary>
        [EnumMember(Value = "e-ac-3")]
        Eac3,

        /// <summary>
        /// Dolby Digital Plus with Dolby Atmos
        /// </summary>
        [EnumMember(Value = "e-ac-3-atmos")]
        Eac3Atmos,

        /// <summary>
        /// Dolby TrueHD
        /// </summary>
        [EnumMember(Value = "trueHd")]
        Truehd,

        /// <summary>
        /// Dolby TrueHD with Dolby Atmos
        /// </summary>
        [EnumMember(Value = "trueHd-atmos")]
        TruehdAtmos,

        /// <summary>
        /// DTS
        /// </summary>
        [EnumMember(Value = "dts")]
        Dts,

        /// <summary>
        /// DTS:X
        /// </summary>
        [EnumMember(Value = "dts-X")]
        DtsX,

        /// <summary>
        /// DTS-HD MA
        /// </summary>
        [EnumMember(Value = "dts-hd-ma")]
        DtsHdMa,

        /// <summary>
        /// DTS Express
        /// </summary>
        [EnumMember(Value = "dts-express")]
        DtsExpress,

        /// <summary>
        /// DTS-HD HRA
        /// </summary>
        [EnumMember(Value = "dts-hd-hra")]
        DtsHdHra,

        /// <summary>
        /// DTS-HD 96/24
        /// </summary>
        [EnumMember(Value = "dts-hd")]
        DtsHd,

        /// <summary>
        /// DTS-ES
        /// </summary>
        [EnumMember(Value = "dts-es")]
        DtsEs,

        /// <summary>
        /// Free Lossless Audio Codec
        /// </summary>
        [EnumMember(Value = "flac")]
        Flac,

        /// <summary>
        /// OPUS
        /// </summary>
        [EnumMember(Value = "opus")]
        Opus,

        /// <summary>
        /// True Audio
        /// </summary>
        [EnumMember(Value = "tta1")]
        Tta1,

        /// <summary>
        /// VORBIS
        /// </summary>
        [EnumMember(Value = "vorbis")]
        Vorbis,

        /// <summary>
        /// WavPack v4
        /// </summary>
        [EnumMember(Value = "wavPack4")]
        WavPack4,

        /// <summary>
        /// WavPack
        /// </summary>
        [EnumMember(Value = "wavPack")]
        WavPack,

        /// <summary>
        /// Waveform Audio
        /// </summary>
        [EnumMember(Value = "wave")]
        Wave,

        /// <summary>
        /// Waveform Audio
        /// </summary>
        [EnumMember(Value = "wave64")]
        Wave64,

        /// <summary>
        /// The Real Audio
        /// </summary>
        // ReSharper disable once InconsistentNaming
        [EnumMember(Value = "real-14-4")]
        Real14_4,

        /// <summary>
        /// The Real Audio
        /// </summary>
        // ReSharper disable once InconsistentNaming
        [EnumMember(Value = "real-28-8")]
        Real28_8,

        /// <summary>
        /// The RealAudio Lossless (RealAudio 10)
        /// </summary>
        [EnumMember(Value = "real-10")]
        Real10,

        /// <summary>
        /// The Real Audio
        /// </summary>
        [EnumMember(Value = "realCook")]
        RealCook,

        /// <summary>
        /// The Real Audio
        /// </summary>
        [EnumMember(Value = "realSipr")]
        RealSipr,

        /// <summary>
        /// The Real Audio
        /// </summary>
        [EnumMember(Value = "realRalf")]
        RealRalf,

        /// <summary>
        /// The Real Audio
        /// </summary>
        [EnumMember(Value = "realAtrc")]
        RealAtrc,

        /// <summary>
        /// Meridian Lossless
        /// </summary>
        [EnumMember(Value = "mlp")]
        Mlp,

        /// <summary>
        /// Advanced Audio Coding
        /// </summary>
        [EnumMember(Value = "aac")]
        Aac,

        /// <summary>
        /// Advanced Audio Coding
        /// </summary>
        [EnumMember(Value = "aac-mpeg2-main")]
        AacMpeg2Main,

        /// <summary>
        /// Advanced Audio Coding
        /// </summary>
        [EnumMember(Value = "aac-mpeg2-lc")]
        AacMpeg2Lc,

        /// <summary>
        /// Advanced Audio Coding
        /// </summary>
        [EnumMember(Value = "aac-mpeg2-lc-sbr")]
        AacMpeg2LcSbr,

        /// <summary>
        /// Advanced Audio Coding
        /// </summary>
        [EnumMember(Value = "aac-mpeg2-ssr")]
        AacMpeg2Ssr,

        /// <summary>
        /// Advanced Audio Coding
        /// </summary>
        [EnumMember(Value = "aac-mpeg4-main")]
        AacMpeg4Main,

        /// <summary>
        /// Advanced Audio Coding
        /// </summary>
        [EnumMember(Value = "aac-mpeg4-lc")]
        AacMpeg4Lc,

        /// <summary>
        /// Advanced Audio Coding
        /// </summary>
        [EnumMember(Value = "aac-mpeg4-lc-sbr")]
        AacMpeg4LcSbr,

        /// <summary>
        /// Advanced Audio Coding
        /// </summary>
        [EnumMember(Value = "aac-mpeg4-lc-sbr-ps")]
        AacMpeg4LcSbrPs,

        /// <summary>
        /// Advanced Audio Coding
        /// </summary>
        [EnumMember(Value = "aac-mpeg4-ssr")]
        AacMpeg4Ssr,

        /// <summary>
        /// Advanced Audio Coding
        /// </summary>
        [EnumMember(Value = "aac-mpeg4-ltp")]
        AacMpeg4Ltp,

        /// <summary>
        /// Apple Lossless
        /// </summary>
        [EnumMember(Value = "alac")]
        Alac,

        /// <summary>
        /// Monkey's Audio
        /// </summary>
        [EnumMember(Value = "ape")]
        Ape,

        /// <summary>
        /// Windows Media Audio
        /// </summary>
        [EnumMember(Value = "wma1")]
        Wma1,

        /// <summary>
        /// Windows Media Audio v2
        /// </summary>
        [EnumMember(Value = "wma3")]
        Wma2,

        /// <summary>
        /// Windows Media Audio v3
        /// </summary>
        [EnumMember(Value = "wma3")]
        Wma3,

        /// <summary>
        /// Windows Media Audio Voice
        /// </summary>
        [EnumMember(Value = "wma-voice")]
        WmaVoice,

        /// <summary>
        /// Windows Media Audio Pro
        /// </summary>
        [EnumMember(Value = "wma-pro")]
        WmaPro,

        /// <summary>
        /// Windows Media Audio Lossless
        /// </summary>
        [EnumMember(Value = "wma-lossless")]
        WmaLossless,

        /// <summary>
        /// Adaptive differential pulse-code modulation
        /// </summary>
        [EnumMember(Value = "adpcm")]
        Adpcm,

        /// <summary>
        /// Adaptive multi rate
        /// </summary>
        [EnumMember(Value = "amr")]
        Amr,

        /// <summary>
        /// Adaptive Transform Acoustic Coding (SDDS)
        /// </summary>
        [EnumMember(Value = "atrac1")]
        Atrac1,

        /// <summary>
        /// Adaptive Transform Acoustic Coding 3
        /// </summary>
        [EnumMember(Value = "atrac3")]
        Atrac3,

        /// <summary>
        /// ATRAC3plus
        /// </summary>
        [EnumMember(Value = "atrac3-plus")]
        Atrac3Plus,

        /// <summary>
        /// ATRAC Advanced Lossless
        /// </summary>
        [EnumMember(Value = "atrac-losseless")]
        AtracLossless,

        /// <summary>
        /// ATRAC9
        /// </summary>
        [EnumMember(Value = "atrac9")]
        Atrac9,

        /// <summary>
        /// Direct Stream Digital
        /// </summary>
        [EnumMember(Value = "dsd")]
        Dsd,

        /// <summary>
        /// MAC3
        /// </summary>
        [EnumMember(Value = "mac3")]
        Mac3,

        /// <summary>
        /// MAC6
        /// </summary>
        [EnumMember(Value = "mac6")]
        Mac6,

        /// <summary>
        /// G.723.1
        /// </summary>
        [EnumMember(Value = "g-723-1")]
        G_723_1,

        /// <summary>
        /// Truespeech
        /// </summary>
        [EnumMember(Value = "truespeech")]
        Truespeech,

        /// <summary>
        /// RK Audio
        /// </summary>
        [EnumMember(Value = "rk-audio")]
        RkAudio,

        /// <summary>
        /// MPEG-4 Audio Lossless Coding
        /// </summary>
        [EnumMember(Value = "als")]
        Als,

        /// <summary>
        /// Ligos IAC2
        /// </summary>
        [EnumMember(Value = "iac2")]
        Iac2,

        /// <summary>
        /// MPEG-H 3D Audio
        /// </summary>
        [EnumMember(Value = "mpeg3DAudio")]
        Mpeg3DAudio,

        /// <summary>
        /// Nellymoser codec
        /// </summary>
        [EnumMember(Value = "nellymoser")]
        Nellymoser,

        /// <summary>
        /// The Qualcomm Pure Voice
        /// </summary>
        [EnumMember(Value = "qualcomm-pure-voice")]
        QualcommPureVoice,

        /// <summary>
        /// QDesign Music 1
        /// </summary>
        [EnumMember(Value = "qDesignMusic1")]
        QDesignMusic1,

        /// <summary>
        /// QDesign Music 2
        /// </summary>
        [EnumMember(Value = "qDesignMusic2")]
        QDesignMusic2,

        /// <summary>
        /// Dolby AC-4
        /// </summary>
        [EnumMember(Value = "ac-4")]
        Ac4,

        /// <summary>
        /// Dolby E codec
        /// </summary>
        [EnumMember(Value = "dolby-e")]
        DolbyE,

        /// <summary>
        /// Dolby ED2
        /// </summary>
        [EnumMember(Value = "dolby-ed2")]
        DolbyEd2
    }
}