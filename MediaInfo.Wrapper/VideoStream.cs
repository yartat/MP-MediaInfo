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
    [PublicAPI]
    public Size Size => new Size(Width, Height);

    /// <inheritdoc />
    protected override void AnalyzeInternal()
    {
      base.AnalyzeInternal();
      FrameRate = Get<double>("FrameRate", double.TryParse);
      Width = Get<int>("Width", int.TryParse);
      Height = Get<int>("Height", int.TryParse);
      AspectRatio = Get<AspectRatio>("DisplayAspectRatio", TryGetAspectRatio);
      Interlaced = GetInterlaced(Get("ScanType"));
      Stereoscopic = Get<int>("MultiView_Count", int.TryParse) >= 2
                       ? Get<StereoMode>("MultiView_Layout", TryGetStereoscopic)
                       : StereoMode.Mono;
      Format = Get("Format");
      Codec = Get<VideoCodec>("CodecID", TryGetCodecId);
      if (Codec == VideoCodec.Undefined)
      {
        Codec = Get<VideoCodec>("Codec", TryGetCodec);
      }

      Duration = TimeSpan.FromMilliseconds(Get<double>("Duration", double.TryParse));
      BitDepth = Get<int>("BitDepth", int.TryParse);
      CodecName = GetFullCodecName();
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