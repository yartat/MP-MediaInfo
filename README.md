# MP-MediaInfo
MP-MediaInfo is .NET wrapper for [MediaArea MediaInfo](https://github.com/MediaArea/MediaInfo) and use native packages [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Native)](https://www.nuget.org/packages/MediaInfo.Native) and [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Core.Native)](https://www.nuget.org/packages/MediaInfo.Core.Native).

[![Build status](https://ci.appveyor.com/api/projects/status/67ubhtmijuhyhq6q?svg=true)](https://ci.appveyor.com/project/yartat/mp-mediainfo)

## Available packages
| Framework | Package |
|-----------|---------|
| .NET Framework 4.0 | [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Wrapper)](https://www.nuget.org/packages/MediaInfo.Wrapper) |
| .NET Framework 4.5 | [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Wrapper)](https://www.nuget.org/packages/MediaInfo.Wrapper) |
| .NET Standard 2.0 | [![NuGet Badge](https://buildstats.info/nuget/MediaInfo.Wrapper.Core)](https://www.nuget.org/packages/MediaInfo.Wrapper.Core) |

## Installation
There are 2 packages for .NET Core and full .NET. If your project is designed to run only on Windows and you are not using .NET Core, use the full .NET package. .NET Core package is designed for ASP.NET Core services only.
### .NET Core
```sh
dotnet add package MediaInfo.Wrapper.Core --version 20.8.0
```
### Full .NET
```ps
Install-Package MediaInfo.Wrapper -Version 20.8.0
```
## Dependencies
Make sure that the following dependencies are installed in the operating system before starting the project
* [libzen](https://github.com/MediaArea/ZenLib)
* [zlib](https://zlib.net)

.NET Core package supports next operating systems

| Operation system | Version |
|-----------|---------|
| [MacOS](#MacOS) | 10.5 (Leopard) and above |
| [Ubuntu](#Ubuntu) | 16.04, 18.04, 18.10, 19.04, 19.10 and 20.04 |
| CenOS | 6 and above |
| Fedora | 30 and above |
| OpenSUSE | 15.2 and Tumbleweed |
| RedHat | 6 and above |
| Debian | 8 and above |
| Windows | 7 and above |
### MacOS
Some dependencies are available with MacPorts. To install MacPorts:
https://guide.macports.org/#installing

```sh
port install zlib curl zenlib
```
### Ubuntu
```sh
sudo apt-get update
sudo apt-get install libzen0v5 libmms0 libssh-4 libssl1.1 openssl zlib1g zlibc libsqlite3-0 libnghttp2-14 librtmp1 curl
```
### Windows
Windows package contains all dependencies and does not required any actions.