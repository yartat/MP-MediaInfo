#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaInfo.Analysis.Tests.Fakes;

/// <summary>What a test server does with a request for a path.</summary>
public enum HttpBehaviour
{
  /// <summary>Answers as a server that serves parts of the resource does.</summary>
  Normal,

  /// <summary>Takes the request and never answers it.</summary>
  Silent,

  /// <summary>Sends the headers and the first bytes of the body, then nothing.</summary>
  StallBody
}

/// <summary>One path a test server serves.</summary>
public sealed class HttpResource
{
  /// <summary>Gets or sets the content.</summary>
  public byte[] Data { get; set; } = [];

  /// <summary>Gets or sets the entity tag, with its quotes.</summary>
  public string ETag { get; set; } = "\"v1\"";

  /// <summary>Gets or sets a value indicating whether a range request is answered with the whole body.</summary>
  public bool IgnoreRange { get; set; }

  /// <summary>Gets or sets what the server does with a request.</summary>
  public HttpBehaviour Behaviour { get; set; }

  /// <summary>Gets or sets the location to redirect to, when it is not served.</summary>
  public string? RedirectTo { get; set; }

  /// <summary>Gets or sets the status to answer with instead of serving the content.</summary>
  public int? Status { get; set; }

  /// <summary>Gets or sets the user name and password a request has to carry as Basic credentials.</summary>
  public (string User, string Password)? Basic { get; set; }
}

/// <summary>A request a test server received.</summary>
/// <param name="Path">The path.</param>
/// <param name="Range">The <c>Range</c> header.</param>
/// <param name="IfRange">The <c>If-Range</c> header.</param>
/// <param name="Authorization">The <c>Authorization</c> header.</param>
public sealed record HttpRequestRecord(string Path, string? Range, string? IfRange, string? Authorization);

/// <summary>
/// A small HTTP server on the loopback interface, for the tests of the code that reads a media from one.
/// </summary>
public sealed class TestHttpServer : IDisposable
{
  private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
  private readonly X509Certificate2? _certificate;
  private readonly CancellationTokenSource _stop = new();
  private readonly Task _accepting;

  /// <summary>Initializes a new instance of the <see cref="TestHttpServer"/> class.</summary>
  /// <param name="tls">Whether the server speaks TLS, with a certificate that nothing trusts.</param>
  public TestHttpServer(bool tls = false)
  {
    _listener.Start();
    Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
    Scheme = tls ? "https" : "http";
    _certificate = tls ? CreateCertificate() : null;
    _accepting = Task.Run(AcceptAsync);
  }

  /// <summary>Gets the port.</summary>
  public int Port { get; }

  /// <summary>Gets the scheme.</summary>
  public string Scheme { get; }

  /// <summary>Gets the paths the server serves.</summary>
  public ConcurrentDictionary<string, HttpResource> Resources { get; } = new();

  /// <summary>Gets the requests the server received, in the order they came.</summary>
  public ConcurrentQueue<HttpRequestRecord> Requests { get; } = new();

  /// <summary>Gets the number of bytes of body the server sent.</summary>
  public long BytesSent => Interlocked.Read(ref _bytesSent);

  private long _bytesSent;

  /// <summary>Gets the location of a path.</summary>
  /// <param name="path">The path, with its leading slash.</param>
  /// <returns>Returns the location.</returns>
  public string Url(string path) => $"{Scheme}://127.0.0.1:{Port}{path}";

  /// <summary>Adds a path.</summary>
  /// <param name="path">The path, with its leading slash.</param>
  /// <param name="data">The content.</param>
  /// <returns>Returns the resource, to change what the server does with it.</returns>
  public HttpResource Add(string path, byte[] data) => Resources[path] = new HttpResource { Data = data };

  /// <inheritdoc />
  public void Dispose()
  {
    _stop.Cancel();
    _listener.Stop();
    try
    {
      _accepting.Wait(TimeSpan.FromSeconds(2));
    }
    catch (AggregateException)
    {
    }

    _certificate?.Dispose();
    _stop.Dispose();
  }

