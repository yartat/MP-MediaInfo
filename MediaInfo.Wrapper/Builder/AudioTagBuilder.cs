#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using MediaInfo.Model;

namespace MediaInfo.Builder;

internal class AudioTagBuilder : GeneralTagBuilder<AudioTags>
{
    #region Tag items

    private static readonly List<(NativeMethods.Audio AudioTagType, ParseDelegate<object> ParseFunc)> GeneralTagItems;

    #endregion

    static AudioTagBuilder()
    {
        var values = typeof(NativeMethods.Audio).GetEnumValues();
        GeneralTagItems = new List<(NativeMethods.Audio, ParseDelegate<object>)>(values.Length);
        foreach (NativeMethods.Audio item in values)
        {
            GeneralTagItems.Add((item, TagBuilderHelper.TryGetString));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioTagBuilder"/> class.
    /// </summary>
    /// <param name="mediaInfo">The media information.</param>
    /// <param name="streamPosition">The stream position.</param>
    public AudioTagBuilder(MediaInfo mediaInfo, int streamPosition)
        : base(mediaInfo, streamPosition)
    {
    }

    public override AudioTags Build()
    {
        var result = base.Build();
        foreach (var tagItem in GeneralTagItems)
        {
            var value = MediaInfo.Get(StreamKind.Audio, StreamPosition, (int)tagItem.AudioTagType);
            if (!string.IsNullOrEmpty(value) && tagItem.ParseFunc(value, out var tagValue) && tagValue is not null)
            {
                result.AudioDataTags.Add(tagItem.AudioTagType, tagValue);
            }
        }

        return result;
    }
}