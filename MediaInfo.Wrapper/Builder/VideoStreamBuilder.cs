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
  /// Describes base methods to build video stream.
  /// </summary>
  internal class VideoStreamBuilder : LanguageMediaStreamBuilder<VideoStream>
  {
    #region match dictionaries

    private static readonly Dictionary<string, AspectRatio> Ratios = new Dictionary<string, AspectRatio>
    {
      { "1:1", AspectRatio.Opaque },
      { "5:4", AspectRatio.HighEndDataGraphics },
      { "1.2", AspectRatio.HighEndDataGraphics },
      { "3:3", AspectRatio.StandardSlides },
      { "4:3", AspectRatio.FullScreen },
      { "1.334", AspectRatio.FullScreen },
      { "3:2", AspectRatio.DigitalSlrCameras },
      { "1.5", AspectRatio.DigitalSlrCameras },
      { "16:9", AspectRatio.HighDefinitionTv },
      { "1.778", AspectRatio.HighDefinitionTv },
      { "16:10", AspectRatio.WideScreenDisplay },
      { "1.6", AspectRatio.WideScreenDisplay },
      { "1.85", AspectRatio.WideScreen },
      { "21:9", AspectRatio.CinemaScope },
      { "2.334", AspectRatio.CinemaScope }
    };

    private static readonly Dictionary<string, StereoMode> StereoModes = new Dictionary<string, StereoMode>
    {
      { "side-by-side (left eye is first)", StereoMode.SideBySideLeft },
      { "top-bottom (right eye is first)", StereoMode.TopBottomRight },
      { "top-bottom (left eye is first)", StereoMode.TopBottomLeft },
      { "checkerboard (right eye is first)", StereoMode.CheckerboardRight },
      { "checkerboard (left eye is first)", StereoMode.CheckerboardLeft },
      { "row interleaved (right eye is first)", StereoMode.RowInterleavedRight },
      { "row interleaved (left eye is first)", StereoMode.RowInterleavedLeft },
      { "column interleaved (right eye is first)", StereoMode.ColumnInterleavedRight },
      { "column interleaved (left eye is first)", StereoMode.ColumnInterleavedLeft },
      { "anaglyph (cyan/red)", StereoMode.AnaglyphCyanRed },
      { "side-by-side (right eye is first)", StereoMode.SideBySideRight },
      { "anaglyph (green/magenta)", StereoMode.AnaglyphGreenMagenta },
      { "both eyes laced in one block (left eye is first)", StereoMode.BothEyesLacedLeft },
      { "both eyes laced in one block (right eye is first)", StereoMode.BothEyesLacedRight }
    };

    private static readonly Dictionary<string, VideoCodec> VideoCodecs = new Dictionary<string, VideoCodec>
    {
      { "V_UNCOMPRESSED", VideoCodec.Uncompressed },
      { "V_DIRAC", VideoCodec.Dirac },
      { "V_MPEG4/IS0/SP", VideoCodec.Mpeg4Is0Sp },
      { "V_MPEG4/IS0/ASP", VideoCodec.Mpeg4Is0Asp },
      { "V_MPEG4/IS0/AP", VideoCodec.Mpeg4Is0Ap },
      { "V_MPEG4/IS0/AVC", VideoCodec.Mpeg4Is0Avc },
      { "V_MPEG4/ISO/SP", VideoCodec.Mpeg4IsoSp },
      { "V_MPEG4/ISO/ASP", VideoCodec.Mpeg4IsoAsp },
      { "V_MPEG4/ISO/AP", VideoCodec.Mpeg4IsoAp },
      { "V_MPEG4/ISO/AVC", VideoCodec.Mpeg4IsoAvc },
      { "V_MPEGH/ISO/HEVC", VideoCodec.MpeghIsoHevc },
      { "V_MPEG4/MS/V2", VideoCodec.Mpeg4MsV2 },
      { "V_MPEG4/MS/V3", VideoCodec.Mpeg4MsV3 },
      { "V_MPEG1", VideoCodec.Mpeg1 },
      { "V_MPEG2", VideoCodec.Mpeg2 },
      { "V_PRORES", VideoCodec.Prores },
      { "V_REAL/RV10", VideoCodec.RealRv10 },
      { "V_REAL/RV20", VideoCodec.RealRv20 },
      { "V_REAL/RV30", VideoCodec.RealRv30 },
      { "V_REAL/RV40", VideoCodec.RealRv40 },
      { "V_THEORA", VideoCodec.Theora },
      { "V_VP8", VideoCodec.Vp8 },
      { "V_VP9", VideoCodec.Vp9 },
      { "AVC1", VideoCodec.Mpeg4IsoAvc },
      { "AVC", VideoCodec.Mpeg4IsoAvc },
      { "H264", VideoCodec.Mpeg4IsoAvc },
      { "DAVC", VideoCodec.Mpeg4IsoAvc },
      { "MPEG-2V", VideoCodec.Mpeg2 },
      { "MPEG-2", VideoCodec.Mpeg2 },
      { "MPEG-1", VideoCodec.Mpeg1 },
      { "MPEG-1V", VideoCodec.Mpeg1 },
      { "VC1", VideoCodec.Vc1 },
      { "VC-1", VideoCodec.Vc1 },
      { "OVC1", VideoCodec.Vc1 },
      { "WVC1", VideoCodec.Vc1 },
      { "SORENSON H263", VideoCodec.Sprk },
      { "SPRK", VideoCodec.Sprk },
      { "SVQ1", VideoCodec.Svq1 },
      { "SVQ2", VideoCodec.Svq2 },
      { "SVQ3", VideoCodec.Svq3 },
      { "DX50", VideoCodec.Divx50 },
      { "DVX1", VideoCodec.Divx1 },
      { "DIV1", VideoCodec.Divx1 },
      { "DVX2", VideoCodec.Divx2 },
      { "DIV2", VideoCodec.Divx2 },
      { "DVX3", VideoCodec.Divx3 },
      { "DIV3", VideoCodec.Divx3 },
      { "DIV4", VideoCodec.Divx3 },
      { "DIV5", VideoCodec.Divx50 },
      { "DIV6", VideoCodec.Mpeg4MsV3 },
      { "DIVX", VideoCodec.Divx4 },
      { "XVID", VideoCodec.Xvid },
      { "FFV1", VideoCodec.Ffv1 },
      { "FFV2", VideoCodec.Ffv2 },
      { "S263", VideoCodec.H263 },
      { "H263", VideoCodec.H263 },
      { "D263", VideoCodec.H263 },
      { "L263", VideoCodec.H263 },
      { "M263", VideoCodec.H263 },
      { "ILVR", VideoCodec.H263 },
      { "S261", VideoCodec.H261 },
      { "H261", VideoCodec.H261 },
      { "D261", VideoCodec.H261 },
      { "L261", VideoCodec.H261 },
      { "M261", VideoCodec.H261 },
      { "IF09", VideoCodec.H261 },
      { "H260", VideoCodec.H260 },
      { "IR21", VideoCodec.Iv21 },
      { "IV30", VideoCodec.Iv30 },
      { "IV31", VideoCodec.Iv30 },
      { "IV32", VideoCodec.Iv30 },
      { "IV33", VideoCodec.Iv30 },
      { "IV34", VideoCodec.Iv30 },
      { "IV35", VideoCodec.Iv30 },
      { "IV36", VideoCodec.Iv30 },
      { "IV37", VideoCodec.Iv30 },
      { "IV38", VideoCodec.Iv30 },
      { "IV39", VideoCodec.Iv30 },
      { "IV40", VideoCodec.Iv40 },
      { "IV41", VideoCodec.Iv40 },
      { "IV42", VideoCodec.Iv40 },
      { "IV43", VideoCodec.Iv40 },
      { "IV44", VideoCodec.Iv40 },
      { "IV45", VideoCodec.Iv40 },
      { "IV46", VideoCodec.Iv40 },
      { "IV47", VideoCodec.Iv40 },
      { "IV48", VideoCodec.Iv40 },
      { "IV49", VideoCodec.Iv40 },
      { "IAN", VideoCodec.Iv40 },
      { "IV50", VideoCodec.Iv50 },
      { "RV10", VideoCodec.RealRv10 },
      { "RV13", VideoCodec.RealRv10 },
      { "RV20", VideoCodec.RealRv20 },
      { "RV30", VideoCodec.RealRv30 },
      { "RV40", VideoCodec.RealRv40 },
      { "FLV1", VideoCodec.Sprk },
      { "FLV4", VideoCodec.Vp6 },
      { "FFVH", VideoCodec.Ffvh },
      { "FFDS", VideoCodec.Ffds },
      { "FPS1", VideoCodec.Fraps },
      { "M4S2", VideoCodec.Mpeg4MsV2 },
      { "COL0", VideoCodec.Mpeg4MsV3 },
      { "COL1", VideoCodec.Mpeg4MsV3 },
      { "THEORA", VideoCodec.Theora },
      { "MJPG", VideoCodec.Mjpg },
      { "MPEG-4V", VideoCodec.Mpeg4 },
      { "3IV0", VideoCodec.Mpeg4 },
      { "3IV1", VideoCodec.Mpeg4 },
      { "3IV2", VideoCodec.Mpeg4 },
      { "3IVD", VideoCodec.Mpeg4 },
      { "3IVX", VideoCodec.Mpeg4 },
      { "3VID", VideoCodec.Mpeg4 },
      { "AP41", VideoCodec.Mpeg4 },
      { "AP42", VideoCodec.Mpeg4 },
      { "ATM4", VideoCodec.Mpeg4 },
      { "BLZ0", VideoCodec.Mpeg4 },
      { "DM4V", VideoCodec.Mpeg4 },
      { "DP02", VideoCodec.Mpeg4 },
      { "FMP4", VideoCodec.Mpeg4 },
      { "M4CC", VideoCodec.Mpeg4 },
      { "MP41", VideoCodec.Mpeg4MsV1 },
      { "MP42", VideoCodec.Mpeg4MsV2 },
      { "MP43", VideoCodec.Mpeg4MsV3 },
      { "DV", VideoCodec.Dv },
      { "HDV", VideoCodec.Hdv },
      { "WMV1", VideoCodec.Wmv1 },
      { "WMV2", VideoCodec.Wmv2 },
      { "WMV3", VideoCodec.Wmv3 },
    };

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoStreamBuilder"/> class.
    /// </summary>
    /// <param name="info">The media info object.</param>
    /// <param name="number">The stream number.</param>
    /// <param name="position">The stream position.</param>
    public VideoStreamBuilder(MediaInfo info, int number, int position)
      : base(info, number, position)
    {
    }

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Video;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Video;

    public override VideoStream Build()
    {
      var result = base.Build();
      result.FrameRate = Get<double>("FrameRate", double.TryParse);
      result.Width = Get<int>("Width", int.TryParse);
      result.Height = Get<int>("Height", int.TryParse);
      result.Bitrate = Get<double>("BitRate", double.TryParse);
      if (Math.Abs(result.Bitrate) < 1E-7)
      {
        result.Bitrate = Get<double>("BitRate_Maximum", double.TryParse);
      }
      result.AspectRatio = Get<AspectRatio>("DisplayAspectRatio", TryGetAspectRatio);
      result.Interlaced = GetInterlaced(Get("ScanType"));
      result.Stereoscopic = Get<int>("MultiView_Count", int.TryParse) >= 2
                       ? Get<StereoMode>("MultiView_Layout", TryGetStereoscopic)
                       : StereoMode.Mono;
      result.Format = Get("Format");
      result.Codec = Get<VideoCodec>("CodecID", TryGetCodecId);
      if (result.Codec == VideoCodec.Undefined)
      {
        result.Codec = Get<VideoCodec>("Codec", TryGetCodec);
      }

      result.Duration = TimeSpan.FromMilliseconds(Get<double>("Duration", double.TryParse));
      result.BitDepth = Get<int>("BitDepth", int.TryParse);
      result.CodecName = GetFullCodecName();

      return result;
    }

    private static bool TryGetCodecId(string codec, out VideoCodec result)
    {
      return VideoCodecs.TryGetValue(codec.ToUpper(), out result);
    }

    private static bool TryGetCodec(string codec, out VideoCodec result)
    {
      return VideoCodecs.TryGetValue(codec.ToUpper(), out result);
    }

    private static bool TryGetStereoscopic(string layout, out StereoMode result)
    {
      return StereoModes.TryGetValue(layout.ToLower(), out result);
    }

    private static bool GetInterlaced(string source)
    {
      return source?.ToLower().Contains("interlaced") ?? false;
    }

    private static bool TryGetAspectRatio(string source, out AspectRatio result)
    {
      return Ratios.TryGetValue(source, out result);
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
          var strCodecProf = Get("Format_Profile").ToUpper();
          if (strCodecProf != "MAIN@MAIN")
          {
            strCodec = (strCodec + " " + strCodecProf).Trim();
          }
        }
      }

      return strCodec.Replace("+", "PLUS");
    }
  }
}