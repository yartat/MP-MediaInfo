#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

namespace MediaInfo.Model
{
  /// <summary>
  /// Describes the Dolby Vision metadata of a video stream, parsed from the RPU or from Dolby Vision XML metadata.
  /// </summary>
  /// <remarks>Values are kept as reported by the library. An absent value is an empty string.</remarks>
  public sealed class DolbyVisionInfo
  {
    /// <summary>Gets the summary, for example "Profile 8.1, MEL".</summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>Gets the Dolby Vision profile, for example "8.1".</summary>
    public string Profile { get; init; } = string.Empty;

    /// <summary>Gets the enhancement layer type (MEL or FEL, with frame counts when mixed).</summary>
    public string EnhancementLayer { get; init; } = string.Empty;

    /// <summary>Gets the bit depth of the Dolby Vision metadata.</summary>
    public string BitDepth { get; init; } = string.Empty;

    /// <summary>Gets the color range of the base layer.</summary>
    public string BaseLayerColorRange { get; init; } = string.Empty;

    /// <summary>Gets the content mapping (CM) version.</summary>
    public string CmVersion { get; init; } = string.Empty;

    /// <summary>Gets the display management (DM) version index.</summary>
    public string DmVersion { get; init; } = string.Empty;

    /// <summary>Gets the name of the mastering display.</summary>
    public string MasteringDisplayName { get; init; } = string.Empty;

    /// <summary>Gets the identifier of the mastering display.</summary>
    public string MasteringDisplayId { get; init; } = string.Empty;

    /// <summary>Gets the color primaries of the mastering display.</summary>
    public string MasteringDisplayColorPrimaries { get; init; } = string.Empty;

    /// <summary>Gets the luminance range of the mastering display.</summary>
    public string MasteringDisplayLuminance { get; init; } = string.Empty;

    /// <summary>Gets the diagonal of the mastering display.</summary>
    public string MasteringDisplayDiagonal { get; init; } = string.Empty;

    /// <summary>Gets the target displays of the trim passes.</summary>
    public string TargetDisplays { get; init; } = string.Empty;

    /// <summary>Gets the active area.</summary>
    public string ActiveArea { get; init; } = string.Empty;

    /// <summary>Gets the display aspect ratio of the active area.</summary>
    public string ActiveDisplayAspectRatio { get; init; } = string.Empty;

    /// <summary>Gets the content type.</summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>Gets the white point.</summary>
    public string WhitePoint { get; init; } = string.Empty;

    /// <summary>Gets the reference mode.</summary>
    public string ReferenceMode { get; init; } = string.Empty;
  }
}
