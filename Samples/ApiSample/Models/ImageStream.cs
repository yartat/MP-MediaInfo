#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models;

/// <summary>
/// Describes properties of the image stream
/// </summary>
/// <seealso cref="MediaStream" />
[DataContract]
public class ImageStream : MediaStream
{
    /// <summary>
    /// An image type, for example "Gain map". Empty for a regular image.
    /// </summary>
    [DataMember(Name = "type")]
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// An image format.
    /// </summary>
    [DataMember(Name = "format")]
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// An image format profile.
    /// </summary>
    [DataMember(Name = "formatProfile")]
    [JsonPropertyName("formatProfile")]
    public string? FormatProfile { get; set; }

    /// <summary>
    /// An image width.
    /// </summary>
    [DataMember(Name = "width")]
    [JsonPropertyName("width")]
    public int Width { get; set; }

    /// <summary>
    /// An image height.
    /// </summary>
    [DataMember(Name = "height")]
    [JsonPropertyName("height")]
    public int Height { get; set; }

    /// <summary>
    /// An image bit depth.
    /// </summary>
    [DataMember(Name = "bitDepth")]
    [JsonPropertyName("bitDepth")]
    public int BitDepth { get; set; }

    /// <summary>
    /// An image color space.
    /// </summary>
    [DataMember(Name = "colorSpace")]
    [JsonPropertyName("colorSpace")]
    public string? ColorSpace { get; set; }

    /// <summary>
    /// An image chroma subsampling.
    /// </summary>
    [DataMember(Name = "chromaSubsampling")]
    [JsonPropertyName("chromaSubsampling")]
    public string? ChromaSubsampling { get; set; }

    /// <summary>
    /// An image HDR type.
    /// </summary>
    [DataMember(Name = "hdr")]
    [JsonPropertyName("hdr")]
    public Hdr Hdr { get; set; }

    /// <summary>
    /// An image rotation in degrees.
    /// </summary>
    [DataMember(Name = "rotation")]
    [JsonPropertyName("rotation")]
    public double Rotation { get; set; }

    /// <summary>
    /// An image mirroring derived from the Exif orientation.
    /// </summary>
    [DataMember(Name = "mirror")]
    [JsonPropertyName("mirror")]
    public string? Mirror { get; set; }

    /// <summary>
    /// A number of slices per image, for example the cells of a HEIF grid.
    /// </summary>
    [DataMember(Name = "sliceCount")]
    [JsonPropertyName("sliceCount")]
    public int SliceCount { get; set; }

    /// <summary>
    /// An image this image is derived from.
    /// </summary>
    [DataMember(Name = "derivedFrom")]
    [JsonPropertyName("derivedFrom")]
    public string? DerivedFrom { get; set; }

    /// <summary>
    /// ISO 21496-1 gain map metadata, if any.
    /// </summary>
    [DataMember(Name = "gainMap")]
    [JsonPropertyName("gainMap")]
    public GainMapInfo? GainMap { get; set; }
}

/// <summary>
/// Describes the ISO 21496-1 gain map metadata of an image.
/// </summary>
[DataContract]
public sealed class GainMapInfo
{
    /// <summary>
    /// An HDR headroom of the base image.
    /// </summary>
    [DataMember(Name = "baseHeadroom")]
    [JsonPropertyName("baseHeadroom")]
    public string? BaseHeadroom { get; set; }

    /// <summary>
    /// An HDR headroom of the alternate image.
    /// </summary>
    [DataMember(Name = "alternateHeadroom")]
    [JsonPropertyName("alternateHeadroom")]
    public string? AlternateHeadroom { get; set; }

    /// <summary>
    /// A minimum gain map value.
    /// </summary>
    [DataMember(Name = "min")]
    [JsonPropertyName("min")]
    public string? Min { get; set; }

    /// <summary>
    /// A maximum gain map value.
    /// </summary>
    [DataMember(Name = "max")]
    [JsonPropertyName("max")]
    public string? Max { get; set; }

    /// <summary>
    /// A gain map gamma.
    /// </summary>
    [DataMember(Name = "gamma")]
    [JsonPropertyName("gamma")]
    public string? Gamma { get; set; }

    /// <summary>
    /// An offset of the base image.
    /// </summary>
    [DataMember(Name = "baseOffset")]
    [JsonPropertyName("baseOffset")]
    public string? BaseOffset { get; set; }

    /// <summary>
    /// An offset of the alternate image.
    /// </summary>
    [DataMember(Name = "alternateOffset")]
    [JsonPropertyName("alternateOffset")]
    public string? AlternateOffset { get; set; }

    /// <summary>
    /// A value indicating whether the base color space is used to apply the gain map.
    /// </summary>
    [DataMember(Name = "useBaseColorSpace")]
    [JsonPropertyName("useBaseColorSpace")]
    public bool UseBaseColorSpace { get; set; }
}
