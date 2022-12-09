#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.Model;

/// <summary>
/// Provides properties and overridden methods for the analyze chapter in media
/// and contains information about chapter.
/// </summary>
/// <seealso cref="MediaStream" />
public class ChapterStream : MediaStream
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChapterStream"/> class.
    /// </summary>
    /// <param name="offset">The offset of the stream.</param>
    /// <param name="description">The stream description.</param>
    public ChapterStream(double offset, string description)
    {
        Offset = offset;
        Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChapterStream"/> class.
    /// </summary>
    public ChapterStream()
    {
    }

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Menu;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Other;

    /// <summary>
    /// The chapter offset.
    /// </summary>
    public double Offset { get; } = 0.0;

    /// <summary>
    /// The chapter description.
    /// </summary>
    public string? Description { get; }
}