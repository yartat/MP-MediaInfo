#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MediaInfo.Analysis.Http;

/// <summary>
/// Thrown when a server answers a request with something that cannot be read as a media.
/// </summary>
internal sealed class HttpMediaException : IOException
{
  public HttpMediaException(string message, HttpStatusCode? status = null)
    : base(message)
  {
    Status = status;
  }

  /// <summary>Gets the status the server answered with, when there was one.</summary>
  public HttpStatusCode? Status { get; }
}

/// <summary>
/// Opens an <c>http</c> or <c>https</c> location as a stream the analysis can read.
/// </summary>
/// <remarks>
/// A server that honours range requests gives a stream that can seek, which is read in blocks, so the library can ask
/// for the end of a file first, as an MP4 with its index last needs. A server that does not gives the body as it comes.
/// Every wait is limited, and what is waited for is never the native library.
/// </remarks>
internal static class HttpMediaStream
{
  public static async Task<Stream> OpenAsync(
    HttpClient client,
    Uri uri,
    HttpAnalysisOptions options,
    CancellationToken cancellationToken)
  {
    HttpResponseMessage? response = await SendAsync(client, uri, options, 0L, options.BlockSize - 1L, null, cancellationToken)
      .ConfigureAwait(false);
    try
    {
      switch ((int)response.StatusCode)
      {
        case 206 when response.Content.Headers.ContentRange is { From: 0, Length: { } total }:
          var stream = new HttpRangeStream(client, uri, options, total, ValidatorOf(response));
          await stream.PrimeAsync(response, cancellationToken).ConfigureAwait(false);
          return stream;

        case 206:
          // A range of a media of unknown length: the part that came is of no use, and the rest is read as it comes.
          response.Dispose();
          response = await SendAsync(client, uri, options, null, null, null, cancellationToken).ConfigureAwait(false);
          return await OpenBodyAsync(HandOver(ref response), options, cancellationToken).ConfigureAwait(false);

        case 200:
          return await OpenBodyAsync(HandOver(ref response), options, cancellationToken).ConfigureAwait(false);

        case 416 when response.Content.Headers.ContentRange?.Length is 0:
          return new HttpRangeStream(client, uri, options, 0L, null);

        case 404:
        case 410:
          throw new HttpMediaException($"The server answered {(int)response.StatusCode} {response.ReasonPhrase}.", response.StatusCode);

        default:
          throw new HttpMediaException($"The server answered {(int)response.StatusCode} {response.ReasonPhrase}.", response.StatusCode);
      }
    }
    finally
    {
      // A response that went into a stream belongs to the stream; what is left here is released.
      response?.Dispose();
    }
  }

  private static HttpResponseMessage HandOver(ref HttpResponseMessage? response)
  {
    var owned = response!;
    response = null;
    return owned;
  }

  internal static async Task<HttpResponseMessage> SendAsync(
    HttpClient client,
    Uri uri,
    HttpAnalysisOptions options,
    long? from,
    long? to,
    string? ifRange,
    CancellationToken cancellationToken)
  {
    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    timeout.CancelAfter(options.RequestTimeout);

    var request = new HttpRequestMessage(HttpMethod.Get, uri);
    request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("identity"));
    if (from is not null)
    {
      request.Headers.Range = new RangeHeaderValue(from, to);
    }

    if (ifRange is not null)
    {
      request.Headers.TryAddWithoutValidation("If-Range", ifRange);
    }

    try
    {
      return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
      throw new TimeoutException($"The server did not answer within {options.RequestTimeout}.");
    }
  }

  // The resource is asked for in several requests, and an answer to a later one that belongs to a changed resource
  // would be mixed into the first. A validator makes the server say so instead.
  private static string? ValidatorOf(HttpResponseMessage response)
  {
    if (response.Headers.ETag is { IsWeak: false } tag)
    {
      return tag.ToString();
    }

    return response.Content.Headers.LastModified?.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
  }

  private static async Task<Stream> OpenBodyAsync(
    HttpResponseMessage response,
    HttpAnalysisOptions options,
    CancellationToken cancellationToken)
  {
    try
    {
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpMediaException($"The server answered {(int)response.StatusCode} {response.ReasonPhrase}.", response.StatusCode);
      }

      var body = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
      cancellationToken.ThrowIfCancellationRequested();
      return new HttpBodyStream(response, body, options.ReceiveTimeout);
    }
    catch
    {
      response.Dispose();
      throw;
    }
  }

  internal static async Task<int> ReadAsync(
    Stream source,
    Memory<byte> destination,
    TimeSpan timeout,
    CancellationToken cancellationToken)
  {
    using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    limit.CancelAfter(timeout);
    try
    {
      return await source.ReadAsync(destination, limit.Token).ConfigureAwait(false);
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
      throw new TimeoutException($"The server sent nothing for {timeout}.");
    }
  }
}

/// <summary>
/// The body of a response that is read as it comes, because the server does not serve parts of the media.
/// </summary>
internal sealed class HttpBodyStream : Stream
{
  private readonly HttpResponseMessage _response;
  private readonly Stream _body;
  private readonly TimeSpan _receiveTimeout;

