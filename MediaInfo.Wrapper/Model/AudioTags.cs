#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaInfo.Model
{
    /// <summary>
    /// Describes properties of the audio tags
    /// </summary>
    /// <seealso cref="BaseTags" />
    public class AudioTags : BaseTags
  {
    /// <summary>
    /// Gets or sets the audio tags.
    /// </summary>
    /// <value>
    /// The audio tags.
    /// </value>
    internal IDictionary<NativeMethods.Audio, object> AudioDataTags { get; } = new Dictionary<NativeMethods.Audio, object>();

    /// <summary>
    /// Gets the title of the album.
    /// </summary>
    /// <value>
    /// The title of the album.
    /// </value>
    public string Album
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

        var resultItems = result.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
        return resultItems.Length > 1 ? resultItems[1].Trim() : resultItems[0].Trim();
      }
    }

    /// <summary>
    /// Gets the title of the track.
    /// </summary>
    /// <value>
    /// The title of the track.
    /// </value>
    public string Track
    {
      get
      {
        var result = GeneralTags.TryGetValue(NativeMethods.General.General_Track, out var track) ? (string)track : null;
        if (!string.IsNullOrEmpty(result))
        {
          return result;
        }

        result = GeneralTags.TryGetValue(NativeMethods.General.General_Label, out var title) ? (string) title : null;
        if (string.IsNullOrEmpty(result))
        {
          result = GeneralTags.TryGetValue(NativeMethods.General.General_Title, out var trackTitle) ? (string)trackTitle : null;
          if (!string.IsNullOrEmpty(result))
          {
            return result.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
          }
        }

        if (string.IsNullOrEmpty(result))
        {
          result = AudioDataTags.TryGetValue(NativeMethods.Audio.Audio_Title, out var trackTitle) ? (string)trackTitle : null;
        }

        return !string.IsNullOrEmpty(result)
          ? result.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim()
          : result;
      }
    }

    /// <summary>
    /// Gets the title of the subtrack.
    /// </summary>
    /// <value>
    /// The title of the subtrack.
    /// </value>
    public string SubTrack => GeneralTags.TryGetValue(NativeMethods.General.General_SubTrack, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the original album name (in case of a remake/remix).
    /// </summary>
    /// <value>
    /// The original album name (in case of a remake/remix).
    /// </value>
    public string OriginalAlbum => GeneralTags.TryGetValue(NativeMethods.General.General_Original_Album, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the original track name (in case of a remake/remix).
    /// </summary>
    /// <value>
    /// The original track name (in case of a remake/remix).
    /// </value>
    public string OriginalTrack => GeneralTags.TryGetValue(NativeMethods.General.General_Original_Track, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the number of the current track.
    /// </summary>
    /// <value>
    /// The number of the current track.
    /// </value>
    public int? TrackPosition => GeneralTags.TryGetValue(NativeMethods.General.General_Track_Position, out var result) ? (int?)result : null;

    /// <summary>
    /// Gets the number of all tracks.
    /// </summary>
    /// <value>
    /// The number of all tracks.
    /// </value>
    public int? TotalTracks => GeneralTags.TryGetValue(NativeMethods.General.General_Track_Position_Total, out var result) ? (int?)result : null;

    /// <summary>
    /// Gets the number of the current part in a multi-disc album.
    /// </summary>
    /// <value>
    /// The number of the current part in a multi-disc album.
    /// </value>
    public int? DiscNumber => GeneralTags.TryGetValue(NativeMethods.General.General_Part_Position, out var result) ? (int?)result : null;

    /// <summary>
    /// Gets the number of all parts in a multi-disc album.
    /// </summary>
    /// <value>
    /// The number of all parts in a multi-disc album.
    /// </value>
    public int? TotalDiscs => GeneralTags.TryGetValue(NativeMethods.General.General_Part_Position_Total, out var result) ? (int?)result : null;

    /// <summary>
    /// Gets a person or band/collective generally considered responsible for the work : Singer, Implementor.
    /// </summary>
    /// <value>
    /// A person or band/collective generally considered responsible for the work : Singer, Implementor.
    /// </value>
    public string Artist => GeneralTags.TryGetValue(NativeMethods.General.General_Performer, out var performer) ?
      (string)performer :
      GeneralTags.TryGetValue(NativeMethods.General.General_Album_Performer, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the album artist.
    /// </summary>
    /// <value>
    /// The album artist.
    /// </value>
    public string AlbumArtist => GeneralTags.TryGetValue(NativeMethods.General.General_Album_Performer, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the official artist/performer web page.
    /// </summary>
    /// <value>
    /// The official artist/performer web page.
    /// </value>
    public string ArtistUrl => GeneralTags.TryGetValue(NativeMethods.General.General_Performer_Url, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the accompaniment name.
    /// </summary>
    /// <value>
    /// The accompaniment name.
    /// </value>
    public string Accompaniment => GeneralTags.TryGetValue(NativeMethods.General.General_Accompaniment, out var result) ? (string)result : null;

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
    /// Gets who mastered track.
    /// </summary>
    /// <value>
    /// Who mastered track.
    /// </value>
    public string MasteredBy => GeneralTags.TryGetValue(NativeMethods.General.General_MasteredBy, out var result) ? (string)result : null;

    /// <summary>
    /// Gets who remixed track.
    /// </summary>
    /// <value>
    /// Who remixed track.
    /// </value>
    public string RemixedBy => GeneralTags.TryGetValue(NativeMethods.General.General_RemixedBy, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the label name.
    /// </summary>
    /// <value>
    /// The label name.
    /// </value>
    public string Label => GeneralTags.TryGetValue(NativeMethods.General.General_Label, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the recorded date.
    /// </summary>
    /// <value>
    /// The recorded date.
    /// </value>
    public DateTime? RecordedDate => GeneralTags.TryGetValue(NativeMethods.General.General_Recorded_Date, out var result) ? (DateTime?)result : null;

    /// <summary>
    /// Gets the genre.
    /// </summary>
    /// <value>
    /// The genre.
    /// </value>
    public string Genre => GeneralTags.TryGetValue(NativeMethods.General.General_Genre, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the mood.
    /// </summary>
    /// <value>
    /// The mood.
    /// </value>
    public string Mood => GeneralTags.TryGetValue(NativeMethods.General.General_Mood, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the track ISRC.
    /// </summary>
    /// <value>
    /// The track ISRC.
    /// </value>
    public string Isrc => GeneralTags.TryGetValue(NativeMethods.General.General_ISRC, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the bar code.
    /// </summary>
    /// <value>
    /// The bar code.
    /// </value>
    public string BarCode => GeneralTags.TryGetValue(NativeMethods.General.General_BarCode, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the LCCN.
    /// </summary>
    /// <value>
    /// The LCCN.
    /// </value>
    public string Lccn => GeneralTags.TryGetValue(NativeMethods.General.General_LCCN, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the catalog number.
    /// </summary>
    /// <value>
    /// The catalog number.
    /// </value>
    public string CatalogNumber => GeneralTags.TryGetValue(NativeMethods.General.General_CatalogNumber, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the label code.
    /// </summary>
    /// <value>
    /// The label code.
    /// </value>
    public string LabelCode => GeneralTags.TryGetValue(NativeMethods.General.General_LabelCode, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the person or organization that encoded/ripped the audio file.
    /// </summary>
    /// <value>
    /// The name of the person or organization that encoded/ripped the audio file.
    /// </value>
    public string EncodedBy => GeneralTags.TryGetValue(NativeMethods.General.General_EncodedBy, out var result) ? (string)result : null;
  }
}