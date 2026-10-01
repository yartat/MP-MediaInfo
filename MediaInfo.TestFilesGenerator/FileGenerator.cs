#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.TestFilesGenerator.Models;
using MediaToolkitNet.Abstractions.Transcoding;

namespace MediaInfo.TestFilesGenerator;

/// <summary>
/// Pre-generates all <see cref="AudioParameters"/> with a seeded RNG for
/// reproducibility, then transcodes the MKA files in parallel through
/// MediaToolkit.NET, from one silent master.
/// </summary>
/// <param name="outputDir">The directory to write generated files to.</param>
/// <param name="seed">The seed for the random parameter generator.</param>
/// <param name="parallelism">The number of files to transcode at once.</param>
/// <param name="transcoderFor">Picks the transcoder that runs a request.</param>
internal sealed class FileGenerator(
  string outputDir,
  int seed,
  int parallelism,
  Func<TranscodeRequest, IMediaTranscoder> transcoderFor)
{
  private readonly string _outputDir = outputDir;
  private readonly ParameterGenerator _paramGen = new ParameterGenerator(seed);
  private readonly int _parallelism = parallelism;
  private readonly Func<TranscodeRequest, IMediaTranscoder> _transcoderFor = transcoderFor;

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

    // Step 2 — the one input every file is cut from
    using var master = SourceMaster.Create(items.Count == 0 ? 1 : items.Max(x => x.Params.DurationSeconds));

    Console.WriteLine($"Starting parallel generation with {_parallelism} worker(s)...");
    Console.WriteLine();

    // Step 3 — manifest array (pre-allocated, each slot written by a single worker)
    var manifest = new string[count + 1];
    manifest[0] = "Index,Format,Channels,BitDepth,Bitrate,BitrateMode," +
                  "SampleRate,Duration,VbrQuality,FileName,Status";

    var wallClock = Stopwatch.StartNew();

    // Step 4 — transcode in parallel and fill the manifest. ForEachAsync takes
    // the items from one shared enumerator, one at a time and in order, which is
    // what keeps the longest-first order above.
    await Parallel.ForEachAsync(
      items,
      new ParallelOptions { MaxDegreeOfParallelism = _parallelism },
      async (item, cancellation) =>
      {
        var failure = await Transcode(item, master.Path, cancellation);
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

    // Step 5 — write manifest
    var manifestPath = Path.Combine(_outputDir, "manifest.csv");
    await File.WriteAllLinesAsync(manifestPath, manifest);

    Console.WriteLine();
    Console.WriteLine($"Finished in {wallClock.Elapsed:hh\\:mm\\:ss}");
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
  /// Transcodes one file.
  /// </summary>
  /// <param name="item">The file to make.</param>
  /// <param name="source">The silent master.</param>
  /// <param name="cancellation">Stops the job.</param>
  /// <returns>Returns <see langword="null"/> on success, or why it failed.</returns>
  /// <remarks>
  /// A request a transcoder cannot carry out is refused before anything is
  /// written, and one that fails part way is deleted by the transcoder, so a
  /// failed file never looks like a good one. A file the transcoder did make, but
  /// not as asked, is deleted here: the manifest states every parameter of every
  /// file, and a file that differs from its line is worse than none.
  /// </remarks>
  private async Task<string?> Transcode(GenerationItem item, string source, CancellationToken cancellation)
  {
    try
    {
      var request = EncodingPlan.For(item.Params, source, item.FilePath);
      var result = await _transcoderFor(request).RunAsync(request, cancellation: cancellation);

      if (result.Warnings.Count > 0)
      {
        Delete(item.FilePath);
        return $"made, but not as asked: {string.Join("; ", result.Warnings.Select(w => w.Message))}";
      }

      return File.Exists(item.FilePath) ? null : "the transcoder wrote no file";
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      return ex.Message;
    }
  }

  private static void Delete(string path)
  {
    try
    {
      File.Delete(path);
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
