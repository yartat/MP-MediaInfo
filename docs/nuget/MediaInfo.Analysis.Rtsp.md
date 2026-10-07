# MediaInfo.Analysis.Rtsp

Analysis of `rtsp://` sources for [`MediaInfo.Wrapper.Core`](https://www.nuget.org/packages/MediaInfo.Wrapper.Core).
Targets .NET Standard 2.1, .NET 8.0 and .NET 10.0.

```shell
dotnet add package MediaInfo.Analysis.Rtsp
```

## Usage

```csharp
using MediaInfo.Analysis;
using MediaInfo.Analysis.Rtsp;

var analyzer = MediaInfoAnalyzerBuilder.Create()
    .WithRtsp(rtsp => rtsp.CaptureDuration = TimeSpan.FromSeconds(3))
    .Build();

var result = await analyzer.AnalyzeAsync("rtsp://user:password@camera.local/stream1", cancellationToken);

if (result.Success)
{
    var video = result.BestVideoStream!;
    Console.WriteLine($"{video.CodecName} {video.Width}x{video.Height} {video.FrameRate} fps");
}
```

Without `.WithRtsp()` an `rtsp://` source is declined.

| | |
| --- | --- |
| Video | H.264 and H.265 |
| Audio | Described from the session description, not played |
| Authentication | Basic and Digest, from `RtspAnalysisOptions.Credentials` or the URL |
| Capture limits | `CaptureDuration`, `MaximumCaptureBytes`, `SufficientFrameCount` |

`Duration` and `Size` are 0. Other video codecs are declined with a message.

## Links

- [Guide](https://github.com/yartat/MP-MediaInfo/blob/master/docs/async-api.md#rtsp)
- [Release notes](https://github.com/yartat/MP-MediaInfo/blob/master/docs/release-notes/26.10.0.md)
- [Issues](https://github.com/yartat/MP-MediaInfo/issues)

BSD 2-Clause. This product uses the MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
