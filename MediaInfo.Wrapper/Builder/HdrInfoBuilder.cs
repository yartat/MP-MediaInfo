#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using MediaInfo.Model;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Builds the HDR properties of a video stream, including the Dolby Vision metadata.
  /// </summary>
  /// <param name="info">The media information source.</param>
  /// <param name="streamPosition">The position of the video stream within the media information.</param>
  internal class HdrInfoBuilder(IMediaInfoReader info, int streamPosition)
  {
    /// <summary>
    /// Builds the HDR properties of the video stream.
    /// </summary>
    /// <returns>Returns the HDR properties. The format is <see cref="Hdr.None"/> when the stream is not HDR.</returns>
    public HdrInfo Build() => new()
    {
      Format = GetFormat(),
      Commercial = Get(NativeMethods.Video.Video_HDR_Format_Commercial),
      Version = Get(NativeMethods.Video.Video_HDR_Format_Version),
      Profile = Get(NativeMethods.Video.Video_HDR_Format_Profile),
      Level = Get(NativeMethods.Video.Video_HDR_Format_Level),
      Settings = Get(NativeMethods.Video.Video_HDR_Format_Settings),
      Compression = Get(NativeMethods.Video.Video_HDR_Format_Compression),
      Compatibility = Get(NativeMethods.Video.Video_HDR_Format_Compatibility),
      MasteringDisplayColorPrimaries = Get(NativeMethods.Video.Video_MasteringDisplay_ColorPrimaries),
      MasteringDisplayLuminance = Get(NativeMethods.Video.Video_MasteringDisplay_Luminance),
      MaxCll = Get(NativeMethods.Video.Video_MaxCLL),
      MaxFall = Get(NativeMethods.Video.Video_MaxFALL),
      DolbyVision = GetDolbyVision()
    };

    private Hdr GetFormat()
    {
      foreach (var parameter in new[]
               {
                 NativeMethods.Video.Video_HDR_Format,
                 NativeMethods.Video.Video_HDR_Format_Compatibility,
                 NativeMethods.Video.Video_transfer_characteristics
               })
      {
        if (HdrUtils.TryGetHdr(Get(parameter), out var format) && format != Hdr.None)
        {
          return format;
        }
      }

      return Hdr.None;
    }

    private DolbyVisionInfo? GetDolbyVision()
    {
      var summary = Get(DolbyVisionField.Summary);
      if (summary.Length == 0)
      {
        return null;
      }

      return new DolbyVisionInfo
      {
        Summary = summary,
        Profile = Get(DolbyVisionField.Profile),
        EnhancementLayer = Get(DolbyVisionField.EnhancementLayer),
        BitDepth = Get(DolbyVisionField.BitDepth),
        BaseLayerColorRange = Get(DolbyVisionField.BaseLayer_ColorRange),
        CmVersion = Get(DolbyVisionField.CMVersion),
        DmVersion = Get(DolbyVisionField.DMVersion),
        MasteringDisplayName = Get(DolbyVisionField.MasteringDisplay_Name),
        MasteringDisplayId = Get(DolbyVisionField.MasteringDisplay_ID),
        MasteringDisplayColorPrimaries = Get(DolbyVisionField.MasteringDisplay_ColorPrimaries),
        MasteringDisplayLuminance = Get(DolbyVisionField.MasteringDisplay_Luminance),
        MasteringDisplayDiagonal = Get(DolbyVisionField.MasteringDisplay_Diagonal),
        TargetDisplays = Get(DolbyVisionField.TargetDisplays),
        ActiveArea = Get(DolbyVisionField.ActiveArea),
        ActiveDisplayAspectRatio = Get(DolbyVisionField.Active_DisplayAspectRatio),
        ContentType = Get(DolbyVisionField.Content_Type),
        WhitePoint = Get(DolbyVisionField.WhitePoint),
        ReferenceMode = Get(DolbyVisionField.ReferenceMode),
        MasteringDisplayLuminanceHdr10 = Get(DolbyVisionField.MasteringDisplay_Luminance_HDR10),
        ShotCount = Get(DolbyVisionField.ShotCount)
      };
    }

    private string Get(NativeMethods.Video parameter) =>
      info.Get(StreamKind.Video, streamPosition, (int)parameter, InfoKind.Text) ?? string.Empty;

    private string Get(DolbyVisionField field) =>
      info.Get(StreamKind.Video, streamPosition, field.ToParameterName()) ?? string.Empty;
  }
}
