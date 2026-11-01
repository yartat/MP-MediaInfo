#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

namespace MediaInfo.Model
{
  /// <summary>
  /// Describes properties of the image stream
  /// </summary>
  /// <seealso cref="MediaStream" />
  public class ImageStream : MediaStream
  {
    /// <summary>
    /// Gets or sets the image type, for example "Gain map". Empty for a regular image.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the image format, for example "JPEG" or "HEIF".
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the image format profile.
    /// </summary>
    public string FormatProfile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the width of the image in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the image in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the bit depth of the image.
    /// </summary>
    public int BitDepth { get; set; }

    /// <summary>
    /// Gets or sets the color space of the image, for example "YUV" or "RGB".
    /// </summary>
    public string ColorSpace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the chroma subsampling of the image, for example "4:2:0".
    /// </summary>
    public string ChromaSubsampling { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HDR format of the image.
    /// </summary>
    public Hdr Hdr { get; set; }

    /// <summary>
    /// Gets or sets the rotation of the image in degrees, derived from the Exif orientation.
    /// </summary>
    public double Rotation { get; set; }

    /// <summary>
    /// Gets or sets the number of slices per image (for example, the cells of a HEIF grid). 0 when not reported.
    /// </summary>
    public int SliceCount { get; set; }

    /// <summary>
    /// Gets or sets the ISO 21496-1 gain map metadata, or <see langword="null"/> when the image has none.
    /// </summary>
    public GainMapInfo? GainMap { get; set; }

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Image;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Image;
  }

  /// <summary>
  /// Describes the ISO 21496-1 gain map metadata of an image. Per-channel values are kept as reported by the library.
  /// </summary>
  public sealed class GainMapInfo
  {
    /// <summary>Gets the HDR headroom of the base image.</summary>
    public string BaseHeadroom { get; init; } = string.Empty;

    /// <summary>Gets the HDR headroom of the alternate image.</summary>
    public string AlternateHeadroom { get; init; } = string.Empty;

    /// <summary>Gets the minimum gain map value.</summary>
    public string Min { get; init; } = string.Empty;

    /// <summary>Gets the maximum gain map value.</summary>
    public string Max { get; init; } = string.Empty;

    /// <summary>Gets the gain map gamma.</summary>
    public string Gamma { get; init; } = string.Empty;

    /// <summary>Gets the offset of the base image.</summary>
    public string BaseOffset { get; init; } = string.Empty;

    /// <summary>Gets the offset of the alternate image.</summary>
    public string AlternateOffset { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether the base color space is used to apply the gain map.</summary>
    public bool UseBaseColorSpace { get; init; }
  }
}
