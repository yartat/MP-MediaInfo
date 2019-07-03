using MediaInfo.Model;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Tag builder helper class
  /// </summary>
  public static class TagBuilderHelper
  {
    private static readonly Dictionary<string, bool> BooleanValues = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
    {
        { "1", true },
        { "0", false },
        { "y", true },
        { "n", false },
        { "yes", true },
        { "no", false },
        { "t", true },
        { "f", false },
        { "true", true },
        { "false", false }
    };

    private static readonly Dictionary<string, StereoMode> StereoModes = new Dictionary<string, StereoMode>(StringComparer.OrdinalIgnoreCase)
    {
      { "SideBySideRF", StereoMode.SideBySideRight },
      { "SideBySideLF", StereoMode.SideBySideLeft },
      { "TopBottomLF", StereoMode.TopBottomLeft },
      { "TopBottomRF", StereoMode.TopBottomRight },
    };

    /// <summary>
    /// Tries the parse.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="result">if set to <c>true</c> [result].</param>
    /// <returns><c>true</c> if source string is boolean value, <c>false</c> otherwise.</returns>
    public static bool TryGetBool(this string source, out bool result)
    {
        return BooleanValues.TryGetValue(source, out result);
    }

    /// <summary>
    /// Tries the get string.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The result value.</param>
    /// <returns><c>true</c> if source string is not empty, <c>false</c> otherwise.</returns>
    public static bool TryGetString(this string source, out object value)
    {
      value = source;
      return !string.IsNullOrEmpty(source);
    }

    /// <summary>
    /// Tries the get string.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The result value.</param>
    /// <returns><c>true</c> if source string is not empty, <c>false</c> otherwise.</returns>
    public static bool TryGetString(this string source, out string value)
    {
      value = source;
      return !string.IsNullOrEmpty(source);
    }

    /// <summary>
    /// Tries the get string in BASE64.
    /// </summary>
    /// <param name="source">The source BASE64 string.</param>
    /// <param name="value">The result value.</param>
    /// <returns><c>true</c> if source string is not empty and valid BASE64 string, <c>false</c> otherwise.</returns>
    public static bool TryGetBase64(this string source, out object value)
    {
      if (!string.IsNullOrEmpty(source))
      {
        value = Convert.FromBase64String(source);
        return true;
      }

      value = null;
      return false;
    }

    /// <summary>
    /// Tries the get string in BASE64.
    /// </summary>
    /// <param name="source">The source BASE64 string.</param>
    /// <param name="value">The result value as byte array.</param>
    /// <returns><c>true</c> if source string is not empty and valid BASE64 string, <c>false</c> otherwise.</returns>
    public static bool TryGetBase64(this string source, out byte[] value)
    {
      if (!string.IsNullOrEmpty(source))
      {
        value = Convert.FromBase64String(source);
        return true;
      }

      value = null;
      return false;
    }

    /// <summary>
    /// Tries the get int value.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The result int value.</param>
    /// <returns><c>true</c> if source string is not empty and valid integer value, <c>false</c> otherwise.</returns>
    public static bool TryGetInt(this string source, out object value)
    {
      var result = int.TryParse(source, out var resultValue);
      value = resultValue;
      return result;
    }

    /// <summary>
    /// Tries the get int value.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The result int value.</param>
    /// <returns><c>true</c> if source string is not empty and valid integer value, <c>false</c> otherwise.</returns>
    public static bool TryGetInt(this string source, out int value)
    {
      var result = int.TryParse(source, out value);
      return result;
    }

    /// <summary>
    /// Tries the get double value.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The result double value.</param>
    /// <returns><c>true</c> if source string is not empty and valid double value, <c>false</c> otherwise.</returns>
    public static bool TryGetDouble(this string source, out object value)
    {
      var result = double.TryParse(source, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var resultValue);
      value = resultValue;
      return result;
    }

    /// <summary>
    /// Tries the get double value.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The result double value.</param>
    /// <returns><c>true</c> if source string is not empty and valid double value, <c>false</c> otherwise.</returns>
    public static bool TryGetDouble(this string source, out double value)
    {
      var result = double.TryParse(source, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);      

      return result;
    }

    /// <summary>A string extension method that attempts to get stereo mode a StereoMode from the given string.</summary>
    /// <param name="source">The source.</param>
    /// <param name="mode">The stereo mode.</param>
    /// <returns>True if it succeeds, false if it fails.</returns>
    public static bool TryGetStereoMode(this string source, out StereoMode mode)
    {
      return StereoModes.TryGetValue(source, out mode);
    }

    /// <summary>
    /// Tries the get date value.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The result date value.</param>
    /// <returns><c>true</c> if source string is not empty and valid date value, <c>false</c> otherwise.</returns>
    public static bool TryGetDate(this string source, out object value)
    {
      var result = DateTime.TryParseExact(
      source,
      new[] { "yyyy", "yyyy-MM", "yyyy-MM-dd", "yyyy-M", "yyyy-M-d", "yyyy-MM-d", "yyyy-M-dd", "UTC yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.f",  "yyyy-MM-dd HH:mm:ss.ff", "yyyy-MM-dd HH:mm:ss.fff",  "yyyy-MM-dd HH:mm:ss.ffff",  "yyyy-MM-dd HH:mm:ss.fffff",
              "yyyy/MM/dd", "yyyy/M/d", "yyyy/M/dd", "yyyy/M/d", "UTC yyyy/MM/dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss.f", "yyyy/MM/dd HH:mm:ss.ff", "yyyy/MM/dd HH:mm:ss.fff", "yyyy/MM/dd HH:mm:ss.ffff", "yyyy/MM/dd HH:mm:ss.fffff",
              "dd.MM.yyyy", "d.MM.yyyy", "dd.M.yyyy", "d.M.yyyy", "UTC dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss.f", "dd.MM.yyyy HH:mm:ss.ff", "dd.MM.yyyy HH:mm:ss.fff", "dd.MM.yyyy HH:mm:ss.ffff", "dd.MM.yyyy HH:mm:ss.fffff",
              "MM/dd/yyyy", "M/dd/yyyy", "MM/d/yyyy", "M/d/yyyy", "UTC MM/dd/yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss.f", "dd.MM.yyyy HH:mm:ss.ff", "dd.MM.yyyy HH:mm:ss.fff", "dd.MM.yyyy HH:mm:ss.ffff", "dd.MM.yyyy HH:mm:ss.fffff",
      },
      CultureInfo.InvariantCulture,
       DateTimeStyles.None,
      out var resultValue);
      value = resultValue;
      return result;
    }

    /// <summary>
    /// Tries the get date value.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The result date value.</param>
    /// <returns><c>true</c> if source string is not empty and valid date value, <c>false</c> otherwise.</returns>
    public static bool TryGetDate(this string source, out DateTime value)
    {
      var result = DateTime.TryParseExact(
      source,
      new[] { "yyyy", "yyyy-MM", "yyyy-MM-dd", "yyyy-M", "yyyy-M-d", "yyyy-MM-d", "yyyy-M-dd", "UTC yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.f",  "yyyy-MM-dd HH:mm:ss.ff", "yyyy-MM-dd HH:mm:ss.fff",  "yyyy-MM-dd HH:mm:ss.ffff",  "yyyy-MM-dd HH:mm:ss.fffff",
              "yyyy/MM/dd", "yyyy/M/d", "yyyy/M/dd", "yyyy/M/d", "UTC yyyy/MM/dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss.f", "yyyy/MM/dd HH:mm:ss.ff", "yyyy/MM/dd HH:mm:ss.fff", "yyyy/MM/dd HH:mm:ss.ffff", "yyyy/MM/dd HH:mm:ss.fffff",
              "dd.MM.yyyy", "d.MM.yyyy", "dd.M.yyyy", "d.M.yyyy", "UTC dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss.f", "dd.MM.yyyy HH:mm:ss.ff", "dd.MM.yyyy HH:mm:ss.fff", "dd.MM.yyyy HH:mm:ss.ffff", "dd.MM.yyyy HH:mm:ss.fffff",
              "MM/dd/yyyy", "M/dd/yyyy", "MM/d/yyyy", "M/d/yyyy", "UTC MM/dd/yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss.f", "dd.MM.yyyy HH:mm:ss.ff", "dd.MM.yyyy HH:mm:ss.fff", "dd.MM.yyyy HH:mm:ss.ffff", "dd.MM.yyyy HH:mm:ss.fffff",
      },
      CultureInfo.InvariantCulture,
       DateTimeStyles.None,
      out value);
      return result;
    }
  }
}