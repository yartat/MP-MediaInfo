param(
    [Parameter(Mandatory)] [string] $Solution,
    [Parameter(Mandatory)] [string] $Version
)

# Restores, builds and tests one solution in Release.
# The two solutions share the obj folder of the MediaInfo.Wrapper directory, so each one
# has to be restored, built and tested before the other is restored.

$ErrorActionPreference = 'Stop'

dotnet restore $Solution --configfile .github/ci-sources.config -p:Version=$Version
if ($LASTEXITCODE) { exit $LASTEXITCODE }

dotnet build $Solution --configuration Release --no-restore -p:Version=$Version
if ($LASTEXITCODE) { exit $LASTEXITCODE }

dotnet test $Solution --configuration Release --no-build --logger trx --results-directory "TestResults/$Solution"
exit $LASTEXITCODE
