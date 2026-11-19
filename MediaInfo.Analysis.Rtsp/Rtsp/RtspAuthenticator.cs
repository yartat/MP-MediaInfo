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
/// them with the nonce the server chose, together with a nonce of the client and a request counter when the server
/// offers the <c>auth</c> quality of protection.
/// <para>
/// Once a server has asked for digest, a later offer of basic alone is declined: a server, or something between the
/// client and it, that could take the password in the clear would otherwise only have to ask for it. Basic can also be
/// switched off altogether.
/// </para>
/// <para>
/// Digest is specified over MD5, so MD5 is what is used here. That is a property of the protocol rather than a choice
/// about how to protect a secret: a camera that asks for digest will not accept anything else. The credentials never
/// leave the process in any other form, and an installation that cares should be reached over a tunnel rather than
/// trusting the scheme.
/// </para>
/// </remarks>
internal sealed class RtspAuthenticator
{
  private static readonly char[] ChallengeSeparator = ['\n'];

  private readonly NetworkCredential? _credentials;
  private readonly bool _allowBasic;
  private readonly Func<string> _createClientNonce;

  private string? _scheme;
  private string? _realm;
  private string? _nonce;
  private string? _opaque;
  private bool _useQualityOfProtection;
  private int _nonceCount;

  /// <summary>
  /// Initializes a new instance of the <see cref="RtspAuthenticator"/> class.
  /// </summary>
  /// <param name="credentials">The credentials to answer a challenge with, or <see langword="null"/> when there are none.</param>
  /// <param name="allowBasic">Whether the basic scheme may be used, which sends the password without protecting it.</param>
  /// <param name="createClientNonce">Creates the client nonce of a digest response, or <see langword="null"/> for a random one.</param>
  public RtspAuthenticator(
    NetworkCredential? credentials,
    bool allowBasic = true,
    Func<string>? createClientNonce = null)
  {
    _credentials = credentials;
    _allowBasic = allowBasic;
    _createClientNonce = createClientNonce ?? CreateRandomClientNonce;
  }

  /// <summary>
  /// Gets a value indicating whether a challenge has been accepted and can be answered.
  /// </summary>
  public bool CanAuthenticate => _scheme is not null && _credentials is not null;

  /// <summary>
  /// Remembers the challenge a server sent, so that the request can be sent again with an answer.
  /// </summary>
  /// <remarks>
  /// A server may send several challenges, one per <c>WWW-Authenticate</c> header, separated here by a line feed. Digest
  /// is preferred to basic whatever the order.
  /// </remarks>
  /// <param name="challenge">The value of the <c>WWW-Authenticate</c> header, or the values of several of them.</param>
  /// <returns>Returns <see langword="true"/> when the challenge is one that can be answered.</returns>
  public bool AcceptChallenge(string? challenge)
  {
    if (string.IsNullOrWhiteSpace(challenge) || _credentials is null)
    {
      return false;
    }

    var basic = false;
    foreach (var line in challenge!.Split(ChallengeSeparator, StringSplitOptions.RemoveEmptyEntries))
    {
      var trimmed = line.Trim();
      var space = trimmed.IndexOf(' ');
      var scheme = space < 0 ? trimmed : trimmed.Substring(0, space);

      if (scheme.Equals("Digest", StringComparison.OrdinalIgnoreCase))
      {
        if (TryAcceptDigest(space < 0 ? string.Empty : trimmed.Substring(space + 1)))
        {
          return true;
        }
      }
      else if (scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
      {
        basic = true;
      }
    }

    // Having asked for digest, a server does not get the password in the clear by asking for basic instead.
    if (!basic || !_allowBasic || _scheme == "Digest")
    {
      return false;
    }

    _scheme = "Basic";
    return true;
  }

  /// <summary>
  /// Creates the value of the <c>Authorization</c> header for a request.
  /// </summary>
  /// <param name="method">The method of the request.</param>
  /// <param name="uri">The location the request is sent to.</param>
  /// <returns>
  /// Returns the header value, or <see langword="null"/> when no challenge has been accepted or a value could not be
  /// put into a header safely.
  /// </returns>
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

    // A value with a line break would end the header and start another one.
    if (HasControlCharacters(_credentials.UserName) || HasControlCharacters(_realm) ||
        HasControlCharacters(_nonce) || HasControlCharacters(_opaque) || HasControlCharacters(uri))
    {
      return null;
    }

    _nonceCount++;
    var ha1 = Hash($"{_credentials.UserName}:{_realm}:{_credentials.Password}");
    var ha2 = Hash($"{method}:{uri}");

    var authorization = new StringBuilder("Digest ");
    authorization
      .Append("username=").Append(Quote(_credentials.UserName))
      .Append(", realm=").Append(Quote(_realm))
      .Append(", nonce=").Append(Quote(_nonce))
      .Append(", uri=").Append(Quote(uri));

    if (_useQualityOfProtection)
    {
      var count = _nonceCount.ToString("x8", CultureInfo.InvariantCulture);
      var clientNonce = _createClientNonce();
      if (HasControlCharacters(clientNonce))
      {
        return null;
      }

      authorization
        .Append(", response=").Append(Quote(Hash($"{ha1}:{_nonce}:{count}:{clientNonce}:auth:{ha2}")))
        .Append(", qop=auth, nc=").Append(count)
        .Append(", cnonce=").Append(Quote(clientNonce));
    }
    else
    {
      authorization.Append(", response=").Append(Quote(Hash($"{ha1}:{_nonce}:{ha2}")));
    }

    if (_opaque is not null)
    {
      authorization.Append(", opaque=").Append(Quote(_opaque));
    }

    return authorization.ToString();
  }

  private bool TryAcceptDigest(string parameterList)
  {
    var parameters = ParseParameters(parameterList);
    if (!parameters.TryGetValue("nonce", out var nonce))
    {
      return false;
    }

    // MD5 is the algorithm of RFC 2069 and RFC 2617. A challenge for another one cannot be answered with it.
    if (parameters.TryGetValue("algorithm", out var algorithm) &&
        !algorithm.Equals("MD5", StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }

    var useQualityOfProtection = false;
    if (parameters.TryGetValue("qop", out var qop))
    {
      foreach (var option in qop.Split(','))
      {
        useQualityOfProtection |= option.Trim().Equals("auth", StringComparison.OrdinalIgnoreCase);
      }

      // A server that offers a protection and not the one that can be given has nothing to be answered.
      if (!useQualityOfProtection)
      {
        return false;
      }
    }

    _scheme = "Digest";
    _realm = parameters.TryGetValue("realm", out var realm) ? realm : string.Empty;
    _nonce = nonce;
    _opaque = parameters.TryGetValue("opaque", out var opaque) ? opaque : null;
    _useQualityOfProtection = useQualityOfProtection;
    _nonceCount = 0;
    return true;
  }

  private static bool HasControlCharacters(string? value)
  {
    if (value is null)
    {
      return false;
    }

    foreach (var character in value)
    {
      if (character < ' ' || character == '\u007F')
      {
        return true;
      }
    }

    return false;
  }

  // A quoted string may hold a quote or a backslash only when it is escaped.
  private static string Quote(string? value) =>
    "\"" + (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

  private static string CreateRandomClientNonce()
  {
    var bytes = new byte[8];
    using var generator = RandomNumberGenerator.Create();
    generator.GetBytes(bytes);

    var text = new StringBuilder(bytes.Length * 2);
    foreach (var b in bytes)
    {
      text.Append(b.ToString("x2", CultureInfo.InvariantCulture));
    }

    return text.ToString();
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