  public HttpBodyStream(HttpResponseMessage response, Stream body, TimeSpan receiveTimeout)
  {
    _response = response;
    _body = body;
    _receiveTimeout = receiveTimeout;
  }

  public override bool CanRead => true;

  public override bool CanSeek => false;

  public override bool CanWrite => false;

  public override long Length => throw new NotSupportedException();

  public override long Position
  {
    get => throw new NotSupportedException();
    set => throw new NotSupportedException();
  }

  public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
    new(HttpMediaStream.ReadAsync(_body, buffer, _receiveTimeout, cancellationToken));

  // The analysis reads asynchronously. This is for a caller that does not, and waits for the same limited read.
  public override int Read(byte[] buffer, int offset, int count) =>
    ReadAsync(buffer.AsMemory(offset, count), CancellationToken.None).AsTask().GetAwaiter().GetResult();

  public override void Flush()
  {
  }

  public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

  public override void SetLength(long value) => throw new NotSupportedException();

  public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      _body.Dispose();
      _response.Dispose();
    }

    base.Dispose(disposing);
  }
}

/// <summary>
/// A media on a server that serves parts of it, read one block at a time.
/// </summary>
internal sealed class HttpRangeStream : Stream
{
  private readonly HttpClient _client;
  private readonly Uri _uri;
  private readonly HttpAnalysisOptions _options;
  private readonly long _length;
  private readonly string? _validator;

  private byte[]? _block;
  private long _blockStart;
  private int _blockLength;
  private long _position;

  public HttpRangeStream(HttpClient client, Uri uri, HttpAnalysisOptions options, long length, string? validator)
  {
    _client = client;
    _uri = uri;
    _options = options;
    _length = length;
    _validator = validator;
  }

  public override bool CanRead => true;

  public override bool CanSeek => true;

  public override bool CanWrite => false;

  public override long Length => _length;

  public override long Position
  {
    get => _position;
    set => _position = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
  }

  /// <summary>Takes the first block from the response that told the length.</summary>
  public Task PrimeAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
    FillAsync(response, 0L, Math.Min(_options.BlockSize, _length), cancellationToken);

  public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
  {
    if (buffer.IsEmpty || _position >= _length)
    {
      return 0;
    }

    if (_block is null || _position < _blockStart || _position >= _blockStart + _blockLength)
    {
      await LoadAsync(_position, cancellationToken).ConfigureAwait(false);
    }

    var offset = (int)(_position - _blockStart);
    var count = Math.Min(buffer.Length, _blockLength - offset);
    _block.AsMemory(offset, count).CopyTo(buffer);
    _position += count;
    return count;
  }

  // The analysis reads asynchronously. This is for a caller that does not, and waits for the same limited read.
  public override int Read(byte[] buffer, int offset, int count) =>
    ReadAsync(buffer.AsMemory(offset, count), CancellationToken.None).AsTask().GetAwaiter().GetResult();

  public override long Seek(long offset, SeekOrigin origin)
  {
    var target = origin switch
    {
      SeekOrigin.Begin => offset,
      SeekOrigin.Current => _position + offset,
      _ => _length + offset
    };

    return target >= 0 ? _position = target : throw new IOException("An attempt was made to move before the start of the media.");
  }

  public override void Flush()
  {
  }

  public override void SetLength(long value) => throw new NotSupportedException();

  public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

  private async Task LoadAsync(long start, CancellationToken cancellationToken)
  {
    var end = Math.Min(start + _options.BlockSize, _length) - 1;
    using var response = await HttpMediaStream
      .SendAsync(_client, _uri, _options, start, end, _validator, cancellationToken)
      .ConfigureAwait(false);

    if ((int)response.StatusCode != 206 || response.Content.Headers.ContentRange is not { } range || range.From != start)
    {
      // A server that answers an If-Range with the whole body is saying that the media is not the one that was read.
      throw new HttpMediaException(
        (int)response.StatusCode == 200
          ? "The media changed while it was being read."
          : $"The server answered a request for a part of the media with {(int)response.StatusCode} {response.ReasonPhrase}.",
        response.StatusCode);
    }

    await FillAsync(response, start, end - start + 1, cancellationToken).ConfigureAwait(false);
  }

  private async Task FillAsync(HttpResponseMessage response, long start, long count, CancellationToken cancellationToken)
  {
    _block ??= new byte[_options.BlockSize];
    var body = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

    var filled = 0;
    while (filled < count)
    {
      var read = await HttpMediaStream
        .ReadAsync(body, _block.AsMemory(filled, (int)count - filled), _options.ReceiveTimeout, cancellationToken)
        .ConfigureAwait(false);
      if (read == 0)
      {
        break;
      }

      filled += read;
    }

    if (filled == 0)
    {
      throw new HttpMediaException("The server ended the media before the length it announced.");
    }

    _blockStart = start;
    _blockLength = filled;
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      _block = null;
    }

    base.Dispose(disposing);
  }
}
