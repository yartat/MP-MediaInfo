#region Copyright (C) 2005-2019 Team MediaPortal

// Copyright (C) 2005-2019 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Text;

namespace MediaInfo
{
  /// <summary>A log extensions.</summary>
  public static class LogExtensions
  {
    /// <summary>Logs a verbose message.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message.</param>
    /// <param name="parameters">A variable-length parameters list containing message parameters.</param>
    public static void LogVerbose(this ILogger logger, string message, params object[] parameters)
    {
      if (logger != null)
      { 
        logger.Log(LogLevel.Verbose, message, parameters);
      }
    }

    /// <summary>Logs a debug message.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message.</param>
    /// <param name="parameters">A variable-length parameters list containing message parameters.</param>
    public static void LogDebug(this ILogger logger, string message, params object[] parameters)
    {
      if (logger != null)
      {
        logger.Log(LogLevel.Debug, message, parameters);
      }
    }

    /// <summary>Logs a information message.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message.</param>
    /// <param name="parameters">A variable-length parameters list containing message parameters.</param>
    public static void LogInformation(this ILogger logger, string message, params object[] parameters)
    {
      if (logger != null)
      {
        logger.Log(LogLevel.Information, message, parameters);
      }
    }

    /// <summary>Logs a warning message.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message.</param>
    /// <param name="parameters">A variable-length parameters list containing message parameters.</param>
    public static void LogWarning(this ILogger logger, string message, params object[] parameters)
    {
      if (logger != null)
      {
        logger.Log(LogLevel.Warning, message, parameters);
      }
    }

    /// <summary>Logs a error message.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message.</param>
    /// <param name="parameters">A variable-length parameters list containing message parameters.</param>
    public static void LogError(this ILogger logger, string message, params object[] parameters)
    {
      if (logger != null)
      {
        logger.Log(LogLevel.Error, message, parameters);
      }
    }

    /// <summary>Logs a error message.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message.</param>
    /// <param name="parameters">A variable-length parameters list containing message parameters.</param>
    public static void LogError(this ILogger logger, Exception exception, string message, params object[] parameters)
    {
      if (logger != null)
      {
        var errorMessage = string.Format(message, parameters);
        if (exception != null)
        { 
          var msg = new StringBuilder();
          msg.AppendFormat(message, parameters);
          msg.AppendLine();
  
          msg.AppendLine($"Exception: {exception.Message}");
          msg.AppendLine("Callstack:");
          msg.Append(exception.StackTrace);

          errorMessage = msg.ToString();
        }

        logger.Log(LogLevel.Error, errorMessage);
      }
    }

    /// <summary>Logs a critical message.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message.</param>
    /// <param name="parameters">A variable-length parameters list containing message parameters.</param>
    public static void LogCritical(this ILogger logger, string message, params object[] parameters)
    {
      if (logger != null)
      {
        logger.Log(LogLevel.Critical, message, parameters);
      }
    }

    /// <summary>Logs a critical message.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message.</param>
    /// <param name="parameters">A variable-length parameters list containing message parameters.</param>
    public static void LogCritical(this ILogger logger, Exception exception, string message, params object[] parameters)
    {
      if (logger != null)
      {
        var errorMessage = string.Format(message, parameters);
        if (exception != null)
        {
          var msg = new StringBuilder();
          msg.AppendFormat(message, parameters);
          msg.AppendLine();

          msg.AppendLine($"Exception: {exception.Message}");
          msg.AppendLine("Callstack:");
          msg.Append(exception.StackTrace);

          errorMessage = msg.ToString();
        }

        logger.Log(LogLevel.Critical, errorMessage);
      }
    }
  }
}
