#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaInfo.Analysis.Rtsp.Rtsp;

/// <summary>
/// The answer a server gave to one request.
/// </summary>
/// <param name="StatusCode">The status code, where 200 means the request succeeded.</param>
/// <param name="ReasonPhrase">The text the server put next to the status code.</param>
/// <param name="Headers">The headers, compared without regard to case.</param>
/// <param name="Body">The body, which is the session description for a DESCRIBE.</param>
internal sealed record RtspResponse(
  int StatusCode,
  string ReasonPhrase,
  IReadOnlyDictionary<string, string> Headers,
  string Body)
{
  /// <summary>Gets a value indicating whether the request succeeded.</summary>
  public bool IsSuccess => StatusCode is >= 200 and < 300;

  /// <summary>Gets a header, or <see langword="null"/> when the server did not send it.</summary>
  public string? Header(string name) => Headers.TryGetValue(name, out var value) ? value : null;
}

/// <summary>
/// One block of data the server sent between responses, on a channel that SETUP assigned.
/// </summary>
/// <param name="Channel">The channel the block arrived on, where the even one carries media and the odd one control.</param>
/// <param name="Data">The block, which is an RTP or RTCP packet.</param>
internal readonly record struct InterleavedBlock(byte Channel, byte[] Data);

/// <summary>
/// Speaks RTSP over a TCP connection, and reads the media the server interleaves onto it.
/// </summary>
/// <remarks>
/// Only what a probe needs is implemented: describing a session, setting one track up, playing it and tearing it
/// down. The media is carried on the same connection as the requests, which every server supports and which needs no
/// second port opened through whatever sits between the two ends.
/// </remarks>
internal sealed class RtspConnection : IDisposable
{
  private const int MaximumBlockSize = 0xFFFF;

  // The only body this client reads is a session description, which is a few kilobytes at most. A server that announces
  // more is either broken or trying to make the client allocate whatever it asks for.
  internal const int MaximumBodySize = 1024 * 1024;

  // A response is a status line and a handful of headers. A server that sends far more is not speaking RTSP.
  private const int MaximumHeaderCount = 128;
  private const int MaximumHeaderBytes = 64 * 1024;

  // One read is limited by the receive timeout. A server could still answer one byte at a time, each just in time, so
  // the whole response gets a budget too: it needs one round trip, and a few receive timeouts are generous for that.
  private const int ResponseBudgetInReceiveTimeouts = 4;

  private readonly Uri _uri;
  private readonly RtspAuthenticator _authenticator;
  private readonly string _userAgent;
  private readonly TimeSpan _receiveTimeout;

  private readonly byte[] _buffer = new byte[8192];
  private int _bufferStart;
  private int _bufferEnd;
  private bool _timedOut;

  private TcpClient? _client;
  private NetworkStream? _stream;
  private int _sequence;
  private string? _session;

  /// <summary>
  /// Initializes a new instance of the <see cref="RtspConnection"/> class.
  /// </summary>
  /// <param name="uri">The location of the stream.</param>
  /// <param name="credentials">The credentials to answer a challenge with, when the server asks for one.</param>
  /// <param name="userAgent">The user agent to identify as.</param>
  /// <param name="receiveTimeout">How long a single read may take before the connection is considered dead.</param>
  public RtspConnection(Uri uri, NetworkCredential? credentials, string userAgent, TimeSpan receiveTimeout)
  {
    _uri = uri ?? throw new ArgumentNullException(nameof(uri));
    _authenticator = new RtspAuthenticator(credentials);
    _userAgent = userAgent;
    _receiveTimeout = receiveTimeout;
  }

  /// <summary>Gets the location the requests are addressed to, without any credentials it was given with.</summary>
  public string ControlUri { get; private set; } = string.Empty;

  /// <summary>
  /// Opens the connection.
  /// </summary>
  /// <param name="connectTimeout">How long the connection may take to open.</param>
  /// <param name="cancellationToken">The token that abandons the attempt.</param>
  public async Task ConnectAsync(TimeSpan connectTimeout, CancellationToken cancellationToken)
  {
    ControlUri = new UriBuilder(_uri) { UserName = string.Empty, Password = string.Empty }.Uri.ToString();

    _client = new TcpClient { NoDelay = true };

    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    timeout.CancelAfter(connectTimeout);

    var port = _uri.IsDefaultPort || _uri.Port <= 0 ? 554 : _uri.Port;
    var connect = _client.ConnectAsync(_uri.Host, port);
    var completed = await Task.WhenAny(connect, Task.Delay(Timeout.Infinite, timeout.Token)).ConfigureAwait(false);

    if (completed != connect)
    {
      cancellationToken.ThrowIfCancellationRequested();
      throw new RtspProtocolException($"Timed out connecting to {_uri.Host}:{port}.");
    }

    await connect.ConfigureAwait(false);

    _stream = _client.GetStream();
    _client.ReceiveTimeout = (int)_receiveTimeout.TotalMilliseconds;
  }

