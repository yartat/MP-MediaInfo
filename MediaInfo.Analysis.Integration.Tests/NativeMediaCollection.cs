#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;
using Xunit;

namespace MediaInfo.Analysis.Integration.Tests;

/// <summary>
/// Serializes every test that touches the native library.
/// </summary>
/// <remarks>
/// A handle of the library is not thread safe and each one holds a noticeable amount of native memory. Running the
/// integration tests one at a time keeps the memory footprint of the test run predictable.
/// </remarks>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class NativeMediaCollection
{
  /// <summary>The name of the collection.</summary>
  public const string Name = "native-media";
}

/// <summary>
/// Locates the media the integration tests run against.
/// </summary>
public static class TestMedia
{
  /// <summary>The environment variable that points at a DVD structure to test against.</summary>
  public const string DvdPathVariable = "MP_MEDIAINFO_DVD_PATH";

  /// <summary>Gets the folder the shared media corpus was copied to.</summary>
  public static string DataDirectory { get; } = System.IO.Path.Combine(AppContext.BaseDirectory, "Data");

  /// <summary>Gets the path of a file of the shared media corpus.</summary>
  public static string Path(string fileName) => System.IO.Path.Combine(DataDirectory, fileName);

  /// <summary>
  /// The video samples folder the wrapper tests already reference, relative to the test output directory.
  /// </summary>
  /// <remarks>
  /// A DVD structure is several gigabytes and cannot be committed, so it lives beside the repository in the shared
  /// sample collection, exactly as the large audio and high definition samples of the wrapper tests do.
  /// </remarks>
  private const string SampleVideoDirectory = "../../../../../MP-MediaInfo.Samples/Video";

  /// <summary>
  /// Gets the DVD structure to test against, or <see langword="null"/> when none is available.
  /// </summary>
  /// <remarks>
  /// Three locations are consulted, in order: a VIDEO_TS folder placed next to the rest of the corpus, the folder
  /// named by <see cref="DvdPathVariable"/>, and any disc in the shared sample collection beside the repository.
  /// </remarks>
  public static string? FindDvd()
  {
    var bundled = System.IO.Path.Combine(DataDirectory, "VIDEO_TS");
    if (IsVideoTs(bundled))
    {
      return bundled;
    }

    var configured = Environment.GetEnvironmentVariable(DvdPathVariable);
    if (!string.IsNullOrWhiteSpace(configured))
    {
      // The variable may name either the disc folder or the VIDEO_TS folder inside it.
      if (IsVideoTs(configured))
      {
        return configured;
      }

      var nested = System.IO.Path.Combine(configured, "VIDEO_TS");
      if (IsVideoTs(nested))
      {
        return nested;
      }
    }

    return FindSampleDvd();
  }

  private static string? FindSampleDvd()
  {
    var videos = System.IO.Path.Combine(AppContext.BaseDirectory, SampleVideoDirectory);
    if (!Directory.Exists(videos))
    {
      return null;
    }

    foreach (var disc in Directory.EnumerateDirectories(videos))
    {
      var videoTs = System.IO.Path.Combine(disc, "VIDEO_TS");
      if (IsVideoTs(videoTs))
      {
        return System.IO.Path.GetFullPath(videoTs);
      }
    }

    return null;
  }

  private static bool IsVideoTs(string path) =>
    Directory.Exists(path) && Directory.GetFiles(path, "*.IFO").Length > 0;
}

/// <summary>
/// A fact that is skipped unless a DVD structure is available to test against.
/// </summary>
public sealed class DvdFactAttribute : FactAttribute
{
  /// <summary>Initializes a new instance of the <see cref="DvdFactAttribute"/> class.</summary>
  public DvdFactAttribute()
  {
    if (TestMedia.FindDvd() is null)
    {
      Skip =
        "No DVD sample available. Put a VIDEO_TS folder in MediaInfo.Wrapper.Tests\\Data, " +
        $"set {TestMedia.DvdPathVariable} to a folder that holds one, " +
        "or place a disc in MP-MediaInfo.Samples\\Video beside the repository.";
    }
  }
}
