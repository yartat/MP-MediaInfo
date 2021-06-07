#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ApiSample.Models
{
    /// <summary>
    /// Describes properties of the video tags
    /// </summary>
    /// <seealso cref="BaseTags" />
    [DataContract]
    public class VideoTags : BaseTags
    {
        /// <summary>
        /// A title of the collection.
        /// </summary>
        /// <example>Title of the collection</example>
        [DataMember(Name = "collection")]
        [JsonPropertyName("collection")]
        public string Collection { get; set; }

        /// <summary>
        /// A title of the season.
        /// </summary>
        /// <example>Title of the season</example>
        [DataMember(Name = "season")]
        [JsonPropertyName("season")]
        public string Season { get; set; }

        /// <summary>
        /// A title of the part.
        /// </summary>
        /// <example>Title of the part</example>
        [DataMember(Name = "part")]
        [JsonPropertyName("part")]
        public string Part { get; set; }

        /// <summary>
        /// A title of the file for a video file.
        /// </summary>
        /// <example>Title of the file</example>
        [DataMember(Name = "movie")]
        [JsonPropertyName("movie")]
        public string Movie { get; set; }

        /// <summary>
        /// A title of chapter.
        /// </summary>
        /// <example>Title of the chapter</example>
        [DataMember(Name = "chapter")]
        [JsonPropertyName("chapter")]
        public string Chapter { get; set; }

        /// <summary>
        /// An original movie.
        /// </summary>
        /// <example>Movie</example>
        [DataMember(Name = "originalMovie")]
        [JsonPropertyName("originalMovie")]
        public string OriginalMovie { get; set; }

        /// <summary>
        /// A track position.
        /// </summary>
        /// <example>1</example>
        [DataMember(Name = "trackPosition")]
        [JsonPropertyName("trackPosition")]
        public int? TrackPosition { get; set; }

        /// <summary>
        /// A composer name.
        /// </summary>
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
        /// A real name of actor.
        /// </summary>
        [DataMember(Name = "actor")]
        [JsonPropertyName("actor")]
        public string Actor { get; set; }

        /// <summary>
        /// A name of the character an actor or actress plays in this movie.
        /// </summary>
        [DataMember(Name = "actorCharacter")]
        [JsonPropertyName("actorCharacter")]
        public string ActorCharacter { get; set; }

        /// <summary>
        /// An author of the story or script.
        /// </summary>
        [DataMember(Name = "writtenBy")]
        [JsonPropertyName("writtenBy")]
        public string WrittenBy { get; set; }

        /// <summary>
        /// an author of the screenplay or scenario (used for movies and TV shows).
        /// </summary>
        [DataMember(Name = "screenplayBy")]
        [JsonPropertyName("screenplayBy")]
        public string ScreenplayBy { get; set; }

        /// <summary>
        /// A name of the director.
        /// </summary>
        [DataMember(Name = "director")]
        [JsonPropertyName("director")]
        public string Director { get; set; }

        /// <summary>
        /// A name of assistant director.
        /// </summary>
        [DataMember(Name = "assistantDirector")]
        [JsonPropertyName("assistantDirector")]
        public string AssistantDirector { get; set; }

        /// <summary>
        /// A name of the director of photography, also known as cinematographer.
        /// </summary>
        [DataMember(Name = "directorOfPhotography")]
        [JsonPropertyName("directorOfPhotography")]
        public string DirectorOfPhotography { get; set; }

        /// <summary>
        /// A person who oversees the artists and craftspeople who build the sets.
        /// </summary>
        [DataMember(Name = "artDirector")]
        [JsonPropertyName("artDirector")]
        public string ArtDirector { get; set; }

        /// <summary>
        /// An editor name.
        /// </summary>
        [DataMember(Name = "editedBy")]
        [JsonPropertyName("editedBy")]
        public string EditedBy { get; set; }

        /// <summary>
        /// A name of producer the movie.
        /// </summary>
        [DataMember(Name = "producer")]
        [JsonPropertyName("producer")]
        public string Producer { get; set; }

        /// <summary>
        /// A name of co-producer.
        /// </summary>
        [DataMember(Name = "coProducer")]
        [JsonPropertyName("coProducer")]
        public string CoProducer { get; set; }

        /// <summary>
        /// A name of executive producer.
        /// </summary>
        [DataMember(Name = "executiveProducer")]
        [JsonPropertyName("executiveProducer")]
        public string ExecutiveProducer { get; set; }

        /// <summary>
        /// An artist responsible for designing the overall visual appearance of a movie.
        /// </summary>
        [DataMember(Name = "productionDesigner")]
        [JsonPropertyName("productionDesigner")]
        public string ProductionDesigner { get; set; }

        /// <summary>
        /// A name of the costume designer.
        /// </summary>
        /// <value>
        /// The name of the costume designer.
        /// </value>
        [DataMember(Name = "costumeDesigner")]
        [JsonPropertyName("costumeDesigner")]
        public string CostumeDesigner { get; set; }

        /// <summary>
        /// A name of the choreographer.
        /// </summary>
        [DataMember(Name = "choreographer")]
        [JsonPropertyName("choreographer")]
        public string Choreographer { get; set; }

        /// <summary>
        /// A name of production studio.
        /// </summary>
        [DataMember(Name = "productionStudio")]
        [JsonPropertyName("productionStudio")]
        public string ProductionStudio { get; set; }

        /// <summary>
        /// A date and time that the composition of the music/script began.
        /// </summary>
        /// <example>1992-03-06</example>
        [DataMember(Name = "writtenDate")]
        [JsonPropertyName("writtenDate")]
        public DateTime? WrittenDate { get; set; }

        /// <summary>
        /// A main genre of the audio or video; e.g. "classical", "ambient-house", "synthpop", "sci-fi", "drama", etc.
        /// </summary>
        /// <example>sci-fi</example>
        [DataMember(Name = "genre")]
        [JsonPropertyName("genre")]
        public string Genre { get; set; }

        /// <summary>
        /// Gets intended to reflect the mood of the item with a few keywords, e.g. "Romantic", "Sad", "Uplifting", etc.
        /// </summary>
        /// <example>romantic</example>
        [DataMember(Name = "mood")]
        [JsonPropertyName("mood")]
        public string Mood { get; set; }

        /// <summary>
        /// A name of the software package used to create the file, such as "Microsoft WaveEdit."
        /// </summary>
        [DataMember(Name = "encodedApplication")]
        [JsonPropertyName("encodedApplication")]
        public string EncodedApplication { get; set; }

        /// <summary>
        /// Software or hardware used to encode this item; e.g. "LAME" or "XviD"
        /// </summary>
        [DataMember(Name = "encodedLibrary")]
        [JsonPropertyName("encodedLibrary")]
        public string EncodedLibrary { get; set; }

        /// <summary>
        /// A list of the settings used for encoding this item. No specific format.
        /// </summary>
        [DataMember(Name = "encodedLibrarySettings")]
        [JsonPropertyName("encodedLibrarySettings")]
        public string EncodedLibrarySettings { get; set; }

        /// <summary>
        /// A plot outline or a summary of the story.
        /// </summary>
        [DataMember(Name = "summary")]
        [JsonPropertyName("summary")]
        public string Summary { get; set; }
    }
}