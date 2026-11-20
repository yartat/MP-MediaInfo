# MP-MediaInfo (next release)

Version number not decided. Previous release: 26.10.0.

## Added

- Image streams: `ImageStream`, `ImageStreamBuilder`, `GainMapInfo` (ISO 21496-1), `ImageStreams` and `HasImages` on the analysis result and on `MediaInfoWrapper`. Image-only media is a successful analysis.
- `VideoStream.HdrInfo`: format, commercial name, version, profile, level, settings, compression, compatibility, HDR10 static metadata (`MasteringDisplayColorPrimaries`, `MasteringDisplayLuminance`, `MaxCll`, `MaxFall`) and `DolbyVisionInfo` (including `MasteringDisplayLuminanceHdr10`, `ShotCount`).
- `ImageStream.Mirror`, `ImageStream.DerivedFrom`, `VideoStream.ChromaSubsamplingPosition`, `VideoStream.Mirror`, `GeneralMediaInfo.CompliantWith`.
- `GeneralMediaInfo.Iptc` (`IptcInfo`): IPTC-IIM and XMP metadata of a file, `null` when there is none.
- `AudioStream.MuxingMode`; PS2 Audio is identified. New known extensions: `.ss2`, `.bw64`, `.cmfa`, `.cmfv`, `.hif`.
- `MediaStreamKind.Chapter`. Chapters are reported with this kind instead of `Menu`; existing values keep their numbers.
- HTTP: `http` and `https` locations are read with `HttpClient` ([guide](https://github.com/yartat/MP-MediaInfo/blob/master/docs/async-api.md#http)).
  - `MediaInfoAnalyzerBuilder.WithHttp`, `HttpAnalysisOptions`, `HttpAnalysisStrategy`.
  - Request and receive timeouts, cancellation, `Range` requests, redirect limit, Basic and Digest credentials.
  - `AllowInvalidCertificates` accepts a self-signed certificate.
- `MediaAnalysisOptions.MaximumAbandonedNativeCalls` and `AnalysisFailureReason.Busy`: past the limit of native calls that outlived a timeout, a new analysis fails at once.
- `MediaAnalysisOptions.MaximumStreamBytes`: limit for reading a stream.
- `MediaInfoAnalyzerBuilder.WithStrategy`: adds a strategy next to the default ones; `WithRtsp` and `WithHttp` use it.
- `RtspAnalysisOptions.AllowBasicAuthentication` (default `true`).
- `MediaInfo.LoadFailure`: why the native library could not be loaded (.NET Framework).
- `MediaAnalysisResult.Warnings` reports a library whose version or field layout differs from the expected one.

## Changed

- `http` and `https` locations are no longer handed to the native library. Set `HttpAnalysisOptions.Enabled = false` to restore that. A server without `Range` support is read from the start and `Size` is 0. `MediaInfoWrapper` is unchanged.
- The wrapper reads fields by name, so it works with the official library and with builds that add fields (`General.CompliantWith`, `Image.Rotation`, `Image.Format_Settings_SliceCount`).
- `CachingAnalyzer` returns a separate copy of the result to every caller. Concurrent analyses of the same file share one run; it is cancelled when every caller has given up.
- Decorator order: logging, validation, external subtitles, caching, timeout, retry, concurrency limit.
- A seekable stream is read up to four times its length plus 32 MB by default; a result cut short by the limit carries a warning.
- `ThrottlingAnalyzer` releases its slot when a timeout fires; the native call that is still running is counted (see `MaximumAbandonedNativeCalls`).
- Locations in results, failure messages, progress, logs and the RTSP stream name show `***@` in place of `user:password@`. `NetworkMediaSource.Location` is unchanged.
- Updated packages: CsvHelper 34, CommandLine 3, Mapster 10.0.14, Scalar 2.21.

## Fixed

- Languages and subtitle codecs were lost under cultures such as `tr-TR`.
- Timeout and cancellation: the caller is released at once; the native call finishes on its own thread and releases its handle.
- `RetryingAnalyzer` no longer repeats a stream source, and a long series of attempts cannot overflow the delay.
- A duration that does not fit a `TimeSpan` becomes zero instead of failing the analysis; a negative `Rotation` is kept.
- `CachingAnalyzer` no longer throws for a path the file system rejects; external subtitle files that appear later are noticed.
- Folder scans: discs are found at any depth and reported once, service files are not analyzed as media, depth is limited to 64, an unreadable folder is skipped, and symbolic links and junctions are not followed.
- The total size of a Blu-ray disc is no longer zero when a subfolder cannot be read.
- DVD and Blu-ray detection no longer takes a folder of discs for a disc.
- `FallbackDiscStructureReader` falls back to the folder reader when the preferred reader throws.
- Disc names that differ only in case no longer fail on case-sensitive file systems.
- IFO and MPLS files larger than 16 MiB are not read; IFO sector numbers cannot wrap around; a crafted IFO file cannot produce millions of chapters.
- Name patterns with `*` or `?` no longer widen the result.
- HDR detection handles lists such as `Dolby Vision / SMPTE ST 2086` and returns the most specific format.
- The process locale is set once, before any media is opened.
- `MediaInfoWrapper`: folder size is summed without recursion and without following links; a stream that cannot seek is not asked for its length.
- `MediaInfoLibAdapter.Close` keeps the handle.
- RTSP:
  - Response bodies over 1 MiB, more than 128 headers and headers over 64 KiB are refused.
  - Reads honour `ReceiveTimeout`; a timeout during a capture ends it and keeps what was received.
  - Oversized fragmented units are dropped.
  - Every `WWW-Authenticate` header is considered and Digest is preferred. Basic is declined once Digest was requested. Digest answers `qop=auth` correctly; unsupported challenges are declined.
  - PCMA and PCMU map to A-law and mu-law; H.265 streams with decoding order numbers are refused; `IsRtsp` checks only the start of a location.
  - A cancelled connection attempt reports the cancellation.
- .NET Framework: the native library is looked up next to the assembly first, and loaded by full path.

## Known limitations

- Analyzing a file cannot be cancelled while the native library opens it; a timeout releases the caller, and the call runs on until the library returns.
- Other protocols given to the native library and `MediaInfoWrapper` URLs have no timeouts.
- Linux: `MediaInfoWrapper` reports no streams for a Blu-ray folder, because the native library 26.10.0 cannot list the playlists of the disc there. Open the playlist (`.mpls`) instead. Fixed in the next native version.
