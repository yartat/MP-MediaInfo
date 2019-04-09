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

namespace MediaInfo.Model
{
  /// <summary>
  /// Describes properties of the video tags
  /// </summary>
  /// <seealso cref="BaseTags" />
  public class VideoTags : BaseTags
  {
#if DEBUG
    /// <summary>
    /// Gets or sets the video tags.
    /// </summary>
    /// <value>
    /// The video tags.
    /// </value>
    internal IDictionary<NativeMethods.Video, object> VideoDataTags { get; } = new Dictionary<NativeMethods.Video, object>();
#endif

    /// <summary>
    /// Gets the title of the collection.
    /// </summary>
    /// <value>
    /// The title of the collection.
    /// </value>
    public string Collection => GeneralTags.TryGetValue(NativeMethods.General.General_Collection, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the title of the season.
    /// </summary>
    /// <value>
    /// The title of the season.
    /// </value>
    public string Season => GeneralTags.TryGetValue(NativeMethods.General.General_Season, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the title of part.
    /// </summary>
    /// <value>
    /// The title of part.
    /// </value>
    public string Part => GeneralTags.TryGetValue(NativeMethods.General.General_Part, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the title of the file for a video file.
    /// </summary>
    /// <value>
    /// The title of the file for a video file.
    /// </value>
    public string Movie
    {
      get
      {
        var result = GeneralTags.TryGetValue(NativeMethods.General.General_Movie, out var movie) ? (string)movie : null;
        if (!string.IsNullOrEmpty(result))
        {
          return result;
        }

        result = GeneralTags.TryGetValue(NativeMethods.General.General_Title, out var title) ? (string)title : null;
        if (string.IsNullOrEmpty(result))
        {
          return result;
        }

        var resultItems = result.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
        return resultItems.Length > 1 ? resultItems[1].Trim() : resultItems[0].Trim();
      }
    }

    /// <summary>
    /// Gets the title of chapter.
    /// </summary>
    /// <value>
    /// The title of chapter.
    /// </value>
    public string Chapter
    {
      get
      {
        var result = GeneralTags.TryGetValue(NativeMethods.General.General_Chapter, out var chapter) ? (string)chapter : null;
        if (!string.IsNullOrEmpty(result))
        {
          return result;
        }

        result = GeneralTags.TryGetValue(NativeMethods.General.General_Title, out var title) ? (string)title : null;
        if (string.IsNullOrEmpty(result))
        {
          return result;
        }

        var resultItems = result.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
        return resultItems.Length > 1 ? resultItems[0].Trim() : null;
      }
    }

    /// <summary>
    /// Gets the original movie.
    /// </summary>
    /// <value>
    /// The original movie.
    /// </value>
    public string OriginalMovie => GeneralTags.TryGetValue(NativeMethods.General.General_Original_Movie, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the track position.
    /// </summary>
    /// <value>
    /// The track position.
    /// </value>
    public int? TrackPosition => GeneralTags.TryGetValue(NativeMethods.General.General_Track_Position, out var result) ? (int?)result : null;

    /// <summary>
    /// Gets the composer name.
    /// </summary>
    /// <value>
    /// The composer name.
    /// </value>
    public string Composer => GeneralTags.TryGetValue(NativeMethods.General.General_Composer, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the composer nationality.
    /// </summary>
    /// <value>
    /// The composer nationality.
    /// </value>
    public string ComposerNationality => GeneralTags.TryGetValue(NativeMethods.General.General_Composer_Nationality, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the arranger name.
    /// </summary>
    /// <value>
    /// The arranger name.
    /// </value>
    public string Arranger => GeneralTags.TryGetValue(NativeMethods.General.General_Arranger, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the lyricist name.
    /// </summary>
    /// <value>
    /// The lyricist name.
    /// </value>
    public string Lyricist => GeneralTags.TryGetValue(NativeMethods.General.General_Lyricist, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the conductor name.
    /// </summary>
    /// <value>
    /// The conductor name.
    /// </value>
    public string Conductor => GeneralTags.TryGetValue(NativeMethods.General.General_Conductor, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the sound engineer name.
    /// </summary>
    /// <value>
    /// The sound engineer name.
    /// </value>
    public string SoundEngineer => GeneralTags.TryGetValue(NativeMethods.General.General_SoundEngineer, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the real name of actor.
    /// </summary>
    /// <value>
    /// The real name of actor.
    /// </value>
    public string Actor => GeneralTags.TryGetValue(NativeMethods.General.General_Actor, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the character an actor or actress plays in this movie.
    /// </summary>
    /// <value>
    /// The name of the character an actor or actress plays in this movie.
    /// </value>
    public string ActorCharacter => GeneralTags.TryGetValue(NativeMethods.General.General_Actor_Character, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the author of the story or script.
    /// </summary>
    /// <value>
    /// The author of the story or script.
    /// </value>
    public string WrittenBy => GeneralTags.TryGetValue(NativeMethods.General.General_WrittenBy, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the author of the screenplay or scenario (used for movies and TV shows).
    /// </summary>
    /// <value>
    /// The author of the screenplay or scenario (used for movies and TV shows).
    /// </value>
    public string ScreenplayBy => GeneralTags.TryGetValue(NativeMethods.General.General_ScreenplayBy, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the director.
    /// </summary>
    /// <value>
    /// The name of the director.
    /// </value>
    public string Director => GeneralTags.TryGetValue(NativeMethods.General.General_Director, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of assistant director.
    /// </summary>
    /// <value>
    /// The name of assistant director.
    /// </value>
    public string AssistantDirector => GeneralTags.TryGetValue(NativeMethods.General.General_AssistantDirector, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the director of photography, also known as cinematographer.
    /// </summary>
    /// <value>
    /// The name of the director of photography, also known as cinematographer.
    /// </value>
    public string DirectorOfPhotography => GeneralTags.TryGetValue(NativeMethods.General.General_DirectorOfPhotography, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the person who oversees the artists and craftspeople who build the sets.
    /// </summary>
    /// <value>
    /// The person who oversees the artists and craftspeople who build the sets.
    /// </value>
    public string ArtDirector => GeneralTags.TryGetValue(NativeMethods.General.General_ArtDirector, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the editor name.
    /// </summary>
    /// <value>
    /// The editor name.
    /// </value>
    public string EditedBy => GeneralTags.TryGetValue(NativeMethods.General.General_EditedBy, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of producer the movie.
    /// </summary>
    /// <value>
    /// The name of producer the movie.
    /// </value>
    public string Producer => GeneralTags.TryGetValue(NativeMethods.General.General_Producer, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of co-producer.
    /// </summary>
    /// <value>
    /// The name of co-producer.
    /// </value>
    public string CoProducer => GeneralTags.TryGetValue(NativeMethods.General.General_CoProducer, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of executive producer.
    /// </summary>
    /// <value>
    /// The name of executive producer.
    /// </value>
    public string ExecutiveProducer => GeneralTags.TryGetValue(NativeMethods.General.General_ExecutiveProducer, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the artist responsible for designing the overall visual appearance of a movie.
    /// </summary>
    /// <value>
    /// The artist responsible for designing the overall visual appearance of a movie.
    /// </value>
    public string ProductionDesigner => GeneralTags.TryGetValue(NativeMethods.General.General_ProductionDesigner, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the costume designer.
    /// </summary>
    /// <value>
    /// The name of the costume designer.
    /// </value>
    public string CostumeDesigner => GeneralTags.TryGetValue(NativeMethods.General.General_CostumeDesigner, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the choreographer.
    /// </summary>
    /// <value>
    /// The name of the choreographer.
    /// </value>
    public string Choreographer => GeneralTags.TryGetValue(NativeMethods.General.General_Choregrapher, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of production studio.
    /// </summary>
    /// <value>
    /// The name of production studio.
    /// </value>
    public string ProductionStudio => GeneralTags.TryGetValue(NativeMethods.General.General_ProductionStudio, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the time that the composition of the music/script began.
    /// </summary>
    /// <value>
    /// The time that the composition of the music/script began.
    /// </value>
    public DateTime? WrittenDate => GeneralTags.TryGetValue(NativeMethods.General.General_Written_Date, out var result) ? (DateTime?)result : null;

    /// <summary>
    /// Gets the main genre of the audio or video; e.g. "classical", "ambient-house", "synthpop", "sci-fi", "drama", etc.
    /// </summary>
    /// <value>
    /// The main genre of the audio or video; e.g. "classical", "ambient-house", "synthpop", "sci-fi", "drama", etc.
    /// </value>
    public string Genre => GeneralTags.TryGetValue(NativeMethods.General.General_Genre, out var result) ? (string)result : null;

    /// <summary>
    /// Gets intended to reflect the mood of the item with a few keywords, e.g. "Romantic", "Sad", "Uplifting", etc.
    /// </summary>
    /// <value>
    /// Intended to reflect the mood of the item with a few keywords, e.g. "Romantic", "Sad", "Uplifting", etc.
    /// </value>
    public string Mood => GeneralTags.TryGetValue(NativeMethods.General.General_Mood, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the software package used to create the file, such as "Microsoft WaveEdit."
    /// </summary>
    /// <value>
    /// The name of the software package used to create the file, such as "Microsoft WaveEdit."
    /// </value>
    public string EncodedApplication => GeneralTags.TryGetValue(NativeMethods.General.General_Encoded_Application, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the software or hardware used to encode this item; e.g. "LAME" or "XviD"
    /// </summary>
    /// <value>
    /// The software or hardware used to encode this item; e.g. "LAME" or "XviD".
    /// </value>
    public string EncodedLibrary => GeneralTags.TryGetValue(NativeMethods.General.General_Encoded_Library, out var result) ? (string)result : null;

    /// <summary>
    /// Gets a list of the settings used for encoding this item. No specific format.
    /// </summary>
    /// <value>
    /// A list of the settings used for encoding this item. No specific format.
    /// </value>
    public string EncodedLibrarySettings => GeneralTags.TryGetValue(NativeMethods.General.General_Encoded_Library_Settings, out var result) ? (string)result : null;

    /// <summary>
    /// Gets a plot outline or a summary of the story.
    /// </summary>
    /// <value>
    /// A plot outline or a summary of the story.
    /// </value>
    public string Summary => GeneralTags.TryGetValue(NativeMethods.General.General_Summary, out var result) ? (string)result : null;
  }
}