#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models;

/// <summary>
/// Describes properties of the menu
/// </summary>
/// <seealso cref="MediaStream" />
[DataContract]
public class MenuStream : MediaStream
{
    /// <summary>
    /// A menu duration.
    /// </summary>
    /// <example>10.1</example>
    [DataMember(Name = "duration")]
    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// A chapter list.
    /// </summary>
    [DataMember(Name = "chapters")]
    [JsonPropertyName("chapters")]
    public ICollection<Chapter> Chapters { get; } = new List<Chapter>();
}

/// <summary>
/// Describes properties of the menu chapter
/// </summary>
[DataContract]
public sealed class Chapter
{
    /// <summary>
    /// A menu position.
    /// </summary>
    /// <example>10:13</example>
    [DataMember(Name = "position")]
    [JsonPropertyName("position")]
    public TimeSpan Position { get; set; }

    /// <summary>
    /// A menu chapter name.
    /// </summary>
    /// <example>Chapter 1</example>
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}