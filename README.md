# MP-MediaInfo

.NET wrapper for [MediaArea MediaInfo](https://github.com/MediaArea/MediaInfo). It uses the native packages [![NuGet Badge](https://img.shields.io/nuget/v/MediaInfo.Native.svg)](https://www.nuget.org/packages/MediaInfo.Native) and [![NuGet Badge](https://img.shields.io/nuget/v/MediaInfo.Core.Native.svg)](https://www.nuget.org/packages/MediaInfo.Core.Native).

[![License](https://img.shields.io/badge/License-BSD_2--Clause-orange.svg)](https://opensource.org/licenses/BSD-2-Clause)
![Build Core](https://github.com/yartat/MP-MediaInfo/actions/workflows/mp-mediainfo-core.yml/badge.svg)
![Build](https://github.com/yartat/MP-MediaInfo/actions/workflows/mp-mediainfo.yml/badge.svg)

## Packages

| Package | Frameworks | NuGet |
|---------|------------|-------|
| `MediaInfo.Wrapper.Core` | .NET Standard 2.1, .NET 8.0, .NET 10.0 | [![NuGet Badge](https://img.shields.io/nuget/v/MediaInfo.Wrapper.Core.svg)](https://www.nuget.org/packages/MediaInfo.Wrapper.Core) |
| `MediaInfo.Wrapper` | .NET Framework 4.0, 4.5, 4.8.1 | [![NuGet Badge](https://img.shields.io/nuget/v/MediaInfo.Wrapper.svg)](https://www.nuget.org/packages/MediaInfo.Wrapper) |
| `MediaInfo.Analysis.Rtsp` | .NET Standard 2.1, .NET 8.0, .NET 10.0 | [![NuGet Badge](https://img.shields.io/nuget/v/MediaInfo.Analysis.Rtsp.svg)](https://www.nuget.org/packages/MediaInfo.Analysis.Rtsp) |

Package versions follow the MediaInfoLib version they contain: 26.10.0 is MediaInfoLib 26.10.

```shell
dotnet add package MediaInfo.Wrapper.Core --version 26.10.0
dotnet add package MediaInfo.Analysis.Rtsp --version 26.10.0   # optional, rtsp:// sources
```

```powershell
Install-Package MediaInfo.Wrapper -Version 26.10.0
```

## Usage

**.NET Standard 2.1, .NET 8.0, .NET 10.0:** use the asynchronous analyzer, described in [docs/async-api.md](docs/async-api.md).

```csharp
var analyzer = MediaInfoAnalyzer.CreateDefault();
var result = await analyzer.AnalyzeAsync("path/to/media/file.mp4");

if (result.Success)
{
    Console.WriteLine($"{result.General.Format}, {result.BestVideoStream?.CodecName}");
}
```

**.NET Framework:** use `MediaInfoWrapper`. It is also available on .NET, where it is marked obsolete.

```csharp
using MediaInfo;

var media = new MediaInfoWrapper("path/to/media/file.mp4");
if (!media.Success)
{
    return;
}

Console.WriteLine($"{media.Format}, {media.Duration} ms");   // Duration is in milliseconds
Console.WriteLine($"{media.IsDvd} {media.IsBluRay} {media.IsStreamable}");

foreach (var video in media.VideoStreams)
{
    Console.WriteLine($"{video.Codec} {video.Width}x{video.Height} {video.FrameRate} fps, {video.Hdr}, interlaced: {video.Interlaced}, rotation: {video.Rotation}");
}

foreach (var audio in media.AudioStreams)
{
    // AudioChannelsFriendly is "Stereo", "5.1", "7.1.4"; DynamicObjects counts Dolby Atmos and DTS:X objects.
    Console.WriteLine($"{audio.Language} {audio.Codec} {audio.AudioChannelsFriendly} {audio.SamplingRate} Hz {audio.BitDepth} bit {audio.BitrateMode} {audio.DynamicObjects} objects");
}

foreach (var subtitle in media.Subtitles)
{
    Console.WriteLine($"{subtitle.Language} {subtitle.Codec} {subtitle.MaxCharactersPerLine}");
}

foreach (var chapter in media.Chapters)
{
    Console.WriteLine(chapter.Offset);
}

var tags = media.AudioStreams.Select(x => x.Tags);   // Artist, Album, Title, ...
var fileTags = media.Tags;
```

A logger is the optional second argument. On .NET it is a `Microsoft.Extensions.Logging.ILogger`; on .NET Framework it is `MediaInfo.ILogger`:

```csharp
public class ConsoleLogger : ILogger
{
    public void Log(LogLevel logLevel, string message, params object[] parameters) =>
        Console.WriteLine($"{logLevel}: {string.Format(message, parameters)}");
}

var media = new MediaInfoWrapper("path/to/media/file.mp4", new ConsoleLogger());
```

## Release notes

- [26.10.0](docs/release-notes/26.10.0.md)
- [All releases](https://github.com/yartat/MP-MediaInfo/releases)

## Demo application

[Samples/ApiSample](Samples/ApiSample) is an ASP.NET Core API that uses the analyzer and runs in a Docker container. It demonstrates usage and is not production code.

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
