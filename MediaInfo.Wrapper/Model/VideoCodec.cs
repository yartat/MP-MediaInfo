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

namespace MediaInfo
{
  /// <summary>
  /// Describes type of video codecs
  /// </summary>
  public enum VideoCodec
  {
    /// <summary>
    /// The undefined
    /// </summary>
    Undefined,

    /// <summary>
    /// The uncompressed
    /// </summary>
    Uncompressed,

    /// <summary>
    /// Dirac
    /// </summary>
    Dirac,

    /// <summary>
    /// MPEG4
    /// </summary>
    Mpeg4,

    /// <summary>
    /// MPEG4 Simple Profile
    /// </summary>
    Mpeg4Is0Sp,

    /// <summary>
    /// MPEG4 Advanced Simple Profile
    /// </summary>
    Mpeg4Is0Asp,

    /// <summary>
    /// MPEG4 Advanced Profile
    /// </summary>
    Mpeg4Is0Ap,

    /// <summary>
    /// MPEG4 AVC
    /// </summary>
    Mpeg4Is0Avc,

    /// <summary>
    /// MPEG4 ISO Simple Profile
    /// </summary>
    Mpeg4IsoSp,

    /// <summary>
    /// MPEG4 ISO Advanced Simple Profile
    /// </summary>
    Mpeg4IsoAsp,

    /// <summary>
    /// MPEG4 ISO Advanced Profile
    /// </summary>
    Mpeg4IsoAp,

    /// <summary>
    /// MPEG4 ISO AVC
    /// </summary>
    Mpeg4IsoAvc,

    /// <summary>
    /// MPEG4 ISO HEVC
    /// </summary>
    MpeghIsoHevc,

    /// <summary>
    /// The Windows Media MPEG4 V1
    /// </summary>
    Mpeg4MsV1,

    /// <summary>
    /// The Windows Media MPEG4 V2
    /// </summary>
    Mpeg4MsV2,

    /// <summary>
    /// The Windows Media MPEG4 V3
    /// </summary>
    Mpeg4MsV3,

    /// <summary>
    /// VC1
    /// </summary>
    Vc1,

    /// <summary>
    /// The MPEG1
    /// </summary>
    Mpeg1,

    /// <summary>
    /// The MPEG2
    /// </summary>
    Mpeg2,

    /// <summary>
    /// The ProRes
    /// </summary>
    Prores,

    /// <summary>
    /// Real Video v1
    /// </summary>
    RealRv10,

    /// <summary>
    /// Real Video v2
    /// </summary>
    RealRv20,

    /// <summary>
    /// Real Video v3
    /// </summary>
    RealRv30,

    /// <summary>
    /// Real Video v4
    /// </summary>
    RealRv40,

    /// <summary>
    /// Theora
    /// </summary>
    Theora,

    /// <summary>
    /// TrueMotion VP6
    /// </summary>
    Vp6,

    /// <summary>
    /// VP8
    /// </summary>
    Vp8,

    /// <summary>
    /// VP9
    /// </summary>
    Vp9,

    /// <summary>
    /// DivX v1
    /// </summary>
    Divx1,

    /// <summary>
    /// DivX v2
    /// </summary>
    Divx2,

    /// <summary>
    /// DivX v3.x
    /// </summary>
    Divx3,

    /// <summary>
    /// DivX v4
    /// </summary>
    Divx4,

    /// <summary>
    /// DivX v5
    /// </summary>
    Divx50,

    /// <summary>
    /// The XVid
    /// </summary>
    Xvid,

    /// <summary>
    /// Sorenson Video v1
    /// </summary>
    Svq1,

    /// <summary>
    /// Sorenson Video v2
    /// </summary>
    Svq2,

    /// <summary>
    /// Sorenson Video v3
    /// </summary>
    Svq3,

    /// <summary>
    /// The Sorenson Spark
    /// </summary>
    Sprk,

    /// <summary>
    /// H.260
    /// </summary>
    H260,

    /// <summary>
    /// H.261
    /// </summary>
    H261,

    /// <summary>
    /// H.263
    /// </summary>
    H263,

    /// <summary>
    /// AVdv
    /// </summary>
    Avdv,

    /// <summary>
    /// Autodesk Digital Video v1
    /// </summary>
    Avd1,

    /// <summary>
    /// FF video codec 1
    /// </summary>
    Ffv1,

    /// <summary>
    /// FF video codec 2
    /// </summary>
    Ffv2,

    /// <summary>
    /// IV21
    /// </summary>
    Iv21,

    /// <summary>
    /// IV30
    /// </summary>
    Iv30,

    /// <summary>
    /// IV40
    /// </summary>
    Iv40,

    /// <summary>
    /// IV50
    /// </summary>
    Iv50,

    /// <summary>
    /// The FFDShow MPEG-4 Video
    /// </summary>
    Ffds,

    /// <summary>
    /// The FFDShow MPEG-4 Video
    /// </summary>
    Fraps,

    /// <summary>
    /// HuffYUV 2.2
    /// </summary>
    Ffvh,

    /// <summary>
    /// Motion JPEG
    /// </summary>
    Mjpg,

    /// <summary>
    /// Digital Video
    /// </summary>
    Dv,

    /// <summary>
    /// Digital Video HD
    /// </summary>
    Hdv,

    /// <summary>
    /// DVCPRO50
    /// </summary>
    DvcPro50,

    /// <summary>
    /// DVCPRO HD
    /// </summary>
    DvcProHd,

    /// <summary>
    /// Windows Media Video V7
    /// </summary>
    Wmv1,

    /// <summary>
    /// Windows Media Video V8
    /// </summary>
    Wmv2,

    /// <summary>
    /// Windows Media Video V9
    /// </summary>
    Wmv3,

    /// <summary>
    /// QuickTime 8BPS
    /// </summary>
    Q8Bps,

    /// <summary>
    /// Bink video
    /// </summary>
    BinkVideo,
  }
}