#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using MediaInfo.Model;

namespace MediaInfo.Builder
{
  internal class VideoTagBuilder: GeneralTagBuilder<VideoTags>
  {
#region Tag items

    private static readonly List<Tuple<NativeMethods.Video, ParseDelegate<object>>> GeneralTagItems;

#endregion

    static VideoTagBuilder()
    {
      var values = typeof(NativeMethods.Video).GetEnumValues();
      GeneralTagItems = new List<Tuple<NativeMethods.Video, ParseDelegate<object>>>(values.Length);
      foreach (NativeMethods.Video item in values)
      {
        GeneralTagItems.Add(new Tuple<NativeMethods.Video, ParseDelegate<object>>(item, TagBuilderHelper.TryGetString));
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoTagBuilder"/> class.
    /// </summary>
    /// <param name="mediaInfo">The media information.</param>
    /// <param name="streamPosition">The stream position.</param>
    public VideoTagBuilder(MediaInfo mediaInfo, int streamPosition)
      : base(mediaInfo, streamPosition)
    {
    }

    public override VideoTags Build()
    {
      var result = base.Build();
      foreach (var tagItem in GeneralTagItems)
      {
        var value = MediaInfo.Get(StreamKind.Video, StreamPosition, (int)tagItem.Item1);
        if (!string.IsNullOrEmpty(value) && tagItem.Item2(value, out var tagValue))
        {
          result.VideoDataTags.Add(tagItem.Item1, tagValue);
        }
      }

      return result;
    }
  }
}