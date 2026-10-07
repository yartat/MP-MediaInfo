# MediaInfo.Wrapper

.NET Framework wrapper for [MediaArea MediaInfo](https://mediaarea.net/en/MediaInfo). Targets .NET Framework 4.0, 4.5 and 4.8.1.
The native library comes from [`MediaInfo.Native`](https://www.nuget.org/packages/MediaInfo.Native) of the same version.
For .NET Standard 2.1, .NET 8.0 and .NET 10.0, and for the asynchronous analyzer, use
[`MediaInfo.Wrapper.Core`](https://www.nuget.org/packages/MediaInfo.Wrapper.Core).

```powershell
Install-Package MediaInfo.Wrapper
```

## Usage

```csharp
using MediaInfo;

var media = new MediaInfoWrapper(@"C:\Videos\film.mkv");
if (!media.Success)
{
    return;
}

Console.WriteLine($"{media.Format}, {media.Duration} ms, {media.Size} bytes");

foreach (var video in media.VideoStreams)
{
    Console.WriteLine($"{video.Codec} {video.Width}x{video.Height} {video.FrameRate} fps, {video.Hdr}");
}

foreach (var audio in media.AudioStreams)
{
    Console.WriteLine($"{audio.Language} {audio.Codec} {audio.AudioChannelsFriendly}, {audio.DynamicObjects} objects");
}

foreach (var subtitle in media.Subtitles)
{
    Console.WriteLine($"{subtitle.Language} {subtitle.Codec}");
}
```

`IsDvd` and `IsBluRay` identify a disc folder. `Text` is the library's full report. A `MediaInfo.ILogger` is the optional second argument.

## Links

- [Documentation](https://github.com/yartat/MP-MediaInfo#usage)
- [Release notes](https://github.com/yartat/MP-MediaInfo/blob/master/docs/release-notes/26.10.0.md)
- [Issues](https://github.com/yartat/MP-MediaInfo/issues)

BSD 2-Clause. This product uses the MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
