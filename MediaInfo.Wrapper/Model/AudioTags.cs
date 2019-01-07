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
using System.Linq;

namespace MediaInfo
{
  /// <summary>
  /// Describes properties of the audio tags
  /// </summary>
  /// <seealso cref="BaseTags" />
  public class AudioTags : BaseTags
  {
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
        var result = Tags.TryGetValue("Album", out var album) ? (string)album : null;
        if (!string.IsNullOrEmpty(result))
        {
          return result;
        }

        result = Tags.TryGetValue("Title", out var title) ? (string)title : null;
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
        var result = Tags.TryGetValue("Track", out var track) ? (string)track : null;
        if (!string.IsNullOrEmpty(result))
        {
          return result;
        }

        result = Tags.TryGetValue("Title", out var title) ? (string) title : null;
        return !string.IsNullOrEmpty(result)
                 ? result.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim()
                 : result;
      }
    }

    /// <summary>
    /// Gets the title of the subtrack.
    /// </summary>
    /// <value>
    /// The title of the subtrack.
    /// </value>
    public string SubTrack => Tags.TryGetValue("SubTrack", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the original album name (in case of a remake/remix).
    /// </summary>
    /// <value>
    /// The original album name (in case of a remake/remix).
    /// </value>
    public string OriginalAlbum => Tags.TryGetValue("Original/Album", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the original track name (in case of a remake/remix).
    /// </summary>
    /// <value>
    /// The original track name (in case of a remake/remix).
    /// </value>
    public string OriginalTrack => Tags.TryGetValue("Original/Track", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the number of the current track.
    /// </summary>
    /// <value>
    /// The number of the current track.
    /// </value>
    public int? TrackPosition => Tags.TryGetValue("Track/Position", out var result) ? (int?)result : null;

    /// <summary>
    /// Gets the number of all tracks.
    /// </summary>
    /// <value>
    /// The number of all tracks.
    /// </value>
    public int? TotalTracks => Tags.TryGetValue("Track/Position_Total", out var result) ? (int?)result : null;

    /// <summary>
    /// Gets the number of the current part in a multi-disc album.
    /// </summary>
    /// <value>
    /// The number of the current part in a multi-disc album.
    /// </value>
    public int? DiscNumber => Tags.TryGetValue("Part/Position", out var result) ? (int?)result : null;

    /// <summary>
    /// Gets the number of all parts in a multi-disc album.
    /// </summary>
    /// <value>
    /// The number of all parts in a multi-disc album.
    /// </value>
    public int? TotalDiscs => Tags.TryGetValue("Part/Position_Total", out var result) ? (int?)result : null;

    /// <summary>
    /// Gets a person or band/collective generally considered responsible for the work : Singer, Implementor.
    /// </summary>
    /// <value>
    /// A person or band/collective generally considered responsible for the work : Singer, Implementor.
    /// </value>
    public string Artist
    {
      get
      {
        var result = Tags.TryGetValue("Performer", out var performer) ? (string)performer : null;
        return string.IsNullOrEmpty(result)
                 ? (Tags.TryGetValue("Artist", out var artist) ? (string)artist : null)
                 : result;
      }
    }

    /// <summary>
    /// Gets the album artist.
    /// </summary>
    /// <value>
    /// The album artist.
    /// </value>
    public string AlbumArtist => Tags.TryGetValue("Album/Performer", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the official artist/performer web page.
    /// </summary>
    /// <value>
    /// The official artist/performer web page.
    /// </value>
    public string ArtistUrl => Tags.TryGetValue("Performer/Url", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the accompaniment name.
    /// </summary>
    /// <value>
    /// The accompaniment name.
    /// </value>
    public string Accompaniment => Tags.TryGetValue("Accompaniment", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the composer name.
    /// </summary>
    /// <value>
    /// The composer name.
    /// </value>
    public string Composer => Tags.TryGetValue("Composer", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the composer nationality.
    /// </summary>
    /// <value>
    /// The composer nationality.
    /// </value>
    public string ComposerNationality => Tags.TryGetValue("Composer/Nationality", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the arranger name.
    /// </summary>
    /// <value>
    /// The arranger name.
    /// </value>
    public string Arranger => Tags.TryGetValue("Arranger", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the lyricist name.
    /// </summary>
    /// <value>
    /// The lyricist name.
    /// </value>
    public string Lyricist => Tags.TryGetValue("Lyricist", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the conductor name.
    /// </summary>
    /// <value>
    /// The conductor name.
    /// </value>
    public string Conductor => Tags.TryGetValue("Conductor", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the sound engineer name.
    /// </summary>
    /// <value>
    /// The sound engineer name.
    /// </value>
    public string SoundEngineer => Tags.TryGetValue("SoundEngineer", out var result) ? (string)result : null;

    /// <summary>
    /// Gets who mastered track.
    /// </summary>
    /// <value>
    /// Who mastered track.
    /// </value>
    public string MasteredBy => Tags.TryGetValue("MasteredBy", out var result) ? (string)result : null;

    /// <summary>
    /// Gets who remixed track.
    /// </summary>
    /// <value>
    /// Who remixed track.
    /// </value>
    public string RemixedBy => Tags.TryGetValue("RemixedBy", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the label name.
    /// </summary>
    /// <value>
    /// The label name.
    /// </value>
    public string Label => Tags.TryGetValue("Label", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the recorded date.
    /// </summary>
    /// <value>
    /// The recorded date.
    /// </value>
    public DateTime? RecordedDate => Tags.TryGetValue("Recorded_Date", out var result) ? (DateTime?)result : null;

    /// <summary>
    /// Gets the genre.
    /// </summary>
    /// <value>
    /// The genre.
    /// </value>
    public string Genre => Tags.TryGetValue("Genre", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the mood.
    /// </summary>
    /// <value>
    /// The mood.
    /// </value>
    public string Mood => Tags.TryGetValue("Mood", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the encoded original.
    /// </summary>
    /// <value>
    /// The encoded original.
    /// </value>
    public string EncodedOriginal => Tags.TryGetValue("Encoded_Original", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the track gain.
    /// </summary>
    /// <value>
    /// The track gain.
    /// </value>
    public string Gain => Tags.TryGetValue("Gain", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the track peak.
    /// </summary>
    /// <value>
    /// The track peak.
    /// </value>
    public string Peak => Tags.TryGetValue("Peak", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the track ISRC.
    /// </summary>
    /// <value>
    /// The track ISRC.
    /// </value>
    public string Isrc => Tags.TryGetValue("ISRC", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the track MSDI.
    /// </summary>
    /// <value>
    /// The track MSDI.
    /// </value>
    public string Msdi => Tags.TryGetValue("MSDI", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the bar code.
    /// </summary>
    /// <value>
    /// The bar code.
    /// </value>
    public string BarCode => Tags.TryGetValue("BarCode", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the LCCN.
    /// </summary>
    /// <value>
    /// The LCCN.
    /// </value>
    public string Lccn => Tags.TryGetValue("LCCN", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the catalog number.
    /// </summary>
    /// <value>
    /// The catalog number.
    /// </value>
    public string CatalogNumber => Tags.TryGetValue("CatalogNumber", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the label code.
    /// </summary>
    /// <value>
    /// The label code.
    /// </value>
    public string LabelCode => Tags.TryGetValue("LabelCode", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the person or organisation that encoded/ripped the audio file.
    /// </summary>
    /// <value>
    /// The name of the person or organisation that encoded/ripped the audio file.
    /// </value>
    public string EncodedBy => Tags.TryGetValue("EncodedBy", out var result) ? (string)result : null;
  }
}