# Releasing

## Checks

`.github/workflows/ci.yml` runs on every pull request and on pushes to `master` and `release/**`. On Windows it builds
and tests both solutions in Release (`MP-MediaInfo.Core.slnx`: net10.0 tests; `MP-MediaInfo.slnx`: net481 tests), builds
the three packages as `<version>-ci.<run number>` and checks them. The packages are attached to the run as the
`packages` artifact.

Tests that need the external sample corpus or the HTTP and RTSP servers are skipped in Release.

## Release

1. In `Directory.Build.props` set `MediaInfoVersion` and `MediaInfoFileVersion`, and `Native` and `CoreNative` to the
   MediaInfo.Native / MediaInfo.Core.Native version. The release version must have the same major.minor as both.
2. Add `docs/release-notes/<version>.md` (GitHub release) and `docs/release-notes/<major.minor>/<package>.txt` for
   `MediaInfo.Wrapper.Core`, `MediaInfo.Wrapper` and `MediaInfo.Analysis.Rtsp` (NuGet).
3. Merge to `master`, then push a tag:

   ```shell
   git tag v26.10.0
   git push origin v26.10.0
   ```

`.github/workflows/release.yml` builds and tests at the tag, publishes the packages to nuget.org, and creates the
GitHub release with `docs/release-notes/<version>.md` and the packages attached. A version with a suffix
(`26.10.1-rc.1`) is published as a prerelease. A missing `<package>.txt` fails the release before anything is published;
a missing `<version>.md` makes GitHub generate the release notes.

To build and verify a release without publishing, run **Release** from the Actions tab with the version and
**Publish** off.

## Repository settings

| Setting | Purpose |
| --- | --- |
| Variable `NUGET_USER` | nuget.org user for trusted publishing; configure the repository and `release.yml` as a trusted publisher on nuget.org |
| Secret `NUGET_API_KEY` | API key, used when `NUGET_USER` is not set |
| Environment `nuget` | Publishing runs in it; add required reviewers to approve each release |
