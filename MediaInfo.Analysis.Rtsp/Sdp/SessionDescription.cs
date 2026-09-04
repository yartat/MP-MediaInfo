#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MediaInfo.Analysis.Rtsp.Sdp;

/// <summary>
/// Describes how one payload type of a media section is encoded.
/// </summary>
/// <param name="PayloadType">The RTP payload type the mapping applies to.</param>
/// <param name="Encoding">The encoding name, for example <c>H264</c>, <c>MPEG4-GENERIC</c> or <c>PCMA</c>.</param>
/// <param name="ClockRate">The RTP clock rate in hertz.</param>
/// <param name="Channels">The number of audio channels, or 0 when the mapping does not state one.</param>
public sealed record SdpRtpMap(int PayloadType, string Encoding, int ClockRate, int Channels);

/// <summary>
/// Describes one media section of a session description.
/// </summary>
public sealed class SdpMedia
{
  /// <summary>Gets the kind of the media, which is <c>video</c>, <c>audio</c> or something the session invents.</summary>
  public string Kind { get; init; } = string.Empty;

  /// <summary>Gets the transport protocol the section declares, for example <c>RTP/AVP</c>.</summary>
  public string Protocol { get; init; } = string.Empty;

  /// <summary>Gets the payload types the section offers, most preferred first.</summary>
  public IReadOnlyList<int> PayloadTypes { get; init; } = [];

  /// <summary>Gets the control attribute that names the track when it is set up.</summary>
  public string? Control { get; init; }

  /// <summary>Gets the encoding of each payload type the section maps.</summary>
  public IReadOnlyDictionary<int, SdpRtpMap> RtpMaps { get; init; } = new Dictionary<int, SdpRtpMap>();

  /// <summary>Gets the format parameters of each payload type that declares them.</summary>
  public IReadOnlyDictionary<int, string> FormatParameters { get; init; } = new Dictionary<int, string>();

  /// <summary>Gets every attribute of the section, in the order it appeared.</summary>
  public IReadOnlyList<KeyValuePair<string, string>> Attributes { get; init; } = [];

  /// <summary>
  /// Gets the payload type the session prefers, or -1 when it offers none.
  /// </summary>
  public int PrimaryPayloadType => PayloadTypes.Count > 0 ? PayloadTypes[0] : -1;

  /// <summary>
  /// Gets the encoding of the preferred payload type.
  /// </summary>
  public SdpRtpMap? PrimaryRtpMap =>
    PrimaryPayloadType >= 0 && RtpMaps.TryGetValue(PrimaryPayloadType, out var map) ? map : null;

  /// <summary>
  /// Gets the value of a format parameter of the preferred payload type.
  /// </summary>
  /// <remarks>
  /// The parameters are a semicolon separated list of <c>name=value</c> pairs, and the names are compared without
  /// regard to case, because senders disagree about how to spell them.
  /// </remarks>
  /// <param name="name">The name of the parameter.</param>
  /// <returns>Returns the value, or <see langword="null"/> when the parameter is not declared.</returns>
  public string? GetFormatParameter(string name)
  {
    if (PrimaryPayloadType < 0 || !FormatParameters.TryGetValue(PrimaryPayloadType, out var parameters))
    {
      return null;
    }

    foreach (var pair in parameters.Split(';'))
    {
      var separator = pair.IndexOf('=');
      if (separator <= 0)
      {
        continue;
      }

      if (pair.AsSpan(0, separator).Trim().Equals(name.AsSpan(), StringComparison.OrdinalIgnoreCase))
      {
        return pair.Substring(separator + 1).Trim();
      }
    }

    return null;
  }
}

/// <summary>
/// Describes an RTSP session as the server answered a DESCRIBE request.
/// </summary>
/// <remarks>
/// Only what is needed to set a track up and to say what it carries is modelled. Timing, bandwidth and encryption
/// lines are kept as raw attributes rather than interpreted.
/// </remarks>
public sealed class SessionDescription
{
  /// <summary>Gets the session name.</summary>
  public string? Name { get; init; }

  /// <summary>Gets the session level control attribute.</summary>
  public string? Control { get; init; }

  /// <summary>Gets the media sections of the session.</summary>
  public IReadOnlyList<SdpMedia> Media { get; init; } = [];

  /// <summary>Gets the first video section, when the session offers one.</summary>
  public SdpMedia? Video =>
    Media.FirstOrDefault(x => string.Equals(x.Kind, "video", StringComparison.OrdinalIgnoreCase));

