#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Collections.Generic;
using MediaInfo.Model;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Builds video tag metadata by extracting and parsing video stream information from the provided media source.
  /// </summary>
  /// <remarks>This class is intended for internal use when constructing video tag data. It processes all
  /// available video tag fields defined in the native video enumeration and adds them to the resulting tag
  /// collection.
  /// </remarks>
  /// <param name="mediaInfo">The media information source from which video stream data is retrieved. Cannot be null.</param>
  /// <param name="streamPosition">The zero-based index of the video stream to process within the media source. Must be non-negative.</param>
  internal class VideoTagBuilder(IMediaInfoReader mediaInfo, int streamPosition) : GeneralTagBuilder<VideoTags>(mediaInfo, streamPosition)
  {
    #region Tag items

    private static readonly List<(NativeMethods.Video VideoType, ParseDelegate<object> ParseMethod)> GeneralTagItems;

    #endregion

    static VideoTagBuilder()
    {
#if NET8_0_OR_GREATER
      var values = System.Enum.GetValues<NativeMethods.Video>();
#else
      var values = typeof(NativeMethods.Video).GetEnumValues();
#endif
      GeneralTagItems = new List<(NativeMethods.Video VideoType, ParseDelegate<object> ParseMethod)>(values.Length);
      foreach (NativeMethods.Video item in values)
      {
        GeneralTagItems.Add((item, TagBuilderHelper.TryGetString));
      }
    }

    public override VideoTags Build()
    {
      var result = base.Build();
      foreach (var tagItem in GeneralTagItems)
      {
        var value = MediaInfo.Get(StreamKind.Video, StreamPosition, (int)tagItem.VideoType);
        if (!string.IsNullOrEmpty(value) && tagItem.ParseMethod(value, out var tagValue))
        {
          result.VideoDataTags.Add(tagItem.VideoType, tagValue);
        }
      }

      return result;
    }
  }
}