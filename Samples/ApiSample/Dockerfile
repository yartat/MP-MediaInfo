#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN apt-get update && apt-get install -y libzen0v5 libmms0 libssh-4 libssl1.1 openssl zlib1g zlibc libsqlite3-0 libnghttp2-14 librtmp1 curl libcurl4-gnutls-dev libglib2.0
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Samples/ApiSample/ApiSample.csproj", "Samples/ApiSample/"]
COPY ["MediaInfo.Wrapper/MediaInfo.Wrapper.Core.csproj", "MediaInfo.Wrapper/"]
RUN dotnet restore "Samples/ApiSample/ApiSample.csproj"
COPY . .
WORKDIR "/src/Samples/ApiSample"
RUN dotnet build "ApiSample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiSample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiSample.dll"]