  /// <summary>Gets the first audio section, when the session offers one.</summary>
  public SdpMedia? Audio =>
    Media.FirstOrDefault(x => string.Equals(x.Kind, "audio", StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Reads a session description.
  /// </summary>
  /// <remarks>
  /// A line that cannot be made sense of is skipped rather than failing the parse, because a session that carries one
  /// unusual attribute still describes tracks that can be played.
  /// </remarks>
  /// <param name="text">The body of a DESCRIBE response.</param>
  /// <returns>Returns the session description, which holds no media when nothing could be read.</returns>
  public static SessionDescription Parse(string text)
  {
    if (string.IsNullOrWhiteSpace(text))
    {
      return new SessionDescription();
    }

    string? name = null;
    string? sessionControl = null;
    var media = new List<SdpMedia>();

    MediaBuilder? current = null;

    foreach (var raw in text.Split('\n'))
    {
      var line = raw.Trim('\r', ' ', '\t');
      if (line.Length < 2 || line[1] != '=')
      {
        continue;
      }

      var key = line[0];
      var value = line.Substring(2).Trim();

      switch (key)
      {
        case 's' when current is null:
          name = value;
          break;

        case 'm':
          current?.AddTo(media);
          current = MediaBuilder.Create(value);
          break;

        case 'a':
          var (attribute, attributeValue) = SplitAttribute(value);
          if (current is null)
          {
            if (string.Equals(attribute, "control", StringComparison.OrdinalIgnoreCase))
            {
              sessionControl = attributeValue;
            }
          }
          else
          {
            current.AddAttribute(attribute, attributeValue);
          }

          break;
      }
    }

    current?.AddTo(media);

    return new SessionDescription { Name = name, Control = sessionControl, Media = media };
  }

  private static (string Name, string Value) SplitAttribute(string value)
  {
    var separator = value.IndexOf(':');
    return separator < 0
      ? (value, string.Empty)
      : (value.Substring(0, separator).Trim(), value.Substring(separator + 1).Trim());
  }

  private sealed class MediaBuilder
  {
    private readonly List<KeyValuePair<string, string>> _attributes = [];
    private readonly Dictionary<int, SdpRtpMap> _rtpMaps = [];
    private readonly Dictionary<int, string> _formats = [];

    private string _kind = string.Empty;
    private string _protocol = string.Empty;
    private List<int> _payloadTypes = [];
    private string? _control;

    public static MediaBuilder? Create(string value)
    {
      // m=<kind> <port> <protocol> <payload type> ...
      var parts = value.Split([' '], StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length < 3)
      {
        return null;
      }

      return new MediaBuilder
      {
        _kind = parts[0],
        _protocol = parts[2],
        _payloadTypes = parts.Skip(3)
          .Select(x => int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pt) ? pt : -1)
          .Where(x => x >= 0)
          .ToList()
      };
    }

    public void AddAttribute(string name, string value)
    {
      _attributes.Add(new KeyValuePair<string, string>(name, value));

      if (string.Equals(name, "control", StringComparison.OrdinalIgnoreCase))
      {
        _control = value;
        return;
      }

      if (string.Equals(name, "rtpmap", StringComparison.OrdinalIgnoreCase))
      {
        var map = ParseRtpMap(value);
        if (map is not null)
        {
          _rtpMaps[map.PayloadType] = map;
        }

        return;
      }

      if (string.Equals(name, "fmtp", StringComparison.OrdinalIgnoreCase))
      {
        var space = value.IndexOf(' ');
        if (space > 0 &&
            int.TryParse(value.AsSpan(0, space).ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var payloadType))
        {
          _formats[payloadType] = value.Substring(space + 1).Trim();
        }
      }
    }

    public void AddTo(List<SdpMedia> media)
    {
      if (_kind.Length == 0)
      {
        return;
      }

      media.Add(new SdpMedia
      {
        Kind = _kind,
        Protocol = _protocol,
        PayloadTypes = _payloadTypes,
        Control = _control,
        RtpMaps = _rtpMaps,
        FormatParameters = _formats,
        Attributes = _attributes
      });
    }

    private static SdpRtpMap? ParseRtpMap(string value)
    {
      // <payload type> <encoding>/<clock rate>[/<channels>]
      var space = value.IndexOf(' ');
      if (space <= 0 ||
          !int.TryParse(value.AsSpan(0, space).ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var payloadType))
      {
        return null;
      }

      var parts = value.Substring(space + 1).Split('/');
      var encoding = parts[0].Trim();
      var clockRate = parts.Length > 1 &&
        int.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var rate) ? rate : 0;
      var channels = parts.Length > 2 &&
        int.TryParse(parts[2].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) ? count : 0;

      return new SdpRtpMap(payloadType, encoding, clockRate, channels);
    }
  }
}