  /// <summary>
  /// Sends one request and reads the answer, retrying once when the server asks to be authenticated.
  /// </summary>
  /// <param name="method">The method of the request.</param>
  /// <param name="uri">The URI of the request, which is the session or a track within it.</param>
  /// <param name="headers">The headers to send beyond the ones every request carries.</param>
  /// <param name="cancellationToken">The token that abandons the request.</param>
  /// <returns>Returns the answer.</returns>
  public async Task<RtspResponse> SendAsync(
    string method,
    string uri,
    IReadOnlyList<KeyValuePair<string, string>>? headers,
    CancellationToken cancellationToken)
  {
    var response = await SendOnceAsync(method, uri, headers, cancellationToken).ConfigureAwait(false);

    if (response.StatusCode == 401 && _authenticator.AcceptChallenge(response.Header("WWW-Authenticate")))
    {
      response = await SendOnceAsync(method, uri, headers, cancellationToken).ConfigureAwait(false);
    }

    if (response.Header("Session") is { Length: > 0 } session)
    {
      // The session identifier may arrive with parameters such as a timeout; only the identifier is echoed back.
      var separator = session.IndexOf(';');
      _session = separator < 0 ? session.Trim() : session.Substring(0, separator).Trim();
    }

    return response;
  }

  /// <summary>
  /// Reads the next block the server interleaved onto the connection.
  /// </summary>
  /// <remarks>
  /// A server is allowed to send a response in the middle of the media, so anything that is not a block is read and
  /// discarded rather than being mistaken for media.
  /// </remarks>
  /// <param name="cancellationToken">The token that abandons the read.</param>
  /// <returns>Returns the block, or <see langword="null"/> when the server closed the connection.</returns>
  public async Task<InterleavedBlock?> ReadBlockAsync(CancellationToken cancellationToken)
  {
    while (true)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (!await FillAsync(1, cancellationToken).ConfigureAwait(false))
      {
        return null;
      }

      if (_buffer[_bufferStart] != (byte)'$')
      {
        // Not media: read it as a response and carry on waiting.
        var response = await ReadResponseAsync(cancellationToken).ConfigureAwait(false);
        if (response is null)
        {
          return null;
        }

        continue;
      }

      if (!await FillAsync(4, cancellationToken).ConfigureAwait(false))
      {
        return null;
      }

      var channel = _buffer[_bufferStart + 1];
      var length = (_buffer[_bufferStart + 2] << 8) | _buffer[_bufferStart + 3];
      _bufferStart += 4;

      if (length is <= 0 or > MaximumBlockSize)
      {
        return null;
      }

      var data = new byte[length];
      if (!await ReadExactlyAsync(data, cancellationToken).ConfigureAwait(false))
      {
        return null;
      }

      return new InterleavedBlock(channel, data);
    }
  }

  /// <inheritdoc />
  public void Dispose()
  {
    _stream?.Dispose();
    _client?.Dispose();
    _stream = null;
    _client = null;
  }

  private async Task<RtspResponse> SendOnceAsync(
    string method,
    string uri,
    IReadOnlyList<KeyValuePair<string, string>>? headers,
    CancellationToken cancellationToken)
  {
    var request = new StringBuilder();
    request.Append(method).Append(' ').Append(uri).Append(" RTSP/1.0\r\n");
    request.Append("CSeq: ").Append(++_sequence).Append("\r\n");
    request.Append("User-Agent: ").Append(_userAgent).Append("\r\n");

    if (_session is not null)
    {
      request.Append("Session: ").Append(_session).Append("\r\n");
    }

    if (_authenticator.CanAuthenticate && _authenticator.CreateAuthorization(method, uri) is { } authorization)
    {
      request.Append("Authorization: ").Append(authorization).Append("\r\n");
    }

    if (headers is not null)
    {
      foreach (var header in headers)
      {
        request.Append(header.Key).Append(": ").Append(header.Value).Append("\r\n");
      }
    }

    request.Append("\r\n");

    var bytes = Encoding.UTF8.GetBytes(request.ToString());
    var stream = _stream ?? throw new RtspProtocolException("The connection is not open.");
    await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

    using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    if (_receiveTimeout > TimeSpan.Zero)
    {
      budget.CancelAfter(TimeSpan.FromTicks(_receiveTimeout.Ticks * ResponseBudgetInReceiveTimeouts));
    }

    _timedOut = false;
    RtspResponse? response;
    try
    {
      response = await ReadResponseAsync(budget.Token).ConfigureAwait(false);
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
      throw new RtspProtocolException($"The server did not finish answering {method} in time.");
    }

    return response ?? throw new RtspProtocolException(
      _timedOut
        ? $"The server did not answer {method} within {_receiveTimeout}."
        : $"The server closed the connection while answering {method}.");
  }

  private async Task<RtspResponse?> ReadResponseAsync(CancellationToken cancellationToken)
  {
    var statusLine = await ReadLineAsync(cancellationToken).ConfigureAwait(false);
    if (statusLine is null)
    {
      return null;
    }

    var parts = statusLine.Split([' '], 3);
    var statusCode = parts.Length > 1 &&
      int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var code) ? code : 0;

    var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    var headerCount = 0;
    var headerBytes = statusLine.Length;
    while (true)
    {
      var line = await ReadLineAsync(cancellationToken).ConfigureAwait(false);
      if (line is null)
      {
        return null;
      }

      if (line.Length == 0)
      {
        break;
      }

      headerBytes += line.Length + 2;
      if (++headerCount > MaximumHeaderCount || headerBytes > MaximumHeaderBytes)
      {
        throw new RtspProtocolException("The server sent more header data than any server should.");
      }

      var separator = line.IndexOf(':');
      if (separator > 0)
      {
        headers[line.Substring(0, separator).Trim()] = line.Substring(separator + 1).Trim();
      }
    }

    var body = string.Empty;
    if (headers.TryGetValue("Content-Length", out var lengthText) && ParseContentLength(lengthText) is var length and > 0)
    {
      var content = new byte[length];
      if (!await ReadExactlyAsync(content, cancellationToken).ConfigureAwait(false))
      {
        return null;
      }

      body = Encoding.UTF8.GetString(content);
    }

    return new RtspResponse(statusCode, parts.Length > 2 ? parts[2] : string.Empty, headers, body);
  }

  internal static int ParseContentLength(string text)
  {
    var trimmed = text.Trim();
    foreach (var character in trimmed)
    {
      // Anything that is not a plain count is treated as no body, as before.
      if (character < '0' || character > '9')
      {
        return 0;
      }
    }

    if (trimmed.Length == 0)
    {
      return 0;
    }

    if (!int.TryParse(trimmed, NumberStyles.None, CultureInfo.InvariantCulture, out var length) || length > MaximumBodySize)
    {
      throw new RtspProtocolException(
        $"The server announced a response body larger than the {MaximumBodySize} bytes this client accepts.");
    }

    return length;
  }

  private async Task<string?> ReadLineAsync(CancellationToken cancellationToken)
  {
    var line = new StringBuilder();

    while (true)
    {
      if (!await FillAsync(1, cancellationToken).ConfigureAwait(false))
      {
        // A line that was cut short by a timeout is not a line the server finished.
        return line.Length > 0 && !_timedOut ? line.ToString() : null;
      }

      var value = _buffer[_bufferStart++];
      if (value == (byte)'\n')
      {
        return line.ToString().TrimEnd('\r');
      }

      line.Append((char)value);

      if (line.Length > MaximumBlockSize)
      {
        throw new RtspProtocolException("The server sent a header line that is far longer than any server should.");
      }
    }
  }

  private async Task<bool> ReadExactlyAsync(byte[] destination, CancellationToken cancellationToken)
  {
    var written = 0;
    while (written < destination.Length)
    {
      if (!await FillAsync(1, cancellationToken).ConfigureAwait(false))
      {
        return false;
      }

      var available = Math.Min(_bufferEnd - _bufferStart, destination.Length - written);
      Buffer.BlockCopy(_buffer, _bufferStart, destination, written, available);
      _bufferStart += available;
      written += available;
    }

    return true;
  }

  private async Task<bool> FillAsync(int required, CancellationToken cancellationToken)
  {
    while (_bufferEnd - _bufferStart < required)
    {
      if (_bufferStart > 0)
      {
        Buffer.BlockCopy(_buffer, _bufferStart, _buffer, 0, _bufferEnd - _bufferStart);
        _bufferEnd -= _bufferStart;
        _bufferStart = 0;
      }

      if (_bufferEnd == _buffer.Length)
      {
        throw new RtspProtocolException("The server sent more than the connection buffer can hold at once.");
      }

      var stream = _stream ?? throw new RtspProtocolException("The connection is not open.");
      // The receive timeout of the socket does not apply to asynchronous reads, so a read is limited here.
      using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      if (_receiveTimeout > TimeSpan.Zero)
      {
        timeout.CancelAfter(_receiveTimeout);
      }

      int read;
      try
      {
        read = await stream
          .ReadAsync(_buffer, _bufferEnd, _buffer.Length - _bufferEnd, timeout.Token)
          .ConfigureAwait(false);
      }
      catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
      {
        // The server said nothing for as long as the receive timeout allows. While media is being captured this ends
        // the capture like a closed connection does, and while a response is awaited it is reported as a timeout.
        _timedOut = true;
        return false;
      }

      if (read <= 0)
      {
        return false;
      }

      _bufferEnd += read;
    }

    return true;
  }
}

/// <summary>
/// Thrown when a server does not speak the protocol the way it has to be spoken.
/// </summary>
public sealed class RtspProtocolException : IOException
{
  /// <summary>Initializes a new instance of the <see cref="RtspProtocolException"/> class.</summary>
  public RtspProtocolException()
  {
  }

  /// <summary>Initializes a new instance of the <see cref="RtspProtocolException"/> class.</summary>
  /// <param name="message">A description of what the server did.</param>
  public RtspProtocolException(string message)
    : base(message)
  {
  }

  /// <summary>Initializes a new instance of the <see cref="RtspProtocolException"/> class.</summary>
  /// <param name="message">A description of what the server did.</param>
  /// <param name="innerException">The exception that caused this one.</param>
  public RtspProtocolException(string message, Exception innerException)
    : base(message, innerException)
  {
  }
}
