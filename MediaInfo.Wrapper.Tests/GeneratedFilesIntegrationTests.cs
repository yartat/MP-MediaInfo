#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FluentAssertions;
#if NET5_0_OR_GREATER
using Microsoft.Extensions.Logging;
#endif
using Xunit;
using Xunit.Abstractions;
using CsvHelper;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;

namespace MediaInfo.Wrapper.Tests
{
  /// <summary>
  /// Integration tests that open every audio file produced by <c>MediaInfo.TestFilesGenerator</c>
  /// and verify both correctness (<see cref="MediaInfoWrapper.Success"/>) and the absence of
  /// managed/native resource leaks. This test is created to validate possible memory leaks reported in 
  /// <see href="https://github.com/yartat/MP-MediaInfo/issues/52">Memory Leak in MediaInfoWrapper Usage</see>.
  /// </summary>
  /// <remarks>
  /// Prerequisites:
  /// <list type="number">
  ///   <item>Run <c>MediaInfo.TestFilesGenerator</c> to populate <see cref="TestAudioDir"/>.</item>
  ///   <item>Execute the tests in DEBUG configuration (or remove the Skip attribute).</item>
  /// </list>
  /// </remarks>
  /// <param name="testOutputHelper">The test output helper.</param>
  [Collection(MemoryMeasurementCollection.Name)]
  public class GeneratedFilesIntegrationTests(ITestOutputHelper testOutputHelper)
    {
    /// <summary>Directory that <c>FileGenerator</c> writes files to (relative to test output).</summary>
    private const string TestAudioDir = "./TestAudio";
    private const string ManifestFileName = "manifest.csv";

    /// <summary>
    /// Maximum allowed growth of managed heap memory after processing all files.
    /// A larger delta suggests that <see cref="MediaInfoWrapper"/> retains objects across calls.
    /// </summary>
    private const long MaxManagedGrowthBytes = 20L * 1024 * 1024; // 20 MB

    /// <summary>
    /// Maximum allowed growth of the process private working set after processing all files.
    /// A larger delta suggests a native (unmanaged) resource leak inside MediaInfo.dll.
    /// </summary>
    private const long MaxPrivateGrowthBytes = 50L * 1024 * 1024; // 50 MB

    private readonly ILogger _logger = new TestLogger(testOutputHelper);
    private readonly ITestOutputHelper _output = testOutputHelper;

#if DEBUG
    [Fact]
#else
    [Fact(Skip = "Integration test — run only in development with generated test files")]
#endif
    public async Task OpenAllGeneratedFiles_ShouldSucceedWithoutResourceLeaks()
    {
      // Arrange
      var manifestPath = Path.Combine(TestAudioDir, ManifestFileName);
      File.Exists(manifestPath).Should().BeTrue($"Manifest not found at '{Path.GetFullPath(manifestPath)}'.\nRun MediaInfo.TestFilesGenerator first to generate the test files.");

      var files = await ReadOkFilesFromManifest(manifestPath);
      _output.WriteLine($"Files in manifest (status=OK) : {files.Count}");
      files.Should().NotBeEmpty("manifest must contain at least one successfully generated file");

      // Establish memory baselines after a full GC so that JIT warm-up costs
      // and previously allocated objects are already collected.
      ForceFullGc();
      var baselineManagedBytes  = GC.GetTotalMemory(false);
      var baselinePrivateBytes  = Process.GetCurrentProcess().PrivateMemorySize64;

      _output.WriteLine($"Baseline managed memory  : {FormatBytes(baselineManagedBytes)}");
      _output.WriteLine($"Baseline private memory  : {FormatBytes(baselinePrivateBytes)}");

      var succeeded = 0;
      var failed    = new List<string>();

      // Act
      foreach (var (filePath, index) in files)
      {
        var wrapper = new MediaInfoWrapper(filePath, _logger);

        if (wrapper.Success)
          succeeded++;
        else
          failed.Add($"[{index:D4}] {Path.GetFileName(filePath)}");
      }

      // Measure
      // Force a full GC + finalizers so that temporary objects from the loop
      // are reclaimed before we snapshot the final memory usage.
      ForceFullGc();
      var finalManagedBytes  = GC.GetTotalMemory(false);
      var finalPrivateBytes  = Process.GetCurrentProcess().PrivateMemorySize64;

      var managedDelta = finalManagedBytes - baselineManagedBytes;
      var privateDelta = finalPrivateBytes - baselinePrivateBytes;

      // Report
      _output.WriteLine(string.Empty);
      _output.WriteLine($"Processed         : {files.Count}");
      _output.WriteLine($"Succeeded         : {succeeded}");
      _output.WriteLine($"Failed            : {failed.Count}");
      _output.WriteLine(string.Empty);
      _output.WriteLine($"Final managed     : {FormatBytes(finalManagedBytes)}  (delta {FormatBytes(managedDelta)})");
      _output.WriteLine($"Final private     : {FormatBytes(finalPrivateBytes)}  (delta {FormatBytes(privateDelta)})");

      if (failed.Count > 0)
      {
        _output.WriteLine(string.Empty);
        _output.WriteLine("Failed files:");
        foreach (var f in failed)
          _output.WriteLine("  " + f);
      }

      // Assert
      failed.Should().BeEmpty(
        "all generated files must be opened successfully by MediaInfoWrapper");

      managedDelta.Should().BeLessThan(
        MaxManagedGrowthBytes,
        $"managed heap grew by {FormatBytes(managedDelta)} after processing {files.Count} files — " +
        $"expected less than {FormatBytes(MaxManagedGrowthBytes)} (possible managed resource leak)");

      privateDelta.Should().BeLessThan(
        MaxPrivateGrowthBytes,
        $"process private memory grew by {FormatBytes(privateDelta)} after processing {files.Count} files — " +
        $"expected less than {FormatBytes(MaxPrivateGrowthBytes)} (possible native resource leak in MediaInfo.dll)");
    }

