#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models;

/// <summary>
/// Provides properties and overridden methods for the analyze subtitle stream
/// and contains information about subtitle.
/// </summary>
/// <seealso cref="LanguageMediaStream" />
[DataContract]
public class SubtitleStream : LanguageMediaStream
{
    /// <summary>
    /// A subtitle format.
    /// </summary>
    /// <example>utf8</example>
    [DataMember(Name = "format")]
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// Gets the subtitle codec.
    /// </summary>
    /// <example>utf8</example>
    [DataMember(Name = "codec")]
    [JsonPropertyName("codec")]
    public SubtitleCodec Codec { get; set; }

    /// <summary>
    /// The maximum count of characters per line; 0 when the format does not report it.
    /// </summary>
    /// <example>32</example>
    [DataMember(Name = "maxCharactersPerLine")]
    [JsonPropertyName("maxCharactersPerLine")]
    public int MaxCharactersPerLine { get; set; }
}