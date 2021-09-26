#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using MediaInfo.Model;

namespace MediaInfo.Builder
{
  internal class AudioTagBuilder: GeneralTagBuilder<AudioTags>
  {
#region Tag items

    private static readonly List<Tuple<NativeMethods.Audio, ParseDelegate<object>>> GeneralTagItems;

#endregion

    static AudioTagBuilder()
    {
      var values = typeof(NativeMethods.Audio).GetEnumValues();
      GeneralTagItems = new List<Tuple<NativeMethods.Audio, ParseDelegate<object>>>(values.Length);
      foreach (NativeMethods.Audio item in values)
      {
        GeneralTagItems.Add(new Tuple<NativeMethods.Audio, ParseDelegate<object>>(item, TagBuilderHelper.TryGetString));
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
          var value = MediaInfo.Get(StreamKind.Audio, StreamPosition, (int)tagItem.Item1);
          if (!string.IsNullOrEmpty(value) && tagItem.Item2(value, out var tagValue))
          {
              result.AudioDataTags.Add(tagItem.Item1, tagValue);
          }
      }

      return result;
    }
  }
}