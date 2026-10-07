param(
    [Parameter(Mandatory)] [string] $Directory,
    [Parameter(Mandatory)] [string] $Version,
    [switch] $RequireReleaseNotes
)

# Opens every package built for the release and checks what a consumer would see.

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$props = [xml](Get-Content -Raw -Path (Join-Path $PSScriptRoot '../../Directory.Build.props'))
function Get-Property([string] $Name) {
    $props.Project.PropertyGroup | ForEach-Object { $_.$Name } | Where-Object { $_ } | Select-Object -First 1
}

# package id -> the dependency it must carry and the version that dependency must have
$expected = [ordered]@{
    'MediaInfo.Wrapper.Core'  = @{ Dependency = 'MediaInfo.Core.Native'; Version = Get-Property 'CoreNative' }
    'MediaInfo.Wrapper'       = @{ Dependency = 'MediaInfo.Native'; Version = Get-Property 'Native' }
    'MediaInfo.Analysis.Rtsp' = @{ Dependency = 'MediaInfo.Wrapper.Core'; Version = $Version }
}

$problems = @()
foreach ($id in $expected.Keys) {
    $path = Join-Path (Resolve-Path $Directory).Path "$id.$Version.nupkg"
    if (-not (Test-Path $path)) {
        $problems += "$id : $path was not built"
        continue
    }

    $zip = [IO.Compression.ZipFile]::OpenRead($path)
    try {
        $nuspec = $zip.Entries | Where-Object { $_.FullName -like '*.nuspec' } | Select-Object -First 1
        $metadata = ([xml](New-Object IO.StreamReader($nuspec.Open())).ReadToEnd()).package.metadata

        if ($metadata.version -ne $Version) { $problems += "$id : version is $($metadata.version)" }
        if ($metadata.readme -ne 'README.md' -or -not ($zip.Entries | Where-Object { $_.FullName -eq 'README.md' })) {
            $problems += "$id : README.md is missing"
        }

        $notes = "$($metadata.releaseNotes)".Trim()
        if (-not $notes) {
            $problems += "$id : release notes are empty"
        }
        elseif ($RequireReleaseNotes -and $notes -eq "$($metadata.projectUrl)/releases") {
            $problems += "$id : no release notes file for $Version (docs/release-notes/<major.minor>/$id.txt)"
        }

        $dependencies = $metadata.dependencies.group.dependency | Where-Object { $_.id -eq $expected[$id].Dependency }
        if (-not $dependencies) {
            $problems += "$id : does not depend on $($expected[$id].Dependency)"
        }
        elseif ($dependencies | Where-Object { $_.version -notlike "$($expected[$id].Version)*" -and $_.version -ne "[$($expected[$id].Version), )" }) {
            $problems += "$id : depends on $($expected[$id].Dependency) $(($dependencies.version | Select-Object -Unique) -join ', '), expected $($expected[$id].Version)"
        }
    }
    finally {
        $zip.Dispose()
    }
}

if ($problems) {
    $problems | ForEach-Object { Write-Error $_ -ErrorAction Continue }
    exit 1
}

Write-Host "Verified $($expected.Count) packages at $Version."
