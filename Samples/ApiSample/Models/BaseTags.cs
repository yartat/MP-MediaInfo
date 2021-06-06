#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models
{
    /// <summary>
    /// Base class to read tags from stream
    /// </summary>
    [DataContract]
    public abstract class BaseTags
    {
        /// <summary>
        /// The title of the media.
        /// </summary>
        /// <example>The title</example>
        [DataMember(Name = "title")]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// A short description of the contents, such as "Two birds flying".
        /// </summary>
        /// <example>Description</example>
        [DataMember(Name = "description")]
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// The keywords to the item separated by a comma, used for searching.
        /// </summary>
        /// <example>Description</example>
        [DataMember(Name = "keywords")]
        [JsonPropertyName("keywords")]
        public string[] Keywords { get; set; }

        /// <summary>
        /// A country.
        /// </summary>
        /// <example>USA</example>
        [DataMember(Name = "country")]
        [JsonPropertyName("country")]
        public string Country { get; set; }

        /// <summary>
        /// The date that the item was originally released.
        /// </summary>
        /// <example>1968-04-12</example>
        [DataMember(Name = "releasedDate")]
        [JsonPropertyName("releasedDate")]
        public DateTime? ReleasedDate { get; set; }

        /// <summary>
        /// The date and time that the encoding of this item was completed began.
        /// </summary>
        /// <example>2021-05-11 11:22:33</example>
        [DataMember(Name = "encodedDate")]
        [JsonPropertyName("encodedDate")]
        public DateTime? EncodedDate { get; set; }

        /// <summary>
        /// The date and time that the tags were done for this item.
        /// </summary>
        /// <example>2021-05-11 11:22:33</example>
        [DataMember(Name = "taggedDate")]
        [JsonPropertyName("taggedDate")]
        public DateTime? TaggedDate { get; set; }

        /// <summary>
        /// Any comment related to the content.
        /// </summary>
        /// <example>Comments</example>
        [DataMember(Name = "comment")]
        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        /// <summary>
        /// A numeric value defining how much a person likes the song/movie. The number is between 0 and 5 with decimal values possible (e.g. 2.7), 5(.0) being the highest possible rating.
        /// </summary>
        /// <example>8.4</example>
        [DataMember(Name = "rating")]
        [JsonPropertyName("rating")]
        public double? Rating { get; set; }

        /// <summary>
        /// The copyright attribution.
        /// </summary>
        [DataMember(Name = "copyright")]
        [JsonPropertyName("copyright")]
        public string Copyright { get; set; }

        /// <summary>
        /// The name of the organization producing the track (i.e. the 'record label').
        /// </summary>
        /// <example>Sony Music Entertainment</example>
        [DataMember(Name = "publisher")]
        [JsonPropertyName("publisher")]
        public string Publisher { get; set; }

        /// <summary>
        /// The publishers official web page.
        /// </summary>
        /// <example>https://www.sonymusic.com</example>
        [DataMember(Name = "publisherUrl")]
        [JsonPropertyName("publisherUrl")]
        public string PublisherUrl { get; set; }

        /// <summary>
        /// The name of the organization distributing track.
        /// </summary>
        /// <example>Sony Music Entertainment</example>
        [DataMember(Name = "distributedBy")]
        [JsonPropertyName("distributedBy")]
        public string DistributedBy { get; set; }

        /// <summary>
        /// An average number of beats per minute in the complete target.
        /// </summary>
        /// <example>1024</example>
        [DataMember(Name = "bpm")]
        [JsonPropertyName("bpm")]
        public int? Bpm { get; set; }

        /// <summary>
        /// The cover media.
        /// </summary>
        [DataMember(Name = "covers")]
        [JsonPropertyName("covers")]
        public IEnumerable<CoverInfo> Covers { get; set; }
    }

    /// <summary>
    /// Describes properties of the cover tags
    /// </summary>
    [DataContract]
    public class CoverInfo
    {
        /// <summary>
        /// A value indicating whether this <see cref="CoverInfo"/> is exists.
        /// </summary>
        [DataMember(Name = "exist")]
        [JsonPropertyName("exist")]
        public bool Exists { get; set; }

        /// <summary>
        /// A description of the cover.
        /// </summary>
        [DataMember(Name = "description")]
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// A type of the cover.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// MIME of the cover.
        /// </summary>
        [DataMember(Name = "mime")]
        [JsonPropertyName("mime")]
        public string Mime { get; set; }
    }
}