#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace MediaInfo.Analysis.Integration.Tests;

/// <summary>
/// Exercises a fully composed analyzer against the real library and the media corpus of the repository.
/// </summary>
[Collection(NativeMediaCollection.Name)]
public class ComposedAnalyzerTests(ITestOutputHelper output) : IDisposable
{
  private readonly string _workspace = Directory
    .CreateDirectory(Path.Combine(Path.GetTempPath(), "mp-mediainfo-compose-" + Guid.NewGuid().ToString("N")))
    .FullName;

  public void Dispose()
  {
    try
    {
      Directory.Delete(_workspace, recursive: true);
    }
    catch (IOException)
    {
      // A file still held open by the native library must not fail the test run.
    }
  }

  [Fact]
  public async Task ComposedAnalyzer_ReadsRealMediaThroughTheWholeChain()
  {
    using var analyzer = (IDisposable)MediaInfoAnalyzerBuilder.Create()
      .WithValidation()
      .WithCaching()
      .WithTimeout(TimeSpan.FromSeconds(30))
      .WithRetry(2)
      .WithConcurrencyLimit(2)
      .WithExternalSubtitles()
      .Build();

    var result = await ((IMediaInfoAnalyzer)analyzer).AnalyzeAsync(TestMedia.Path("Test_H264_DTS1.m2ts"));

    result.Success.Should().BeTrue(result.Failure?.ToString() ?? string.Empty);
    result.HasVideo.Should().BeTrue();
    result.General.Format.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Caching_MakesTheSecondAnalysisOfTheSameFileFree()
  {
    var analyzer = MediaInfoAnalyzerBuilder.Create().WithCaching().Build();
    var path = TestMedia.Path("RTL_7_Darts_WK_2014-2013-12-23_1_h263.3gp");

    var cold = Stopwatch.StartNew();
    var first = await analyzer.AnalyzeAsync(path);
    cold.Stop();

    var warm = Stopwatch.StartNew();
    var second = await analyzer.AnalyzeAsync(path);
    warm.Stop();

    output.WriteLine($"Cold {cold.ElapsedMilliseconds} ms, warm {warm.ElapsedMilliseconds} ms");

    first.Success.Should().BeTrue();
    second.Should().BeSameAs(first, "the second call is answered from the cache");
  }

  [Fact]
  public async Task Caching_ReadsAgainAfterTheFileIsReplaced()
  {
    var path = Path.Combine(_workspace, "clip.m2ts");
    File.Copy(TestMedia.Path("Test_H264.m2ts"), path);

    var analyzer = MediaInfoAnalyzerBuilder.Create().WithCaching().Build();
    var first = await analyzer.AnalyzeAsync(path);

    File.Copy(TestMedia.Path("Test_H264_DTS1.m2ts"), path, overwrite: true);
    var second = await analyzer.AnalyzeAsync(path);

    first.Success.Should().BeTrue();
    second.Success.Should().BeTrue();
    second.Should().NotBeSameAs(first);
    second.General.Size.Should().Be(new FileInfo(path).Length);
  }

  [Fact]
  public async Task ExternalSubtitles_AreFoundNextToRealMedia()
  {
    var media = Path.Combine(_workspace, "feature.m2ts");
    File.Copy(TestMedia.Path("Test_H264.m2ts"), media);
    await File.WriteAllTextAsync(Path.Combine(_workspace, "feature.srt"), "1\n00:00:00,000 --> 00:00:01,000\nhi\n");

    var analyzer = MediaInfoAnalyzerBuilder.Create().WithExternalSubtitles().Build();
    var withSubtitles = await analyzer.AnalyzeAsync(media);
    var withoutSubtitles = await analyzer.AnalyzeAsync(TestMedia.Path("Test_H264.m2ts"));

    withSubtitles.HasExternalSubtitles.Should().BeTrue();
    withoutSubtitles.HasExternalSubtitles.Should().BeFalse();
  }

  [Fact]
  public async Task ConcurrencyLimit_HoldsWhileAFolderIsScanned()
  {
    using var analyzer = (IDisposable)MediaInfoAnalyzerBuilder.Create()
      .WithConcurrencyLimit(2)
      .Build();

    var files = Directory.GetFiles(TestMedia.DataDirectory, "*.*")
      .Where(x => !x.EndsWith(".srt", StringComparison.OrdinalIgnoreCase))
      .ToArray();

    var results = await Task.WhenAll(files.Select(x => ((IMediaInfoAnalyzer)analyzer).AnalyzeAsync(x)));

    results.Should().HaveCount(files.Length);
    results.Should().OnlyContain(x => x.Success);
  }

  [Fact]
  public async Task Timeout_ReportsAFailureWhenTheBudgetIsTooSmallToPumpAStream()
  {
    var analyzer = MediaInfoAnalyzerBuilder.Create()
      .WithTimeout(TimeSpan.FromMilliseconds(1))
      .Build();

    await using var stream = new SlowStream(File.OpenRead(TestMedia.Path("RTL_7_Darts_WK_2014-2013-12-23_1_h263.3gp")));
    var result = await analyzer.AnalyzeAsync(new StreamMediaSource(stream));

    result.Success.Should().BeFalse();
    result.Failure!.Reason.Should().Be(AnalysisFailureReason.Timeout);
  }

  [Fact]
  public async Task Registered_WithDependencyInjection_ResolvesAndAnalyzes()
  {
    var services = new ServiceCollection();
    services.AddMediaInfoAnalyzer(options => options.BufferSize = 32 * 1024);

    await using var provider = services.BuildServiceProvider();
    var analyzer = provider.GetRequiredService<IMediaInfoAnalyzer>();

    var result = await analyzer.AnalyzeAsync(TestMedia.Path("Test_MP3Tags.mp3"));

    result.Success.Should().BeTrue(result.Failure?.ToString() ?? string.Empty);
    result.HasAudio.Should().BeTrue();
  }

  private sealed class SlowStream(Stream inner) : Stream
  {
    public override bool CanRead => true;

    public override bool CanSeek => inner.CanSeek;

    public override bool CanWrite => false;

    public override long Length => inner.Length;

    public override long Position
    {
      get => inner.Position;
      set => inner.Position = value;
    }

    public override async ValueTask<int> ReadAsync(
      Memory<byte> buffer,
      CancellationToken cancellationToken = default)
    {
      await Task.Delay(50, cancellationToken).ConfigureAwait(false);
      return await inner.ReadAsync(buffer[..Math.Min(buffer.Length, 512)], cancellationToken).ConfigureAwait(false);
    }

    public override int Read(byte[] buffer, int offset, int count) =>
      inner.Read(buffer, offset, Math.Min(count, 512));

    public override void Flush() => inner.Flush();

    public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        inner.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
