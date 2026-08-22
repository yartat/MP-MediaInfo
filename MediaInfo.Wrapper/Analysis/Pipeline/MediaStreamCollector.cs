#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Strategies.Selection;
using MediaInfo.Builder;
using MediaInfo.Model;

namespace MediaInfo.Analysis.Pipeline;

/// <summary>
/// Reads the streams and container properties of an opened media into a <see cref="MediaAnalysisResult"/>.
/// </summary>
/// <remarks>
/// The collector drives the existing stream builders. It is the only place in the analysis pipeline that knows the
/// parameter names of the library, which keeps the strategies free of parsing concerns.
/// </remarks>
internal sealed class MediaStreamCollector
{
  private const int StereoscopicTag = 1000;
  private const int StereoscopicModeTag = 1001;

  private readonly IStreamSelectionStrategy _selection;

  /// <summary>
  /// Initializes a new instance of the <see cref="MediaStreamCollector"/> class.
  /// </summary>
  /// <param name="selection">The strategy that selects the streams that best represent the media.</param>
  public MediaStreamCollector(IStreamSelectionStrategy selection)
  {
    _selection = selection ?? throw new ArgumentNullException(nameof(selection));
  }

  /// <summary>
  /// Reads the streams and container properties of the opened media.
  /// </summary>
  /// <param name="reader">The opened media.</param>
  /// <param name="knownSize">The size of the media when it is already known, or 0 to read it from the media.</param>
  /// <returns>Returns the collected media information.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <see langword="null"/>.</exception>
  public MediaAnalysisResult Collect(IMediaInfoReader reader, long knownSize = 0L)
  {
    if (reader is null)
    {
      throw new ArgumentNullException(nameof(reader));
    }

    var videoStreams = new List<VideoStream>();
    var audioStreams = new List<AudioStream>();
    var subtitles = new List<SubtitleStream>();
    var chapters = new List<ChapterStream>();
    var menuStreams = new List<MenuStream>();

    var streamNumber = 0;
    streamNumber = CollectVideo(reader, videoStreams, streamNumber);
    streamNumber = CollectAudio(reader, audioStreams, streamNumber);
    streamNumber = CollectSubtitles(reader, subtitles, streamNumber);
    streamNumber = CollectChapters(reader, chapters, streamNumber);
    CollectMenus(reader, menuStreams, streamNumber);

    var tags = new AudioTagBuilder(reader, 0).Build();
    ApplyStereoscopicFallback(videoStreams, tags);

    var size = knownSize > 0
      ? knownSize
      : reader.Get(StreamKind.General, 0, (int)NativeMethods.General.General_FileSize).TryGetLong(out long fileSize)
        ? fileSize
        : 0L;

    var success = videoStreams.Count != 0 || audioStreams.Count != 0 || subtitles.Count != 0;

    return new MediaAnalysisResult
    {
      Success = success,
      Failure = success
        ? null
        : new AnalysisFailure(
          AnalysisFailureReason.NoStreamsFound,
          "The media was opened but contains no video, audio or subtitle stream."),
      General = new GeneralMediaInfo
      {
        Format = reader.Get(StreamKind.General, 0, "Format"),
        FormatVersion = reader.Get(StreamKind.General, 0, "Format_Version"),
        Profile = reader.Get(StreamKind.General, 0, "Format_Profile"),
        Codec = reader.Get(StreamKind.General, 0, "CodecID"),
        WritingApplication = reader.Get(StreamKind.General, 0, (int)NativeMethods.General.General_Encoded_Application),
        WritingLibrary = reader.Get(StreamKind.General, 0, (int)NativeMethods.General.General_Encoded_Library),
        Attachments = reader.Get(StreamKind.General, 0, "Attachments"),
        IsStreamable =
          reader.Get(StreamKind.General, 0, (int)NativeMethods.General.General_IsStreamable).TryGetBool(out var streamable) && streamable,
        Size = size,
        Duration = DurationReader.Read(reader),
        AudioChannelsTotal = ReadAudioChannelsTotal(reader, audioStreams),
        Tags = tags,
        Text = reader.Inform()
      },
      VideoStreams = videoStreams,
      AudioStreams = audioStreams,
      Subtitles = subtitles,
      Chapters = chapters,
      MenuStreams = menuStreams,
      BestVideoStream = _selection.SelectBestVideo(videoStreams),
      BestAudioStream = _selection.SelectBestAudio(audioStreams)
    };
  }

  private static int CollectVideo(IMediaInfoReader reader, ICollection<VideoStream> target, int streamNumber)
  {
    var count = reader.CountGet(StreamKind.Video);
    for (var i = 0; i < count; ++i)
    {
      target.Add(new VideoStreamBuilder(reader, streamNumber++, i).Build());
    }

    return streamNumber;
  }

  private static int CollectAudio(IMediaInfoReader reader, ICollection<AudioStream> target, int streamNumber)
  {
    var count = reader.CountGet(StreamKind.Audio);
    for (var i = 0; i < count; ++i)
    {
      target.Add(new AudioStreamBuilder(reader, streamNumber++, i).Build());
    }

    return streamNumber;
  }

  private static int CollectSubtitles(IMediaInfoReader reader, ICollection<SubtitleStream> target, int streamNumber)
  {
    var count = reader.CountGet(StreamKind.Text);
    for (var i = 0; i < count; ++i)
    {
      target.Add(new SubtitleStreamBuilder(reader, streamNumber++, i).Build());
    }

    return streamNumber;
  }

  private static int CollectChapters(IMediaInfoReader reader, ICollection<ChapterStream> target, int streamNumber)
  {
    var count = reader.CountGet(StreamKind.Other);
    for (var i = 0; i < count; ++i)
    {
      target.Add(new ChapterStreamBuilder(reader, streamNumber++, i).Build());
    }

    return streamNumber;
  }

  private static void CollectMenus(IMediaInfoReader reader, ICollection<MenuStream> target, int streamNumber)
  {
    var count = reader.CountGet(StreamKind.Menu);
    for (var i = 0; i < count; ++i)
    {
      target.Add(new MenuStreamBuilder(reader, streamNumber++, i).Build());
    }
  }

  private static int ReadAudioChannelsTotal(IMediaInfoReader reader, IReadOnlyList<AudioStream> audioStreams)
  {
    if (reader.Get(StreamKind.General, 0, "Audio_Channels_Total").TryGetInt(out int total) && total > 0)
    {
      return total;
    }

    var sum = 0;
    foreach (var stream in audioStreams)
    {
      sum += stream.Channel;
    }

    return sum;
  }

  private static void ApplyStereoscopicFallback(IReadOnlyList<VideoStream> videoStreams, BaseTags tags)
  {
    // Some containers describe the 3D effect at the container level rather than on the video stream itself.
    if (videoStreams.Count != 1 || !tags.GeneralTags.TryGetValue((NativeMethods.General)StereoscopicTag, out var isStereo))
    {
      return;
    }

    videoStreams[0].Stereoscopic = tags.GeneralTags.TryGetValue((NativeMethods.General)StereoscopicModeTag, out var stereoMode)
      ? (StereoMode)stereoMode
      : (bool)isStereo ? StereoMode.Stereo : StereoMode.Mono;
  }
}
