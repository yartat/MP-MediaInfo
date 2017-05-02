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
using System.Drawing;

using JetBrains.Annotations;

namespace MediaInfo
{
  /// <summary>
  /// Describes video aspect ratio
  /// </summary>
  public enum AspectRatio
  {
    /// <summary>
    /// The opaque (1:1)
    /// </summary>
    Opaque,

    /// <summary>
    /// The high end data graphics (5:4)
    /// </summary>
    HighEndDataGraphics,

    /// <summary>
    /// The full screen (4:3)
    /// </summary>
    FullScreen,

    /// <summary>
    /// The standard slides (3:3)
    /// </summary>
    StandardSlides,

    /// <summary>
    /// The digital SLR cameras (3:2)
    /// </summary>
    DigitalSlrCameras,

    /// <summary>
    /// The High Definition TV (16:9)
    /// </summary>
    HighDefinitionTv,

    /// <summary>
    /// The wide screen display (16:10)
    /// </summary>
    WideScreenDisplay,

    /// <summary>
    /// The wide screen (1.85:1)
    /// </summary>
    WideScreen,

    /// <summary>
    /// The cinema scope (21:9)
    /// </summary>
    CinemaScope
  }

  /// <summary>
  /// Describes 3D stereo mode
  /// </summary>
  public enum StereoMode
  {
    /// <summary>
    /// No 3D (mono)
    /// </summary>
    Mono,

    /// <summary>
    /// The side by side left eye is first
    /// </summary>
    SideBySideLeft,

    /// <summary>
    /// The top bottom right eye is first
    /// </summary>
    TopBottomRight,

    /// <summary>
    /// The top bottom left eye is first
    /// </summary>
    TopBottomLeft,

    /// <summary>
    /// The checkerboard right eye is first
    /// </summary>
    CheckerboardRight,

    /// <summary>
    /// The checkerboard left eye is first
    /// </summary>
    CheckerboardLeft,

    /// <summary>
    /// The row interleaved right eye is first
    /// </summary>
    RowInterleavedRight,

    /// <summary>
    /// The row interleaved left eye is first
    /// </summary>
    RowInterleavedLeft,

    /// <summary>
    /// The column interleaved right eye is first
    /// </summary>
    ColumnInterleavedRight,

    /// <summary>
    /// The column interleaved left eye is first
    /// </summary>
    ColumnInterleavedLeft,

    /// <summary>
    /// The anaglyph cyan-red
    /// </summary>
    AnaglyphCyanRed,

    /// <summary>
    /// The side by side right eye is first
    /// </summary>
    SideBySideRight,

    /// <summary>
    /// The anaglyph green-magenta
    /// </summary>
    AnaglyphGreenMagenta,

    /// <summary>
    /// The both eyes laced left eye is first
    /// </summary>
    BothEyesLacedLeft,

    /// <summary>
    /// The both eyes laced right eye is first
    /// </summary>
    BothEyesLacedRight
  }

  /// <summary>
  /// Describes type of video codecs
  /// </summary>
  public enum VideoCodec
  {
    V_UNDEFINED,
    V_UNCOMPRESSED,
    V_DIRAC,
    V_MPEG4,
    V_MPEG4_IS0_SP,
    V_MPEG4_IS0_ASP,
    V_MPEG4_IS0_AP,
    V_MPEG4_IS0_AVC,
    V_MPEG4_ISO_SP,
    V_MPEG4_ISO_ASP,
    V_MPEG4_ISO_AP,
    V_MPEG4_ISO_AVC,
    V_MPEGH_ISO_HEVC,
    V_MPEG4_MS_V1,
    V_MPEG4_MS_V2,
    V_MPEG4_MS_V3,
    V_VC1,
    V_MPEG1,
    V_MPEG2,
    V_PRORES,
    V_REAL_RV10,
    V_REAL_RV20,
    V_REAL_RV30,
    V_REAL_RV40,
    V_THEORA,
    V_VP6,
    V_VP8,
    V_VP9,
    V_DIVX1,
    V_DIVX2,
    V_DIVX3,
    V_DIVX4,
    V_DIVX50,
    V_XVID,
    V_SVQ1,
    V_SVQ2,
    V_SVQ3,
    V_SPRK,
    V_H260,
    V_H261,
    V_H263,
    V_AVDV,
    V_AVD1,
    V_FFV1,
    V_FFV2,
    V_IV21,
    V_IV30,
    V_IV40,
    V_IV50,
    V_FFDS,
    V_FRAPS,
    V_FFVH,
    V_MJPG,
    V_DV,
    V_HDV,
    V_DVCPRO50,
    V_DVCPRO100,
    V_WMV1,
    V_WMV2,
    V_WMV3,
    V_8BPS,
    V_BINKVIDEO,
  }

