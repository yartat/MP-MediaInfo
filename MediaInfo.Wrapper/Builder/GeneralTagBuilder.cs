#region Copyright (C) 2005-2019 Team MediaPortal

// Copyright (C) 2005-2019 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Linq;
using MediaInfo.Model;
using static MediaInfo.Model.BaseTags;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Converts the string representation of a value to specified type
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="source">The source value.</param>
  /// <param name="result">The result.</param>
  /// <returns><b>true</b> if s was converted successfully; otherwise, <b>false</b>.</returns>
  public delegate bool ParseDelegate<T>(string source, out T result);

  internal class GeneralTagBuilder<T> where T : BaseTags, new()
  {
    #region Tag items

    private static readonly List<Tuple<NativeMethods.General, ParseDelegate<object>>> GeneralTagItems = new List<Tuple<NativeMethods.General, ParseDelegate<object>>>
    {
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Collection, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Season, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Album, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Title, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Movie, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Part, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Track, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Chapter, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_SubTrack, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Original_Album, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Original_Movie, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Original_Track, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Track_More, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Track_Position, TagBuilderHelper.TryGetInt),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Track_Position_Total, TagBuilderHelper.TryGetInt),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Part_Position, TagBuilderHelper.TryGetInt),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Part_Position_Total, TagBuilderHelper.TryGetInt),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Album_Performer, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Performer, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Performer_Sort, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Performer_Url, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Original_Performer, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Accompaniment, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Composer, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Composer_Nationality, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Arranger, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Lyricist, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Original_Lyricist, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Conductor, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Actor, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Actor_Character, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_WrittenBy, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_ScreenplayBy, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Director, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_AssistantDirector, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_DirectorOfPhotography, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_ArtDirector, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_EditedBy, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Producer, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_CoProducer, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_ExecutiveProducer, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_ProductionDesigner, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_CostumeDesigner, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_SoundEngineer, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_MasteredBy, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_RemixedBy, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_ProductionStudio, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Label, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Publisher, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Publisher_URL, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_DistributedBy, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_EncodedBy, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_ThanksTo, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_CommissionedBy, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_ContentType, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Subject, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Summary, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Description, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Keywords, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Period, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_LawRating, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Country, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Written_Date, TagBuilderHelper.TryGetDate),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Recorded_Date, TagBuilderHelper.TryGetDate),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Released_Date, TagBuilderHelper.TryGetDate),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Mastered_Date, TagBuilderHelper.TryGetDate),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Encoded_Date, TagBuilderHelper.TryGetDate),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Tagged_Date, TagBuilderHelper.TryGetDate),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Original_Released_Date, TagBuilderHelper.TryGetDate),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Original_Released_Date, TagBuilderHelper.TryGetDate),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Written_Location, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Recorded_Location, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Archival_Location, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Genre, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Mood, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Comment, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Rating, TagBuilderHelper.TryGetDouble),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Encoded_Application, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Encoded_Library, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Encoded_Library_Settings, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Copyright, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Producer_Copyright, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_TermsOfUse, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_ISRC, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_ISBN, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_BarCode, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_LCCN, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_CatalogNumber, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_LabelCode, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_BPM, TagBuilderHelper.TryGetInt),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Cover, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Cover_Description, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Cover_Type, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Cover_Mime, TagBuilderHelper.TryGetString),
      new Tuple<NativeMethods.General, ParseDelegate<object>>(NativeMethods.General.General_Cover_Data, TagBuilderHelper.TryGetString),
    };

    #endregion

    public GeneralTagBuilder(MediaInfo mediaInfo, int streamPosition)
    {
      MediaInfo = mediaInfo ?? throw new ArgumentNullException(nameof(mediaInfo));
      StreamPosition = streamPosition;
    }

    /// <summary>
    /// Gets the media information.
    /// </summary>
    protected MediaInfo MediaInfo { get; }

    protected int StreamPosition { get; }

    public virtual T Build()
    {
      var result = new T();
      foreach (var tagItem in GeneralTagItems)
      {
        var value = MediaInfo.Get(StreamKind.General, StreamPosition, (int)tagItem.Item1);
        if (!string.IsNullOrEmpty(value) && tagItem.Item2(value, out var tagValue))
        {
          result.GeneralTags.Add(tagItem.Item1, tagValue);
        }
      }

      // parse covers
      result.GeneralTags.TryGetValue(NativeMethods.General.General_Cover_Type, out var coverType);
      result.GeneralTags.TryGetValue(NativeMethods.General.General_Cover_Mime, out var coverMime);
      result.GeneralTags.TryGetValue(NativeMethods.General.General_Cover_Description, out var coverDescription);
      result.GeneralTags.TryGetValue(NativeMethods.General.General_Cover, out var cover);

      if (result.GeneralTags.TryGetValue(NativeMethods.General.General_Cover_Data, out var coverData) ||
          coverType != null ||
          coverMime != null ||
          coverDescription != null ||
          cover != null)
      {
        var coverDataItems = ((string)coverData)?.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
        var coverTypeItems = ((string)coverType)?.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
        var coverMimeItems = ((string)coverMime)?.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
        var coverItems = ((string)cover)?.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
        var coverDescriptionItems = ((string)coverDescription)?.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
        var itemCount = new[] { coverDataItems?.Length ?? 0, coverTypeItems?.Length ?? 0, coverMimeItems?.Length ?? 0, coverDescriptionItems?.Length ?? 0, coverItems?.Length ?? 0 }.Max();
        if (itemCount > 0)
        {
          var covers = new List<CoverInfo>(itemCount);
          for (var i = 0; i < itemCount; ++i)
          {
            var data = coverDataItems.TryGet(i, string.Empty) ?? string.Empty;
            covers.Add(new CoverInfo
            {
              Exists = ToBool(coverItems.TryGet(i, string.Empty)),
              Description = coverDescriptionItems?.TryGet(i, string.Empty) ?? string.Empty,
              Type = coverTypeItems?.TryGet(i, string.Empty) ?? string.Empty,
              Mime = coverMimeItems?.TryGet(i, string.Empty) ?? string.Empty,
              Data = string.IsNullOrEmpty(data) ? null : Convert.FromBase64String(data)
            });
          }

          result.Covers = covers;
        }
        else
        {
          result.Covers = Enumerable.Empty<CoverInfo>();
        }
      }
      else
      {
        result.Covers = Enumerable.Empty<CoverInfo>();
      }

      if (!result.GeneralTags.ContainsKey(NativeMethods.General.General_Album_Performer))
      {
        var value = MediaInfo.Get(StreamKind.Audio, StreamPosition, "ARTIST");
        if (!string.IsNullOrEmpty(value))
        { 
          result.GeneralTags.Add(NativeMethods.General.General_Album_Performer, value);
        }
      }

      if (!result.GeneralTags.ContainsKey(NativeMethods.General.General_Title))
      {
        var value = MediaInfo.Get(StreamKind.Audio, StreamPosition, "Title");
        if (!string.IsNullOrEmpty(value))
        {
          result.GeneralTags.Add(NativeMethods.General.General_Title, value);
        }
      }

      if (!result.GeneralTags.ContainsKey(NativeMethods.General.General_Tagged_Date))
      {
        var value = TagBuilderHelper.TryGetDate(MediaInfo.Get(StreamKind.Audio, StreamPosition, "DATE_TAGGED"), out DateTime res) ? (DateTime?)res : null;
        if (value != null)
        {
          result.GeneralTags.Add(NativeMethods.General.General_Tagged_Date, value);
        }
      }

      if (!result.GeneralTags.ContainsKey(NativeMethods.General.General_Genre))
      {
        var value = MediaInfo.Get(StreamKind.Audio, StreamPosition, "GENRE");
        if (!string.IsNullOrEmpty(value))
        {
          result.GeneralTags.Add(NativeMethods.General.General_Genre, value);
        }
      }

      if (!result.GeneralTags.ContainsKey(NativeMethods.General.General_Rating))
      {
        var value = TagBuilderHelper.TryGetDouble(MediaInfo.Get(StreamKind.Audio, StreamPosition, "RATING"), out double res) ? (double?)res : null;
        if (value != null)
        {
          result.GeneralTags.Add(NativeMethods.General.General_Rating, value);
        }
      }

      if (!result.GeneralTags.ContainsKey(NativeMethods.General.General_Released_Date))
      {
        var value = TagBuilderHelper.TryGetDate(MediaInfo.Get(StreamKind.Audio, StreamPosition, "Released_Date"), out DateTime res) ? (DateTime?)res : null;
        if (value != null)
        {
          result.GeneralTags.Add(NativeMethods.General.General_Released_Date, value);
        }
      }

      if (!result.GeneralTags.ContainsKey(NativeMethods.General.General_Encoded_Library))
      {
        var value = MediaInfo.Get(StreamKind.Audio, StreamPosition, "Encoded_Library");
        if (!string.IsNullOrEmpty(value))
        {
          result.GeneralTags.Add(NativeMethods.General.General_Encoded_Library, value);
        }
      }

      if (!result.GeneralTags.ContainsKey((NativeMethods.General)1000))
      {
        if (TagBuilderHelper.TryGetBool(MediaInfo.Get(StreamKind.General, StreamPosition, "Stereoscopic"), out var value))
        {
          result.GeneralTags.Add((NativeMethods.General)1000, value);
        }
      }

      if (!result.GeneralTags.ContainsKey((NativeMethods.General)1001))
      {
        if (TagBuilderHelper.TryGetStereoMode(MediaInfo.Get(StreamKind.General, StreamPosition, "StereoscopicLayout"), out var value))
        {
          result.GeneralTags.Add((NativeMethods.General)1001, value);
        }
      }

      if (!result.GeneralTags.ContainsKey((NativeMethods.General)1002))
      {
        if (TagBuilderHelper.TryGetInt(MediaInfo.Get(StreamKind.General, StreamPosition, "StereoscopicSkip"), out int value))
        {
          result.GeneralTags.Add((NativeMethods.General)1002, value);
        }
      }

      return result;
    }

    protected static bool ToBool(string source)
    {
      return string.Equals(source, "t", StringComparison.OrdinalIgnoreCase)
             || string.Equals(source, "true", StringComparison.OrdinalIgnoreCase)
             || string.Equals(source, "y", StringComparison.OrdinalIgnoreCase)
             || string.Equals(source, "yes", StringComparison.OrdinalIgnoreCase)
             || string.Equals(source, "1", StringComparison.OrdinalIgnoreCase);
    }
  }

  internal static class ArrayExtensions
  {
    public static T TryGet<T>(this T[] array, int index, T defaultValue)
    {
      if (index < 0)
      {
        throw new ArgumentException($"Parameter {nameof(index)} is a negative value.");
      }

      if (array == null)
      {
        return defaultValue;
      }

      return array.Length > index ? array[index] : defaultValue;
    }
  }
}