#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo
{
  /// <summary>
  /// Removes the credentials from a location before it is shown, logged or returned.
  /// </summary>
  /// <remarks>
  /// A location such as <c>rtsp://admin:secret@camera/stream</c> carries a password. The location itself is needed to
  /// connect, but everything that describes the media to a person or a log has to do without the credentials.
  /// </remarks>
  public static class UrlRedactor
  {
    /// <summary>
    /// The text that stands for the removed credentials.
    /// </summary>
    public const string Mask = "***";

    /// <summary>
    /// Replaces the user information of a URL with <see cref="Mask"/>.
    /// </summary>
    /// <param name="location">A path or a URL. A value that is not a URL with user information is returned as it is.</param>
    /// <returns>Returns the location without the user name and the password.</returns>
    public static string? Redact(string? location)
    {
      if (string.IsNullOrEmpty(location))
      {
        return location;
      }

      var scheme = location!.IndexOf("://", System.StringComparison.Ordinal);
      if (scheme < 0)
      {
        return location;
      }

      var start = scheme + 3;
      var end = location.IndexOfAny(new[] { '/', '?', '#' }, start);
      if (end < 0)
      {
        end = location.Length;
      }

      if (end <= start)
      {
        return location;
      }

      // The password may itself contain an @, so the host starts after the last one.
      var at = location.LastIndexOf('@', end - 1, end - start);
      return at < start ? location : location.Substring(0, start) + Mask + "@" + location.Substring(at + 1);
    }
  }
}
