#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
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

namespace MediaInfo
{
  /// <summary>Values that represent log levels.</summary>
  public enum LogLevel
  {
    /// <summary>
    /// The verbose level
    /// </summary>
    Verbose,

    /// <summary>
    /// The debug level
    /// </summary>
    Debug,

    /// <summary>
    /// The information level
    /// </summary>
    Information,

    /// <summary>
    /// The warning level
    /// </summary>
    Warning,

    /// <summary>
    /// The error level
    /// </summary>
    Error,

    /// <summary>
    /// The critical error level
    /// </summary>
    Critical
  }

  /// <summary>Interface for logger.</summary>
  public interface ILogger
  {
    /// <summary>Logs message.</summary>
    /// <param name="loglevel">The log level.</param>
    /// <param name="message">The message.</param>
    /// <param name="parameters">A variable-length parameters list containing message parameters.</param>
    void Log(LogLevel loglevel, string message, params object[] parameters);
  }
}