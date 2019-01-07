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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using static MediaInfo.BaseTags;

namespace MediaInfo.Builder
{
  internal static class TagHelper
  {
    private static readonly Dictionary<string, bool> BooleanValues = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
    {
      { "1", true },
      { "0", false },
      { "y", true },
      { "n", false },
      { "yes", true },
      { "no", false },
      { "t", true },
      { "f", false },
      { "true", true },
      { "false", false }
    };
    
    /// <summary>
    /// Converts the string representation of a value to specified type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source value.</param>
    /// <param name="result">The result.</param>
    /// <returns><b>true</b> if s was converted successfully; otherwise, <b>false</b>.</returns>
    private delegate bool ParseDelegate<T>(string source, out T result);

    #region Tag items

    private static readonly List<Tuple<string, ParseDelegate<object>>> TagItems = new List<Tuple<string, ParseDelegate<object>>>
    {
      new Tuple<string, ParseDelegate<object>>("Collection", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Season", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Album", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Title", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Movie", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Part", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Track", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Chapter", TryGetString),
      new Tuple<string, ParseDelegate<object>>("SubTrack", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Original/Album", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Original/Movie", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Original/Track", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Track/More", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Track/Position", TryGetInt),
      new Tuple<string, ParseDelegate<object>>("Track/Position_Total", TryGetInt),
      new Tuple<string, ParseDelegate<object>>("Part/Position", TryGetInt),
      new Tuple<string, ParseDelegate<object>>("Part/Position_Total", TryGetInt),
      new Tuple<string, ParseDelegate<object>>("Album/Track_Total", TryGetInt),
      new Tuple<string, ParseDelegate<object>>("Album/Part/Track_Total", TryGetInt),
      new Tuple<string, ParseDelegate<object>>("Album/Part_Total", TryGetInt),
      new Tuple<string, ParseDelegate<object>>("Album/Performer", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Performer", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Performer/Sort", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Performer/Url", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Original/Performer", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Accompaniment", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Composer", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Composer/Nationality", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Arranger", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Lyricist", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Original/Lyricist", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Conductor", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Actor", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Artist", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Date_Tagged", TryGetDate),
      new Tuple<string, ParseDelegate<object>>("Actor_Character", TryGetString),
      new Tuple<string, ParseDelegate<object>>("WrittenBy", TryGetString),
      new Tuple<string, ParseDelegate<object>>("ScreenplayBy", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Director", TryGetString),
      new Tuple<string, ParseDelegate<object>>("AssistantDirector", TryGetString),
      new Tuple<string, ParseDelegate<object>>("DirectorOfPhotography", TryGetString),
      new Tuple<string, ParseDelegate<object>>("ArtDirector", TryGetString),
      new Tuple<string, ParseDelegate<object>>("EditedBy", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Producer", TryGetString),
      new Tuple<string, ParseDelegate<object>>("CoProducer", TryGetString),
      new Tuple<string, ParseDelegate<object>>("ExecutiveProducer", TryGetString),
      new Tuple<string, ParseDelegate<object>>("ProductionDesigner", TryGetString),
      new Tuple<string, ParseDelegate<object>>("CostumeDesigner", TryGetString),
      new Tuple<string, ParseDelegate<object>>("SoundEngineer", TryGetString),
      new Tuple<string, ParseDelegate<object>>("MasteredBy", TryGetString),
      new Tuple<string, ParseDelegate<object>>("RemixedBy", TryGetString),
      new Tuple<string, ParseDelegate<object>>("ProductionStudio", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Label", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Publisher", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Publisher/URL", TryGetString),
      new Tuple<string, ParseDelegate<object>>("DistributedBy", TryGetString),
      new Tuple<string, ParseDelegate<object>>("EncodedBy", TryGetString),
      new Tuple<string, ParseDelegate<object>>("ThanksTo", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Technician", TryGetString),
      new Tuple<string, ParseDelegate<object>>("CommissionedBy", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Encoded_Original/DistributedBy", TryGetString),
      new Tuple<string, ParseDelegate<object>>("RadioStation", TryGetString),
      new Tuple<string, ParseDelegate<object>>("RadioStation/Owner", TryGetString),
      new Tuple<string, ParseDelegate<object>>("RadioStation/URL", TryGetString),
      new Tuple<string, ParseDelegate<object>>("ContentType", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Subject", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Synopsys", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Summary", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Description", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Keywords", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Period", TryGetString),
      new Tuple<string, ParseDelegate<object>>("LawRating", TryGetString),
      new Tuple<string, ParseDelegate<object>>("IRCA", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Medium", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Product", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Country", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Written_Date", TryGetDate),
      new Tuple<string, ParseDelegate<object>>("Recorded_Date", TryGetDate),
      new Tuple<string, ParseDelegate<object>>("Released_Date", TryGetDate),
      new Tuple<string, ParseDelegate<object>>("Mastered_Date", TryGetDate),
      new Tuple<string, ParseDelegate<object>>("Encoded_Date", TryGetDate),
      new Tuple<string, ParseDelegate<object>>("Tagged_Date", TryGetDate),
      new Tuple<string, ParseDelegate<object>>("Original/Released_Date", TryGetDate),
      new Tuple<string, ParseDelegate<object>>("Original/Released_Date", TryGetDate),
      new Tuple<string, ParseDelegate<object>>("Purchased_Date", TryGetDate),
      new Tuple<string, ParseDelegate<object>>("Written_Location", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Recorded_Location", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Archival_Location", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Genre", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Mood", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Comment", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Rating", TryGetDouble),
      new Tuple<string, ParseDelegate<object>>("Purchased_Owner", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Purchased_Seller", TryGetString),
      new Tuple<string, ParseDelegate<object>>("PlayCounter", TryGetInt),
      new Tuple<string, ParseDelegate<object>>("Encoded_Application", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Encoded_Library", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Encoded_Library/Settings", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Encoded_Original", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Encoded_Original/URL", TryGetString),
      new Tuple<string, ParseDelegate<object>>("FileName_Original", TryGetString),
      new Tuple<string, ParseDelegate<object>>("File_Url", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Gain", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Peak", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Purchase_Info", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Purchase_Price", TryGetDouble),
      new Tuple<string, ParseDelegate<object>>("Purchase_Currency", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Purchase_Item", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Copyright", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Producer_Copyright", TryGetString),
      new Tuple<string, ParseDelegate<object>>("TermsOfUse", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Copyright/URL", TryGetString),
      new Tuple<string, ParseDelegate<object>>("ISRC", TryGetString),
      new Tuple<string, ParseDelegate<object>>("MSDI", TryGetString),
      new Tuple<string, ParseDelegate<object>>("ISBN", TryGetString),
      new Tuple<string, ParseDelegate<object>>("BarCode", TryGetString),
      new Tuple<string, ParseDelegate<object>>("LCCN", TryGetString),
      new Tuple<string, ParseDelegate<object>>("CatalogNumber", TryGetString),
      new Tuple<string, ParseDelegate<object>>("LabelCode", TryGetString),
      new Tuple<string, ParseDelegate<object>>("BPM", TryGetInt),
      new Tuple<string, ParseDelegate<object>>("Cover", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Cover_Description", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Cover_Type", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Cover_Mime", TryGetString),
      new Tuple<string, ParseDelegate<object>>("Cover_Data", TryGetString),
    };

    #endregion

    public static T GetAllTags<T>(MediaInfo mediaInfo, StreamKind kind, int streamPosition) where T : BaseTags, new()
    {
      if (mediaInfo == null)
      {
        throw new ArgumentNullException(nameof(mediaInfo));
      }

      var result = new T();
      foreach (var tagItem in TagItems)
      {
        var value = mediaInfo.Get(kind, streamPosition, tagItem.Item1);
        if (!string.IsNullOrEmpty(value) && tagItem.Item2(value, out var tagValue))
        {
          result.Tags.Add(tagItem.Item1, tagValue);
        }
        else
        {
          value = mediaInfo.Get(kind, streamPosition, tagItem.Item1.ToUpper());
          if (!string.IsNullOrEmpty(value) && tagItem.Item2(value, out var tagUpperValue))
          {
            result.Tags.Add(tagItem.Item1, tagUpperValue);
          }
        }
      }

      // parse covers
      object coverType = null;
      object coverMime = null;
      object coverDescription = null;
      object cover = null;
      result.Tags.TryGetValue("Cover_Type", out coverType);
      result.Tags.TryGetValue("Cover_Mime", out coverMime);
      result.Tags.TryGetValue("Cover_Description", out coverDescription);
      result.Tags.TryGetValue("Cover", out cover);

      if (result.Tags.TryGetValue("Cover_Data", out var coverData) ||
          coverType != null ||
          coverMime != null ||
          coverDescription != null ||
          cover != null)
      {
        var coverDataItems = ((string)coverData).Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
        var coverTypeItems = ((string)coverType)?.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
        var coverMimeItems = ((string)coverMime)?.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
        var coverItems = ((string)cover)?.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
        var coverDescriptionItems = ((string)coverDescription)?.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
        var itemCount = new int[] { coverDataItems.Length, coverTypeItems?.Length ?? 0, coverMimeItems?.Length ?? 0, coverDescriptionItems?.Length ?? 0, coverItems?.Length ?? 0 }.Max();
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

      return result;
    }

    public static bool TryParse(string source, out bool result)
    {
      return BooleanValues.TryGetValue(source, out result);
    }

    private static bool ToBool(string source)
    {
      return string.Equals(source, "t", StringComparison.OrdinalIgnoreCase)
             || string.Equals(source, "true", StringComparison.OrdinalIgnoreCase)
             || string.Equals(source, "y", StringComparison.OrdinalIgnoreCase)
             || string.Equals(source, "yes", StringComparison.OrdinalIgnoreCase)
             || string.Equals(source, "1", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetString(string source, out object value)
    {
      value = source;
      return !string.IsNullOrEmpty(source);
    }

    private static bool TryGetBase64(string source, out object value)
    {
      if (!string.IsNullOrEmpty(source))
      {
        value = Convert.FromBase64String(source);
        return true;
      }

      value = null;
      return false;
    }

    private static bool TryGetInt(string source, out object value)
    {
      int resultValue;
      var result = int.TryParse(source, out resultValue);
      value = resultValue;
      return result;
    }

    private static bool TryGetDouble(string source, out object value)
    {
      double resultValue;
      var result = double.TryParse(source, out resultValue);
      value = resultValue;
      return result;
    }

    private static bool TryGetDate(string source, out object value)
    {
      DateTime resultValue;
      var result = DateTime.TryParseExact(
        source,
        new[] { "yyyy", "yyyy-MM", "yyyy-MM-dd", "yyyy-M", "yyyy-M-d", "yyyy-MM-d", "yyyy-M-dd", "UTC yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.f",  "yyyy-MM-dd HH:mm:ss.ff", "yyyy-MM-dd HH:mm:ss.fff",  "yyyy-MM-dd HH:mm:ss.ffff",  "yyyy-MM-dd HH:mm:ss.fffff",
                "yyyy/MM/dd", "yyyy/M/d", "yyyy/M/dd", "yyyy/M/d", "UTC yyyy/MM/dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss.f", "yyyy/MM/dd HH:mm:ss.ff", "yyyy/MM/dd HH:mm:ss.fff", "yyyy/MM/dd HH:mm:ss.ffff", "yyyy/MM/dd HH:mm:ss.fffff",
                "dd.MM.yyyy", "d.MM.yyyy", "dd.M.yyyy", "d.M.yyyy", "UTC dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss.f", "dd.MM.yyyy HH:mm:ss.ff", "dd.MM.yyyy HH:mm:ss.fff", "dd.MM.yyyy HH:mm:ss.ffff", "dd.MM.yyyy HH:mm:ss.fffff",
                "MM/dd/yyyy", "M/dd/yyyy", "MM/d/yyyy", "M/d/yyyy", "UTC MM/dd/yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss.f", "dd.MM.yyyy HH:mm:ss.ff", "dd.MM.yyyy HH:mm:ss.fff", "dd.MM.yyyy HH:mm:ss.ffff", "dd.MM.yyyy HH:mm:ss.fffff",
        },
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out resultValue);
      value = resultValue;
      return result;
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