#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;

namespace MediaInfo.Model;

/// <summary>
/// Describes properties of the video tags
/// </summary>
/// <seealso cref="BaseTags" />
public class VideoTags : BaseTags
{
    /// <summary>
    /// The video tags.
    /// </summary>
    internal IDictionary<NativeMethods.Video, object> VideoDataTags { get; } = new Dictionary<NativeMethods.Video, object>();

    /// <summary>
    /// The title of the collection.
    /// </summary>
    public string? Collection =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Collection, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The title of the season.
    /// </summary>
    public string? Season =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Season, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The title of part.
    /// </summary>
    public string? Part =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Part, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The title of the file for a video file.
    /// </summary>
    public string? Movie
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

            var resultItems = result!.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
            return resultItems.Length > 1 ? resultItems[1].Trim() : resultItems[0].Trim();
        }
    }

    /// <summary>
    /// The title of chapter.
    /// </summary>
    public string? Chapter
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

            var resultItems = result!.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
            return resultItems.Length > 1 ? resultItems[0].Trim() : null;
        }
    }

    /// <summary>
    /// The original movie.
    /// </summary>
    public string? OriginalMovie =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Original_Movie, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The track position.
    /// </summary>
    public int? TrackPosition =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Track_Position, out var result) ?
            (int?)result :
            null;

    /// <summary>
    /// The composer name.
    /// </summary>
    public string? Composer =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Composer, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The composer nationality.
    /// </summary>
    public string? ComposerNationality =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Composer_Nationality, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The arranger name.
    /// </summary>
    public string? Arranger =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Arranger, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The lyricist name.
    /// </summary>
    public string? Lyricist =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Lyricist, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The conductor name.
    /// </summary>
    public string? Conductor =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Conductor, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The sound engineer name.
    /// </summary>
    public string? SoundEngineer =>
        GeneralTags.TryGetValue(NativeMethods.General.General_SoundEngineer, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The real name of the actor.
    /// </summary>
    public string? Actor =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Actor, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the character an actor or actress plays in this movie.
    /// </summary>
    public string? ActorCharacter =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Actor_Character, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The author of the story or script.
    /// </summary>
    public string? WrittenBy =>
        GeneralTags.TryGetValue(NativeMethods.General.General_WrittenBy, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The author of the screenplay or scenario (used for movies and TV shows).
    /// </summary>
    public string? ScreenplayBy =>
        GeneralTags.TryGetValue(NativeMethods.General.General_ScreenplayBy, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the director.
    /// </summary>
    public string? Director =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Director, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of assistant director.
    /// </summary>
    public string? AssistantDirector =>
        GeneralTags.TryGetValue(NativeMethods.General.General_AssistantDirector, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the director of photography, also known as cinematographer.
    /// </summary>
    public string? DirectorOfPhotography =>
        GeneralTags.TryGetValue(NativeMethods.General.General_DirectorOfPhotography, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The person who oversees the artists and craftspeople who build the sets.
    /// </summary>
    public string? ArtDirector =>
        GeneralTags.TryGetValue(NativeMethods.General.General_ArtDirector, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The editor name.
    /// </summary>
    public string? EditedBy =>
        GeneralTags.TryGetValue(NativeMethods.General.General_EditedBy, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of producer the movie.
    /// </summary>
    public string? Producer =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Producer, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the co-producer.
    /// </summary>
    public string? CoProducer =>
        GeneralTags.TryGetValue(NativeMethods.General.General_CoProducer, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of executive producer.
    /// </summary>
    public string? ExecutiveProducer =>
        GeneralTags.TryGetValue(NativeMethods.General.General_ExecutiveProducer, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The artist responsible for designing the overall visual appearance of a movie.
    /// </summary>
    public string? ProductionDesigner =>
        GeneralTags.TryGetValue(NativeMethods.General.General_ProductionDesigner, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the costume designer.
    /// </summary>
    public string? CostumeDesigner =>
        GeneralTags.TryGetValue(NativeMethods.General.General_CostumeDesigner, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the choreographer.
    /// </summary>
    public string? Choreographer =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Choreographer, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of production studio.
    /// </summary>
    public string? ProductionStudio =>
        GeneralTags.TryGetValue(NativeMethods.General.General_ProductionStudio, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The time that the composition of the music/script began.
    /// </summary>
    public DateTime? WrittenDate =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Written_Date, out var result) ?
            (DateTime?)result :
            null;

    /// <summary>
    /// The main genre of the audio or video; e.g. "classical", "ambient-house", "synthpop", "sci-fi", "drama", etc.
    /// </summary>
    public string? Genre =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Genre, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// Intended to reflect the mood of the item with a few keywords, e.g. "Romantic", "Sad", "Uplifting", etc.
    /// </summary>
    public string? Mood =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Mood, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the software package used to create the file, such as "Microsoft WaveEdit."
    /// </summary>
    public string? EncodedApplication =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Encoded_Application, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The software or hardware used to encode this item; e.g. "LAME" or "XviD".
    /// </summary>
    public string? EncodedLibrary =>
        VideoDataTags.TryGetValue(NativeMethods.Video.Video_Encoded_Library, out var videoResult) ?
            (string)videoResult :
            GeneralTags.TryGetValue(NativeMethods.General.General_Encoded_Library, out var result) ?
                (string)result :
                null;

    /// <summary>
    /// A list of the settings used for encoding this item. No specific format.
    /// </summary>
    public string? EncodedLibrarySettings =>
        VideoDataTags.TryGetValue(NativeMethods.Video.Video_Encoded_Library_Settings, out var videoResult) ?
            (string)videoResult :
            GeneralTags.TryGetValue(NativeMethods.General.General_Encoded_Library_Settings, out var result) ?
                (string)result :
                null;

    /// <summary>
    /// A plot outline or a summary of the story.
    /// </summary>
    public string? Summary =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Summary, out var result) ?
            (string)result :
            null;
}