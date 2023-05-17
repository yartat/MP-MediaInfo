#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using MediaInfo.Model;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Describes base methods to build video stream.
  /// </summary>
  internal class VideoStreamBuilder : LanguageMediaStreamBuilder<VideoStream>
  {
    #region match dictionaries

    private static readonly Dictionary<string, VideoStandard> VideoStandards = new Dictionary<string, VideoStandard>(StringComparer.OrdinalIgnoreCase)
    {
      { "NTSC", VideoStandard.NTSC },
      { "PAL", VideoStandard.PAL },
    };

    private static readonly Dictionary<string, ChromaSubSampling> ChromaSubSamplings = new Dictionary<string, ChromaSubSampling>(StringComparer.OrdinalIgnoreCase)
    {
      { "3:3:2", ChromaSubSampling.Sampling332 },
      { "4:1:0", ChromaSubSampling.Sampling410 },
      { "4:1:0 (4x4)", ChromaSubSampling.Sampling410 },
      { "4:1:1", ChromaSubSampling.Sampling411 },
      { "4:2:0", ChromaSubSampling.Sampling420 },
      { "4:2:2", ChromaSubSampling.Sampling422 },
      { "4:4:4", ChromaSubSampling.Sampling444 },
      { "4:4:4:4", ChromaSubSampling.Sampling4444 },
      { "5:5:5", ChromaSubSampling.Sampling555 },
      { "5:6:5", ChromaSubSampling.Sampling565 },
      { "8:8:8", ChromaSubSampling.Sampling888 },
    };

    private static readonly Dictionary<string, ColorSpace> ColorSpaces = new Dictionary<string, ColorSpace>(StringComparer.OrdinalIgnoreCase)
    {
      { "Display P3", ColorSpace.DisplayP3 },
      { "DCI P3", ColorSpace.DCIP3 },
      { "Printing density", ColorSpace.PrintingDensity },
      { "SMPTE 274M", ColorSpace.SMPTE274M },
      { "BT.709", ColorSpace.BT709 },
      { "BT.601 PAL", ColorSpace.BT601 },
      { "BT.601 NTSC", ColorSpace.BT601 },
      { "Composite NTSC", ColorSpace.NTSC },
      { "Composite PAL", ColorSpace.PAL },
      { "BT.2020", ColorSpace.BT2020 },
      { "BT.1361", ColorSpace.BT1361 },
      { "BT.2100", ColorSpace.BT2100 },
      { "XYZ", ColorSpace.XYZ },
      { "BT.470 System M", ColorSpace.BT470M },
      { "SMPTE 240M", ColorSpace.SMPTE240M },
      { "Generic film", ColorSpace.Generic },
      { "EBU Tech 3213", ColorSpace.EBUTech3213 }
    };

    private static readonly Dictionary<string, TransferCharacteristic> TransferCharacteristics = new Dictionary<string, TransferCharacteristic>(StringComparer.OrdinalIgnoreCase)
    {
      { "Printing density", TransferCharacteristic.PrintingDensity },
      { "SMPTE 274M", TransferCharacteristic.SMPTE274M },
      { "BT.709", TransferCharacteristic.BT709 },
      { "BT.601 PAL", TransferCharacteristic.BT601PAL },
      { "BT.601 NTSC", TransferCharacteristic.BT601NTSC },
      { "Composite PAL", TransferCharacteristic.CompositePAL },
      { "Composite NTSC", TransferCharacteristic.CompositeNTSC },
      { "Z (depth) - homogeneous", TransferCharacteristic.ZHomogeneous },
      { "Z (depth) - linear", TransferCharacteristic.ZLinear }
    };

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

    private static readonly Dictionary<string, StereoMode> StereoModes = new Dictionary<string, StereoMode>(StringComparer.OrdinalIgnoreCase)
    {
      { "side-by-side (left eye first)", StereoMode.SideBySideLeft },
      { "top-bottom (right eye first)", StereoMode.TopBottomRight },
      { "top-bottom (left eye first)", StereoMode.TopBottomLeft },
      { "checkerboard (right eye first)", StereoMode.CheckerboardRight },
      { "checkerboard (left eye first)", StereoMode.CheckerboardLeft },
      { "row interleaved (right eye first)", StereoMode.RowInterleavedRight },
      { "row interleaved (left eye first)", StereoMode.RowInterleavedLeft },
      { "column interleaved (right eye first)", StereoMode.ColumnInterleavedRight },
      { "column interleaved (left eye first)", StereoMode.ColumnInterleavedLeft },
      { "anaglyph (cyan/red)", StereoMode.AnaglyphCyanRed },
      { "side-by-side (right eye first)", StereoMode.SideBySideRight },
      { "anaglyph (green/magenta)", StereoMode.AnaglyphGreenMagenta },
      { "both eyes laced in one block (left eye first)", StereoMode.BothEyesLacedLeft },
      { "both eyes laced in one block (right eye first)", StereoMode.BothEyesLacedRight }
    };

    private static readonly Dictionary<string, Hdr> HdrFormats = new Dictionary<string, Hdr>(StringComparer.OrdinalIgnoreCase)
    {
      { "Dolby Vision", Hdr.DolbyVision },
      { "HDR10", Hdr.HDR10 },
      { "PQ", Hdr.HDR10 },
      { "SMPTE ST 2086", Hdr.HDR10 },
      { "HDR10 Plus", Hdr.HDR10Plus },
      { "HDR10+", Hdr.HDR10Plus },
      { "SL-HDR", Hdr.SLHDR1 },
      { "HLG", Hdr.HLG },
    };

    private static readonly Dictionary<string, VideoCodec> VideoCodecs = new Dictionary<string, VideoCodec>(StringComparer.OrdinalIgnoreCase)
    {
      { "V_UNCOMPRESSED", VideoCodec.Uncompressed },
      { "V_DIRAC", VideoCodec.Dirac },
      { "DIRAC", VideoCodec.Dirac },
      { "MPEG-4 Visual", VideoCodec.Mpeg4 },
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
      { "V_MS/VFW/FOURCC / DX50", VideoCodec.Divx50 },
      { "V_MS/VFW/FOURCC / DVX1", VideoCodec.Divx1 },
      { "V_MS/VFW/FOURCC / DIV1", VideoCodec.Divx1 },
      { "V_MS/VFW/FOURCC / DVX2", VideoCodec.Divx2 },
      { "V_MS/VFW/FOURCC / DIV2", VideoCodec.Divx2 },
      { "V_MS/VFW/FOURCC / DVX3", VideoCodec.Divx3 },
      { "V_MS/VFW/FOURCC / DIV3", VideoCodec.Divx3 },
      { "V_MS/VFW/FOURCC / DIV4", VideoCodec.Divx4 },
      { "V_MS/VFW/FOURCC / DIV5", VideoCodec.Divx50 },
      { "V_MS/VFW/FOURCC / DIV6", VideoCodec.Mpeg4MsV3 },
      { "V_MS/VFW/FOURCC / DIVX", VideoCodec.Divx4 },
      { "V_MPEG1", VideoCodec.Mpeg1 },
      { "MPEG1", VideoCodec.Mpeg1 },
      { "MPEG-1", VideoCodec.Mpeg1 },
      { "MPEG-1V", VideoCodec.Mpeg1 },
      { "V_MPEG2", VideoCodec.Mpeg2 },
      { "MPEG2", VideoCodec.Mpeg2 },
      { "MPEG-2V", VideoCodec.Mpeg2 },
      { "MPEG-2", VideoCodec.Mpeg2 },
      { "MPEG Video", VideoCodec.Mpeg2 },
      { "V_PRORES", VideoCodec.ProRes },
      { "PRORES", VideoCodec.ProRes },
      { "V_REAL/RV10", VideoCodec.RealRv10 },
      { "V_REAL/RV20", VideoCodec.RealRv20 },
      { "V_REAL/RV30", VideoCodec.RealRv30 },
      { "V_REAL/RV40", VideoCodec.RealRv40 },
      { "V_THEORA", VideoCodec.Theora },
      { "V_VP8", VideoCodec.Vp8 },
      { "VP8", VideoCodec.Vp8 },
      { "V_VP9", VideoCodec.Vp9 },
      { "VP9", VideoCodec.Vp9 },
      { "AVC1", VideoCodec.Mpeg4IsoAvc },
      { "AVC", VideoCodec.Mpeg4IsoAvc },
      { "H264", VideoCodec.Mpeg4IsoAvc },
      { "H.264", VideoCodec.Mpeg4IsoAvc },
      { "DAVC", VideoCodec.Mpeg4IsoAvc },
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
      { "HuffYUV", VideoCodec.HuffYUV },
      { "S263", VideoCodec.H263 },
      { "H263", VideoCodec.H263 },
      { "H.263", VideoCodec.H263 },
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
      { "DVHE", VideoCodec.MpeghIsoHevc },
      { "DVAV", VideoCodec.Mpeg4IsoAvc },
      { "HEVC", VideoCodec.MpeghIsoHevc },
      { "AV01", VideoCodec.Av1 },
      { "AV1", VideoCodec.Av1 },
      { "JPEG", VideoCodec.Mjpg },
      { "Default (H.263)", VideoCodec.H263 },
    };

    private static readonly Dictionary<string, FrameRateMode> FrameRateModes = new Dictionary<string, FrameRateMode>(StringComparer.OrdinalIgnoreCase)
    {
      { "CFR", FrameRateMode.Constant },
      { "VFR", FrameRateMode.Variable }
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
      result.FrameRate = Get<double>((int)NativeMethods.Video.Video_FrameRate, InfoKind.Text, TagBuilderHelper.TryGetDouble);
      result.FrameRateMode = Get<FrameRateMode>((int)NativeMethods.Video.Video_FrameRate_Mode, InfoKind.Text, TryGetFrameRateMode);
      result.Width = Get<int>((int)NativeMethods.Video.Video_Width, InfoKind.Text, TagBuilderHelper.TryGetInt);
      result.Height = Get<int>((int)NativeMethods.Video.Video_Height, InfoKind.Text, TagBuilderHelper.TryGetInt);
      result.Bitrate = Get<double>((int)NativeMethods.Video.Video_BitRate, InfoKind.Text, TagBuilderHelper.TryGetDouble);
      if (Math.Abs(result.Bitrate) < 1E-7)
      {
        result.Bitrate = Get<double>((int)NativeMethods.Video.Video_BitRate_Maximum, InfoKind.Text, TagBuilderHelper.TryGetDouble);
      }
      result.AspectRatio = Get<AspectRatio>((int)NativeMethods.Video.Video_DisplayAspectRatio, InfoKind.Text, TryGetAspectRatio);
      result.Interlaced = GetInterlaced(Get((int)NativeMethods.Video.Video_ScanType, InfoKind.Text));
      var multiViewCount = Get<int>((int)NativeMethods.Video.Video_MultiView_Count, InfoKind.Text, TagBuilderHelper.TryGetInt);
      result.Stereoscopic = multiViewCount >= 2
                       ? Get<StereoMode>((int)NativeMethods.Video.Video_MultiView_Layout, InfoKind.Text, TryGetStereoscopic)
                       : StereoMode.Mono;
      if (result.Stereoscopic == StereoMode.Mono)
      {
        // Support Matroska stereo
        if (multiViewCount >= 2)
        {
          result.Stereoscopic = StereoMode.Stereo;
        }
        else
        {
          // Support BD3D
          var idValues = Get((int)NativeMethods.Video.Video_ID, InfoKind.Text);
          if (idValues.Split('/').Count(x => !string.IsNullOrWhiteSpace(x)) > 1)
          {
            result.Stereoscopic = StereoMode.Stereo;
          }
        }
      }

      result.Format = Get((int)NativeMethods.Video.Video_Format, InfoKind.Text);
      result.Codec = Get<VideoCodec>((int)NativeMethods.Video.Video_Format, InfoKind.Text, TryGetCodecId);
      if (result.Codec == VideoCodec.Undefined)
      {
        result.Codec = Get<VideoCodec>((int)NativeMethods.Video.Video_Codec, InfoKind.Text, TryGetCodec);
        if (result.Codec == VideoCodec.Undefined)
        {
          result.Codec = Get<VideoCodec>((int)NativeMethods.Video.Video_CodecID, InfoKind.Text, TryGetCodec);
        }
      }

      switch (result.Codec)
      {
        case VideoCodec.Mpeg4:
          var additionalCodec = Get<VideoCodec>((int)NativeMethods.Video.Video_CodecID, InfoKind.Text, TryGetCodec);
          if (additionalCodec != VideoCodec.Undefined)
          {
            result.Codec = additionalCodec;
            break;
          }

          additionalCodec = Get<VideoCodec>((int)NativeMethods.Video.Video_Format_Settings_Matrix, InfoKind.Text, TryGetCodec);
          if (additionalCodec != VideoCodec.Undefined)
          {
            result.Codec = additionalCodec;
          }
          break;
      }

      result.CodecProfile = Get((int)NativeMethods.Video.Video_Format_Profile, InfoKind.Text);
      result.Duration = TimeSpan.FromMilliseconds(Get<double>((int)NativeMethods.Video.Video_Duration, InfoKind.Text, TagBuilderHelper.TryGetDouble));
      result.BitDepth = Get<int>((int)NativeMethods.Video.Video_BitDepth, InfoKind.Text, TagBuilderHelper.TryGetInt);
      result.ColorSpace = Get<ColorSpace>((int)NativeMethods.Video.Video_colour_primaries, InfoKind.Text, TryGetColorSpace);
      result.TransferCharacteristics = Get<TransferCharacteristic>((int)NativeMethods.Video.Video_transfer_characteristics, InfoKind.Text, TryGetTransferCharacteristics);
      result.Standard = Get<VideoStandard>((int)NativeMethods.Video.Video_Standard, InfoKind.Text, TryGetStandard);
      result.SubSampling = Get<ChromaSubSampling>((int)NativeMethods.Video.Video_ChromaSubsampling, InfoKind.Text, TryGetSubSampling);
      result.CodecName = GetFullCodecName(result.CodecProfile);
      result.Hdr = Get<Hdr>((int)NativeMethods.Video.Video_HDR_Format, InfoKind.Text, TryGetHdr);
      if (result.Hdr == Hdr.None)
      {
        result.Hdr = Get<Hdr>((int)NativeMethods.Video.Video_transfer_characteristics, InfoKind.Text, TryGetHdr);
      }

      result.Tags = new VideoTagBuilder(Info, StreamPosition).Build();
            
      result.FrameRate = Get<double>((int)NativeMethods.Video.Video_FrameRate, InfoKind.Text, TagBuilderHelper.TryGetDouble);
      result.Rotation = Get<double>((int)NativeMethods.Video.Video_Rotation, InfoKind.Text, TagBuilderHelper.TryGetDouble);

      return result;
    }

    private static bool TryGetCodecId(string codec, out VideoCodec result) =>
      VideoCodecs.TryGetValue(codec, out result);

    private static bool TryGetCodec(string codec, out VideoCodec result) =>
      VideoCodecs.TryGetValue(codec, out result);

    private static bool TryGetStereoscopic(string layout, out StereoMode result) =>
      StereoModes.TryGetValue(layout, out result);

    private static bool GetInterlaced(string source) =>
      source?.ToLower().Contains("interlaced") ?? false;

    private static bool TryGetAspectRatio(string source, out AspectRatio result) =>
      Ratios.TryGetValue(source, out result);

    private static bool TryGetColorSpace(string source, out ColorSpace result) =>
      ColorSpaces.TryGetValue(source, out result);

    private static bool TryGetTransferCharacteristics(string source, out TransferCharacteristic result) =>
    TransferCharacteristics.TryGetValue(source, out result);

    private static bool TryGetStandard(string source, out VideoStandard result) =>
      VideoStandards.TryGetValue(source, out result);

    private static bool TryGetSubSampling(string source, out ChromaSubSampling result) =>
      ChromaSubSamplings.TryGetValue(source, out result);

    private static bool TryGetHdr(string source, out Hdr result) =>
      HdrFormats.TryGetValue(source, out result);

    private static bool TryGetFrameRateMode(string source, out FrameRateMode result) =>
      FrameRateModes.TryGetValue(source, out result);

    private string GetFullCodecName(string codecProfile)
    {
      var strFormat = Get((int)NativeMethods.Video.Video_Format_Commercial, InfoKind.Text);
      var strCodec = Get((int)NativeMethods.Video.Video_CodecID, InfoKind.Text);

      var strCodecVer = Get((int)NativeMethods.Video.Video_Format_Version, InfoKind.Text).ToUpper();
      if (strCodec == "MPEG-4 VISUAL")
      {
        strCodec = Get((int)NativeMethods.Video.Video_CodecID, InfoKind.Text).ToUpperInvariant();
      }
      else
      {
        if (!string.IsNullOrEmpty(strCodecVer))
        {
          strCodec = (strCodec + " " + strCodecVer).Trim();
          var strCodecProf = Get((int)NativeMethods.Video.Video_Format_Profile, InfoKind.Text).ToUpper();
          if (strCodecProf != "MAIN@MAIN")
          {
            strCodec = (strCodec + " " + strCodecProf).Trim();
          }
        }
      }

      return strFormat + (string.IsNullOrEmpty(codecProfile) ? string.Empty : " " + codecProfile);
      //return strCodec.Replace("+", "PLUS");
    }
  }
}