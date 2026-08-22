#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using MediaInfo.Model;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Converts the string representation of a value to specified type. The method returns a boolean value that
  /// indicates whether the conversion succeeded or failed. The converted value is returned through an output parameter.
  /// </summary>
  /// <remarks>Implementations of this delegate should attempt to convert the input string to the specified type <typeparamref name="T"/>.
  /// If the conversion is successful, the method should return <b>true</b> and set the output parameter to the converted value.
  /// If the conversion fails, the method should return <b>false</b> and set the output parameter to the default value of type <typeparamref name="T"/>.
  /// </remarks>
  /// <typeparam name="T">The type of the value to convert.</typeparam>
  /// <param name="source">The string representation of the value to convert.</param>
  /// <param name="result">The result of the conversion.</param>
  /// <returns><b>true</b> if the value was converted successfully; otherwise, <b>false</b>.</returns>
  public delegate bool ParseDelegate<T>(string source, out T result);

  /// <summary>
  /// Provides functionality to build and populate a tag object of type T with general and audio metadata extracted from
  /// a media stream.
  /// </summary>
  /// <remarks>This class is intended for internal use to aggregate and parse general and audio tag information,
  /// including cover metadata, from a specified media stream. It supports a wide range of standard and extended tag
  /// fields. If certain tags are not present in the stream, they will be omitted from the resulting tag object. Cover
  /// information is parsed and included as a collection, which may be empty if no cover data is found.</remarks>
  /// <typeparam name="T">The type of tag object to build. Must inherit from BaseTags and have a parameterless constructor.</typeparam>
  /// <param name="mediaInfo">The MediaInfo instance used to retrieve metadata from the media stream. Cannot be null.</param>
  /// <param name="streamPosition">The zero-based index of the stream within the media file for which tags are to be extracted.</param>
  internal class GeneralTagBuilder<T>(IMediaInfoReader mediaInfo, int streamPosition) where T : BaseTags, new()
  {
    #region Tag items

    private static readonly List<(NativeMethods.General Tag, ParseDelegate<object> ParseFunc)> GeneralTagItems = new()
    {
      (NativeMethods.General.General_Collection, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Season, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Album, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Title, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Movie, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Part, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Track, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Chapter, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_SubTrack, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Original_Album, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Original_Movie, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Original_Track, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Track_More, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Track_Position, TagBuilderHelper.TryGetInt),
      (NativeMethods.General.General_Track_Position_Total, TagBuilderHelper.TryGetInt),
      (NativeMethods.General.General_Part_Position, TagBuilderHelper.TryGetInt),
      (NativeMethods.General.General_Part_Position_Total, TagBuilderHelper.TryGetInt),
      (NativeMethods.General.General_Album_Performer, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Performer, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_MusicBy, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_RemixedBy, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Performer_Sort, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Performer_Url, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Original_Performer, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Accompaniment, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Composer, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Composer_Nationality, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Arranger, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Lyricist, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Original_Lyricist, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Conductor, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Actor, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Actor_Character, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_WrittenBy, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_ScreenplayBy, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Director, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_AssistantDirector, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_DirectorOfPhotography, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_ArtDirector, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_EditedBy, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Producer, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_CoProducer, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_ExecutiveProducer, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_ProductionDesigner, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_CostumeDesigner, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_SoundEngineer, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_MasteredBy, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_RemixedBy, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_ProductionStudio, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Label, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Publisher, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Publisher_URL, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_DistributedBy, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_EncodedBy, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_ThanksTo, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_CommissionedBy, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_ContentType, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Subject, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Summary, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Description, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Keywords, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Period, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_LawRating, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Country, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Written_Date, TagBuilderHelper.TryGetDate),
      (NativeMethods.General.General_Recorded_Date, TagBuilderHelper.TryGetDate),
      (NativeMethods.General.General_Released_Date, TagBuilderHelper.TryGetDate),
      (NativeMethods.General.General_Mastered_Date, TagBuilderHelper.TryGetDate),
      (NativeMethods.General.General_Encoded_Date, TagBuilderHelper.TryGetDate),
      (NativeMethods.General.General_Tagged_Date, TagBuilderHelper.TryGetDate),
      (NativeMethods.General.General_Original_Released_Date, TagBuilderHelper.TryGetDate),
      (NativeMethods.General.General_Written_Location, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Recorded_Location, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Archival_Location, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Genre, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Mood, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Comment, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Rating, TagBuilderHelper.TryGetDouble),
      (NativeMethods.General.General_Encoded_Application, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Encoded_Library, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Encoded_Library_Settings, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Copyright, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Producer_Copyright, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_TermsOfUse, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_ISRC, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_ISBN, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_BarCode, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_LCCN, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_CatalogNumber, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_LabelCode, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_BPM, TagBuilderHelper.TryGetInt),
      (NativeMethods.General.General_Cover, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Cover_Description, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Cover_Type, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Cover_Mime, TagBuilderHelper.TryGetString),
      (NativeMethods.General.General_Cover_Data, TagBuilderHelper.TryGetString),
    };

    #endregion

    /// <summary>
    /// Gets the media information. The <c>MediaInfo</c> instance is used to retrieve tag values.
    /// </summary>
    protected IMediaInfoReader MediaInfo { get; } = mediaInfo ?? throw new ArgumentNullException(nameof(mediaInfo));

    /// <summary>
    /// Stream position to get tags for. For example, if the media file contains 2 audio streams and the <c>streamPosition</c> is set to 1,
    /// then the tags will be retrieved for the second audio stream.
    /// </summary>
    protected int StreamPosition { get; } = streamPosition;

    /// <summary>
    /// Builds and returns a new instance of type T populated with general and audio tag information extracted from the
    /// media stream.
    /// </summary>
    /// <remarks>The returned object includes general tags, audio-specific tags, and cover metadata if
    /// available. If certain tags are not present in the media stream, they will not be included in the result. Cover
    /// information is parsed and included as a collection, which may be empty if no cover data is found.
    /// </remarks>
    /// <returns>A new instance of type T containing the parsed general tags, audio tags, and cover information from the media
    /// stream.
    /// </returns>
    public virtual T Build()
    {
      var result = new T();
      foreach (var (tag, parseFunc) in GeneralTagItems)
      {
        var value = MediaInfo.Get(StreamKind.General, StreamPosition, (int)tag);
        if (!string.IsNullOrEmpty(value) && parseFunc(value, out var tagValue))
        {
          result.GeneralTags.Add(tag, tagValue);
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
        var coverDataItems = ((string?)coverData)?.Split([" / "], StringSplitOptions.RemoveEmptyEntries);
        var coverTypeItems = ((string?)coverType)?.Split([" / "], StringSplitOptions.RemoveEmptyEntries);
        var coverMimeItems = ((string?)coverMime)?.Split([" / "], StringSplitOptions.RemoveEmptyEntries);
        var coverItems = ((string?)cover)?.Split([" / "], StringSplitOptions.RemoveEmptyEntries);
        var coverDescriptionItems = ((string?)coverDescription)?.Split([" / "], StringSplitOptions.RemoveEmptyEntries);
        var itemCount = new[] { coverDataItems?.Length ?? 0, coverTypeItems?.Length ?? 0, coverMimeItems?.Length ?? 0, coverDescriptionItems?.Length ?? 0, coverItems?.Length ?? 0 }.Max();
        if (itemCount > 0)
        {
          var covers = new List<CoverInfo>(itemCount);
          for (var i = 0; i < itemCount; ++i)
          {
            var data = coverDataItems?.TryGet(i, string.Empty) ?? string.Empty;
            covers.Add(new CoverInfo
            {
              Exists = ToBool(coverItems?.TryGet(i, string.Empty)),
              Description = coverDescriptionItems?.TryGet(i, string.Empty) ?? string.Empty,
              Type = coverTypeItems?.TryGet(i, string.Empty) ?? string.Empty,
              Mime = coverMimeItems?.TryGet(i, string.Empty) ?? string.Empty,
              Data = data.TryGetBase64(out byte[]? bytes) ? bytes : null,
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
        var value = MediaInfo.Get(StreamKind.Audio, StreamPosition, "DATE_TAGGED").TryGetDate(out DateTime res) ? (DateTime?)res : null;
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
        var value = MediaInfo.Get(StreamKind.Audio, StreamPosition, "RATING").TryGetDouble(out double res) ? (double?)res : null;
        if (value != null)
        {
          result.GeneralTags.Add(NativeMethods.General.General_Rating, value);
        }
      }

      if (!result.GeneralTags.ContainsKey(NativeMethods.General.General_Released_Date))
      {
        var value = MediaInfo.Get(StreamKind.Audio, StreamPosition, "Released_Date").TryGetDate(out DateTime res) ? (DateTime?)res : null;
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

      if (!result.GeneralTags.ContainsKey((NativeMethods.General)1000) &&
          MediaInfo.Get(StreamKind.General, StreamPosition, "Stereoscopic").TryGetBool(out var stereoscopic))
      {
        result.GeneralTags.Add((NativeMethods.General)1000, stereoscopic);
      }

      if (!result.GeneralTags.ContainsKey((NativeMethods.General)1001) &&
          MediaInfo.Get(StreamKind.General, StreamPosition, "StereoscopicLayout").TryGetStereoMode(out var stereoscopicLayout))
      {
        result.GeneralTags.Add((NativeMethods.General)1001, stereoscopicLayout);
      }

      if (!result.GeneralTags.ContainsKey((NativeMethods.General)1002) &&
          MediaInfo.Get(StreamKind.General, StreamPosition, "StereoscopicSkip").TryGetInt(out int stereoscopicSkip))
      {
        result.GeneralTags.Add((NativeMethods.General)1002, stereoscopicSkip);
      }

      return result;
    }

    protected static bool ToBool(string? source) =>
      !string.IsNullOrEmpty(source) &&
       (string.Equals(source, "t", StringComparison.OrdinalIgnoreCase)
        || string.Equals(source, "true", StringComparison.OrdinalIgnoreCase)
        || string.Equals(source, "y", StringComparison.OrdinalIgnoreCase)
        || string.Equals(source, "yes", StringComparison.OrdinalIgnoreCase)
        || string.Equals(source, "1", StringComparison.OrdinalIgnoreCase));
  }

  internal static class ArrayExtensions
  {
    /// <summary>
    /// Attempts to retrieve the element at the specified index in the array, or returns a default value if the index is
    /// out of range or the array is null.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The array from which to retrieve the element. Can be null.</param>
    /// <param name="index">The zero-based index of the element to retrieve. Must be non-negative.</param>
    /// <param name="defaultValue">The value to return if the array is null or the index is outside the bounds of the array.</param>
    /// <returns>The element at the specified index if it exists; otherwise, the specified default value.</returns>
    /// <exception cref="ArgumentException">Thrown if index is a negative value.</exception>
    public static T TryGet<T>(this T[] array, int index, T defaultValue)
    {
      if (index < 0)
      {
        throw new ArgumentException($"Parameter {nameof(index)} is a negative value.");
      }

      if (array is null)
      {
        return defaultValue;
      }

      return array.Length > index ? array[index] : defaultValue;
    }
  }
}