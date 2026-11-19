#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Debian 13 (trixie) images; the default .NET 10 tags are Ubuntu. The native libmediainfo needs libcurl (GnuTLS flavour)
# and libmms, and brings libzen with it.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-trixie-slim AS base
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends libcurl3t64-gnutls libmms0 \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-trixie-slim AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.config", "Directory.Build.props", "Directory.Build.targets", "./"]
COPY ["Samples/ApiSample/ApiSample.csproj", "Samples/ApiSample/"]
COPY ["MediaInfo.Wrapper/MediaInfo.Wrapper.Core.csproj", "MediaInfo.Wrapper/"]
RUN dotnet restore "Samples/ApiSample/ApiSample.csproj"
COPY . .
WORKDIR "/src/Samples/ApiSample"
RUN dotnet build "ApiSample.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore -p:GeneratePackageOnBuild=false

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ApiSample.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false -p:GeneratePackageOnBuild=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiSample.dll"]
