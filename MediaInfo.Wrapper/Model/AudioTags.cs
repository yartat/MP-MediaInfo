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
    /// Gets the media album name.
    /// </summary>
    /// <value>
    /// The media album name.
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
    /// Gets the media track name.
    /// </summary>
    /// <value>
    /// The media track name.
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
    /// Gets the media sub track name.
    /// </summary>
    /// <value>
    /// The media sub track name.
    /// </value>
    public string SubTrack => Tags.TryGetValue("SubTrack", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the original album name.
    /// </summary>
    /// <value>
    /// The original album name.
    /// </value>
    public string OriginalAlbum => Tags.TryGetValue("Original/Album", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the original track.
    /// </summary>
    /// <value>
    /// The original track.
    /// </value>
    public string OriginalTrack => Tags.TryGetValue("Original/Track", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the track position.
    /// </summary>
    /// <value>
    /// The track position.
    /// </value>
    public int? TrackPosition => Tags.TryGetValue("Track/Position", out var result) ? (int?)result : null;

    /// <summary>
    /// Gets the artist name.
    /// </summary>
    /// <value>
    /// The artist name.
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
    /// Gets the artist URL.
    /// </summary>
    /// <value>
    /// The artist URL.
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