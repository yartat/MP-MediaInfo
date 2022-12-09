#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.Model;

/// <summary>
/// Provides properties and overridden methods for the analyze stream
/// and contains information about media stream.
/// </summary>
/// <seealso cref="MediaStream" />
public abstract class LanguageMediaStream : MediaStream
{
    /// <summary>
    /// The media stream language.
    /// </summary>
    public string Language { get; set; } = default!;

    /// <summary>
    /// The media stream LCID.
    /// </summary>
    public int Lcid { get; set; }

    /// <summary>
    /// A value indicating whether this <see cref="LanguageMediaStream"/> is default.
    /// </summary>
    /// <value>
    ///   <c>true</c> if default; otherwise, <c>false</c>.
    /// </value>
    public bool Default { get; set; }

    /// <summary>
    /// A value indicating whether this <see cref="LanguageMediaStream"/> is forced.
    /// </summary>
    /// <value>
    ///   <c>true</c> if forced; otherwise, <c>false</c>.
    /// </value>
    public bool Forced { get; set; }

    /// <summary>
    /// The stream size (bytes).
    /// </summary>
    public long StreamSize { get; set; }
}