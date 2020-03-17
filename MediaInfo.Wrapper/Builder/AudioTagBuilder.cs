#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

#if DEBUG
using System;
using System.Collections.Generic;
#endif
using MediaInfo.Model;

namespace MediaInfo.Builder
{
  internal class AudioTagBuilder: GeneralTagBuilder<AudioTags>
  {
#if DEBUG
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
#endif

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
#if DEBUG
      foreach (var tagItem in GeneralTagItems)
      {
          var value = MediaInfo.Get(StreamKind.Audio, StreamPosition, (int)tagItem.Item1);
          if (!string.IsNullOrEmpty(value) && tagItem.Item2(value, out var tagValue))
          {
              result.AudioDataTags.Add(tagItem.Item1, tagValue);
          }
      }
#else
      var tagValue = MediaInfo.Get(StreamKind.Audio, StreamPosition, (int)NativeMethods.Audio.Audio_Title);
      result.AudioDataTags.Add(NativeMethods.Audio.Audio_Title, tagValue);
#endif

      return result;
    }
  }
}