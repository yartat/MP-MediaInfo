# MP-MediaInfo
MP-MediaInfo is .NET wrapper for [MediaArea MediaInfo](https://github.com/MediaArea/MediaInfo) and use native packages [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Native)](https://www.nuget.org/packages/MediaInfo.Native) and [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Core.Native)](https://www.nuget.org/packages/MediaInfo.Core.Native).

[![Build status](https://ci.appveyor.com/api/projects/status/67ubhtmijuhyhq6q?svg=true)](https://ci.appveyor.com/project/yartat/mp-mediainfo)

## Available packages
| Framework | Package |
|-----------|---------|
| .NET Framework 4.0 | [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Wrapper)](https://www.nuget.org/packages/MediaInfo.Wrapper) |
| .NET Framework 4.5 | [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Wrapper)](https://www.nuget.org/packages/MediaInfo.Wrapper) |
| .NET Standard 2.0 | [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Wrapper.Core)](https://www.nuget.org/packages/MediaInfo.Wrapper.Core) |
| .NET Standard 2.1 | [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Wrapper.Core)](https://www.nuget.org/packages/MediaInfo.Wrapper.Core) |
| .NET 5.0 | [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Wrapper.Core)](https://www.nuget.org/packages/MediaInfo.Wrapper.Core) |

## Installation
There are 2 packages for .NET Core and full .NET. If your project is designed to run only on Windows and you are not using .NET Core, use the full .NET package. .NET Core package is designed for ASP.NET Core services only.
### .NET Core
```sh
dotnet add package MediaInfo.Wrapper.Core --version 21.3.5
```
### Full .NET
```ps
Install-Package MediaInfo.Wrapper -Version 21.3.5
```
## Dependencies
Make sure that the following dependencies are installed in the operating system before starting the project
* [libzen](https://github.com/MediaArea/ZenLib)
* [zlib](https://zlib.net)

.NET Core package supports next operating systems

| Operation system | Version |
|-----------|---------|
| [MacOS](#macos) | 10.5 (Leopard) and above |
| [Ubuntu](#ubuntu) | 16.04, 18.04, 19.10, 20.04 and 20.10 |
| [CenOS](#centos) | 6 and above |
| [Fedora](#fedora) | 30 and above |
| [OpenSUSE](#opensuse) | 15.2 and Tumbleweed |
| [RedHat](#redhat) | 6 and above |
| [Debian](#debian) | 8 and above |
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
sudo apt-get install libzen0v5 libmms0 libssh-4 libssl1.1 openssl zlib1g zlibc libsqlite3-0 libnghttp2-14 librtmp1 curl libcurl4-gnutls-dev libglib2.0-dev
```
### CentOS
```sh
sudo yum -y update
sudo yum -y install epel-release
sudo yum -y update
sudo yum -y install zlib curl libzen libssh
```
### Fedora
```sh
sudo yum update
sudo yum -y install zlib curl libzen libssh openssl tinyxm2 libmms
```
### OpenSUSE
```sh
sudo zipper refresh
sudo zipper update
sudo zipper install -y zlib curl libmms0 openssl libnghttp2-14
```
### RedHat
#### RedHat 7
```sh
sudo yum -y update
sudo rpm -ivh https://dl.fedoraproject.org/pub/epel/epel-release-latest-7.noarch.rpm
sudo yum -y update
sudo yum -y install zlib curl libzen libssh
```
#### RedHat 8
```sh
sudo dnf -y update
sudo dnf -y install https://dl.fedoraproject.org/pub/epel/epel-release-latest-8.noarch.rpm
sudo dnf -y update
sudo dnf -y install zlib curl libzen libssh
```
### Debian
```sh
sudo apt-get update
sudo apt-get install libzen0v5 libmms0 libssh-4 libssl1.1 openssl zlib1g zlibc libsqlite3-0 libnghttp2-14 librtmp1 curl libcurl4-gnutls-dev libglib2.0
```
### Windows
Windows package contains all dependencies and does not required any actions.

### Docker
```sh
FROM mcr.microsoft.com/dotnet/aspnet:3.1
RUN apt-get update && apt-get install -y libzen0v5 libmms0 libssh-4 libssl1.1 openssl zlib1g zlibc libsqlite3-0 libnghttp2-14 librtmp1 curl libcurl4-gnutls-dev libglib2.0
```
