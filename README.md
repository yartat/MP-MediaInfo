# MP-MediaInfo
MP-MediaInfo is .NET wrapper for [MediaArea MediaInfo](https://github.com/MediaArea/MediaInfo) and use native packages [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Native)](https://www.nuget.org/packages/MediaInfo.Native) and [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Core.Native)](https://www.nuget.org/packages/MediaInfo.Core.Native).

[![Build status](https://ci.appveyor.com/api/projects/status/67ubhtmijuhyhq6q?svg=true)](https://ci.appveyor.com/project/yartat/mp-mediainfo)

## Features
* Wraps the MediaInfo library
* Provides properties for almost all information  available using the MediaInfo library
* Targets .NET Framework, .NET Standard

## Available packages
| Framework | Package |
|-----------|---------|
| .NET Framework 4.0 | [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Wrapper)](https://www.nuget.org/packages/MediaInfo.Wrapper) |
| .NET Framework 4.5 | [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Wrapper)](https://www.nuget.org/packages/MediaInfo.Wrapper) |
| .NET Standard 2.0 | [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Wrapper.Core)](https://www.nuget.org/packages/MediaInfo.Wrapper.Core) |
| .NET Standard 2.1 | [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Wrapper.Core)](https://www.nuget.org/packages/MediaInfo.Wrapper.Core) |
| .NET 5.0 | [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Wrapper.Core)](https://www.nuget.org/packages/MediaInfo.Wrapper.Core) |
| .NET 6.0 | [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Wrapper.Core)](https://www.nuget.org/packages/MediaInfo.Wrapper.Core) |

## Installation
There are 2 packages for .NET Core and .NET Framework. If your project is designed to run only on Windows and you are not using .NET Core, use the .NET Framework package. .NET Core package is designed for ASP.NET Core services only.
### .NET Core
```sh
dotnet add package MediaInfo.Wrapper.Core --version 21.9.2
```
### .NET Framework
```ps
Install-Package MediaInfo.Wrapper -Version 21.9.2
```
## Usage

Add to usings:
```csharp
using MediaInfo;
```
Instantiate an object of class `MediaInfoWrapper`, providing the full path to the media file and logger instance if it is required.
```csharp
  var media = new MediaInfoWrapper(mediaFileLocation);
```
Check successfully parsing of the parameters of the media file.
```csharp
  if (media.Success)
  {
      ...
  }
```
Retrieve technical and tag data from the video or audio file:
```csharp
  var containerFormat = media.Format;
  var mediaHasVideo = media.HasVideo;
  var videoBitRate = media.VideoRate;
  foreach (var stream in media.AudioStreams)
  {
      var codec = stream.Codec;
  }
```
## Demo application

ASP.NET Core demo application is [available](https://github.com/yartat/MP-MediaInfo/tree/master/Samples/ApiSample) which shows the usage of the package, serialization and running from the docker container. Code from this demo should not be used in production code, the code is merely to demonstrate the usage of this package.

## Dependencies
Make sure that the following dependencies are installed in the operating system before starting the project
* [libzen](https://github.com/MediaArea/ZenLib)
* [zlib](https://zlib.net)

.NET Core package supports next operating systems

| Operation system | Version |
|-----------|---------|
| [MacOS](#macos) | 10.15 (Catalina), 11 (Big Sur) |
| [Ubuntu](#ubuntu) | 16.04, 18.04, 20.04 and 21.04 |
| [CenOS](#centos) | 7 and above |
| [Fedora](#fedora) | 32 and above |
| [OpenSUSE](#opensuse) | 15.2 and Tumbleweed |
| [RedHat](#redhat) | 7 and above |
| [Debian](#debian) | 9 and above |
| [Arch Linux](#archlinux) | |
| [Windows](#windows) | 7 and above |
| [Docker](#docker) | buster |
### MacOS
Some dependencies are available with MacPorts. To install MacPorts:
https://guide.macports.org/#installing

```sh
port install zlib curl zenlib
```
### Ubuntu
```sh
sudo apt-get update
sudo apt-get install libzen0v5 libmms0 zlib1g zlibc libnghttp2-14 librtmp1 curl libcurl4-gnutls-dev libglib2.0-dev
```
### CentOS
#### CentOS 7
```sh
sudo rpm -ivh https://dl.fedoraproject.org/pub/epel/epel-release-latest-7.noarch.rpm
sudo yum -y update
sudo yum -y install zlib curl libzen bzip2 libcurl
sudo rpm -ivh https://download1.rpmfusion.org/free/el/updates/7/x86_64/l/libmms-0.6.4-2.el7.x86_64.rpm
```
#### CentOS 8
```sh
sudo rpm -ivh https://dl.fedoraproject.org/pub/epel/epel-release-latest-8.noarch.rpm
sudo yum -y update
sudo yum -y install zlib curl libzen bzip2 libcurl
sudo rpm -ivh https://download1.rpmfusion.org/free/el/updates/8/x86_64/l/libmms-0.6.4-8.el8.x86_64.rpm
```
### Fedora
```sh
sudo dnf update
sudo dnf -y install zlib curl libzen openssl libmms
```
### OpenSUSE
```sh
sudo zypper refresh
sudo zypper update -y
sudo zypper install -y zlib curl libmms0 openssl libnghttp2-14
```
### RedHat
#### RedHat 7
```sh
sudo rpm -ivh https://dl.fedoraproject.org/pub/epel/epel-release-latest-7.noarch.rpm
sudo yum -y update
sudo yum -y install zlib curl libzen bzip2 libcurl
sudo rpm -ivh https://download1.rpmfusion.org/free/el/updates/7/x86_64/l/libmms-0.6.4-2.el7.x86_64.rpm
```
#### RedHat 8
```sh
sudo rpm -ivh https://dl.fedoraproject.org/pub/epel/epel-release-latest-8.noarch.rpm
sudo yum -y update
sudo yum -y install zlib curl libzen bzip2 libcurl
sudo rpm -ivh https://download1.rpmfusion.org/free/el/updates/8/x86_64/l/libmms-0.6.4-8.el8.x86_64.rpm
```
### Debian
```sh
sudo apt-get update
sudo apt-get install libzen0v5 libmms0 openssl zlib1g zlibc libnghttp2-14 librtmp1 curl libcurl4-gnutls-dev libglib2.0
```
### Windows
Windows package contains all dependencies and does not required any actions.

### ArchLinux
```sh
sudo pacman -Syu
sudo pacman -S libcurl-gnutls libzen libmms libssh librtmp0
```
### Docker
#### .NET Core 3.1
```sh
FROM mcr.microsoft.com/dotnet/aspnet:3.1
RUN apt-get update && apt-get install -y libzen0v5 libmms0 openssl zlib1g zlibc libnghttp2-14 librtmp1 curl libcurl4-gnutls-dev libglib2.0
```
#### .NET 5.0
```sh
FROM mcr.microsoft.com/dotnet/aspnet:5.0
RUN apt-get update && apt-get install -y libzen0v5 libmms0 openssl zlib1g zlibc libnghttp2-14 librtmp1 curl libcurl4-gnutls-dev libglib2.0
```
#### .NET 6.0
```sh
FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN apt-get update && apt-get install -y libzen0v5 libmms0 openssl zlib1g zlibc libnghttp2-14 librtmp1 curl libcurl4-gnutls-dev libglib2.0
```