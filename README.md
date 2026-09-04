# MP-MediaInfo

MP-MediaInfo is .NET wrapper for [MediaArea MediaInfo](https://github.com/MediaArea/MediaInfo) and use native packages [![NuGet Badge](https://img.shields.io/nuget/v/MediaInfo.Native.svg)](https://www.nuget.org/packages/MediaInfo.Native) and [![NuGet Badge](https://img.shields.io/nuget/v/MediaInfo.Core.Native.svg)](https://www.nuget.org/packages/MediaInfo.Core.Native).

[![License](https://img.shields.io/badge/License-BSD_2--Clause-orange.svg)](https://opensource.org/licenses/BSD-2-Clause)
![Build Core](https://github.com/yartat/MP-MediaInfo/actions/workflows/mp-mediainfo-core.yml/badge.svg)
![Build](https://github.com/yartat/MP-MediaInfo/actions/workflows/mp-mediainfo.yml/badge.svg)


## Features

* **Comprehensive Media Analysis**: Wraps the MediaInfo library to provide detailed information about video, audio, subtitle, and metadata streams
* **Rich Property Access**: Exposes properties for video codecs, bitrates, resolution, frame rates, audio properties, subtitles, chapters, and extensive tag information
* **Multi-Format Support**: Supports analysis of virtually all video and audio formats supported by MediaInfo (see [Supported Formats](#supported-formats))
* **Stream Information**: Provides detailed access to individual video streams, audio streams, subtitle streams, chapters, and menu information
* **Metadata Extraction**: Extract technical tags and general metadata from media files
* **Cross-Platform**: Targets .NET Framework 4.0+, .NET Standard 2.1, .NET 8.0, and .NET 10.0
* **Optional Logging**: Built-in support for custom logging to track analysis operations

## Available packages

| Framework | Package |
|-----------|---------|
| .NET Framework 4.0 | [![NuGet Badge](https://img.shields.io/nuget/v/MediaInfo.Wrapper.svg)](https://www.nuget.org/packages/MediaInfo.Wrapper) |
| .NET Framework 4.5 | [![NuGet Badge](https://img.shields.io/nuget/v/MediaInfo.Wrapper.svg)](https://www.nuget.org/packages/MediaInfo.Wrapper) |
| .NET Standard 2.1 | [![NuGet Badge](https://img.shields.io/nuget/v/MediaInfo.Wrapper.Core.svg)](https://www.nuget.org/packages/MediaInfo.Wrapper.Core) |
| .NET 8.0 | [![NuGet Badge](https://img.shields.io/nuget/v/MediaInfo.Wrapper.Core.svg)](https://www.nuget.org/packages/MediaInfo.Wrapper.Core) |
| .NET 10.0 | [![NuGet Badge](https://img.shields.io/nuget/v/MediaInfo.Wrapper.Core.svg)](https://www.nuget.org/packages/MediaInfo.Wrapper.Core) |

## Installation

Two packages are available:

- **MediaInfo.Wrapper.Core** - For .NET Standard 2.1, .NET 8.0 and .NET 10.0. Recommended for modern applications, cross-platform projects, and ASP.NET Core services
- **MediaInfo.Wrapper** - For .NET Framework 4.0+. Use this if you're on Windows only with .NET Framework

Choose based on your target framework:

### .NET Core

```Shell{:copy}
dotnet add package MediaInfo.Wrapper.Core --version 26.1.0
```

### .NET Framework

```PowerShell{:copy}
Install-Package MediaInfo.Wrapper -Version 26.1.0
```

## Usage

> **New in 27.0 — the asynchronous analyzer.** `MediaInfoWrapper` does all its work in its constructor, so an analysis
> cannot be awaited, cancelled or retried, and it describes a DVD or Blu-ray only as a flag and a folder size. It is
> still supported and still works, but it is now marked obsolete on .NET. New code should use
> [`IMediaInfoAnalyzer`](#asynchronous-analysis), described below. Everything after that section documents the
> original wrapper, whose behaviour is unchanged.

## Asynchronous analysis

Available on `MediaInfo.Wrapper.Core` for `netstandard2.1`, `net8.0` and `net10.0`. The .NET Framework package
keeps the original wrapper only, because the pipeline needs `Span<T>` and `IAsyncEnumerable<T>`.

```csharp
using MediaInfo.Analysis;

var analyzer = MediaInfoAnalyzer.CreateDefault();
var result = await analyzer.AnalyzeAsync("path/to/media/file.mp4");

if (result.Success)
{
    Console.WriteLine($"{result.General.Format}, {result.General.Duration}");
    Console.WriteLine($"{result.BestVideoStream?.CodecName} {result.BestVideoStream?.Width}x{result.BestVideoStream?.Height}");
}
else
{
    // A failure says why, instead of leaving you to guess from an empty result.
    Console.WriteLine($"{result.Failure!.Reason}: {result.Failure.Message}");
}
```

The result is an immutable record, so it can be cached, shared between threads and compared without repeating the
analysis. The same call accepts a file path, an `http`/`https` URL, or a folder holding a disc.

### Streams and cancellation

```csharp
await using var stream = File.OpenRead("path/to/media/file.mkv");
var result = await analyzer.AnalyzeAsync(stream, leaveOpen: true, cancellationToken);
```

The stream path is asynchronous end to end and observes cancellation between 64 KB blocks. Analyzing a **file** or a
**URL** hands the path to the native library, which opens it with a blocking call: the token is honoured before that
call and at the next await, but not during it.

### DVD and Blu-ray

```csharp
var result = await analyzer.AnalyzeAsync(@"D:\Movies\SomeFilm\VIDEO_TS");

if (result.Disc is DvdStructure dvd)
{
    Console.WriteLine($"{dvd.DvdTitles.Count} title set(s), {dvd.TotalSize:N0} bytes");
    foreach (var title in dvd.DvdTitles)
    {
        Console.WriteLine($"VTS_{title.TitleSetNumber:00}: {title.Duration}, {title.VobFiles.Count} VOB(s)");
    }
}
```

The stream information describes the main title, while `result.Disc` carries every title on the disc. Passing the disc
root, the `VIDEO_TS`/`BDMV` folder, or any `.IFO` inside it all resolve to the same disc.

### Composing behaviour

Every cross-cutting concern is opt-in and independent:

```csharp
var analyzer = MediaInfoAnalyzerBuilder.Create()
    .WithValidation()                                  // reject bad input before opening anything
    .WithCaching(TimeSpan.FromMinutes(10))             // reuse the result for an unchanged file or disc
    .WithTimeout(TimeSpan.FromSeconds(30))             // total latency budget
    .WithRetry(attempts: 3)                            // repeat only failures that may not persist
    .WithConcurrencyLimit(Environment.ProcessorCount)  // bound how many native handles exist at once
    .WithExternalSubtitles()                           // report .srt and friends sitting beside the media
    .Build();
```

The order you request them in does not matter; the chain is always built so that logging sees rejections, a cache hit
costs nothing, and the latency budget covers the wait for a free slot as well as the analysis itself.

With dependency injection:

```csharp
services.AddMediaInfoAnalyzer(builder => builder
    .WithCaching()
    .WithConcurrencyLimit(4));
```

`IMediaInfoAnalyzer` is registered as a singleton and picks up a logger from the container when one is registered.

### RTSP streams

The native library cannot open an `rtsp://` location, so an RTSP source is declined by default. The
`MediaInfo.Analysis.Rtsp` package teaches the analyzer to speak the protocol itself, with no third party client:

```csharp
using MediaInfo.Analysis.Rtsp;

var analyzer = MediaInfoAnalyzerBuilder.Create()
    .WithRtsp(rtsp => rtsp.CaptureDuration = TimeSpan.FromSeconds(3))
    .Build();

var result = await analyzer.AnalyzeAsync("rtsp://camera.local/stream1");
Console.WriteLine(result.BestVideoStream?.CodecName);   // AVC High@L4.1
Console.WriteLine(result.BestVideoStream?.Width);       // 1920
```

It describes the session, plays the video track for a bounded window, rebuilds the elementary stream the RTP packets
carried and hands that to the ordinary stream analysis. The video is therefore described as fully as a file is,
because the same parser reads the same bitstream.

| | |
| --- | --- |
| Video | H.264 (RFC 6184) and H.265 (RFC 7798), rebuilt from single, aggregated and fragmented packets and read by the library |
| Audio | Taken from the session description: encoding, sample rate and channel count. The track is not played |
| Transport | RTP interleaved on the RTSP connection, so no second port has to be opened |
| Authentication | Basic and Digest, from `RtspAnalysisOptions.Credentials` or from the URL itself |
| Bounded by | `CaptureDuration`, `MaximumCaptureBytes` and `SufficientFrameCount`, whichever comes first |

`Duration` and `Size` are reported as zero: a live stream has no length, and how much of it the capture happened to
take says nothing about the stream. Cancellation is observed between packets.

A stream whose video track is neither H.264 nor H.265 is declined with a message saying so. Decoding order numbers
are not read, which only matters for a sender that interleaves units, and no sender does that in answer to a plain
play request.

### Scanning a folder

```csharp
await foreach (var result in analyzer.AnalyzeFolderAsync(@"D:\Movies", cancellationToken: token))
{
    Console.WriteLine($"{result.SourcePath}: {result.General.Duration}");
}
```

Results arrive as they are ready. A subfolder holding `VIDEO_TS` or `BDMV` is reported as one disc rather than as its
individual files, and a file that cannot be read is yielded as a failure instead of ending the scan.

### Migrating from `MediaInfoWrapper`

`AsLegacy()` presents the result through the same property names the wrapper used, so only the line that produces the
object has to change:

```csharp
// var media = new MediaInfoWrapper(path);
var media = (await analyzer.AnalyzeAsync(path)).AsLegacy();

// Everything below is unchanged.
if (media.Success)
{
    Console.WriteLine($"{media.VideoCodec} {media.Width}x{media.Height}, {media.AudioChannelsFriendly}");
}
```

The values are identical, which the parity suite asserts file by file over the whole test corpus. `media.Duration` is
still in milliseconds. The one intentional difference is a DVD: the wrapper read the stream details out of a `.BUP`
navigation file, while the analyzer reads them out of the feature itself, so the codecs it reports are those of the
media that actually plays.

### Samples

| Sample | Shows |
| --- | --- |
| `Samples/AnalyzerSample` | One media at a time: file, stream with a progress bar, disc, URL. DI wiring, cancellation, failure reporting. |
| `Samples/BatchSample` | Walking a media library with `AnalyzeFolderAsync`, bounded concurrency, caching, CSV output. |
| `Samples/ApiSample` | An ASP.NET Core API using the analyzer, including a `POST /media/disc` endpoint returning disc structure. |


### Basic Setup

Add to usings:

```csharp
using MediaInfo;
```

Instantiate a `MediaInfoWrapper` with the path to your media file:

```csharp
var media = new MediaInfoWrapper("path/to/media/file.mp4");
```

### Checking Analysis Success

Always verify the file was successfully analyzed:

```csharp
if (media.Success)
{
    // File analyzed successfully
}
else
{
    // Handle analysis failure
    Console.WriteLine("Failed to analyze media file");
}
```

### General Information

Access basic media information:

```csharp
var containerCodec = media.Codec;               // e.g., "MPEG-4"
var containerFormat = media.Format;             // e.g., "MP4"
var duration = media.Duration;                  // Duration in milliseconds
var isMediaBluRay = media.IsBluRay;             // media is Blu-ray or not
var isMediaDvd = media.IsDvd;                   // media is DVD or not
var isMediaStreamable = media.IsStreamable;	    // media is streamable or not

```

### Video Stream Analysis

Extract detailed video information:

```csharp
if (media.HasVideo)
{
    var videoStream = media.VideoStreams.FirstOrDefault();
    if (videoStream != null)
    {
        var width = videoStream.Width;                          // Resolution width
        var height = videoStream.Height;                        // Resolution height
        var codec = videoStream.Codec;                          // e.g., AVC, HEVC, VP9, etc.
        var frameRate = videoStream.FrameRate;                  // Frames per second
        var frameRateMode = videoStream.FrameRateMode;          // e.g., CFR, VFR
        var bitRate = videoStream.Bitrate;                      // Video stream bitrate
        var standard = videoStream.Standard;                    // e.g., NTSC, PAL
        var aspectRatio = videoStream.AspectRatio;              // e.g., HighEndDataGraphics, Square, etc.
        var chromaSubSampling = videoStream.SubSampling;        // e.g., Sampling420, Sampling422, Sampling444
        var colorSpace = videoStream.ColorSpace;                // e.g., BT2020, BT709, NTSC, etc.
        var hdrFormat = videoStream.Hdr;                        // e.g., HDR10, DolbyVision, HLG, etc.
        var isInterlaced = videoStream.IsInterlaced;            // true if video is interlaced
        var stereoscopic = videoStream.Stereoscopic;            // e.g., Mono, Stereo, SideBySideLeft, TopBottomRight, etc.
    }
}
```

### Audio Stream Analysis

Access audio track information:

```csharp
foreach (var audioStream in media.AudioStreams)
{
    var language = audioStream.Language;                      // ISO 639-2 language code
    var codec = audioStream.Codec;                            // e.g., AAC, AC-3, DTS, etc.
    var bitRate = audioStream.Bitrate;                        // Audio bitrate
    var channelCount = audioStream.Channel;                   // Number of audio channels
    var channelsFriendly = audioStream.AudioChannelsFriendly; // e.g., "5.1", "Stereo"
    var samplingRate = audioStream.SamplingRate;              // Sample rate in Hz
    var bitDepth = audioStream.BitDepth;                      // Bits per sample
    var bitrateMode = audioStream.BitrateMode;                // CBR, VBR, etc.
    var name = audioStream.Name;                              // Track name if available
}
```

### Subtitle Stream Analysis

Retrieve subtitle information:

```csharp
foreach (var subtitleStream in media.SubtitleStreams)
{
    var codec = subtitleStream.Codec;              // e.g., "PGS", "ASS"
    var language = subtitleStream.Language;        // ISO 639-2 language code
    var name = subtitleStream.Name;                // Subtitle track name
}
```

### Audio and Video Tags

Extract metadata:

```csharp
var audioTags = media.AudioStreams.Select(x => x.Tags);  // Audio metadata (album, artist, etc.)
var videoTags = media.VideoStreams.Select(x => x.Tags);  // Video metadata (title, description, etc.)
var generalInfo = media.Tags;                            // File-level metadata

foreach (var audioTag in audioTags)
{
    var artist = audioTag.Performer;
    var album = audioTag.Album;
    var title = audioTag.Title;
}
```

### Chapter Information

Access chapter/menu information:

```csharp
foreach (var chapter in media.ChapterStreams)
{
    var startTime = chapter.Offset;  // Chapter start timestamp
    // Chapter stream information available
}
```

### Using a Logger

Optionally provide a logger to track analysis:

```csharp
public class ConsoleLogger : ILogger
{
    public void Log(string message) => Console.WriteLine(message);
}

var logger = new ConsoleLogger();
var media = new MediaInfoWrapper("path/to/file.mp4", logger);
```

## Supported Formats

MP-MediaInfo supports analysis of virtually all media formats supported by the underlying MediaInfo library, including:

**Video Formats**: MP4, MKV, AVI, MOV, FLV, WebM, WMV, 3GP, and many more

**Audio Codecs**: H.264/AVC, H.265/HEVC, MPEG-4, VP8, VP9, AV1, Theora, and others

**Audio Formats**: MP3, AAC, FLAC, Opus, Vorbis, AC-3, DTS, TrueHD, Atmos, and more

**Subtitle Formats**: PGS, ASS/SSA, SRT, SUBRIP, DVB, and other subtitle types

For a complete and detailed list, refer to the [MediaInfo documentation](https://github.com/MediaArea/MediaInfo).

## Advanced Usage

### Error Handling

Handle potential errors gracefully:

```csharp
try
{
    var media = new MediaInfoWrapper(filePath);

    if (!media.Success)
    {
        Console.WriteLine("Analysis was not successful");
        return;
    }

    // Process media information
}
catch (FileNotFoundException)
{
    Console.WriteLine("Media file not found");
}
catch (Exception ex)
{
    Console.WriteLine($"Error analyzing media: {ex.Message}");
}
```

### Batch Processing Multiple Files

Analyze multiple files efficiently:

```csharp
var mediaFiles = Directory.GetFiles("mediaFolder", "*.mp4");

foreach (var file in mediaFiles)
{
    try
    {
        var media = new MediaInfoWrapper(file);
        if (media.Success)
        {
            Console.WriteLine($"{Path.GetFileName(file)}: {media.Format}");
            // Process each file
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing {file}: {ex.Message}");
    }
}
```

### Checking Specific Media Characteristics

```csharp
var media = new MediaInfoWrapper(filePath);

// Check if file has video
if (media.HasVideo)
{
    var videoStream = media.VideoStreams.FirstOrDefault();
    if (videoStream?.Hdr != null)
    {
        Console.WriteLine($"HDR Format: {videoStream.Hdr}");
    }
}

// Check number of audio tracks
Console.WriteLine($"Audio tracks: {media.AudioStreams.Count}");

// Find specific audio Dolby Digital codec
var ac3Tracks = media.AudioStreams
    .Where(x => x.Codec == AudioCodec.Ac3)
    .ToList();
```

## Troubleshooting

### Analysis Returns False (media.Success == false)

- Verify the file path is correct and the file exists
- Ensure the file is not corrupted
- Check that the media format is supported
- On Linux/macOS, verify all dependencies (libzen, zlib) are installed
- Try enabling logging to see detailed error messages

### Missing Native Libraries

**Windows**: The native MediaInfo libraries are included in the NuGet package. No additional installation needed.

**Linux/macOS**: Install system dependencies as described in the [Dependencies](#dependencies) section.

## Demo application

ASP.NET Core demo application is [available](https://github.com/yartat/MP-MediaInfo/tree/master/Samples/ApiSample) which shows the usage of the package, serialization and running from the docker container. Code from this demo should not be used in production code, the code is merely to demonstrate the usage of this package.

## Dependencies

Make sure that the following dependencies are installed in the operating system before starting the project

* [libzen](https://github.com/MediaArea/ZenLib)
* [zlib](https://zlib.net)

.NET Core package supports next operating systems

| Operation system | Version |
|-----------|---------|
| [Alpine](#alpine) | 3.17, 3.18, 3.19 and 3.20 |
| [MacOS](#macos) | 10.15 and above |
| [Ubuntu](#ubuntu) | 16.04, 18.04, 20.04, 21.04, 22.04, 24.04 and 25.10 |
| [CenOS](#centos) | 8 and above |
| [Fedora](#fedora) | 32 and above |
| [OpenSUSE](#opensuse) | 15.4 and above |
| [RedHat](#redhat) | 7 and above |
| [Debian](#debian) | 9 and above |
| [Kali Linux](#kali-linux) | |
| [Windows](#windows) | 7 and above |
| [Docker](#docker) | buster |

### Alpine

```sh{:copy}
apk update
apk add --no-cache libmms-dev libssh openssl curl-dev ca-certificates
```

### MacOS

Some dependencies are available with MacPorts. To install MacPorts: <https://guide.macports.org/#installing>

```sh{:copy}
port install zlib curl zenlib
```

### Ubuntu

```Shell{:copy}
sudo apt-get update
sudo apt-get install libzen0v5 libmms0 zlib1g zlibc libnghttp2-14 librtmp1 curl libcurl4-gnutls-dev libglib2.0-dev
```

### CentOS

#### CentOS 8

```Shell{:copy}
sudo rpm -ivh https://dl.fedoraproject.org/pub/epel/epel-release-latest-8.noarch.rpm
sudo yum -y update
sudo yum -y install zlib curl libzen bzip2 libcurl
sudo rpm -ivh https://www.rpmfind.net/linux/epel/8/Everything/x86_64/Packages/l/libmms-0.6.4-24.el8.x86_64.rpm
```

#### CentOS 9

```Shell{:copy}
sudo rpm -ivh https://dl.fedoraproject.org/pub/epel/epel-release-latest-9.noarch.rpm
sudo yum -y update
sudo yum -y install zlib curl libzen bzip2 libcurl libmms
sudo rpm -ivh https://www.rpmfind.net/linux/epel/9/Everything/x86_64/Packages/l/libmms-0.6.4-24.el9.x86_64.rpm
```

#### CentOS 10

```Shell{:copy}
sudo rpm -ivh https://dl.fedoraproject.org/pub/epel/epel-release-latest-10.noarch.rpm
sudo yum -y update
sudo yum -y install zlib curl libzen bzip2 libcurl libmms
```

### Fedora

```Shell{:copy}
sudo dnf update
sudo dnf -y install zlib curl libzen openssl libmms
```

### OpenSUSE

```Shell{:copy}
sudo zypper refresh
sudo zypper update -y
sudo zypper install -y zlib curl libmms0 openssl libnghttp2-14
```

### RedHat

#### RedHat 7

#### RedHat 8

```Shell{:copy}
sudo rpm -ivh https://dl.fedoraproject.org/pub/epel/epel-release-latest-8.noarch.rpm
sudo yum -y update
sudo yum -y install zlib curl libzen bzip2 libcurl
sudo rpm -ivh https://www.rpmfind.net/linux/epel/8/Everything/x86_64/Packages/l/libmms-0.6.4-24.el8.x86_64.rpm
```

#### RedHat 9

```Shell{:copy}
sudo rpm -ivh https://dl.fedoraproject.org/pub/epel/epel-release-latest-9.noarch.rpm
sudo yum -y update
sudo yum -y install zlib curl libzen bzip2 libcurl
sudo rpm -ivh https://www.rpmfind.net/linux/epel/9/Everything/x86_64/Packages/l/libmms-0.6.4-24.el9.x86_64.rpm
```

#### RedHat 10

```Shell{:copy}
sudo rpm -ivh https://dl.fedoraproject.org/pub/epel/epel-release-latest-10.noarch.rpm
sudo yum -y update
sudo yum -y install zlib curl libzen bzip2 libcurl libmms
```

### Debian

```Shell{:copy}
sudo apt-get update
sudo apt-get install libzen0v5 libmms0 openssl zlib1g zlibc libnghttp2-14 librtmp1 curl libcurl4-gnutls-dev libglib2.0
```

### Kali Linux

```Shell{:copy}
sudo apt update
sudo apt install -y zlib1g-dev curl libssh-4 libmms-dev openssl libzen-dev openssl libnghttp2-14 librtmp1 libcurl4-gnutls-dev
```

### Windows

Windows package contains all dependencies and does not required any actions.

### ArchLinux

```Shell{:copy}
sudo pacman -Syu
sudo pacman -S libcurl-gnutls libzen libmms libssh librtmp0
```

### Docker

#### .NET 8.0

```Dockerfile{:copy}
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update && apt-get install -y libzen0v5 libmms0 openssl zlib1g zlibc libnghttp2-14 librtmp1 curl libcurl4-gnutls-dev libglib2.0
```

#### .NET 10.0

```Dockerfile{:copy}
FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN apt-get update && apt-get install -y libzen0v5 libmms0 openssl zlib1g zlibc libnghttp2-14 librtmp1 curl libcurl4-gnutls-dev libglib2.0
```
