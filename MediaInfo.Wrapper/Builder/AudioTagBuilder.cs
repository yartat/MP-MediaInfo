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
  /// Builds audio tag data by extracting and parsing audio stream metadata from the provided media information.
  /// </summary>
  /// <remarks>This class is intended for internal use when constructing audio tag data. It processes all
  /// available audio tag fields defined in the underlying native methods and adds them to the resulting tag
  /// collection if present.</remarks>
  /// <param name="mediaInfo">The media information source from which audio stream metadata is retrieved. Cannot be null.</param>
  /// <param name="streamPosition">The zero-based index of the audio stream to process within the media information.</param>
  internal class AudioTagBuilder(IMediaInfoReader mediaInfo, int streamPosition) : GeneralTagBuilder<AudioTags>(mediaInfo, streamPosition)
  {
    #region Tag items

    private static readonly List<(NativeMethods.Audio Audio, ParseDelegate<object> ParseFunc)> GeneralTagItems;

    #endregion

    static AudioTagBuilder()
    {
#if NET8_0_OR_GREATER
      var values = System.Enum.GetValues<NativeMethods.Audio>();
#else
      var values = typeof(NativeMethods.Audio).GetEnumValues();
#endif
      GeneralTagItems = new List<(NativeMethods.Audio, ParseDelegate<object>)>(values.Length);
      foreach (NativeMethods.Audio item in values)
      {
        GeneralTagItems.Add(new(item, TagBuilderHelper.TryGetString));
      }
    }

    /// <inheritdoc />
    public override AudioTags Build()
    {
      var result = base.Build();
      foreach (var (Audio, ParseFunc) in GeneralTagItems)
      {
        var value = MediaInfo.Get(StreamKind.Audio, StreamPosition, (int)Audio);
        if (!string.IsNullOrEmpty(value) && ParseFunc(value, out var tagValue))
        {
          result.AudioDataTags.Add(Audio, tagValue);
        }
      }

      return result;
    }
  }
}