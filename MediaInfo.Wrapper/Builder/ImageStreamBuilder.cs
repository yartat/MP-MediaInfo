#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using MediaInfo.Model;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Builds an image stream representation from the provided media information.
  /// </summary>
  /// <param name="info">The media information source used to extract image stream data.</param>
  /// <param name="number">The stream number identifying the image stream within the media information.</param>
  /// <param name="position">The position index of the stream within the media information.</param>
  internal class ImageStreamBuilder(IMediaInfoReader info, int number, int position) : MediaStreamBuilder<ImageStream>(info, number, position)
  {
    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Image;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Image;

    /// <inheritdoc />
    public override ImageStream Build()
    {
      var result = base.Build();
      result.Type = Get("Type");
      result.Format = Get((int)NativeMethods.Image.Image_Format, InfoKind.Text);
      result.FormatProfile = Get((int)NativeMethods.Image.Image_Format_Profile, InfoKind.Text);
      result.Width = Get<int>((int)NativeMethods.Image.Image_Width, InfoKind.Text, TagBuilderHelper.TryGetInt);
      result.Height = Get<int>((int)NativeMethods.Image.Image_Height, InfoKind.Text, TagBuilderHelper.TryGetInt);
      result.BitDepth = Get<int>((int)NativeMethods.Image.Image_BitDepth, InfoKind.Text, TagBuilderHelper.TryGetInt);
      result.ColorSpace = Get((int)NativeMethods.Image.Image_ColorSpace, InfoKind.Text);
      result.ChromaSubsampling = Get((int)NativeMethods.Image.Image_ChromaSubsampling, InfoKind.Text);
      result.Hdr = Get<Hdr>((int)NativeMethods.Image.Image_HDR_Format, InfoKind.Text, HdrUtils.TryGetHdr);
      result.Rotation = Get<double>("Rotation", TagBuilderHelper.TryGetDouble);
      result.SliceCount = Get<int>("Format_Settings_SliceCount", TagBuilderHelper.TryGetInt);

      var baseHeadroom = Get(GainMapField.BaseHeadroom);
      if (baseHeadroom.Length > 0)
      {
        result.GainMap = new GainMapInfo
        {
          BaseHeadroom = baseHeadroom,
          AlternateHeadroom = Get(GainMapField.AlternateHeadroom),
          Min = Get(GainMapField.Min),
          Max = Get(GainMapField.Max),
          Gamma = Get(GainMapField.Gamma),
          BaseOffset = Get(GainMapField.BaseOffset),
          AlternateOffset = Get(GainMapField.AlternateOffset),
          UseBaseColorSpace = Get<bool>(GainMapField.UseBaseColorSpace.ToParameterName(), TagBuilderHelper.TryGetBool)
        };
      }

      return result;
    }

    private string Get(GainMapField field) => Get(field.ToParameterName());
  }
}
