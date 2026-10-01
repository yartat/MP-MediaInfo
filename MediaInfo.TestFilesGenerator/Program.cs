#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MediaToolkitNet;
using MediaToolkitNet.Abstractions;
using MediaToolkitNet.Abstractions.Transcoding;
using MediaToolkitNet.FFmpeg;
using MediaToolkitNet.FFmpeg.Native;
using MediaToolkitNet.Interop;

namespace MediaInfo.TestFilesGenerator;

internal static class Program
{
  static async Task<int> Main(string[] args)
  {
    var outputDir = new Option<string>("--output", "-o")
    {
      AllowMultipleArgumentsPerToken = false,
      DefaultValueFactory = (r) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestAudio"),
      Description = "Output folder"
    };
    var seed = new Option<int>("--seed", "-s")
    {
      AllowMultipleArgumentsPerToken = false,
      DefaultValueFactory = (r) => 42,
      Description = "RNG seed"
    };
    var count = new Option<int>("--count", "-c")
    {
      AllowMultipleArgumentsPerToken = false,
      DefaultValueFactory = (r) => 3000,
      Description = "Number of files to generate"
    };
    var ffmpegPath = new Option<string>("--ffmpeg", "-f")
    {
      AllowMultipleArgumentsPerToken = false,
      Description = "Folder holding the FFmpeg shared libraries, e.g. avcodec-63.dll or libavcodec.so.61. " +
                    "Windows x64 gets them from the FFmpeg.GPL package by default; every other system is " +
                    "expected to have FFmpeg installed",
      DefaultValueFactory = (r) => string.Empty
    };
    var parallelism = new Option<int>("--parallelism", "-p")
    {
      AllowMultipleArgumentsPerToken = false,
      DefaultValueFactory = (r) => Environment.ProcessorCount,
      Description = "Number of parallel workers (default: CPU count)"
    };
    var backend = new Option<string>("--backend", "-b")
    {
      AllowMultipleArgumentsPerToken = false,
      DefaultValueFactory = (r) => AutoBackend,
      Description = "Transcoder to use: auto (the first that accepts each file) or the name of one " +
                    "listed at start-up"
    };
    var rootCommand = new RootCommand("MKA Test File Generator")
    {
      outputDir,
      seed,
      count,
      ffmpegPath,
      parallelism,
      backend
    };

    var parseResult = rootCommand.Parse(args);
    await parseResult.InvokeAsync();
    if (parseResult.Errors.Count > 0)
    {
      return -1;
    }

    var outputDirValue = parseResult.GetValue<string>(outputDir);
    var ffmpegPathValue = parseResult.GetValue<string>(ffmpegPath);
    var parallelismValue = parseResult.GetValue<int>(parallelism);
    var seedValue = parseResult.GetValue<int>(seed);
    var countValue = parseResult.GetValue<int>(count);
    var backendValue = parseResult.GetValue<string>(backend)!;

    Console.WriteLine();
    var previousForegroundColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  Output      : {outputDirValue}");
    Console.WriteLine($"  Seed        : {seedValue}");
    Console.WriteLine($"  Count       : {countValue}");
    Console.WriteLine($"  Parallelism : {parallelismValue}");
    Console.WriteLine($"  Backend     : {backendValue}");
    Console.WriteLine();
    Console.ForegroundColor = previousForegroundColor;

    if (!PrepareFfmpeg(ffmpegPathValue!, previousForegroundColor))
    {
      return 1;
    }

    var transcoderFor = ChooseTranscoder(backendValue, previousForegroundColor);
    if (transcoderFor is null)
    {
      return 1;
    }

    await new FileGenerator(outputDirValue!, seedValue, parallelismValue, transcoderFor)
      .Generate(countValue);

    return 0;
  }

  #region Helpers

  private const string AutoBackend = "auto";

