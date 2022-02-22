#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models
{
    /// <summary>
    /// Describes properties of the audio tags
    /// </summary>
    /// <seealso cref="BaseTags" />
    [DataContract]
    public class AudioTags : BaseTags
    {
        /// <summary>
        /// Gets the title of the album.
        /// </summary>
        /// <example>The title of the album</example>
        [DataMember(Name = "album")]
        [JsonPropertyName("album")]
        public string Album { get; set; }

        /// <summary>
        /// The title of the track.
        /// </summary>
        /// <example>The title of the track</example>
        [DataMember(Name = "track")]
        [JsonPropertyName("track")]
        public string Track { get; set; }

        /// <summary>
        /// The title of the subtrack.
        /// </summary>
        /// <example>The title of the subtrack</example>
        [DataMember(Name = "subTrack")]
        [JsonPropertyName("subTrack")]
        public string SubTrack { get; set; }

        /// <summary>
        /// The original album name (in case of a remake/remix).
        /// </summary>
        /// <example>The title</example>
        [DataMember(Name = "originalAlbum")]
        [JsonPropertyName("originalAlbum")]
        public string OriginalAlbum { get; set; }

        /// <summary>
        /// Gets the original track name (in case of a remake/remix).
        /// </summary>
        /// <example>The title of the track</example>
        [DataMember(Name = "originalTrack")]
        [JsonPropertyName("originalTrack")]
        public string OriginalTrack { get; set; }

        /// <summary>
        /// The number of the current track.
        /// </summary>
        /// <example>1</example>
        [DataMember(Name = "trackPosition")]
        [JsonPropertyName("trackPosition")]
        public int? TrackPosition { get; set; }

        /// <summary>
        /// The number of all tracks.
        /// </summary>
        /// <example>16</example>
        [DataMember(Name = "totalTracks")]
        [JsonPropertyName("totalTracks")]
        public int? TotalTracks { get; set; }

        /// <summary>
        /// The number of the current part in a multi-disc album.
        /// </summary>
        /// <example>272644</example>
        [DataMember(Name = "discNumber")]
        [JsonPropertyName("discNumber")]
        public int? DiscNumber { get; set; }

        /// <summary>
        /// The number of all parts in a multi-disc album.
        /// </summary>
        /// <example>1</example>
        [DataMember(Name = "totalDiscs")]
        [JsonPropertyName("totalDiscs")]
        public int? TotalDiscs { get; set; }

        /// <summary>
        /// A person or band/collective generally considered responsible for the work : Singer, Implementor.
        /// </summary>
        /// <example>Bob Dylan</example>
        [DataMember(Name = "artist")]
        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        /// <summary>
        /// An album artist.
        /// </summary>
        /// <example>Bob Dylan</example>
        [DataMember(Name = "albumArtist")]
        [JsonPropertyName("albumArtist")]
        public string AlbumArtist { get; set; }

        /// <summary>
        /// An official artist/performer web page.
        /// </summary>
        /// <example>http://www.bobdylan.com</example>
        [DataMember(Name = "artistUrl")]
        [JsonPropertyName("artistUrl")]
        public string ArtistUrl { get; set; }

        /// <summary>
        /// A accompaniment name.
        /// </summary>
        [DataMember(Name = "accompaniment")]
        [JsonPropertyName("accompaniment")]
        public string Accompaniment { get; set; }

        /// <summary>
        /// A composer name.
        /// </summary>
        /// <example>Bob Dylan</example>
        [DataMember(Name = "composer")]
        [JsonPropertyName("composer")]
        public string Composer { get; set; }

        /// <summary>
        /// A composer nationality.
        /// </summary>
        /// <example>USA</example>
        [DataMember(Name = "composerNationality")]
        [JsonPropertyName("composerNationality")]
        public string ComposerNationality { get; set; }

        /// <summary>
        /// An arranger name.
        /// </summary>
        [DataMember(Name = "arranger")]
        [JsonPropertyName("arranger")]
        public string Arranger { get; set; }

        /// <summary>
        /// A lyricist name.
        /// </summary>
        /// <example>Bob Dylan</example>
        [DataMember(Name = "lyricist")]
        [JsonPropertyName("lyricist")]
        public string Lyricist { get; set; }

        /// <summary>
        /// A conductor name.
        /// </summary>
        [DataMember(Name = "conductor")]
        [JsonPropertyName("conductor")]
        public string Conductor { get; set; }

        /// <summary>
        /// A sound engineer name.
        /// </summary>
        [DataMember(Name = "soundEngineer")]
        [JsonPropertyName("soundEngineer")]
        public string SoundEngineer { get; set; }

        /// <summary>
        /// Who mastered track.
        /// </summary>
        [DataMember(Name = "masteredBy")]
        [JsonPropertyName("masteredBy")]
        public string MasteredBy { get; set; }

        /// <summary>
        /// Who remixed track.
        /// </summary>
        [DataMember(Name = "remixedBy")]
        [JsonPropertyName("remixedBy")]
        public string RemixedBy { get; set; }

        /// <summary>
        /// A label name.
        /// </summary>
        [DataMember(Name = "label")]
        [JsonPropertyName("label")]
        public string Label { get; set; }

        /// <summary>
        /// The recorded date.
        /// </summary>
        [DataMember(Name = "recordedDate")]
        [JsonPropertyName("recordedDate")]
        [JsonConverter(typeof(JsonMicrosoftDateTimeConverter))]
        public DateTime? RecordedDate { get; set; }

        /// <summary>
        /// The genre.
        /// </summary>
        /// <example>rock</example>
        [DataMember(Name = "genre")]
        [JsonPropertyName("genre")]
        public string Genre { get; set; }

        /// <summary>
        /// The mood.
        /// </summary>
        [DataMember(Name = "mood")]
        [JsonPropertyName("mood")]
        public string Mood { get; set; }

        /// <summary>
        /// The track ISRC.
        /// </summary>
        [DataMember(Name = "isrc")]
        [JsonPropertyName("isrc")]
        public string Isrc { get; set; }

        /// <summary>
        /// The bar code.
        /// </summary>
        [DataMember(Name = "barCode")]
        [JsonPropertyName("barCode")]
        public string BarCode { get; set; }

        /// <summary>
        /// The LCCN.
        /// </summary>
        [DataMember(Name = "lccn")]
        [JsonPropertyName("lccn")]
        public string Lccn { get; set; }

        /// <summary>
        /// The catalog number.
        /// </summary>
        [DataMember(Name = "catalogNumber")]
        [JsonPropertyName("catalogNumber")]
        public string CatalogNumber { get; set; }

        /// <summary>
        /// The label code.
        /// </summary>
        [DataMember(Name = "labelCode")]
        [JsonPropertyName("labelCode")]
        public string LabelCode { get; set; }

        /// <summary>
        /// The name of the person or organization that encoded/ripped the audio file.
        /// </summary>
        [DataMember(Name = "encodedBy")]
        [JsonPropertyName("encodedBy")]
        public string EncodedBy { get; set; }
    }
}