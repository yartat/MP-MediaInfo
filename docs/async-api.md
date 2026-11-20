# Asynchronous analysis

`IMediaInfoAnalyzer` analyzes a media without blocking, can be cancelled, and returns an immutable result.
It is in `MediaInfo.Wrapper.Core` (.NET Standard 2.1, .NET 8.0, .NET 10.0).

```shell
dotnet add package MediaInfo.Wrapper.Core
```

```csharp
using MediaInfo.Analysis;
using MediaInfo.Analysis.Results;
```

## Analyze a media

```csharp
IMediaInfoAnalyzer analyzer = MediaInfoAnalyzer.CreateDefault();
var result = await analyzer.AnalyzeAsync("path/to/film.mkv", cancellationToken);

if (result.Success)
{
    Console.WriteLine($"{result.General.Format}, {result.General.Duration}");
    Console.WriteLine($"{result.BestVideoStream?.CodecName} {result.BestVideoStream?.Width}x{result.BestVideoStream?.Height}");
}
else
{
    Console.WriteLine($"{result.Failure!.Reason}: {result.Failure.Message}");
}
```

The source is a file path, an `http`/`https` URL, a disc folder or a `Stream`. Analyzing a file cannot be
interrupted once the native library has started opening it: a timeout or a cancellation releases the caller, and the
native call goes on until the library returns. `MediaAnalysisOptions.MaximumAbandonedNativeCalls` (16) bounds
this: with that many such calls outstanding, a new analysis fails at once with `AnalysisFailureReason.Busy`.

