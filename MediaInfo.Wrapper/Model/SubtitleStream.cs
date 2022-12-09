﻿#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.Model;

/// <summary>
/// Provides properties and overridden methods for the analyze subtitle stream
/// and contains information about subtitle.
/// </summary>
/// <seealso cref="LanguageMediaStream" />
public class SubtitleStream : LanguageMediaStream
{
    /// <summary>
    /// The subtitle format.
    /// </summary>
    public string Format { get; set; } = default!;

    /// <summary>
    /// The subtitle codec.
    /// </summary>
    public SubtitleCodec Codec { get; set; }

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Text;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Text;
}