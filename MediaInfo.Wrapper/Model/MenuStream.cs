#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;

namespace MediaInfo.Model;

/// <summary>
/// Describes properties of the menu
/// </summary>
/// <seealso cref="MediaStream" />
public class MenuStream : MediaStream
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuStream"/> class.
    /// </summary>
    public MenuStream()
    {
        Chapters = new List<Chapter>();
    }

    /// <summary>
    /// The menu duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// The chapters.
    /// </summary>
    public ICollection<Chapter> Chapters { get; } = new List<Chapter>();

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Menu;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Menu;
}

#if NET5_0_OR_GREATER
/// <summary>
/// Describes properties of the menu chapter
/// </summary>
/// <param name="Position">The menu position.</param>
/// <param name="Name">The menu chapter name.</param>
public record Chapter(
    TimeSpan Position,
    string? Name);
#else
/// <summary>
/// Describes properties of the menu chapter
/// </summary>
public record Chapter
{
    /// <summary>
    /// The menu position.
    /// </summary>
    public TimeSpan Position { get; set; }

    /// <summary>
    /// The menu chapter name.
    /// </summary>
    public string? Name { get; set; }
}
#endif