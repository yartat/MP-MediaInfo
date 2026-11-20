#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;

namespace MediaInfo.Analysis.Tests.Fakes;

/// <summary>
/// Builds the paths of the tests for the platform they run on.
/// </summary>
public static class TestPaths
{
  private static readonly string Root = OperatingSystem.IsWindows() ? @"D:\" : "/";

  /// <summary>Gets a rooted path with the separators of the platform.</summary>
  /// <param name="path">The path written with forward slashes, with or without the leading slash.</param>
  /// <returns>Returns the path.</returns>
  public static string P(string path) =>
    Path.Combine(Root, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
}
