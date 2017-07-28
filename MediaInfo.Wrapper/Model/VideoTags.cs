using System;

namespace MediaInfo
{
  /// <summary>
  /// Describes properties of the video tags
  /// </summary>
  /// <seealso cref="BaseTags" />
  public class VideoTags : BaseTags
  {
    /// <summary>
    /// Gets the title of the collection.
    /// </summary>
    /// <value>
    /// The title of the collection.
    /// </value>
    public string Collection => Tags.TryGetValue("Collection", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the title of the season.
    /// </summary>
    /// <value>
    /// The title of the season.
    /// </value>
    public string Season => Tags.TryGetValue("Season", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the title of part.
    /// </summary>
    /// <value>
    /// The title of part.
    /// </value>
    public string Part => Tags.TryGetValue("Part", out var result) ? (string)result : null;

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
        var result = Tags.TryGetValue("Movie", out var movie) ? (string)movie : null;
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
    /// Gets the title of chapter.
    /// </summary>
    /// <value>
    /// The title of chapter.
    /// </value>
    public string Chapter
    {
      get
      {
        var result = Tags.TryGetValue("Chapter", out var chapter) ? (string)chapter : null;
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
        return resultItems.Length > 1 ? resultItems[0].Trim() : null;
      }
    }

    /// <summary>
    /// Gets the original movie.
    /// </summary>
    /// <value>
    /// The original movie.
    /// </value>
    public string OriginalMovie => Tags.TryGetValue("Original/Movie", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the track position.
    /// </summary>
    /// <value>
    /// The track position.
    /// </value>
    public int? TrackPosition => Tags.TryGetValue("Track/Position", out var result) ? (int?)result : null;

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
    /// Gets the real name of actor.
    /// </summary>
    /// <value>
    /// The real name of actor.
    /// </value>
    public string Actor => Tags.TryGetValue("Actor", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the character an actor or actress plays in this movie.
    /// </summary>
    /// <value>
    /// The name of the character an actor or actress plays in this movie.
    /// </value>
    public string ActorCharacter => Tags.TryGetValue("Actor_Character", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the author of the story or script.
    /// </summary>
    /// <value>
    /// The author of the story or script.
    /// </value>
    public string WrittenBy => Tags.TryGetValue("WrittenBy", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the author of the screenplay or scenario (used for movies and TV shows).
    /// </summary>
    /// <value>
    /// The author of the screenplay or scenario (used for movies and TV shows).
    /// </value>
    public string ScreenplayBy => Tags.TryGetValue("ScreenplayBy", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the director.
    /// </summary>
    /// <value>
    /// The name of the director.
    /// </value>
    public string Director => Tags.TryGetValue("Director", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of assistant director.
    /// </summary>
    /// <value>
    /// The name of assistant director.
    /// </value>
    public string AssistantDirector => Tags.TryGetValue("AssistantDirector", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the director of photography, also known as cinematographer.
    /// </summary>
    /// <value>
    /// The name of the director of photography, also known as cinematographer.
    /// </value>
    public string DirectorOfPhotography => Tags.TryGetValue("DirectorOfPhotography", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the person who oversees the artists and craftspeople who build the sets.
    /// </summary>
    /// <value>
    /// The person who oversees the artists and craftspeople who build the sets.
    /// </value>
    public string ArtDirector => Tags.TryGetValue("ArtDirector", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the editor name.
    /// </summary>
    /// <value>
    /// The editor name.
    /// </value>
    public string EditedBy => Tags.TryGetValue("EditedBy", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of producer the movie.
    /// </summary>
    /// <value>
    /// The name of producer the movie.
    /// </value>
    public string Producer => Tags.TryGetValue("Producer", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of co-producer.
    /// </summary>
    /// <value>
    /// The name of co-producer.
    /// </value>
    public string CoProducer => Tags.TryGetValue("CoProducer", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of executive producer.
    /// </summary>
    /// <value>
    /// The name of executive producer.
    /// </value>
    public string ExecutiveProducer => Tags.TryGetValue("ExecutiveProducer", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the artist responsible for designing the overall visual appearance of a movie.
    /// </summary>
    /// <value>
    /// The artist responsible for designing the overall visual appearance of a movie.
    /// </value>
    public string ProductionDesigner => Tags.TryGetValue("ProductionDesigner", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the costume designer.
    /// </summary>
    /// <value>
    /// The name of the costume designer.
    /// </value>
    public string CostumeDesigner => Tags.TryGetValue("CostumeDesigner", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the choreographer.
    /// </summary>
    /// <value>
    /// The name of the choreographer.
    /// </value>
    public string Choreographer => Tags.TryGetValue("Choregrapher", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of production studio.
    /// </summary>
    /// <value>
    /// The name of production studio.
    /// </value>
    public string ProductionStudio => Tags.TryGetValue("ProductionStudio", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the time that the composition of the music/script began.
    /// </summary>
    /// <value>
    /// The time that the composition of the music/script began.
    /// </value>
    public DateTime? WrittenDate => Tags.TryGetValue("Written_Date", out var result) ? (DateTime?)result : null;

    /// <summary>
    /// Gets the main genre of the audio or video; e.g. "classical", "ambient-house", "synthpop", "sci-fi", "drama", etc.
    /// </summary>
    /// <value>
    /// The main genre of the audio or video; e.g. "classical", "ambient-house", "synthpop", "sci-fi", "drama", etc.
    /// </value>
    public string Genre => Tags.TryGetValue("Genre", out var result) ? (string)result : null;

    /// <summary>
    /// Gets intended to reflect the mood of the item with a few keywords, e.g. "Romantic", "Sad", "Uplifting", etc.
    /// </summary>
    /// <value>
    /// Intended to reflect the mood of the item with a few keywords, e.g. "Romantic", "Sad", "Uplifting", etc.
    /// </value>
    public string Mood => Tags.TryGetValue("Mood", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the software package used to create the file, such as "Microsoft WaveEdit."
    /// </summary>
    /// <value>
    /// The name of the software package used to create the file, such as "Microsoft WaveEdit."
    /// </value>
    public string EncodedApplication => Tags.TryGetValue("Encoded_Application", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the software or hardware used to encode this item; e.g. "LAME" or "XviD"
    /// </summary>
    /// <value>
    /// The software or hardware used to encode this item; e.g. "LAME" or "XviD".
    /// </value>
    public string EncodedLibrary => Tags.TryGetValue("Encoded_Library", out var result) ? (string)result : null;

    /// <summary>
    /// Gets a list of the settings used for encoding this item. No specific format.
    /// </summary>
    /// <value>
    /// A list of the settings used for encoding this item. No specific format.
    /// </value>
    public string EncodedLibrarySettings => Tags.TryGetValue("Encoded_Library/Settings", out var result) ? (string)result : null;

    /// <summary>
    /// Gets a plot outline or a summary of the story.
    /// </summary>
    /// <value>
    /// A plot outline or a summary of the story.
    /// </value>
    public string Summary => Tags.TryGetValue("Summary", out var result) ? (string)result : null;
  }
}