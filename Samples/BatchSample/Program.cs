#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis;
using MediaInfo.Analysis.Results;

namespace BatchSample;

/// <summary>
/// Walks a folder of media and writes what it found, as the results arrive.
/// </summary>
/// <remarks>
/// This is where the decorator stack earns its place. A library of thousands of files is bounded by the concurrency
/// limit rather than by how fast the walk produces paths, repeat runs over an unchanged folder are answered from the
/// cache, and a file that cannot be read is reported and stepped over instead of ending the scan.
/// </remarks>
internal static class Program
{
  private static async Task<int> Main(string[] args)
  {
    if (args.Length < 1 || args[0] is "-h" or "--help")
    {
      Usage();
      return 1;
    }

    var folder = args[0];
    var csvPath = ValueOf(args, "--csv");
    var concurrency = int.TryParse(ValueOf(args, "--jobs"), out var jobs) ? jobs : Environment.ProcessorCount;
    var recursive = !args.Contains("--flat", StringComparer.OrdinalIgnoreCase);

    using var cancellation = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
      e.Cancel = true;
      Console.WriteLine();
      Console.WriteLine("Stopping after the analyses already in flight…");
      cancellation.Cancel();
    };

    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .WithValidation()
      .WithCaching(TimeSpan.FromMinutes(30))
      .WithTimeout(TimeSpan.FromMinutes(2))
      .WithConcurrencyLimit(concurrency)
      .Build();

    var scan = new MediaFolderScanOptions { Recursive = recursive };

    Console.WriteLine($"Scanning {folder} with {concurrency} concurrent analyses…");
    Console.WriteLine();

    var csv = csvPath is null ? null : new StreamWriter(csvPath, append: false, Encoding.UTF8);
    csv?.WriteLine("Path,Kind,Success,Failure,Container,Duration,Size,Video,Width,Height,Audio,Channels,Subtitles");

    var clock = Stopwatch.StartNew();
    var total = 0;
    var failed = 0;
    var bytes = 0L;

    try
    {
      await foreach (var result in analyzer.AnalyzeFolderAsync(folder, scan, cancellationToken: cancellation.Token))
      {
        total++;
        if (result.Success)
        {
          bytes += result.General.Size;
        }
        else
        {
          failed++;
        }

        Console.WriteLine(Describe(result));
        csv?.WriteLine(ToCsv(result));
      }
    }
    catch (OperationCanceledException)
    {
      Console.WriteLine("Cancelled.");
    }
    finally
    {
      csv?.Dispose();
      (analyzer as IDisposable)?.Dispose();
    }

    clock.Stop();

    Console.WriteLine();
    Console.WriteLine(
      $"{total} media in {clock.Elapsed:hh\\:mm\\:ss}, {failed} could not be read, {bytes / (1024d * 1024 * 1024):N2} GiB described.");

    if (csvPath is not null)
    {
      Console.WriteLine($"Wrote {csvPath}");
    }

    return failed == 0 ? 0 : 2;
  }

  private static string Describe(MediaAnalysisResult result)
  {
    var name = Path.GetFileName(result.SourcePath?.TrimEnd('/', '\\')) ?? "<unknown>";
    if (!result.Success)
    {
      return $"  ✗ {name,-44} {result.Failure?.Reason}";
    }

    var kind = result.Disc is { } disc ? disc.Kind.ToString().ToUpperInvariant() : "FILE";
    var video = result.BestVideoStream is { } v ? $"{v.CodecName} {v.Width}x{v.Height}" : "no video";
    var audio = result.BestAudioStream is { } a ? $"{a.CodecName} {a.AudioChannelsFriendly}" : "no audio";

    return $"  ✓ {name,-44} {kind,-6} {result.General.Duration:hh\\:mm\\:ss}  {video,-28} {audio}";
  }

  private static string ToCsv(MediaAnalysisResult result)
  {
    var fields = new[]
    {
      result.SourcePath ?? string.Empty,
      result.Disc?.Kind.ToString() ?? "File",
      result.Success.ToString(),
      result.Failure?.Reason.ToString() ?? string.Empty,
      result.General.Format,
      result.General.Duration.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture),
      result.General.Size.ToString(CultureInfo.InvariantCulture),
      result.BestVideoStream?.CodecName ?? string.Empty,
      (result.BestVideoStream?.Width ?? 0).ToString(CultureInfo.InvariantCulture),
      (result.BestVideoStream?.Height ?? 0).ToString(CultureInfo.InvariantCulture),
      result.BestAudioStream?.CodecName ?? string.Empty,
      (result.BestAudioStream?.Channel ?? 0).ToString(CultureInfo.InvariantCulture),
      result.Subtitles.Count.ToString(CultureInfo.InvariantCulture)
    };

    return string.Join(',', fields.Select(Quote));

    static string Quote(string value) =>
      value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
  }

  private static string? ValueOf(string[] args, string name)
  {
    var index = Array.FindIndex(args, x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
  }

  private static void Usage()
  {
    Console.WriteLine("Walk a folder of media and describe everything in it.");
    Console.WriteLine();
    Console.WriteLine("  BatchSample <folder> [--csv <file>] [--jobs <n>] [--flat]");
    Console.WriteLine();
    Console.WriteLine("  --csv <file>   also write the results as CSV");
    Console.WriteLine("  --jobs <n>     how many analyses may run at once (default: processor count)");
    Console.WriteLine("  --flat         do not walk subfolders");
    Console.WriteLine();
    Console.WriteLine("A subfolder holding VIDEO_TS or BDMV is reported as one disc rather than as its files.");
  }
}