  private static X509Certificate2 CreateCertificate()
  {
    using var key = RSA.Create(2048);
    var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    var names = new SubjectAlternativeNameBuilder();
    names.AddIpAddress(IPAddress.Loopback);
    names.AddDnsName("localhost");
    request.CertificateExtensions.Add(names.Build());
    using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));

    // A key that exists only in memory cannot be used by the TLS stack of Windows; one that went through a file can.
    return X509CertificateLoader.LoadPkcs12(certificate.Export(X509ContentType.Pfx), null, X509KeyStorageFlags.Exportable);
  }

  private async Task AcceptAsync()
  {
    while (!_stop.IsCancellationRequested)
    {
      TcpClient client;
      try
      {
        client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
      }
      catch (Exception exception) when (exception is ObjectDisposedException or SocketException or InvalidOperationException)
      {
        return;
      }

      _ = Task.Run(() => ServeAsync(client));
    }
  }

  private async Task ServeAsync(TcpClient client)
  {
    using (client)
    {
      try
      {
        Stream stream = client.GetStream();
        if (_certificate is not null)
        {
          var secure = new SslStream(stream);
          await secure.AuthenticateAsServerAsync(new SslServerAuthenticationOptions { ServerCertificate = _certificate }, _stop.Token)
            .ConfigureAwait(false);
          stream = secure;
        }

        var head = await ReadHeadAsync(stream).ConfigureAwait(false);
        if (head is null)
        {
          return;
        }

        await AnswerAsync(stream, head).ConfigureAwait(false);
      }
      catch (Exception exception) when (exception is IOException or SocketException or ObjectDisposedException
        or System.Security.Authentication.AuthenticationException or OperationCanceledException)
      {
        // A client that gave up, or one that does not trust the certificate, ends the exchange.
      }
    }
  }

  private static async Task<Dictionary<string, string>?> ReadHeadAsync(Stream stream)
  {
    var bytes = new List<byte>();
    var one = new byte[1];
    while (await stream.ReadAsync(one, 0, 1).ConfigureAwait(false) == 1)
    {
      bytes.Add(one[0]);
      var count = bytes.Count;
      if (count >= 4 && bytes[count - 4] == '\r' && bytes[count - 3] == '\n' && bytes[count - 2] == '\r' && bytes[count - 1] == '\n')
      {
        var lines = Encoding.ASCII.GetString([.. bytes]).Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries);
        var head = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { [":request"] = lines[0] };
        foreach (var line in lines[1..])
        {
          var colon = line.IndexOf(':');
          if (colon > 0)
          {
            head[line.Substring(0, colon).Trim()] = line.Substring(colon + 1).Trim();
          }
        }

        return head;
      }
    }

    return null;
  }

  private async Task AnswerAsync(Stream stream, Dictionary<string, string> head)
  {
    var path = head[":request"].Split(' ')[1];
    head.TryGetValue("Range", out var range);
    head.TryGetValue("If-Range", out var ifRange);
    head.TryGetValue("Authorization", out var authorization);
    Requests.Enqueue(new HttpRequestRecord(path, range, ifRange, authorization));

    if (!Resources.TryGetValue(path, out var resource))
    {
      await WriteAsync(stream, 404, "Not Found", [], []).ConfigureAwait(false);
      return;
    }

    if (resource.Behaviour == HttpBehaviour.Silent)
    {
      await Task.Delay(Timeout.Infinite, _stop.Token).ConfigureAwait(false);
    }

    if (resource.Basic is { } basic &&
        authorization != "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{basic.User}:{basic.Password}")))
    {
      await WriteAsync(stream, 401, "Unauthorized", [("WWW-Authenticate", "Basic realm=\"test\"")], []).ConfigureAwait(false);
      return;
    }

    if (resource.RedirectTo is not null)
    {
      await WriteAsync(stream, 302, "Found", [("Location", resource.RedirectTo)], []).ConfigureAwait(false);
      return;
    }

    if (resource.Status is { } status)
    {
      await WriteAsync(stream, status, "Status", [], []).ConfigureAwait(false);
      return;
    }

    var data = resource.Data;
    var headers = new List<(string, string)> { ("ETag", resource.ETag), ("Accept-Ranges", "bytes") };

    // A validator that is not the current one means the client asks about a resource that has changed.
    var rangeApplies = range is not null && !resource.IgnoreRange && (ifRange is null || ifRange == resource.ETag);
    if (rangeApplies && range!.StartsWith("bytes=", StringComparison.Ordinal))
    {
      var bounds = range.Substring(6).Split('-');
      var from = long.Parse(bounds[0], CultureInfo.InvariantCulture);
      if (from >= data.Length)
      {
        await WriteAsync(stream, 416, "Range Not Satisfiable", [("Content-Range", $"bytes */{data.Length}")], []).ConfigureAwait(false);
        return;
      }

      var to = bounds[1].Length == 0 ? data.Length - 1 : Math.Min(long.Parse(bounds[1], CultureInfo.InvariantCulture), data.Length - 1);
      headers.Add(("Content-Range", $"bytes {from}-{to}/{data.Length}"));
      await WriteAsync(stream, 206, "Partial Content", headers, data.AsMemory((int)from, (int)(to - from + 1)).ToArray(), resource.Behaviour)
        .ConfigureAwait(false);
      return;
    }

    await WriteAsync(stream, 200, "OK", headers, data, resource.Behaviour).ConfigureAwait(false);
  }

  private async Task WriteAsync(
    Stream stream,
    int status,
    string reason,
    IEnumerable<(string Name, string Value)> headers,
    byte[] body,
    HttpBehaviour behaviour = HttpBehaviour.Normal)
  {
    var head = new StringBuilder($"HTTP/1.1 {status} {reason}\r\n");
    foreach (var (name, value) in headers)
    {
      head.Append(name).Append(": ").Append(value).Append("\r\n");
    }

    head.Append("Content-Length: ").Append(body.Length.ToString(CultureInfo.InvariantCulture)).Append("\r\nConnection: close\r\n\r\n");
    var bytes = Encoding.ASCII.GetBytes(head.ToString());
    await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);

    if (behaviour == HttpBehaviour.StallBody)
    {
      var some = Math.Min(10, body.Length);
      await stream.WriteAsync(body, 0, some).ConfigureAwait(false);
      Interlocked.Add(ref _bytesSent, some);
      await stream.FlushAsync().ConfigureAwait(false);
      await Task.Delay(Timeout.Infinite, _stop.Token).ConfigureAwait(false);
      return;
    }

    await stream.WriteAsync(body, 0, body.Length).ConfigureAwait(false);
    Interlocked.Add(ref _bytesSent, body.Length);
    await stream.FlushAsync().ConfigureAwait(false);
  }
}
