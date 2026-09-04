#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace MediaInfo.Analysis.Rtsp.Rtsp;

/// <summary>
/// Answers the authentication challenge of an RTSP server.
/// </summary>
/// <remarks>
/// Both schemes RFC 2326 allows are supported. Basic sends the credentials encoded but not hashed, and digest hashes
/// them with the nonce the server chose.
/// <para>
/// Digest is specified over MD5, so MD5 is what is used here. That is a property of the protocol rather than a choice
/// about how to protect a secret: a camera that asks for digest will not accept anything else. The credentials never
/// leave the process in any other form, and an installation that cares should be reached over a tunnel rather than
/// trusting the scheme.
/// </para>
/// </remarks>
internal sealed class RtspAuthenticator
{
  private readonly NetworkCredential? _credentials;

  private string? _scheme;
  private string? _realm;
  private string? _nonce;
  private int _nonceCount;

  /// <summary>
  /// Initializes a new instance of the <see cref="RtspAuthenticator"/> class.
  /// </summary>
  /// <param name="credentials">The credentials to answer a challenge with, or <see langword="null"/> when there are none.</param>
  public RtspAuthenticator(NetworkCredential? credentials)
  {
    _credentials = credentials;
  }

  /// <summary>
  /// Gets a value indicating whether a challenge has been accepted and can be answered.
  /// </summary>
  public bool CanAuthenticate => _scheme is not null && _credentials is not null;

  /// <summary>
  /// Remembers the challenge a server sent, so that the request can be sent again with an answer.
  /// </summary>
  /// <param name="challenge">The value of the <c>WWW-Authenticate</c> header.</param>
  /// <returns>Returns <see langword="true"/> when the challenge is one that can be answered.</returns>
  public bool AcceptChallenge(string? challenge)
  {
    if (string.IsNullOrWhiteSpace(challenge) || _credentials is null)
    {
      return false;
    }

    var space = challenge!.IndexOf(' ');
    var scheme = space < 0 ? challenge : challenge.Substring(0, space);

    if (scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
    {
      _scheme = "Basic";
      return true;
    }

    if (!scheme.Equals("Digest", StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }

    var parameters = ParseParameters(space < 0 ? string.Empty : challenge.Substring(space + 1));
    if (!parameters.TryGetValue("nonce", out var nonce))
    {
      return false;
    }

    _scheme = "Digest";
    _realm = parameters.TryGetValue("realm", out var realm) ? realm : string.Empty;
    _nonce = nonce;
    _nonceCount = 0;
    return true;
  }

  /// <summary>
  /// Builds the value of the <c>Authorization</c> header for one request.
  /// </summary>
  /// <param name="method">The method of the request.</param>
  /// <param name="uri">The URI of the request.</param>
  /// <returns>Returns the header value, or <see langword="null"/> when there is no challenge to answer.</returns>
  public string? CreateAuthorization(string method, string uri)
  {
    if (_credentials is null || _scheme is null)
    {
      return null;
    }

    if (_scheme == "Basic")
    {
      var pair = $"{_credentials.UserName}:{_credentials.Password}";
      return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(pair));
    }

    _nonceCount++;

    var ha1 = Hash($"{_credentials.UserName}:{_realm}:{_credentials.Password}");
    var ha2 = Hash($"{method}:{uri}");
    var response = Hash($"{ha1}:{_nonce}:{ha2}");

    return string.Format(
      CultureInfo.InvariantCulture,
      "Digest username=\"{0}\", realm=\"{1}\", nonce=\"{2}\", uri=\"{3}\", response=\"{4}\"",
      _credentials.UserName,
      _realm,
      _nonce,
      uri,
      response);
  }

  internal static IReadOnlyDictionary<string, string> ParseParameters(string value)
  {
    var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    var index = 0;

    while (index < value.Length)
    {
      var equals = value.IndexOf('=', index);
      if (equals < 0)
      {
        break;
      }

      var name = value.Substring(index, equals - index).Trim(' ', ',', '\t');
      index = equals + 1;

      // A server is free to space the list out, and several do.
      while (index < value.Length && (value[index] == ' ' || value[index] == '\t'))
      {
        index++;
      }

      string parameterValue;
      if (index < value.Length && value[index] == '"')
      {
        var closing = value.IndexOf('"', index + 1);
        if (closing < 0)
        {
          break;
        }

        parameterValue = value.Substring(index + 1, closing - index - 1);
        index = closing + 1;
      }
      else
      {
        var comma = value.IndexOf(',', index);
        comma = comma < 0 ? value.Length : comma;
        parameterValue = value.Substring(index, comma - index).Trim();
        index = comma;
      }

      if (name.Length > 0)
      {
        parameters[name] = parameterValue;
      }

      while (index < value.Length && (value[index] == ',' || value[index] == ' '))
      {
        index++;
      }
    }

    return parameters;
  }

  private static string Hash(string value)
  {
#pragma warning disable CA5351 // Digest authentication is specified over MD5; the scheme leaves no alternative.
    using var md5 = MD5.Create();
#pragma warning restore CA5351
    var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(value));

    var text = new StringBuilder(bytes.Length * 2);
    foreach (var b in bytes)
    {
      text.Append(b.ToString("x2", CultureInfo.InvariantCulture));
    }

    return text.ToString();
  }
}
