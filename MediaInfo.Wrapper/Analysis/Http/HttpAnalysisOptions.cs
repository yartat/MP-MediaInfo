#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Net;

namespace MediaInfo.Analysis.Http;

/// <summary>
/// Describes how an <c>http</c> or <c>https</c> location is read.
/// </summary>
/// <remarks>
/// The location is read by the analyzer itself, with <see cref="System.Net.Http.HttpClient"/>, and the bytes are handed
/// to the library block by block. The native library is not given the location, because its network code has no
/// timeouts: a server that never answers keeps a call, a thread and a connection for ever.
/// </remarks>
public sealed class HttpAnalysisOptions
{
  private TimeSpan _requestTimeout = TimeSpan.FromSeconds(15);
  private TimeSpan _receiveTimeout = TimeSpan.FromSeconds(15);
  private int _blockSize = 512 * 1024;
  private int _maximumRedirects = 3;

  /// <summary>
  /// Gets or sets a value indicating whether <c>http</c> and <c>https</c> locations are read by the analyzer.
  /// </summary>
  /// <remarks>
  /// When it is <see langword="false"/> the location is handed to the native library instead, which then has no
  /// timeouts (see <see cref="MediaAnalysisOptions.MaximumAbandonedNativeCalls"/>).
  /// </remarks>
  /// <value>The default value is <see langword="true"/>.</value>
  public bool Enabled { get; set; } = true;

  /// <summary>
  /// Gets or sets how long a request may take to connect and to answer with its headers.
  /// </summary>
  /// <value>The default value is 15 seconds.</value>
  /// <exception cref="ArgumentOutOfRangeException">The value is not greater than zero.</exception>
  public TimeSpan RequestTimeout
  {
    get => _requestTimeout;
    set => _requestTimeout = value > TimeSpan.Zero
      ? value
      : throw new ArgumentOutOfRangeException(nameof(value), value, "The timeout must be greater than zero.");
  }

  /// <summary>
  /// Gets or sets how long the server may stay silent while a body is being received.
  /// </summary>
  /// <value>The default value is 15 seconds.</value>
  /// <exception cref="ArgumentOutOfRangeException">The value is not greater than zero.</exception>
  public TimeSpan ReceiveTimeout
  {
    get => _receiveTimeout;
    set => _receiveTimeout = value > TimeSpan.Zero
      ? value
      : throw new ArgumentOutOfRangeException(nameof(value), value, "The timeout must be greater than zero.");
  }

  /// <summary>
  /// Gets or sets the size of the part of the media one request asks for.
  /// </summary>
  /// <remarks>
  /// The library asks for the parts of a media it needs, and a part that is not in the block that was read last costs
  /// one more request. A larger block means fewer requests and more bytes read that were not needed.
  /// </remarks>
  /// <value>The default value is 512 kilobytes.</value>
  /// <exception cref="ArgumentOutOfRangeException">The value is not greater than zero.</exception>
  public int BlockSize
  {
    get => _blockSize;
    set => _blockSize = value > 0
      ? value
      : throw new ArgumentOutOfRangeException(nameof(value), value, "A block must hold at least one byte.");
  }

  /// <summary>
  /// Gets or sets the greatest number of redirections that are followed.
  /// </summary>
  /// <value>The default value is 3.</value>
  /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
  public int MaximumRedirects
  {
    get => _maximumRedirects;
    set => _maximumRedirects = value >= 0
      ? value
      : throw new ArgumentOutOfRangeException(nameof(value), value, "The number of redirections cannot be negative.");
  }

  /// <summary>
  /// Gets or sets a value indicating whether a server certificate that does not validate is accepted.
  /// </summary>
  /// <remarks>
  /// For a server of your own with a self-signed certificate. It turns off the check that the server is the one the
  /// location names, so anyone between the two can read what is sent, including <see cref="Credentials"/>.
  /// </remarks>
  /// <value>The default value is <see langword="false"/>.</value>
  public bool AllowInvalidCertificates { get; set; }

  /// <summary>
  /// Gets or sets the credentials to answer a Basic or Digest challenge with.
  /// </summary>
  /// <remarks>
  /// When it is <see langword="null"/> the user name and password of the location are used, if it has any. They are
  /// offered to the server the location names and to no other, also after a redirection.
  /// </remarks>
  /// <value>The default value is <see langword="null"/>.</value>
  public NetworkCredential? Credentials { get; set; }

  /// <summary>
  /// Gets or sets the user agent the requests identify as.
  /// </summary>
  /// <value>The default value is <c>MP-MediaInfo</c>.</value>
  public string UserAgent { get; set; } = "MP-MediaInfo";
}
