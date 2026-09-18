#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.CommandLine;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MediaToolkitNet.Abstractions;
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
    var rootCommand = new RootCommand("MKA Test File Generator")
    {
      outputDir,
      seed,
      count,
      ffmpegPath,
      parallelism
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

    Console.WriteLine();
    var previousForegroundColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  Output      : {outputDirValue}");
    Console.WriteLine($"  Seed        : {seedValue}");
    Console.WriteLine($"  Count       : {countValue}");
    Console.WriteLine($"  Parallelism : {parallelismValue}");
    Console.WriteLine();
    Console.ForegroundColor = previousForegroundColor;

    if (!LoadFfmpeg(ffmpegPathValue!, previousForegroundColor))
    {
      return 1;
    }

    await new FileGenerator(outputDirValue!, seedValue, parallelismValue)
      .Generate(countValue);

    return 0;
  }

  #region Helpers

  /// <summary>
  /// Loads FFmpeg and reports which one answered.
  /// </summary>
  /// <param name="directory">Where to look first, or empty for the system search path.</param>
  /// <param name="previousForegroundColor">The console colour to restore.</param>
  /// <returns>Returns <see langword="true"/> when the libraries are usable.</returns>
  private static bool LoadFfmpeg(string directory, ConsoleColor previousForegroundColor)
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

    try
    {
      FFmpegLibraries.EnsureLoaded();

      // The recorder asks each encoder about one sample format after another
      // until one is accepted, and FFmpeg writes out every refusal along the way.
      // This tool reports its own failures, one line per file.
      FFmpegBackend.SetLogLevel(AVConstants.LogFatal);
    }
    catch (MediaToolkitNetException ex)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.WriteLine($"ERROR: {ex.Message}");
      Console.Error.WriteLine($"       {WhereToGetFfmpeg()}");
      Console.ForegroundColor = previousForegroundColor;
      return false;
    }

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
        "libraries as --ffmpeg.";

  #endregion
}
