#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models;

/// <summary>
/// Describes type of the video codec
/// </summary>
[DataContract]
[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum VideoCodec
{
    /// <summary>
    /// The undefined
    /// </summary>
    [EnumMember(Value = "undefined")]
    Undefined,

    /// <summary>
    /// The uncompressed
    /// </summary>
    [EnumMember(Value = "uncompressed")]
    Uncompressed,

    /// <summary>
    /// Dirac
    /// </summary>
    [EnumMember(Value = "dirac")]
    Dirac,

    /// <summary>
    /// MPEG4
    /// </summary>
    [EnumMember(Value = "mpeg-4")]
    Mpeg4,

    /// <summary>
    /// MPEG4 Simple Profile
    /// </summary>
    [EnumMember(Value = "mpeg4-sp")]
    Mpeg4Is0Sp,

    /// <summary>
    /// MPEG4 Advanced Simple Profile
    /// </summary>
    [EnumMember(Value = "mpeg4-asp")]
    Mpeg4Is0Asp,

    /// <summary>
    /// MPEG4 Advanced Profile
    /// </summary>
    [EnumMember(Value = "mpeg4-ap")]
    Mpeg4Is0Ap,

    /// <summary>
    /// MPEG4 AVC
    /// </summary>
    [EnumMember(Value = "mpeg4-avc")]
    Mpeg4Is0Avc,

    /// <summary>
    /// MPEG4 ISO Simple Profile
    /// </summary>
    [EnumMember(Value = "mpeg4-iso-sp")]
    Mpeg4IsoSp,

    /// <summary>
    /// MPEG4 ISO Advanced Simple Profile
    /// </summary>
    [EnumMember(Value = "mpeg4-iso-asp")]
    Mpeg4IsoAsp,

    /// <summary>
    /// MPEG4 ISO Advanced Profile
    /// </summary>
    [EnumMember(Value = "mpeg4-iso-ap")]
    Mpeg4IsoAp,

    /// <summary>
    /// MPEG4 ISO AVC
    /// </summary>
    [EnumMember(Value = "mpeg4-iso-avc")]
    Mpeg4IsoAvc,

    /// <summary>
    /// MPEG4 ISO HEVC
    /// </summary>
    [EnumMember(Value = "mpeg4-iso-hevc")]
    MpeghIsoHevc,

    /// <summary>
    /// The Windows Media MPEG4 V1
    /// </summary>
    [EnumMember(Value = "mpeg4-ms-v1")]
    Mpeg4MsV1,

    /// <summary>
    /// The Windows Media MPEG4 V2
    /// </summary>
    [EnumMember(Value = "mpeg4-ms-v2")]
    Mpeg4MsV2,

    /// <summary>
    /// The Windows Media MPEG4 V3
    /// </summary>
    [EnumMember(Value = "mpeg4-ms-v3")]
    Mpeg4MsV3,

    /// <summary>
    /// VC1
    /// </summary>
    [EnumMember(Value = "vc1")]
    Vc1,

    /// <summary>
    /// The MPEG1
    /// </summary>
    [EnumMember(Value = "mpeg1")]
    Mpeg1,

    /// <summary>
    /// The MPEG2
    /// </summary>
    [EnumMember(Value = "mpeg2")]
    Mpeg2,

    /// <summary>
    /// The ProRes
    /// </summary>
    [EnumMember(Value = "proRes")]
    ProRes,

    /// <summary>
    /// Real Video v1
    /// </summary>
    [EnumMember(Value = "real-v1")]
    RealRv10,

    /// <summary>
    /// Real Video v2
    /// </summary>
    [EnumMember(Value = "real-v2")]
    RealRv20,

    /// <summary>
    /// Real Video v3
    /// </summary>
    [EnumMember(Value = "real-v3")]
    RealRv30,

    /// <summary>
    /// Real Video v4
    /// </summary>
    [EnumMember(Value = "real-v4")]
    RealRv40,

    /// <summary>
    /// Theora
    /// </summary>
    [EnumMember(Value = "theora")]
    Theora,

    /// <summary>
    /// TrueMotion VP6
    /// </summary>
    [EnumMember(Value = "vp6")]
    Vp6,

    /// <summary>
    /// VP8
    /// </summary>
    [EnumMember(Value = "vp8")]
    Vp8,

    /// <summary>
    /// VP9
    /// </summary>
    [EnumMember(Value = "vp9")]
    Vp9,

    /// <summary>
    /// DivX v1
    /// </summary>
    [EnumMember(Value = "divx1")]
    Divx1,

    /// <summary>
    /// DivX v2
    /// </summary>
    [EnumMember(Value = "divx2")]
    Divx2,

    /// <summary>
    /// DivX v3.x
    /// </summary>
    [EnumMember(Value = "divx3")]
    Divx3,

    /// <summary>
    /// DivX v4
    /// </summary>
    [EnumMember(Value = "divx4")]
    Divx4,

    /// <summary>
    /// DivX v5
    /// </summary>
    [EnumMember(Value = "divx5")]
    Divx50,

    /// <summary>
    /// The XVid
    /// </summary>
    [EnumMember(Value = "xvid")]
    Xvid,

    /// <summary>
    /// Sorenson Video v1
    /// </summary>
    [EnumMember(Value = "svq1")]
    Svq1,

    /// <summary>
    /// Sorenson Video v2
    /// </summary>
    [EnumMember(Value = "svq2")]
    Svq2,

    /// <summary>
    /// Sorenson Video v3
    /// </summary>
    [EnumMember(Value = "svq3")]
    Svq3,

    /// <summary>
    /// The Sorenson Spark
    /// </summary>
    [EnumMember(Value = "sprk")]
    Sprk,

    /// <summary>
    /// H.260
    /// </summary>
    [EnumMember(Value = "h.260")]
    H260,

    /// <summary>
    /// H.261
    /// </summary>
    [EnumMember(Value = "h.261")]
    H261,

    /// <summary>
    /// H.263
    /// </summary>
    [EnumMember(Value = "h.263")]
    H263,

    /// <summary>
    /// AVdv
    /// </summary>
    [EnumMember(Value = "avdv")]
    Avdv,

    /// <summary>
    /// Autodesk Digital Video v1
    /// </summary>
    [EnumMember(Value = "avd1")]
    Avd1,

    /// <summary>
    /// FF video codec 1
    /// </summary>
    [EnumMember(Value = "ffv1")]
    Ffv1,

    /// <summary>
    /// FF video codec 2
    /// </summary>
    [EnumMember(Value = "ffv2")]
    Ffv2,

    /// <summary>
    /// IV21
    /// </summary>
    [EnumMember(Value = "iv21")]
    Iv21,

    /// <summary>
    /// IV30
    /// </summary>
    [EnumMember(Value = "iv30")]
    Iv30,

    /// <summary>
    /// IV40
    /// </summary>
    [EnumMember(Value = "iv40")]
    Iv40,

    /// <summary>
    /// IV50
    /// </summary>
    [EnumMember(Value = "iv50")]
    Iv50,

    /// <summary>
    /// The FFDShow MPEG-4 Video
    /// </summary>
    [EnumMember(Value = "ffds")]
    Ffds,

    /// <summary>
    /// The FFDShow MPEG-4 Video
    /// </summary>
    [EnumMember(Value = "fraps")]
    Fraps,

    /// <summary>
    /// HuffYUV 2.2
    /// </summary>
    [EnumMember(Value = "ffvh")]
    Ffvh,

    /// <summary>
    /// Motion JPEG
    /// </summary>
    [EnumMember(Value = "mjpg")]
    Mjpg,

    /// <summary>
    /// Digital Video
    /// </summary>
    [EnumMember(Value = "dv")]
    Dv,

    /// <summary>
    /// Digital Video HD
    /// </summary>
    [EnumMember(Value = "hdv")]
    Hdv,

    /// <summary>
    /// DVCPRO50
    /// </summary>
    [EnumMember(Value = "cvcPro50")]
    DvcPro50,

    /// <summary>
    /// DVCPRO HD
    /// </summary>
    [EnumMember(Value = "cvcProHd")]
    DvcProHd,

    /// <summary>
    /// Windows Media Video V7
    /// </summary>
    [EnumMember(Value = "wmv1")]
    Wmv1,

    /// <summary>
    /// Windows Media Video V8
    /// </summary>
    [EnumMember(Value = "wmv2")]
    Wmv2,

    /// <summary>
    /// Windows Media Video V9
    /// </summary>
    [EnumMember(Value = "wmv3")]
    Wmv3,

    /// <summary>
    /// QuickTime 8BPS
    /// </summary>
    [EnumMember(Value = "q8Bps")]
    Q8Bps,

    /// <summary>
    /// Bink video
    /// </summary>
    [EnumMember(Value = "binkVideo")]
    BinkVideo,

    /// <summary>
    /// AV1
    /// </summary>
    [EnumMember(Value = "av1")]
    Av1,

    /// <summary>
    /// HuffYUV
    /// </summary>
    [EnumMember(Value = "huffYUV")]
    HuffYUV,
}