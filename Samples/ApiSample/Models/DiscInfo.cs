#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;

namespace ApiSample.Models;

/// <summary>
/// Describes one chapter of a disc title.
/// </summary>
public class DiscChapterInfo
{
    /// <summary>
    /// Gets or sets the ordinal of the chapter within the title, starting at one.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the offset of the chapter from the start of the title.
    /// </summary>
    public TimeSpan Start { get; set; }

    /// <summary>
    /// Gets or sets the duration of the chapter.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the name of the chapter, when the disc declares one.
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Describes one playable title of an optical disc.
/// </summary>
public class DiscTitleInfo
{
    /// <summary>
    /// Gets or sets the ordinal of the title on the disc, starting at one.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the duration of the title.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the total size of the media files of the title, in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the media file the stream information of the title was read from.
    /// </summary>
    public string? PrimaryFile { get; set; }

    /// <summary>
    /// Gets or sets the number of media files that make up the title.
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// Gets or sets the chapters of the title. A DVD declares these in its
    /// navigation tables; a Blu-ray playlist carries none that this API reads.
    /// </summary>
    public IList<DiscChapterInfo>? Chapters { get; set; }
}

/// <summary>
/// Describes the structure of an optical disc.
/// </summary>
public class DiscInfo
{
    /// <summary>
    /// Gets or sets the kind of the disc, either <c>Dvd</c> or <c>BluRay</c>.
    /// </summary>
    public string? Kind { get; set; }

    /// <summary>
    /// Gets or sets the folder the disc structure is rooted at.
    /// </summary>
    public string? RootPath { get; set; }

    /// <summary>
    /// Gets or sets the total size of the disc content in bytes.
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// Gets or sets the ordinal of the title that is most likely to be the main feature.
    /// </summary>
    public int? MainTitleNumber { get; set; }

    /// <summary>
    /// Gets or sets the titles the disc holds.
    /// </summary>
    public IList<DiscTitleInfo>? Titles { get; set; }
}
