#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models;

/// <summary>
/// Describes the IPTC metadata of a file.
/// </summary>
[DataContract]
public sealed class IptcInfo
{
    /// <summary>A headline.</summary>
    [DataMember(Name = "headline")]
    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    /// <summary>A credit line.</summary>
    [DataMember(Name = "credit")]
    [JsonPropertyName("credit")]
    public string? Credit { get; set; }

    /// <summary>A source.</summary>
    [DataMember(Name = "source")]
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>A job title of the creator.</summary>
    [DataMember(Name = "creatorJobTitle")]
    [JsonPropertyName("creatorJobTitle")]
    public string? CreatorJobTitle { get; set; }

    /// <summary>A person who wrote the description.</summary>
    [DataMember(Name = "descriptionWriter")]
    [JsonPropertyName("descriptionWriter")]
    public string? DescriptionWriter { get; set; }

    /// <summary>A contact.</summary>
    [DataMember(Name = "contact")]
    [JsonPropertyName("contact")]
    public string? Contact { get; set; }

    /// <summary>Special instructions.</summary>
    [DataMember(Name = "instructions")]
    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    /// <summary>A job identifier.</summary>
    [DataMember(Name = "jobId")]
    [JsonPropertyName("jobId")]
    public string? JobId { get; set; }

    /// <summary>An urgency, from 1 to 8.</summary>
    [DataMember(Name = "urgency")]
    [JsonPropertyName("urgency")]
    public string? Urgency { get; set; }

    /// <summary>A category.</summary>
    [DataMember(Name = "category")]
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>Supplemental categories.</summary>
    [DataMember(Name = "supplementalCategories")]
    [JsonPropertyName("supplementalCategories")]
    public string? SupplementalCategories { get; set; }

    /// <summary>An intellectual genre.</summary>
    [DataMember(Name = "intellectualGenre")]
    [JsonPropertyName("intellectualGenre")]
    public string? IntellectualGenre { get; set; }

    /// <summary>A subject code.</summary>
    [DataMember(Name = "subjectCode")]
    [JsonPropertyName("subjectCode")]
    public string? SubjectCode { get; set; }

    /// <summary>A scene code.</summary>
    [DataMember(Name = "sceneCode")]
    [JsonPropertyName("sceneCode")]
    public string? SceneCode { get; set; }

    /// <summary>A reference to the subject.</summary>
    [DataMember(Name = "subjectReference")]
    [JsonPropertyName("subjectReference")]
    public string? SubjectReference { get; set; }

    /// <summary>A reference to the attribute of the object.</summary>
    [DataMember(Name = "objectAttributeReference")]
    [JsonPropertyName("objectAttributeReference")]
    public string? ObjectAttributeReference { get; set; }

    /// <summary>An edit status.</summary>
    [DataMember(Name = "editStatus")]
    [JsonPropertyName("editStatus")]
    public string? EditStatus { get; set; }

    /// <summary>A fixture identifier.</summary>
    [DataMember(Name = "fixtureIdentifier")]
    [JsonPropertyName("fixtureIdentifier")]
    public string? FixtureIdentifier { get; set; }

    /// <summary>An object cycle.</summary>
    [DataMember(Name = "objectCycle")]
    [JsonPropertyName("objectCycle")]
    public string? ObjectCycle { get; set; }

    /// <summary>A city.</summary>
    [DataMember(Name = "city")]
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>A sublocation.</summary>
    [DataMember(Name = "sublocation")]
    [JsonPropertyName("sublocation")]
    public string? Sublocation { get; set; }

    /// <summary>A province or a state.</summary>
    [DataMember(Name = "state")]
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>A country code.</summary>
    [DataMember(Name = "countryCode")]
    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }

    /// <summary>A code of the location of the content.</summary>
    [DataMember(Name = "contentLocationCode")]
    [JsonPropertyName("contentLocationCode")]
    public string? ContentLocationCode { get; set; }

    /// <summary>A name of the location of the content.</summary>
    [DataMember(Name = "contentLocationName")]
    [JsonPropertyName("contentLocationName")]
    public string? ContentLocationName { get; set; }

    /// <summary>A date after which the content should not be used.</summary>
    [DataMember(Name = "expirationDate")]
    [JsonPropertyName("expirationDate")]
    public string? ExpirationDate { get; set; }

    /// <summary>A digital source type, as the URI of the IPTC code.</summary>
    [DataMember(Name = "digitalSourceType")]
    [JsonPropertyName("digitalSourceType")]
    public string? DigitalSourceType { get; set; }

    /// <summary>A name of the digital source type.</summary>
    [DataMember(Name = "digitalSourceTypeName")]
    [JsonPropertyName("digitalSourceTypeName")]
    public string? DigitalSourceTypeName { get; set; }
}