  /// <summary>
  /// Picks what runs each request, and reports which transcoders this machine has.
  /// </summary>
  /// <param name="name">The backend asked for, or <c>auto</c>.</param>
  /// <param name="previousForegroundColor">The console colour to restore.</param>
  /// <returns>Returns the choice, or <see langword="null"/> when nothing here can transcode.</returns>
  /// <remarks>
  /// With <c>auto</c> each file goes to the first backend that accepts it, which
  /// on most machines is FFmpeg for every one. Naming a backend sends every file
  /// there, and a file it cannot make is reported as failed, with its reasons.
  /// </remarks>
  private static Func<TranscodeRequest, IMediaTranscoder>? ChooseTranscoder(
    string name, ConsoleColor previousForegroundColor)
  {
    var transcoders = MediaToolkitNetBackends.Registered
      .Where(b => (b.Capabilities & BackendCapabilities.Transcoding) != 0)
      .ToList();

    Console.WriteLine("  Transcoders :");
    foreach (var candidate in transcoders)
    {
      var state = candidate.IsAvailable ? candidate.NativeVersion ?? "available" : "not available";
      Console.WriteLine($"    {candidate.Name,-10} {state}");
    }

    Console.WriteLine();

    if (name == AutoBackend)
    {
      if (transcoders.Any(b => b.IsAvailable))
      {
        return MediaToolkitNetBackends.CreateTranscoder;
      }

      Fail("no backend on this system can transcode.", previousForegroundColor);
      return null;
    }

    var chosen = transcoders.FirstOrDefault(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));
    if (chosen is not { IsAvailable: true })
    {
      Fail($"the {name} backend is not available here.", previousForegroundColor);
      return null;
    }

    return _ => chosen.CreateTranscoder();
  }

  private static void Fail(string message, ConsoleColor previousForegroundColor)
  {
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"ERROR: {message}");
    Console.Error.WriteLine($"       {WhereToGetFfmpeg()}");
    Console.ForegroundColor = previousForegroundColor;
  }

  /// <summary>
  /// Points the loader at a private FFmpeg build when one was named, and reports
  /// which FFmpeg answered.
  /// </summary>
  /// <param name="directory">Where to look first, or empty for the system search path.</param>
  /// <param name="previousForegroundColor">The console colour to restore.</param>
  /// <returns>Returns <see langword="false"/> only when the folder named does not exist.</returns>
  /// <remarks>
  /// FFmpeg is not required: without it GStreamer or mpv can still transcode the
  /// formats they know. Its absence is reported with the transcoders, not here.
  /// </remarks>
  private static bool PrepareFfmpeg(string directory, ConsoleColor previousForegroundColor)
  {
    if (!string.IsNullOrWhiteSpace(directory))
    {
      if (!Directory.Exists(directory))
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"ERROR: {directory} does not exist.");
        Console.ForegroundColor = previousForegroundColor;
        return false;
      }

      NativeSearchPaths.Prepend(directory);

      // A Windows FFmpeg release keeps its DLLs in bin/ beside include/ and lib/,
      // so the folder someone names is as often the parent as it is the one.
      var nested = Path.Combine(directory, "bin");
      if (Directory.Exists(nested))
      {
        NativeSearchPaths.Prepend(nested);
      }
    }

    if (!FFmpegLibraries.IsAvailable)
    {
      return true;
    }

    // Encoders are asked about one sample format after another until one is
    // accepted, and FFmpeg writes out every refusal along the way. This tool
    // reports its own failures, one line per file.
    FFmpegBackend.SetLogLevel(AVConstants.LogFatal);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  FFmpeg      : {FFmpegLibraries.AvCodec.FileName}");
    Console.WriteLine($"  Version     : {FFmpegLibraries.VersionString}");
    Console.WriteLine($"  Series      : {FFmpegLibraries.Generation}");
    Console.WriteLine();
    Console.ForegroundColor = previousForegroundColor;
    return true;
  }

  /// <summary>
  /// Says where the shared libraries are meant to come from on this platform.
  /// </summary>
  /// <returns>Returns the advice to print beside the failure.</returns>
  private static string WhereToGetFfmpeg() =>
    OperatingSystem.IsWindows() && RuntimeInformation.ProcessArchitecture == Architecture.X64
      ? "The FFmpeg.GPL package supplies these on Windows x64, so check that the restore ran, " +
        "or pass the folder holding a private build as --ffmpeg."
      : "Install FFmpeg 7.x, 8.x or 9.x from the system package manager (apt install ffmpeg, " +
        "dnf install ffmpeg, brew install ffmpeg), or pass the folder holding its shared " +
        "libraries as --ffmpeg. GStreamer or mpv can stand in for the formats they encode.";

  #endregion
}
