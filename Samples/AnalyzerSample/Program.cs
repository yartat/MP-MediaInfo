#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnalyzerSample;

/// <summary>
/// Shows how to analyze a single media with <see cref="IMediaInfoAnalyzer"/>.
/// </summary>
internal static class Program
{
  private static async Task<int> Main(string[] args)
  {
    if (args.Length < 2)
    {
      Usage();
      return 1;
    }

    var command = args[0].ToLowerInvariant();
    var target = args[1];
    var verbose = args.Contains("--verbose", StringComparer.OrdinalIgnoreCase);

    // Ctrl+C stops the analysis rather than the process, so the pipeline can unwind cleanly.
    using var cancellation = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
      e.Cancel = true;
      Console.WriteLine();
      Console.WriteLine("Stopping…");
      cancellation.Cancel();
    };

    var services = new ServiceCollection();
    if (verbose)
    {
      services.AddLogging(builder => builder.AddSimpleConsole(o => o.SingleLine = true).SetMinimumLevel(LogLevel.Debug));
    }

    // Every concern is opt-in. This composition validates the input, keeps repeat lookups free, gives the
    // analysis a budget and reports subtitle files sitting next to the media.
    services.AddMediaInfoAnalyzer(builder => builder
      .WithValidation()
      .WithCaching(TimeSpan.FromMinutes(5))
      .WithTimeout(TimeSpan.FromMinutes(2))
      .WithExternalSubtitles()
      .UseProgress(new ConsoleProgress()));

    await using var provider = services.BuildServiceProvider();
    var analyzer = provider.GetRequiredService<IMediaInfoAnalyzer>();

    try
    {
      var result = command switch
      {
        "file" or "disc" or "url" => await analyzer.AnalyzeAsync(target, cancellation.Token),
        "strm" => await AnalyzeAsStreamAsync(analyzer, target, cancellation.Token),
        _ => null
      };

      if (result is null)
      {
        Usage();
        return 1;
      }

      Console.WriteLine();
      Report.Write(result);
      return result.Success ? 0 : 2;
    }
    catch (OperationCanceledException)
    {
      Console.WriteLine("Cancelled.");
      return 130;
    }
  }

  private static async Task<MediaAnalysisResult> AnalyzeAsStreamAsync(
    IMediaInfoAnalyzer analyzer,
    string path,
    CancellationToken cancellationToken)
  {
    if (!File.Exists(path))
    {
      return MediaAnalysisResult.Failed(
        new FileMediaSource(path),
        AnalysisFailureReason.SourceNotFound,
        $"The file '{path}' does not exist.");
    }

    // The stream path is the one that is asynchronous end to end and cancellable between blocks.
    await using var stream = File.OpenRead(path);
    return await analyzer.AnalyzeAsync(stream, leaveOpen: true, cancellationToken);
  }

  private static void Usage()
  {
    Console.WriteLine("Analyze one media and describe what it holds.");
    Console.WriteLine();
    Console.WriteLine("  AnalyzerSample file <path>            a single media file");
    Console.WriteLine("  AnalyzerSample strm <path>            the same file, read as a stream, with progress");
    Console.WriteLine("  AnalyzerSample disc <VIDEO_TS|BDMV>   a DVD or Blu-ray folder");
    Console.WriteLine("  AnalyzerSample url  <http-url>        a media served over http");
    Console.WriteLine();
    Console.WriteLine("  --verbose                             log what the pipeline does");
  }

  /// <summary>
  /// Draws a progress bar while a stream is being pumped into the library.
  /// </summary>
  private sealed class ConsoleProgress : IProgress<AnalysisProgress>
  {
    private int _lastPercent = -1;

    public void Report(AnalysisProgress value)
    {
      if (value.Phase == AnalysisPhase.Completed)
      {
        if (_lastPercent >= 0)
        {
          Console.WriteLine();
        }

        return;
      }

      // Only the stream pump reports byte progress; the other phases have nothing to draw.
      if (value.Phase != AnalysisPhase.Parsing || value.Fraction is not { } fraction)
      {
        return;
      }

      var percent = (int)(fraction * 100);
      if (percent == _lastPercent)
      {
        return;
      }

      _lastPercent = percent;
      var filled = percent / 4;
      Console.Write($"\r  [{new string('#', filled)}{new string('·', 25 - filled)}] {percent,3}%");
    }
  }
}
