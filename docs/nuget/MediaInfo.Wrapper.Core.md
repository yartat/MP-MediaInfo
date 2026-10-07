# MediaInfo.Wrapper.Core

.NET wrapper for [MediaArea MediaInfo](https://mediaarea.net/en/MediaInfo). Targets .NET Standard 2.1, .NET 8.0 and .NET 10.0.
The native library comes from [`MediaInfo.Core.Native`](https://www.nuget.org/packages/MediaInfo.Core.Native) of the same version.
For .NET Framework use [`MediaInfo.Wrapper`](https://www.nuget.org/packages/MediaInfo.Wrapper).

```shell
dotnet add package MediaInfo.Wrapper.Core
```

## Usage

```csharp
using MediaInfo.Analysis;

var analyzer = MediaInfoAnalyzer.CreateDefault();
var result = await analyzer.AnalyzeAsync("path/to/film.mkv", cancellationToken);

if (result.Success)
{
    var video = result.BestVideoStream;
    var audio = result.BestAudioStream;
    Console.WriteLine($"{result.General.Format}, {result.General.Duration}");
    Console.WriteLine($"{video?.CodecName} {video?.Width}x{video?.Height}, {audio?.CodecName} {audio?.AudioChannelsFriendly}");
}
else
{
    Console.WriteLine($"{result.Failure!.Reason}: {result.Failure.Message}");
}
```

Files, `http`/`https` URLs, streams, DVD and Blu-ray folders, whole folders, caching, retries, dependency injection and
RTSP are covered in the [guide](https://github.com/yartat/MP-MediaInfo/blob/master/docs/async-api.md).

`MediaInfoWrapper`, the synchronous API, is obsolete. `AsLegacy()` presents a result under its property names.

## Platforms

Windows (x86, x64, ARM64), Linux (x64, ARM64, glibc and musl) and macOS (x64, ARM64). Linux and macOS need
[libzen](https://github.com/MediaArea/ZenLib) and [zlib](https://zlib.net); see the
[repository README](https://github.com/yartat/MP-MediaInfo#dependencies).

## Links

- [Release notes](https://github.com/yartat/MP-MediaInfo/blob/master/docs/release-notes/26.10.0.md)
- [Issues](https://github.com/yartat/MP-MediaInfo/issues)

BSD 2-Clause. This product uses the MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
