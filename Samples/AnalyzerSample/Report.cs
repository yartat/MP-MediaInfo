#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Linq;
using MediaInfo.Analysis.Results;

namespace AnalyzerSample;

/// <summary>
/// Prints an analysis outcome to the console.
/// </summary>
internal static class Report
{
  public static void Write(MediaAnalysisResult result)
  {
    if (!result.Success)
    {
      // A failure says why, rather than leaving the caller to guess from an empty result.
      Console.WriteLine($"Could not analyze {result.SourcePath}");
      Console.WriteLine($"  {result.Failure?.Reason}: {result.Failure?.Message}");
      return;
    }

    Console.WriteLine(result.SourcePath);
    if (result.AnalyzedPath is not null && result.AnalyzedPath != result.SourcePath)
    {
      Console.WriteLine($"  read from       {result.AnalyzedPath}");
    }

    Console.WriteLine($"  container       {Describe(result.General.Format, result.General.FormatVersion)}");
    Console.WriteLine($"  duration        {result.General.Duration:hh\\:mm\\:ss\\.fff}");
    Console.WriteLine($"  size            {result.General.Size:N0} bytes");
    Console.WriteLine($"  analyzed in     {result.Elapsed.TotalMilliseconds:N0} ms by {result.LibraryVersion}");

    if (result.HasExternalSubtitles)
    {
      Console.WriteLine("  subtitles       external subtitle files found beside the media");
    }

    WriteDisc(result.Disc);
    WriteStreams(result);
  }

  private static void WriteDisc(DiscStructure? disc)
  {
    if (disc is null)
    {
      return;
    }

    Console.WriteLine();
    Console.WriteLine($"{disc.Kind} structure at {disc.RootPath} — {disc.TotalSize:N0} bytes, {disc.Titles.Count} title(s)");

    foreach (var title in disc.Titles)
    {
      var main = ReferenceEquals(title, disc.MainTitle) ? " <- main" : string.Empty;
      var name = title switch
      {
        DvdTitle dvd => $"VTS_{dvd.TitleSetNumber:00}",
        BluRayPlaylist playlist => playlist.Name,
        _ => $"#{title.Number}"
      };

      Console.WriteLine(
        $"  {name,-10} {Format(title.Duration),12}  {title.Size,15:N0} bytes  {title.Files.Count} file(s){main}");

      if (title is DvdTitle { Chapters.Count: > 0 } withChapters)
      {
        foreach (var chapter in withChapters.Chapters)
        {
          Console.WriteLine($"      chapter {chapter.Number,3}  starts {Format(chapter.Start),12}  runs {Format(chapter.Duration),12}");
        }
      }
    }
  }

  private static void WriteStreams(MediaAnalysisResult result)
  {
    Console.WriteLine();

    foreach (var video in result.VideoStreams)
    {
      var best = ReferenceEquals(video, result.BestVideoStream) ? " <- best" : string.Empty;
      Console.WriteLine(
        $"  video  #{video.StreamNumber}  {video.CodecName}, {video.Width}x{video.Height}, " +
        $"{video.FrameRate:0.###} fps, {video.BitDepth} bit, {video.Resolution}" +
        $"{(video.Hdr != MediaInfo.Model.Hdr.None ? $", {video.Hdr}" : string.Empty)}" +
        $"{(video.Stereoscopic != MediaInfo.Model.StereoMode.Mono ? ", 3D" : string.Empty)}{best}");
    }

    foreach (var audio in result.AudioStreams)
    {
      var best = ReferenceEquals(audio, result.BestAudioStream) ? " <- best" : string.Empty;
      Console.WriteLine(
        $"  audio  #{audio.StreamNumber}  {audio.CodecName}, {audio.AudioChannelsFriendly}, " +
        $"{audio.SamplingRate / 1000:0.#} kHz, {audio.Bitrate / 1000:N0} kbps" +
        $"{(string.IsNullOrEmpty(audio.Language) ? string.Empty : $", {audio.Language}")}{best}");
    }

    foreach (var subtitle in result.Subtitles)
    {
      Console.WriteLine(
        $"  text   #{subtitle.StreamNumber}  {subtitle.Format}" +
        $"{(string.IsNullOrEmpty(subtitle.Language) ? string.Empty : $", {subtitle.Language}")}" +
        $"{(subtitle.Forced ? ", forced" : string.Empty)}");
    }

    if (result.Chapters.Count > 0)
    {
      Console.WriteLine($"  chapters        {result.Chapters.Count}");
    }

    if (result.MenuStreams.Count > 0)
    {
      Console.WriteLine($"  menus           {result.MenuStreams.Count}");
    }
  }

  private static string Describe(string format, string version) =>
    string.IsNullOrEmpty(version) ? format : $"{format} {version}";

  private static string Format(TimeSpan value) =>
    value == TimeSpan.Zero ? "unknown" : value.ToString(@"hh\:mm\:ss");
}
