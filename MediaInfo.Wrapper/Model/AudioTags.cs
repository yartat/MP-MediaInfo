#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaInfo.Model;

/// <summary>
/// Describes properties of the audio tags
/// </summary>
/// <seealso cref="BaseTags" />
public class AudioTags : BaseTags
{
    /// <summary>
    /// The audio tags.
    /// </summary>
    internal IDictionary<NativeMethods.Audio, object> AudioDataTags { get; } = new Dictionary<NativeMethods.Audio, object>();

    /// <summary>
    /// The title of the album.
    /// </summary>
    public string? Album
    {
        get
        {
            var result = GeneralTags.TryGetValue(NativeMethods.General.General_Album, out var album) ? (string)album : null;
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            result = GeneralTags.TryGetValue(NativeMethods.General.General_Title, out var title) ? (string)title : null;
            if (string.IsNullOrEmpty(result))
            {
                result = AudioDataTags.TryGetValue(NativeMethods.Audio.Audio_Title, out var trackTitle) ? (string)trackTitle : null;
            }

            if (string.IsNullOrEmpty(result))
            {
                return result;
            }

            var resultItems = result!.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
            return resultItems.Length > 1 ? resultItems[1].Trim() : resultItems[0].Trim();
        }
    }

    /// <summary>
    /// The title of the track.
    /// </summary>
    public string? Track
    {
        get
        {
            var result = GeneralTags.TryGetValue(NativeMethods.General.General_Track, out var track) ? (string)track : null;
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            result = GeneralTags.TryGetValue(NativeMethods.General.General_Label, out var title) ? (string)title : null;
            if (string.IsNullOrEmpty(result))
            {
                result = GeneralTags.TryGetValue(NativeMethods.General.General_Title, out var trackTitle) ? (string)trackTitle : null;
                if (!string.IsNullOrEmpty(result))
                {
                    return result!.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
                }
            }

            if (string.IsNullOrEmpty(result))
            {
                result = AudioDataTags.TryGetValue(NativeMethods.Audio.Audio_Title, out var trackTitle) ? (string)trackTitle : null;
            }

            return !string.IsNullOrEmpty(result) ?
                result!.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() :
                result;
        }
    }

    /// <summary>
    /// The title of the subtrack.
    /// </summary>
    public string? SubTrack =>
        GeneralTags.TryGetValue(NativeMethods.General.General_SubTrack, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The original album name (in case of a remake/remix).
    /// </summary>
    public string? OriginalAlbum =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Original_Album, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The original track name (in case of a remake/remix).
    /// </summary>
    public string? OriginalTrack =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Original_Track, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The number of the current track.
    /// </summary>
    public int? TrackPosition =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Track_Position, out var result) ?
            (int?)result :
            null;

    /// <summary>
    /// The number of all tracks.
    /// </summary>
    public int? TotalTracks =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Track_Position_Total, out var result) ?
            (int?)result :
            null;

    /// <summary>
    /// The number of the current part in a multi-disc album.
    /// </summary>
    public int? DiscNumber =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Part_Position, out var result) ?
            (int?)result :
            null;

    /// <summary>
    /// The number of all parts in a multi-disc album.
    /// </summary>
    public int? TotalDiscs =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Part_Position_Total, out var result) ?
            (int?)result :
            null;

    /// <summary>
    /// A person or band/collective generally considered responsible for the work : Singer, Implementor.
    /// </summary>
    public string? Artist =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Performer, out var performer) ?
            (string)performer :
            GeneralTags.TryGetValue(NativeMethods.General.General_Album_Performer, out var result) ?
                (string)result :
                null;

    /// <summary>
    /// The album artist.
    /// </summary>
    public string? AlbumArtist =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Album_Performer, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The official artist/performer web page.
    /// </summary>
    public string? ArtistUrl =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Performer_Url, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the accompaniment.
    /// </summary>
    public string? Accompaniment =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Accompaniment, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the composer.
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
    /// The name of the arranger.
    /// </summary>
    public string? Arranger =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Arranger, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the lyricist.
    /// </summary>
    public string? Lyricist =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Lyricist, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the conductor.
    /// </summary>
    public string? Conductor =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Conductor, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the sound engineer.
    /// </summary>
    public string? SoundEngineer =>
        GeneralTags.TryGetValue(NativeMethods.General.General_SoundEngineer, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// Who mastered the track.
    /// </summary>
    public string? MasteredBy =>
        GeneralTags.TryGetValue(NativeMethods.General.General_MasteredBy, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// Who remixed the track.
    /// </summary>
    public string? RemixedBy =>
        GeneralTags.TryGetValue(NativeMethods.General.General_RemixedBy, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The label name.
    /// </summary>
    public string? Label =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Label, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The date of record track or album.
    /// </summary>
    public DateTime? RecordedDate =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Recorded_Date, out var result) ?
            (DateTime?)result :
            null;

    /// <summary>
    /// The genre.
    /// </summary>
    public string? Genre =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Genre, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The mood.
    /// </summary>
    public string? Mood =>
        GeneralTags.TryGetValue(NativeMethods.General.General_Mood, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The track ISRC.
    /// </summary>
    public string? Isrc =>
        GeneralTags.TryGetValue(NativeMethods.General.General_ISRC, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The bar code.
    /// </summary>
    public string? BarCode =>
        GeneralTags.TryGetValue(NativeMethods.General.General_BarCode, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// LCCN.
    /// </summary>
    public string? Lccn =>
        GeneralTags.TryGetValue(NativeMethods.General.General_LCCN, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The catalog number.
    /// </summary>
    public string? CatalogNumber =>
        GeneralTags.TryGetValue(NativeMethods.General.General_CatalogNumber, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The label code.
    /// </summary>
    public string? LabelCode =>
        GeneralTags.TryGetValue(NativeMethods.General.General_LabelCode, out var result) ?
            (string)result :
            null;

    /// <summary>
    /// The name of the person or organization that encoded/ripped the audio file.
    /// </summary>
    public string? EncodedBy =>
        GeneralTags.TryGetValue(NativeMethods.General.General_EncodedBy, out var result) ?
            (string)result :
            null;
}