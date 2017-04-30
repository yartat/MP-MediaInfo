#region Copyright (C) 2005-2017 Team MediaPortal

// Copyright (C) 2005-2017 Team MediaPortal
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
using System.Runtime.InteropServices;

namespace MediaInfo
{
  /// <summary>
  /// 
  /// </summary>
  /// <seealso cref="IDisposable" />
  public class GlobalMemory : IDisposable
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalMemory"/> class.
    /// </summary>
    /// <param name="handle">The handle.</param>
    private GlobalMemory(IntPtr handle)
    {
      Handle = handle;
    }

    ~GlobalMemory()
    {
      Dispose(false);
    }

    public IntPtr Handle { get; private set; }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Strings to global ANSI string.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    public static GlobalMemory StringToGlobalAnsi(string source)
    {
      return new GlobalMemory(Marshal.StringToHGlobalAnsi(source));
    }

    private void Dispose(bool disposing)
    {
      if (Handle != IntPtr.Zero)
      {
        Marshal.FreeHGlobal(Handle);
        Handle = IntPtr.Zero;
      }
    }
  }
}