  /// <summary>
  /// Describes properties of the video stream and method to analyze stream
  /// </summary>
  /// <seealso cref="LanguageMediaStream" />
  public class VideoStream : LanguageMediaStream
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
      { "V_UNCOMPRESSED", VideoCodec.V_UNCOMPRESSED },
      { "V_DIRAC", VideoCodec.V_DIRAC },
      { "V_MPEG4/IS0/SP", VideoCodec.V_MPEG4_IS0_SP },
      { "V_MPEG4/IS0/ASP", VideoCodec.V_MPEG4_IS0_ASP },
      { "V_MPEG4/IS0/AP", VideoCodec.V_MPEG4_IS0_AP },
      { "V_MPEG4/IS0/AVC", VideoCodec.V_MPEG4_IS0_AVC },
      { "V_MPEG4/ISO/SP", VideoCodec.V_MPEG4_ISO_SP },
      { "V_MPEG4/ISO/ASP", VideoCodec.V_MPEG4_ISO_ASP },
      { "V_MPEG4/ISO/AP", VideoCodec.V_MPEG4_ISO_AP },
      { "V_MPEG4/ISO/AVC", VideoCodec.V_MPEG4_ISO_AVC },
      { "V_MPEGH/ISO/HEVC", VideoCodec.V_MPEGH_ISO_HEVC },
      { "V_MPEG4/MS/V2", VideoCodec.V_MPEG4_MS_V2 },
      { "V_MPEG4/MS/V3", VideoCodec.V_MPEG4_MS_V3 },
      { "V_MPEG1", VideoCodec.V_MPEG1 },
      { "V_MPEG2", VideoCodec.V_MPEG2 },
      { "V_PRORES", VideoCodec.V_PRORES },
      { "V_REAL/RV10", VideoCodec.V_REAL_RV10 },
      { "V_REAL/RV20", VideoCodec.V_REAL_RV20 },
      { "V_REAL/RV30", VideoCodec.V_REAL_RV30 },
      { "V_REAL/RV40", VideoCodec.V_REAL_RV40 },
      { "V_THEORA", VideoCodec.V_THEORA },
      { "V_VP8", VideoCodec.V_VP8 },
      { "V_VP9", VideoCodec.V_VP9 },
      { "AVC1", VideoCodec.V_MPEG4_ISO_AVC },
      { "AVC", VideoCodec.V_MPEG4_ISO_AVC },
      { "H264", VideoCodec.V_MPEG4_ISO_AVC },
      { "DAVC", VideoCodec.V_MPEG4_ISO_AVC },
      { "MPEG-2V", VideoCodec.V_MPEG2 },
      { "MPEG-2", VideoCodec.V_MPEG2 },
      { "MPEG-1", VideoCodec.V_MPEG1 },
      { "MPEG-1V", VideoCodec.V_MPEG1 },
      { "VC1", VideoCodec.V_VC1 },
      { "VC-1", VideoCodec.V_VC1 },
      { "OVC1", VideoCodec.V_VC1 },
      { "WVC1", VideoCodec.V_VC1 },
      { "SORENSON H263", VideoCodec.V_SPRK },
      { "SPRK", VideoCodec.V_SPRK },
      { "SVQ1", VideoCodec.V_SVQ1 },
      { "SVQ2", VideoCodec.V_SVQ2 },
      { "SVQ3", VideoCodec.V_SVQ3 },
      { "DX50", VideoCodec.V_DIVX50 },
      { "DVX1", VideoCodec.V_DIVX1 },
      { "DIV1", VideoCodec.V_DIVX1 },
      { "DVX2", VideoCodec.V_DIVX2 },
      { "DIV2", VideoCodec.V_DIVX2 },
      { "DVX3", VideoCodec.V_DIVX3 },
      { "DIV3", VideoCodec.V_DIVX3 },
      { "DIV4", VideoCodec.V_DIVX3 },
      { "DIV5", VideoCodec.V_DIVX50 },
      { "DIV6", VideoCodec.V_MPEG4_MS_V3 },
      { "DIVX", VideoCodec.V_DIVX4 },
      { "XVID", VideoCodec.V_XVID },
      { "FFV1", VideoCodec.V_FFV1 },
      { "FFV2", VideoCodec.V_FFV2 },
      { "S263", VideoCodec.V_H263 },
      { "H263", VideoCodec.V_H263 },
      { "D263", VideoCodec.V_H263 },
      { "L263", VideoCodec.V_H263 },
      { "M263", VideoCodec.V_H263 },
      { "ILVR", VideoCodec.V_H263 },
      { "S261", VideoCodec.V_H261 },
      { "H261", VideoCodec.V_H261 },
      { "D261", VideoCodec.V_H261 },
      { "L261", VideoCodec.V_H261 },
      { "M261", VideoCodec.V_H261 },
      { "IF09", VideoCodec.V_H261 },
      { "H260", VideoCodec.V_H260 },
      { "IR21", VideoCodec.V_IV21 },
      { "IV30", VideoCodec.V_IV30 },
      { "IV31", VideoCodec.V_IV30 },
      { "IV32", VideoCodec.V_IV30 },
      { "IV33", VideoCodec.V_IV30 },
      { "IV34", VideoCodec.V_IV30 },
      { "IV35", VideoCodec.V_IV30 },
      { "IV36", VideoCodec.V_IV30 },
      { "IV37", VideoCodec.V_IV30 },
      { "IV38", VideoCodec.V_IV30 },
      { "IV39", VideoCodec.V_IV30 },
      { "IV40", VideoCodec.V_IV40 },
      { "IV41", VideoCodec.V_IV40 },
      { "IV42", VideoCodec.V_IV40 },
      { "IV43", VideoCodec.V_IV40 },
      { "IV44", VideoCodec.V_IV40 },
      { "IV45", VideoCodec.V_IV40 },
      { "IV46", VideoCodec.V_IV40 },
      { "IV47", VideoCodec.V_IV40 },
      { "IV48", VideoCodec.V_IV40 },
      { "IV49", VideoCodec.V_IV40 },
      { "IAN", VideoCodec.V_IV40 },
      { "IV50", VideoCodec.V_IV50 },
      { "RV10", VideoCodec.V_REAL_RV10 },
      { "RV13", VideoCodec.V_REAL_RV10 },
      { "RV20", VideoCodec.V_REAL_RV20 },
      { "RV30", VideoCodec.V_REAL_RV30 },
      { "RV40", VideoCodec.V_REAL_RV40 },
      { "FLV1", VideoCodec.V_SPRK },
      { "FLV4", VideoCodec.V_VP6 },
      { "FFVH", VideoCodec.V_FFVH },
      { "FFDS", VideoCodec.V_FFDS },
      { "FPS1", VideoCodec.V_FRAPS },
      { "M4S2", VideoCodec.V_MPEG4_MS_V2 },
      { "COL0", VideoCodec.V_MPEG4_MS_V3 },
      { "COL1", VideoCodec.V_MPEG4_MS_V3 },
      { "THEORA", VideoCodec.V_THEORA },
      { "MJPG", VideoCodec.V_MJPG },
      { "MPEG-4V", VideoCodec.V_MPEG4 },
      { "3IV0", VideoCodec.V_MPEG4 },
      { "3IV1", VideoCodec.V_MPEG4 },
      { "3IV2", VideoCodec.V_MPEG4 },
      { "3IVD", VideoCodec.V_MPEG4 },
      { "3IVX", VideoCodec.V_MPEG4 },
      { "3VID", VideoCodec.V_MPEG4 },
      { "AP41", VideoCodec.V_MPEG4 },
      { "AP42", VideoCodec.V_MPEG4 },
      { "ATM4", VideoCodec.V_MPEG4 },
      { "BLZ0", VideoCodec.V_MPEG4 },
      { "DM4V", VideoCodec.V_MPEG4 },
      { "DP02", VideoCodec.V_MPEG4 },
      { "FMP4", VideoCodec.V_MPEG4 },
      { "M4CC", VideoCodec.V_MPEG4 },
      { "MP41", VideoCodec.V_MPEG4_MS_V1 },
      { "MP42", VideoCodec.V_MPEG4_MS_V2 },
      { "MP43", VideoCodec.V_MPEG4_MS_V3 },
      { "DV", VideoCodec.V_DV },
      { "HDV", VideoCodec.V_HDV },
      { "WMV1", VideoCodec.V_WMV1 },
      { "WMV2", VideoCodec.V_WMV2 },
      { "WMV3", VideoCodec.V_WMV3 },
    };

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoStream"/> class.
    /// </summary>
    /// <param name="info">The media information.</param>
    /// <param name="number">The stream number.</param>
    /// <param name="position">The stream position.</param>
    public VideoStream(MediaInfo info, int number, int position)
        : base(info, number, position)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoStream"/> class.
    /// </summary>
    /// <param name="number">The number.</param>
    /// <param name="position">The position.</param>
    public VideoStream(int number, int position)
        : base(null, number, position)
    {
    }

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Video;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Video;

    /// <summary>
    /// Gets or sets the video frame rate.
    /// </summary>
    /// <value>
    /// The video frame rate.
    /// </value>
    [PublicAPI]
    public double FrameRate { get; set; }

    /// <summary>
    /// Gets or sets the video width.
    /// </summary>
    /// <value>
    /// The video width.
    /// </value>
    [PublicAPI]
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the video height.
    /// </summary>
    /// <value>
    /// The video height.
    /// </value>
    [PublicAPI]
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the video aspect ratio.
    /// </summary>
    /// <value>
    /// The video aspect ratio.
    /// </value>
    [PublicAPI]
    public AspectRatio AspectRatio { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="VideoStream"/> is interlaced.
    /// </summary>
    /// <value>
    ///   <c>true</c> if interlaced; otherwise, <c>false</c>.
    /// </value>
    [PublicAPI]
    public bool Interlaced { get; set; }

    /// <summary>
    /// Gets or sets the video stereoscopic mode.
    /// </summary>
    /// <value>
    /// The video stereoscopic mode.
    /// </value>
    [PublicAPI]
    public StereoMode Stereoscopic { get; set; }

    /// <summary>
    /// Gets or sets the video format.
    /// </summary>
    /// <value>
    /// The video format.
    /// </value>
    [PublicAPI]
    public string Format { get; set; }

    /// <summary>
    /// Gets or sets the video codec.
    /// </summary>
    /// <value>
    /// The video codec.
    /// </value>
    [PublicAPI]
    public VideoCodec Codec { get; set; }

    /// <summary>
    /// Gets or sets the stream duration.
    /// </summary>
    /// <value>
    /// The stream duration.
    /// </value>
    [PublicAPI]
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the video bit depth.
    /// </summary>
    /// <value>
    /// The video bit depth.
    /// </value>
    [PublicAPI]
    public int BitDepth { get; set; }

    /// <summary>
    /// Gets or sets the name of the video codec.
    /// </summary>
    /// <value>
    /// The name of the video codec.
    /// </value>
    [PublicAPI]
    public string CodecName { get; set; }

    /// <summary>
    /// Gets the video resolution.
    /// </summary>
    /// <value>
    /// The video resolution.
    /// </value>
    public string Resolution => GetVideoResolution();

    /// <summary>
    /// Gets the video size.
    /// </summary>
    /// <value>
    /// The vidoe size.
    /// </value>
    public Size Size => new Size(Width, Height);

    /// <inheritdoc />
    protected override void AnalyzeStreamInternal(MediaInfo info)
    {
      base.AnalyzeStreamInternal(info);
      FrameRate = Get<double>(info, "FrameRate", double.TryParse);
      Width = Get<int>(info, "Width", int.TryParse);
      Height = Get<int>(info, "Height", int.TryParse);
      AspectRatio = GetAspectRatio(Get(info, "DisplayAspectRatio"));
      Interlaced = GetInterlaced(Get(info, "ScanType").ToLower());
      Stereoscopic = Get<int>(info, "MultiView_Count", int.TryParse) >= 2
                       ? GetStereoscopic(Get(info, "MultiView_Layout").ToLower())
                       : StereoMode.Mono;
      Format = Get(info, "Format");
      Codec = GetCodecId(Get(info, "CodecID"));
      if (Codec == VideoCodec.V_UNDEFINED)
      {
        Codec = GetCodec(Get(info, "Codec"));
      }

      Duration = TimeSpan.FromMilliseconds(Get<double>(info, "Duration", double.TryParse));
      BitDepth = Get<int>(info, "BitDepth", int.TryParse);
      CodecName = GetFullCodecName(info);
    }

    private static VideoCodec GetCodecId(string codec)
    {
      VideoCodec result;
      return VideoCodecs.TryGetValue(codec.ToUpper(), out result) ? result : VideoCodec.V_UNDEFINED;
    }

    private static VideoCodec GetCodec(string codec)
    {
      VideoCodec result;
      return VideoCodecs.TryGetValue(codec.ToUpper(), out result) ? result : VideoCodec.V_UNDEFINED;
    }

    private static StereoMode GetStereoscopic(string layout)
    {
      StereoMode result;
      return StereoModes.TryGetValue(layout, out result) ? result : StereoMode.Mono;
    }

    private static bool GetInterlaced(string source)
    {
      return source.Contains("interlaced");
    }

    private static AspectRatio GetAspectRatio(string source)
    {
      AspectRatio result;
      return Ratios.TryGetValue(source, out result) ? result : AspectRatio.Opaque;
    }

    private string GetVideoResolution()
    {
      string result;

      if (Width >= 7680 || Height >= 4320)
      {
        result = "4320";
      }
      else if (Width >= 3840 || Height >= 2160)
      {
        result = "2160";
      }
      else if (Width >= 1920 || Height >= 1080)
      {
        result = "1080";
      }
      else if (Width >= 1280 || Height >= 720)
      {
        result = "720";
      }
      else if (Height >= 576)
      {
        result = "576";
      }
      else if (Height >= 480)
      {
        result = "480";
      }
      else if (Height >= 360)
      {
        result = "360";
      }
      else if (Height >= 240)
      {
        result = "240";
      }
      else
      {
        result = "SD";
      }

      if (result != "SD")
      {
        result += Interlaced ? "I" : "P";
      }

      return result;
    }

    private string GetFullCodecName(MediaInfo mediaInfo)
    {
      var strCodec = mediaInfo.Get(StreamKind.Video, StreamPosition, "Format").ToUpper();
      var strCodecVer = mediaInfo.Get(StreamKind.Video, StreamPosition, "Format_Version").ToUpper();
      if (strCodec == "MPEG-4 VISUAL")
      {
        strCodec = mediaInfo.Get(StreamKind.Video, StreamPosition, "CodecID").ToUpperInvariant();
      }
      else
      {
        if (!string.IsNullOrEmpty(strCodecVer))
        {
          strCodec = (strCodec + " " + strCodecVer).Trim();
          var strCodecProf = mediaInfo.Get(StreamKind.Video, StreamPosition, "Format_Profile").ToUpper();
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