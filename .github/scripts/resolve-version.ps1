param(
    [string] $Requested = ''
)

# Picks the package version for a build and checks it against the native packages.
# Without -Requested the version is the one in Directory.Build.props plus -ci.<run number>.

$ErrorActionPreference = 'Stop'

$props = [xml](Get-Content -Raw -Path (Join-Path $PSScriptRoot '../../Directory.Build.props'))

function Get-Property([string] $Name) {
    $props.Project.PropertyGroup | ForEach-Object { $_.$Name } | Where-Object { $_ } | Select-Object -First 1
}

function Get-Line([string] $Version) {
    ($Version -split '\.')[0..1] -join '.'
}

$native = Get-Property 'Native'
$coreNative = Get-Property 'CoreNative'
if ((Get-Line $native) -ne (Get-Line $coreNative)) {
    throw "Native ($native) and CoreNative ($coreNative) must be the same MediaInfoLib release."
}

if ($Requested) {
    if ($Requested -notmatch '^(\d+)\.(\d+)\.(\d+)(-[0-9A-Za-z][0-9A-Za-z.-]*)?$') {
        throw "'$Requested' is not a version of the form major.minor.patch[-suffix]."
    }

    if ("$($Matches[1]).$($Matches[2])" -ne (Get-Line $native)) {
        throw "Version $Requested does not belong to the native packages $native; change Native and CoreNative in Directory.Build.props first."
    }

    $version = $Requested
    $release = 'true'
}
else {
    $run = if ($env:GITHUB_RUN_NUMBER) { $env:GITHUB_RUN_NUMBER } else { '0' }
    $version = "$(Get-Property 'MediaInfoVersion')-ci.$run"
    $release = 'false'
}

Write-Host "version=$version release=$release"
if ($env:GITHUB_OUTPUT) {
    "version=$version" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
    "release=$release" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
}
