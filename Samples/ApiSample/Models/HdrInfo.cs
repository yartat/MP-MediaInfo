#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models;

/// <summary>
/// Describes the HDR properties of a video stream.
/// </summary>
[DataContract]
public sealed class HdrInfo
{
    /// <summary>
    /// A detected HDR format.
    /// </summary>
    [DataMember(Name = "format")]
    [JsonPropertyName("format")]
    public Hdr Format { get; set; }

    /// <summary>
    /// A commercial name of the HDR format.
    /// </summary>
    [DataMember(Name = "commercial")]
    [JsonPropertyName("commercial")]
    public string? Commercial { get; set; }

    /// <summary>
    /// A version of the HDR format.
    /// </summary>
    [DataMember(Name = "version")]
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// A profile of the HDR format, for example "dvhe.08".
    /// </summary>
    [DataMember(Name = "profile")]
    [JsonPropertyName("profile")]
    public string? Profile { get; set; }

    /// <summary>
    /// A level of the HDR format.
    /// </summary>
    [DataMember(Name = "level")]
    [JsonPropertyName("level")]
    public string? Level { get; set; }

    /// <summary>
    /// Settings (layers) of the HDR format, for example "BL+RPU".
    /// </summary>
    [DataMember(Name = "settings")]
    [JsonPropertyName("settings")]
    public string? Settings { get; set; }

    /// <summary>
    /// A compression of the HDR format.
    /// </summary>
    [DataMember(Name = "compression")]
    [JsonPropertyName("compression")]
    public string? Compression { get; set; }

    /// <summary>
    /// A compatibility of the HDR format with commercial formats, for example "HDR10".
    /// </summary>
    [DataMember(Name = "compatibility")]
    [JsonPropertyName("compatibility")]
    public string? Compatibility { get; set; }

    /// <summary>
    /// Color primaries of the mastering display.
    /// </summary>
    [DataMember(Name = "masteringDisplayColorPrimaries")]
    [JsonPropertyName("masteringDisplayColorPrimaries")]
    public string? MasteringDisplayColorPrimaries { get; set; }

    /// <summary>
    /// A luminance range of the mastering display.
    /// </summary>
    [DataMember(Name = "masteringDisplayLuminance")]
    [JsonPropertyName("masteringDisplayLuminance")]
    public string? MasteringDisplayLuminance { get; set; }

    /// <summary>
    /// A maximum content light level (MaxCLL).
    /// </summary>
    [DataMember(Name = "maxCll")]
    [JsonPropertyName("maxCll")]
    public string? MaxCll { get; set; }

    /// <summary>
    /// A maximum frame-average light level (MaxFALL).
    /// </summary>
    [DataMember(Name = "maxFall")]
    [JsonPropertyName("maxFall")]
    public string? MaxFall { get; set; }

    /// <summary>
    /// Dolby Vision metadata, if any.
    /// </summary>
    [DataMember(Name = "dolbyVision")]
    [JsonPropertyName("dolbyVision")]
    public DolbyVisionInfo? DolbyVision { get; set; }
}

/// <summary>
/// Describes the Dolby Vision metadata of a video stream.
/// </summary>
[DataContract]
public sealed class DolbyVisionInfo
{
    /// <summary>A summary, for example "Profile 8.1, MEL".</summary>
    [DataMember(Name = "summary")]
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>A Dolby Vision profile, for example "8.1".</summary>
    [DataMember(Name = "profile")]
    [JsonPropertyName("profile")]
    public string? Profile { get; set; }

    /// <summary>An enhancement layer type (MEL or FEL).</summary>
    [DataMember(Name = "enhancementLayer")]
    [JsonPropertyName("enhancementLayer")]
    public string? EnhancementLayer { get; set; }

    /// <summary>A bit depth of the metadata.</summary>
    [DataMember(Name = "bitDepth")]
    [JsonPropertyName("bitDepth")]
    public string? BitDepth { get; set; }

    /// <summary>A color range of the base layer.</summary>
    [DataMember(Name = "baseLayerColorRange")]
    [JsonPropertyName("baseLayerColorRange")]
    public string? BaseLayerColorRange { get; set; }

    /// <summary>A content mapping (CM) version.</summary>
    [DataMember(Name = "cmVersion")]
    [JsonPropertyName("cmVersion")]
    public string? CmVersion { get; set; }

    /// <summary>A display management (DM) version.</summary>
    [DataMember(Name = "dmVersion")]
    [JsonPropertyName("dmVersion")]
    public string? DmVersion { get; set; }

    /// <summary>A mastering display name.</summary>
    [DataMember(Name = "masteringDisplayName")]
    [JsonPropertyName("masteringDisplayName")]
    public string? MasteringDisplayName { get; set; }

    /// <summary>A mastering display identifier.</summary>
    [DataMember(Name = "masteringDisplayId")]
    [JsonPropertyName("masteringDisplayId")]
    public string? MasteringDisplayId { get; set; }

    /// <summary>Mastering display color primaries.</summary>
    [DataMember(Name = "masteringDisplayColorPrimaries")]
    [JsonPropertyName("masteringDisplayColorPrimaries")]
    public string? MasteringDisplayColorPrimaries { get; set; }

    /// <summary>A mastering display luminance range.</summary>
    [DataMember(Name = "masteringDisplayLuminance")]
    [JsonPropertyName("masteringDisplayLuminance")]
    public string? MasteringDisplayLuminance { get; set; }

    /// <summary>A mastering display luminance range as signalled for HDR10.</summary>
    [DataMember(Name = "masteringDisplayLuminanceHdr10")]
    [JsonPropertyName("masteringDisplayLuminanceHdr10")]
    public string? MasteringDisplayLuminanceHdr10 { get; set; }

    /// <summary>A mastering display diagonal.</summary>
    [DataMember(Name = "masteringDisplayDiagonal")]
    [JsonPropertyName("masteringDisplayDiagonal")]
    public string? MasteringDisplayDiagonal { get; set; }

    /// <summary>Target displays of the trim passes.</summary>
    [DataMember(Name = "targetDisplays")]
    [JsonPropertyName("targetDisplays")]
    public string? TargetDisplays { get; set; }

    /// <summary>An active area.</summary>
    [DataMember(Name = "activeArea")]
    [JsonPropertyName("activeArea")]
    public string? ActiveArea { get; set; }

    /// <summary>A display aspect ratio of the active area.</summary>
    [DataMember(Name = "activeDisplayAspectRatio")]
    [JsonPropertyName("activeDisplayAspectRatio")]
    public string? ActiveDisplayAspectRatio { get; set; }

    /// <summary>A content type.</summary>
    [DataMember(Name = "contentType")]
    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }

    /// <summary>A white point.</summary>
    [DataMember(Name = "whitePoint")]
    [JsonPropertyName("whitePoint")]
    public string? WhitePoint { get; set; }

    /// <summary>A reference mode.</summary>
    [DataMember(Name = "referenceMode")]
    [JsonPropertyName("referenceMode")]
    public string? ReferenceMode { get; set; }

    /// <summary>A number of shots.</summary>
    [DataMember(Name = "shotCount")]
    [JsonPropertyName("shotCount")]
    public string? ShotCount { get; set; }
}