    #region Helper Methods

    /// <summary>
    /// Reads <c>manifest.csv</c> produced by <c>FileGenerator</c> and returns the absolute
    /// paths of every file whose generation status was <c>OK</c>.
    /// </summary>
    /// <remarks>
    /// Manifest columns (0-based index):
    /// <c>Index, Format, Channels, BitDepth, Bitrate, BitrateMode,
    /// SampleRate, Duration, VbrQuality, FileName, Status</c>
    /// </remarks>
    private static async Task<List<(string FilePath, int Index)>> ReadOkFilesFromManifest(string manifestPath)
    {
      var dir = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
      var result = new List<(string, int)>();
      using var stream = new StreamReader(manifestPath, Encoding.UTF8);
      using var reader = new CsvReader(stream, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture));

      var fillRecord = new CsvRecord(0, string.Empty, 0, 0, 0, string.Empty, 0, 0, 0, string.Empty, string.Empty);
      await foreach (var record in reader.EnumerateRecordsAsync(fillRecord))
      {
        if (record.Status != "OK")
          continue;

        result.Add((Path.Combine(dir, record.FileName.Trim()), record.Index));
      }

      return result;
    }

    /// <summary>Performs a blocking full GC including finalizer execution.</summary>
    private static void ForceFullGc()
    {
      GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
      GC.WaitForPendingFinalizers();
      GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
    }

    private static string FormatBytes(long bytes)
    {
      if (bytes < 0)
        return $"-{FormatBytes(-bytes)}";
      if (bytes < 1024L)
        return $"{bytes} B";
      if (bytes < 1024L * 1024L)
        return $"{bytes / 1024.0:F1} KB";
      return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    private record CsvRecord(int Index, string Format, int Channels, int BitDepth, double Bitrate, string BitrateMode,
        double SampleRate, int Duration, int VbrQuality, string FileName, string Status);

    #endregion
  }

  /// <summary>
  /// Runs the tests that measure the process memory alone. Other test classes run in parallel in the same
  /// process, and whatever they allocate would be counted as growth of the measured one.
  /// </summary>
  [CollectionDefinition(Name, DisableParallelization = true)]
  public sealed class MemoryMeasurementCollection
  {
    /// <summary>The name of the collection.</summary>
    public const string Name = "memory-measurement";
  }
}
