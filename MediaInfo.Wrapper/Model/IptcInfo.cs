#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

namespace MediaInfo.Model
{
  /// <summary>
  /// Describes the IPTC metadata of a file, read from the IPTC-IIM block or from XMP.
  /// </summary>
  /// <remarks>
  /// Values are kept as the library reports them. An absent value is an empty string. The title, the description, the
  /// keywords, the creator, the country, the copyright and the dates of the file are not repeated here: they are reported
  /// by the tags of the media.
  /// </remarks>
  public sealed class IptcInfo
  {
    /// <summary>Gets the headline, a short summary of the content.</summary>
    public string Headline { get; init; } = string.Empty;

    /// <summary>Gets the credit line: the provider of the content, not the copyright notice.</summary>
    public string Credit { get; init; } = string.Empty;

    /// <summary>Gets the original owner of the content.</summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>Gets the job title of the creator.</summary>
    public string CreatorJobTitle { get; init; } = string.Empty;

    /// <summary>Gets the person who wrote or edited the description.</summary>
    public string DescriptionWriter { get; init; } = string.Empty;

    /// <summary>Gets the contact information.</summary>
    public string Contact { get; init; } = string.Empty;

    /// <summary>Gets the special instructions about the use of the content.</summary>
    public string Instructions { get; init; } = string.Empty;

    /// <summary>Gets the identifier of the job the content belongs to (the original transmission reference).</summary>
    public string JobId { get; init; } = string.Empty;

    /// <summary>Gets the urgency, from 1 (the most urgent) to 8 (the least). Empty when it is not set.</summary>
    public string Urgency { get; init; } = string.Empty;

    /// <summary>Gets the category.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the supplemental categories.</summary>
    public string SupplementalCategories { get; init; } = string.Empty;

    /// <summary>Gets the intellectual genre.</summary>
    public string IntellectualGenre { get; init; } = string.Empty;

    /// <summary>Gets the subject code.</summary>
    public string SubjectCode { get; init; } = string.Empty;

    /// <summary>Gets the scene code.</summary>
    public string SceneCode { get; init; } = string.Empty;

    /// <summary>Gets the reference to the subject.</summary>
    public string SubjectReference { get; init; } = string.Empty;

    /// <summary>Gets the reference to the attribute of the object.</summary>
    public string ObjectAttributeReference { get; init; } = string.Empty;

    /// <summary>Gets the edit status.</summary>
    public string EditStatus { get; init; } = string.Empty;

    /// <summary>Gets the fixture identifier.</summary>
    public string FixtureIdentifier { get; init; } = string.Empty;

    /// <summary>Gets the object cycle: morning, evening or both.</summary>
    public string ObjectCycle { get; init; } = string.Empty;

    /// <summary>Gets the city.</summary>
    public string City { get; init; } = string.Empty;

    /// <summary>Gets the sublocation within the city.</summary>
    public string Sublocation { get; init; } = string.Empty;

    /// <summary>Gets the province or the state.</summary>
    public string State { get; init; } = string.Empty;

    /// <summary>Gets the code of the country.</summary>
    public string CountryCode { get; init; } = string.Empty;

    /// <summary>Gets the code of the location of the content.</summary>
    public string ContentLocationCode { get; init; } = string.Empty;

    /// <summary>Gets the name of the location of the content.</summary>
    public string ContentLocationName { get; init; } = string.Empty;

    /// <summary>Gets the date after which the content should not be used.</summary>
    public string ExpirationDate { get; init; } = string.Empty;

    /// <summary>Gets the digital source type, as the URI of the IPTC code.</summary>
    public string DigitalSourceType { get; init; } = string.Empty;

    /// <summary>Gets the name of the digital source type, for example "Created using Generative AI".</summary>
    public string DigitalSourceTypeName { get; init; } = string.Empty;
  }
}
