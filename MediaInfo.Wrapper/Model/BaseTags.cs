#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaInfo.Model;

/// <summary>
/// Base class to read tags from stream
/// </summary>
public abstract class BaseTags
{
    /// <summary>
    /// The general tags.
    /// </summary>
    internal IDictionary<NativeMethods.General, object> GeneralTags { get; } = new Dictionary<NativeMethods.General, object>();

    /// <summary>
    /// The title of the media.
    /// </summary>
    public string? Title =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Title, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// A short description of the contents, such as "Two birds flying".
    /// </summary>
    public string? Description =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Description, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The keywords to the item separated by a comma, used for searching.
    /// </summary>
    public string[]? Keywords =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Keywords, out var result) ?
            ((string)result).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray() :
            null;

    /// <summary>
    /// The country.
    /// </summary>
    public string? Country =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Country, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The time that the item was originally released.
    /// </summary>
    public DateTime? ReleasedDate =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Released_Date, out var result) ?
            (DateTime?)result :
            null;

    /// <summary>
    /// The time that the encoding of this item was completed began.
    /// </summary>
    public DateTime? EncodedDate =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Encoded_Date, out var result) ?
            (DateTime?)result :
            null;

    /// <summary>
    /// The time that the tags were done for this item.
    /// </summary>
    public DateTime? TaggedDate =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Tagged_Date, out var result) ?
            (DateTime?)result :
            null;

    /// <summary>
    /// Any comment related to the content.
    /// </summary>
    public string? Comment =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Comment, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// A numeric value defining how much a person likes the song/movie. The number is between 0 and 5 with
    /// decimal values possible (e.g. 2.7), 5(.0) being the highest possible rating.
    /// </summary>
    public double? Rating =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Rating, out var result) ?
            (double?)result :
            null;

    /// <summary>
    /// The copyright attribution.
    /// </summary>
    public string? Copyright =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Copyright, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the organization producing the track (i.e. the 'record label').
    /// </summary>
    public string? Publisher =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Publisher, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The publishers official web page.
    /// </summary>
    public string? PublisherUrl =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Publisher_URL, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the organization distributing track.
    /// </summary>
    public string? DistributedBy =>
        GeneralTags.TryGetValue(NativeMethods.General.General_DistributedBy, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The average number of beats per minute in the complete target.
    /// </summary>
    public int? Bpm =>
        GeneralTags.TryGetValue(NativeMethods.General.General_BPM, out var result) ?
            (int?)result :
            null;

    /// <summary>
    /// The cover media.
    /// </summary>
    public IEnumerable<CoverInfo>? Covers { get; set; }
}

#if NET5_0_OR_GREATER
/// <summary>
/// Describes properties of the cover tags
/// </summary>
/// <param name="Exists">A value indicating whether this <see cref="CoverInfo"/> is exists.</param>
/// <param name="Description">The description of the cover.</param>
/// <param name="Type">The type of the cover.</param>
/// <param name="Mime">The MIME of the cover.</param>
/// <param name="Data">The cover data.</param>
public record CoverInfo(
    bool Exists,
    string? Description,
    string? Type,
    string? Mime,
    byte[]? Data);
#else
    /// <summary>
    /// Describes properties of the cover tags
    /// </summary>
    public record CoverInfo
    {
        /// <summary>
        /// A value indicating whether this <see cref="CoverInfo"/> is exists.
        /// </summary>
        /// <value>
        /// <c>true</c> if exists; otherwise, <c>false</c>.
        /// </value>
        public bool Exists { get; internal set; }
        
        /// <summary>
        /// The description of the cover.
        /// </summary>
        public string? Description { get; internal set; }
        
        /// <summary>
        /// The type of the cover.
        /// </summary>
        public string? Type { get; internal set; }
        
        /// <summary>
        /// The MIME of the cover.
        /// </summary>
        public string? Mime { get; internal set; }
        
        /// <summary>
        /// The cover data.
        /// </summary>
        public byte[]? Data { get; internal set; }
    }
#endif