`http` and `https` locations are read by the analyzer with `HttpClient`, so they do not have this problem; see
[HTTP](#http).

## Result

| Member | Content |
| --- | --- |
| `Success`, `Failure` | `Failure.Reason` is an `AnalysisFailureReason`; `Failure.Message` says why |
| `General` | `Format`, `Duration`, `Size`, `Tags`, `Text` (full report) |
| `VideoStreams`, `AudioStreams`, `Subtitles`, `Chapters`, `MenuStreams` | All streams of the media |
| `BestVideoStream`, `BestAudioStream` | The stream that represents the media |
| `Disc` | `DvdStructure` or `BluRayStructure`, `null` for anything else |
| `SourcePath`, `SourceKind`, `Elapsed`, `LibraryVersion` | Where it came from and how long it took |

## Stream

```csharp
await using var stream = File.OpenRead("path/to/film.mkv");
var result = await analyzer.AnalyzeAsync(stream, leaveOpen: true, cancellationToken);
```

`MediaAnalysisOptions.MaximumStreamBytes` limits how much is read. A stream that cannot seek has no limit by default; a
stream that can is read up to four times its length plus 32 MiB. When the limit is reached the result holds what the
library had, with a warning.

The stream is read in 64 KB blocks and cancellation is observed between blocks.

## DVD and Blu-ray

```csharp
var result = await analyzer.AnalyzeAsync(@"D:\Movies\SomeFilm");

if (result.Disc is DvdStructure dvd)
{
    foreach (var title in dvd.DvdTitles)
    {
        Console.WriteLine($"VTS_{title.TitleSetNumber:00}: {title.Duration}, {title.Chapters.Count} chapters");
    }
}
```

The disc root, the `VIDEO_TS` or `BDMV` folder, and any `.IFO` inside it resolve to the same disc. `result.Disc.Titles`
lists every title and `result.Disc.MainTitle` the likely feature. The stream properties of `result` describe the main
title.

## Folder

```csharp
await foreach (var item in analyzer.AnalyzeFolderAsync(@"D:\Movies", cancellationToken: token))
{
    Console.WriteLine($"{item.SourcePath}: {(item.Success ? item.General.Duration : item.Failure!.Message)}");
}
```

Results arrive as they are ready. A subfolder holding `VIDEO_TS` or `BDMV` is one disc. A file that cannot be read is
returned as a failure. Subfolders that are symbolic links or junctions are skipped. `MediaFolderScanOptions` sets
`Recursive`, `SearchPattern` and `MediaFilesOnly`.

## Behaviour

Each option is independent and optional:

```csharp
var analyzer = MediaInfoAnalyzerBuilder.Create()
    .WithValidation()
    .WithCaching(TimeSpan.FromMinutes(10))
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithRetry(attempts: 3)
    .WithConcurrencyLimit(Environment.ProcessorCount)
    .WithExternalSubtitles()
    .Build();
```

| Option | Effect |
| --- | --- |
| `WithValidation` | Rejects a missing path or an unreadable stream before anything is opened |
| `WithCaching` | Reuses the result for an unchanged file or disc; streams and URLs are never cached |
| `WithTimeout` | Total time budget, including the wait for a free slot; it releases the caller, not the native call |
| `WithRetry` | Repeats only failures that may not persist |
| `WithConcurrencyLimit` | Limits how many native handles are open at once |
| `WithExternalSubtitles` | Reports subtitle files next to the media (`HasExternalSubtitles`); worked out on every call, also when the rest of the answer comes from the cache |

## Dependency injection

```csharp
services.AddMediaInfoAnalyzer(builder => builder
    .WithCaching()
    .WithConcurrencyLimit(4));
```

`IMediaInfoAnalyzer` is registered as a singleton and uses the container's logger.

## HTTP

`http` and `https` locations are read with `HttpClient`, and the library is given the bytes. A server that serves
`Range` requests is read in blocks, so the library can ask for the end of a file first; any other server is read from
the start as far as the library needs (`Size` is 0 then).

```csharp
var analyzer = MediaInfoAnalyzerBuilder.Create()
    .WithHttp(http =>
    {
        http.RequestTimeout = TimeSpan.FromSeconds(10);
        http.AllowInvalidCertificates = true; // a server of your own with a self-signed certificate
    })
    .Build();

var result = await analyzer.AnalyzeAsync("https://localhost:8443/videos/test.mp4", cancellationToken);
```

| `HttpAnalysisOptions` | Default | |
| --- | --- | --- |
| `Enabled` | `true` | `false` hands the location to the native library, which has no timeouts |
| `RequestTimeout` | 15 s | Connect and response headers |
| `ReceiveTimeout` | 15 s | Silence while a body is received |
| `BlockSize` | 512 KB | Size of one `Range` request |
| `MaximumRedirects` | 3 | Redirections followed |
| `AllowInvalidCertificates` | `false` | Accepts any server certificate |
| `Credentials` | none | Basic or Digest; the URL may carry them instead. Never sent to another host after a redirection |
| `UserAgent` | `MP-MediaInfo` | |

A timeout fails with `AnalysisFailureReason.Timeout`, a missing media with `SourceNotFound`, an empty one with
`SourceEmpty`. A media that changes while it is read fails instead of being mixed. The legacy `MediaInfoWrapper`
still hands URLs to the native library.

## RTSP

Add `MediaInfo.Analysis.Rtsp` to analyze `rtsp://` sources.

```shell
dotnet add package MediaInfo.Analysis.Rtsp
```

```csharp
using MediaInfo.Analysis.Rtsp;

var analyzer = MediaInfoAnalyzerBuilder.Create()
    .WithRtsp(rtsp => rtsp.CaptureDuration = TimeSpan.FromSeconds(3))
    .Build();

var result = await analyzer.AnalyzeAsync("rtsp://user:password@camera.local/stream1", cancellationToken);
Console.WriteLine($"{result.BestVideoStream?.CodecName} {result.BestVideoStream?.Width}x{result.BestVideoStream?.Height}");
```

| `RtspAnalysisOptions` | Default | |
| --- | --- | --- |
| `CaptureDuration` | 3 s | How long the video is played |
| `MaximumCaptureBytes` | 8 MB | Capture size limit |
| `SufficientFrameCount` | 60 | Stops early after this many frames |
| `ConnectTimeout`, `ReceiveTimeout` | 5 s | Network timeouts |
| `Credentials` | none | Basic or Digest; the URL may carry them instead |

Video must be H.264 or H.265. Audio is described from the session description and is not played. `Duration` and
`Size` are 0.

## Migrating from `MediaInfoWrapper`

`AsLegacy()` exposes the result under the property names `MediaInfoWrapper` has. `Duration` is in milliseconds, as before.

```csharp
// var media = new MediaInfoWrapper(path);
var media = (await analyzer.AnalyzeAsync(path)).AsLegacy();

if (media.Success)
{
    Console.WriteLine($"{media.VideoCodec} {media.Width}x{media.Height}, {media.AudioChannelsFriendly}");
}
```

For a DVD the stream details come from the feature itself, not from the `.BUP` file the wrapper read.

## Samples

| Sample | Shows |
| --- | --- |
| `Samples/AnalyzerSample` | File, stream, disc and URL analysis, dependency injection, cancellation |
| `Samples/BatchSample` | `AnalyzeFolderAsync` with caching and CSV output |
| `Samples/ApiSample` | ASP.NET Core API, including `POST /media/disc` |
