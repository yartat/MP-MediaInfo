#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaInfo.Analysis.Http;
using MediaInfo.Analysis.Results;
using MediaInfo.Analysis.Sources;

namespace MediaInfo.Analysis.Strategies;

/// <summary>
/// Analyzes a media served over <c>http</c> or <c>https</c>, reading it with <see cref="HttpClient"/>.
/// </summary>
/// <remarks>
/// The native library is given bytes, never the location. Its network code has no timeouts, so a server that does not
/// answer would keep a native call, a thread and a connection for ever, whatever a timeout of the analyzer said. Here
/// every wait has a limit (<see cref="HttpAnalysisOptions"/>), a cancellation ends the request, and the server
/// certificate is checked by the platform.
/// <para>
/// A server that serves parts of the media is read in blocks, so the library can ask for any part first. A server that
/// does not is read from the start, as far as the library needs.
/// </para>
/// </remarks>
public sealed class HttpAnalysisStrategy : IMediaAnalysisStrategy
{
  private readonly HttpAnalysisOptions _options;
  private readonly IMediaAnalysisStrategy _streamStrategy;

  /// <summary>
  /// Initializes a new instance of the <see cref="HttpAnalysisStrategy"/> class.
  /// </summary>
  /// <param name="options">How a location is read, or <see langword="null"/> for the defaults.</param>
  public HttpAnalysisStrategy(HttpAnalysisOptions? options = null)
    : this(options, new StreamAnalysisStrategy())
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="HttpAnalysisStrategy"/> class that hands the stream it reads to the
  /// specified strategy.
  /// </summary>
  /// <param name="options">How a location is read, or <see langword="null"/> for the defaults.</param>
  /// <param name="streamStrategy">The strategy that reads the stream.</param>
  /// <exception cref="ArgumentNullException"><paramref name="streamStrategy"/> is <see langword="null"/>.</exception>
  public HttpAnalysisStrategy(HttpAnalysisOptions? options, IMediaAnalysisStrategy streamStrategy)
  {
    _options = options ?? new HttpAnalysisOptions();
    _streamStrategy = streamStrategy ?? throw new ArgumentNullException(nameof(streamStrategy));
  }

  /// <summary>
  /// Gets the order the strategy is considered in, which is before the strategy that hands a location to the library.
  /// </summary>
  public int Priority => 10;

  /// <inheritdoc />
  public bool CanHandle(IMediaSource source) =>
    _options.Enabled && source is NetworkMediaSource network && IsHttp(network.Location);

  /// <inheritdoc />
  public async Task<MediaAnalysisResult> AnalyzeAsync(
    IMediaSource source,
    MediaAnalysisContext context,
    CancellationToken cancellationToken)
  {
    var network = (NetworkMediaSource)source;

    // A location without a host parses as an absolute URI but names nothing to connect to.
    if (!Uri.TryCreate(network.Location, UriKind.Absolute, out var location) || string.IsNullOrEmpty(location.Host))
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.SourceNotSpecified,
        $"'{network.DisplayName}' is not a usable location.");
    }

    context.Report(new AnalysisProgress(AnalysisPhase.Opening, Detail: network.DisplayName));

    var credentials = _options.Credentials ?? CredentialsOf(location);
    var uri = new UriBuilder(location) { UserName = string.Empty, Password = string.Empty }.Uri;

    try
    {
      using var client = CreateClient(uri, credentials);
      var stream = await HttpMediaStream.OpenAsync(client, uri, _options, cancellationToken).ConfigureAwait(false);

      if (stream is { CanSeek: true, Length: 0 })
      {
        stream.Dispose();
        return MediaAnalysisResult.Failed(source, AnalysisFailureReason.SourceEmpty, $"'{network.DisplayName}' is empty.");
      }

      var result = await _streamStrategy
        .AnalyzeAsync(new StreamMediaSource(stream, leaveOpen: false, network.DisplayName), context, cancellationToken)
        .ConfigureAwait(false);

      context.Report(new AnalysisProgress(AnalysisPhase.Completed, Detail: network.DisplayName));
      return result with
      {
        SourceKind = MediaSourceKind.Network,
        SourcePath = network.DisplayName,
        AnalyzedPath = network.DisplayName
      };
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      throw;
    }
    catch (TimeoutException exception)
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.Timeout,
        $"Could not read '{network.DisplayName}': {exception.Message}",
        exception);
    }
    catch (HttpMediaException exception) when (exception.Status is HttpStatusCode.NotFound or HttpStatusCode.Gone)
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.SourceNotFound,
        $"Could not read '{network.DisplayName}': {exception.Message}",
        exception);
    }
    catch (Exception exception) when (exception is IOException or HttpRequestException)
    {
      return MediaAnalysisResult.Failed(
        source,
        AnalysisFailureReason.NativeOpenFailed,
        $"Could not read '{network.DisplayName}': {Describe(exception)}",
        exception);
    }
  }

  private static bool IsHttp(string? location) =>
    !string.IsNullOrEmpty(location) &&
    (location!.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
     location.StartsWith("https:", StringComparison.OrdinalIgnoreCase));

  // The message of an HttpRequestException is often only "An error occurred while sending the request", and what it
  // was is in the exception inside.
  private static string Describe(Exception exception) =>
    exception.InnerException is { Message.Length: > 0 } inner && inner.Message != exception.Message
      ? $"{exception.Message} {inner.Message}"
      : exception.Message;

  private HttpClient CreateClient(Uri uri, NetworkCredential? credentials)
  {
    var handler = new HttpClientHandler
    {
      AllowAutoRedirect = _options.MaximumRedirects > 0,
      MaxAutomaticRedirections = Math.Max(1, _options.MaximumRedirects),
      AutomaticDecompression = DecompressionMethods.None,
      UseCookies = false
    };

    if (_options.AllowInvalidCertificates)
    {
      handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    if (credentials is not null)
    {
      // Bound to the server the location names: a redirection to another one is not answered with the password.
      var server = new Uri(uri.GetLeftPart(UriPartial.Authority));
      var cache = new CredentialCache
      {
        { server, "Basic", credentials },
        { server, "Digest", credentials }
      };
      handler.Credentials = cache;
      handler.PreAuthenticate = true;
    }

    var client = new HttpClient(handler, disposeHandler: true) { Timeout = System.Threading.Timeout.InfiniteTimeSpan };
    client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
    return client;
  }

  private static NetworkCredential? CredentialsOf(Uri uri)
  {
    if (string.IsNullOrEmpty(uri.UserInfo))
    {
      return null;
    }

    var separator = uri.UserInfo.IndexOf(':');
    return separator < 0
      ? new NetworkCredential(Uri.UnescapeDataString(uri.UserInfo), string.Empty)
      : new NetworkCredential(
        Uri.UnescapeDataString(uri.UserInfo.Substring(0, separator)),
        Uri.UnescapeDataString(uri.UserInfo.Substring(separator + 1)));
  }
}
