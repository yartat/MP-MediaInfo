#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.TestFilesGenerator.Models;
using MediaToolkitNet.FFmpeg;
using ToolkitAudioFormat = MediaToolkitNet.Abstractions.Formats.AudioFormat;

namespace MediaInfo.TestFilesGenerator;

/// <summary>
/// Pre-generates all <see cref="AudioParameters"/> with a seeded RNG for
/// reproducibility, then encodes the MKA files in parallel through
/// MediaToolkit.NET.
/// </summary>
/// <param name="outputDir">The directory to write generated files to.</param>
/// <param name="seed">The seed for the random parameter generator.</param>
/// <param name="parallelism">The number of files to encode at once.</param>
internal sealed class FileGenerator(string outputDir, int seed, int parallelism)
{
  private readonly string _outputDir = outputDir;
  private readonly ParameterGenerator _paramGen = new ParameterGenerator(seed);
  private readonly int _parallelism = parallelism;

  private int _succeeded;
  private int _failed;

  public async Task Generate(int count)
  {
    if (!Directory.Exists(_outputDir))
    {
      Directory.CreateDirectory(_outputDir);
    }

    // Step 1 — pre-generate all parameters deterministically (single thread)
    var items = PreGenerate(count);

    // Longest first. The work per file spans four orders of magnitude — a three
    // second mono MP3 against thirty seconds of eight channel 96 kHz PCM — and a
    // worker that picks the largest one up last holds up everybody who finished.
    // The order files are written in does not reach the manifest, which is filled
    // by index.
    items.Sort((left, right) => Cost(right).CompareTo(Cost(left)));

    Console.WriteLine($"Starting parallel generation with {_parallelism} worker(s)...");
    Console.WriteLine();

    // Step 2 — manifest array (pre-allocated, each slot written by a single thread)
    var manifest = new string[count + 1];
    manifest[0] = "Index,Format,Channels,BitDepth,Bitrate,BitrateMode," +
                  "SampleRate,Duration,VbrQuality,FileName,Status";

    var wallClock = Stopwatch.StartNew();

    // Step 3 — encode in parallel and fill manifest
    // Using Parallel.ForEach to control the degree of parallelism and ensure thread-safe updates to counters and manifest
    Parallel.ForEach(
      // One item at a time. The default partitioner hands out growing chunks,
      // which is right when items cost the same and wrong when they do not.
      Partitioner.Create(items, EnumerablePartitionerOptions.NoBuffering),
      new ParallelOptions { MaxDegreeOfParallelism = _parallelism },
      item =>
      {
        var failure = Encode(item.Params, item.FilePath);
        var isOk = failure is null;
        var total = isOk
          ? Interlocked.Increment(ref _succeeded)
          : Interlocked.Increment(ref _failed);

        total = _succeeded + _failed;
        Console.WriteLine($"[{total,4}/{count}] {(isOk ? "OK    " : "FAILED")} {Path.GetFileName(item.FilePath)}");
        if (failure is not null)
        {
          Console.Error.WriteLine($"         {failure}");
        }

        manifest[item.Index + 1] = BuildManifestLine(
          item.Index,
          item.Params,
          Path.GetFileName(item.FilePath),
          isOk ? "OK" : "FAILED");
      });

    wallClock.Stop();
    await WriteManifest(manifest, wallClock.Elapsed);
  }

  private async Task WriteManifest(string[] manifest, TimeSpan elapsed)
  {
    var manifestPath = Path.Combine(_outputDir, "manifest.csv");
    await File.WriteAllLinesAsync(manifestPath, manifest);

    Console.WriteLine();
    Console.WriteLine($"Finished in {elapsed:hh\\:mm\\:ss}");
    Console.WriteLine($"  OK     : {_succeeded}");
    Console.WriteLine($"  FAILED : {_failed}");
    Console.WriteLine($"  Manifest: {manifestPath}");
  }

  // Pre-generation
  private List<GenerationItem> PreGenerate(int count)
  {
    var items = new List<GenerationItem>(count);
    for (var i = 0; i < count; i++)
    {
      var p = _paramGen.GenerateRandom();
      var name = BuildFileName(i, p);
      items.Add(new GenerationItem(i, p, Path.Combine(_outputDir, name)));
    }

    return items;
  }

  /// <summary>
  /// Encodes one file.
  /// </summary>
  /// <param name="p">The parameters to encode with.</param>
  /// <param name="outputPath">The file to write.</param>
  /// <returns>Returns <see langword="null"/> on success, or why it failed.</returns>
  private static string? Encode(AudioParameters p, string outputPath)
  {
    try
    {
      var (format, settings) = EncodingPlan.For(p);

      using (var recorder = new FFmpegRecorder(outputPath))
      {
        var stream = recorder.AddAudioStream(settings);
        recorder.Start();
        WriteSilence(recorder, stream, format, p.DurationSeconds);
        recorder.Stop();
      }

      return File.Exists(outputPath) ? null : "the encoder wrote no file";
    }
    catch (Exception ex)
    {
      Cleanup(outputPath);
      return ex.Message;
    }
  }

  /// <summary>
  /// Pushes silence for the requested number of seconds.
  /// </summary>
  /// <remarks>
  /// Silence keeps the generator fast and is all these files are for: MediaInfo
  /// reads headers, not waveforms. The buffer stays zero throughout, and the
  /// recorder converts it to whatever the encoder takes — including the 0x80 that
  /// silence is in unsigned eight bit.
  /// </remarks>
  private static unsafe void WriteSilence(
    FFmpegRecorder recorder, int stream, ToolkitAudioFormat format, int seconds)
  {
    const int block = 4096;
    var buffer = new byte[block * format.Channels * 2];
    var remaining = (long)format.SampleRate * seconds;
    var written = 0L;

    while (remaining > 0)
    {
      var take = (int)Math.Min(block, remaining);
      fixed (byte* data = buffer)
      {
        recorder.WriteAudio(
          stream, new MediaToolkitNet.Abstractions.Frames.AudioFrame(
            format, format.DurationOf(written), take, data));
      }

      written += take;
      remaining -= take;
    }
  }

  private static void Cleanup(string outputPath)
  {
    try
    {
      // A half written file would be indistinguishable from a good one later.
      if (File.Exists(outputPath))
      {
        File.Delete(outputPath);
      }
    }
    catch (IOException)
    {
    }
  }

  #region Helpers

  private static string BuildFileName(int index, AudioParameters p)
  {
    var mode = p.BitrateMode == BitrateMode.VBR ?
      $"VBR{p.VbrQuality}" :
      "CBR";
    return $"{index:D4}_{p.Format}_{p.Channels}ch_{p.SampleRate}Hz_" +
      $"{p.BitDepth}bit_{p.Bitrate}kbps_{mode}.mka";
  }

  private static string BuildManifestLine(int index, AudioParameters p, string fileName, string status) =>
    $"{index},{p.Format},{p.Channels},{p.BitDepth},{p.Bitrate}," +
    $"{p.BitrateMode},{p.SampleRate},{p.DurationSeconds},{p.VbrQuality}," +
    $"{fileName},{status}";

  /// <summary>
  /// Roughly how much work one file is: every encoder spends its time per sample,
  /// and for the uncompressed ones the file size follows the same number.
  /// </summary>
  private static long Cost(GenerationItem item) =>
    (long)item.Params.SampleRate * item.Params.Channels * item.Params.DurationSeconds;

  private record GenerationItem(int Index, AudioParameters Params, string FilePath);

  #endregion
}
