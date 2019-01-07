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

namespace MediaInfo
{
  /// <summary>
  /// Describes 3D stereo mode
  /// </summary>
  public enum StereoMode
  {
    /// <summary>
    /// No 3D (mono)
    /// </summary>
    Mono,

    /// <summary>
    /// The side by side left eye is first
    /// </summary>
    SideBySideLeft,

    /// <summary>
    /// The top bottom right eye is first
    /// </summary>
    TopBottomRight,

    /// <summary>
    /// The top bottom left eye is first
    /// </summary>
    TopBottomLeft,

    /// <summary>
    /// The checkerboard right eye is first
    /// </summary>
    CheckerboardRight,

    /// <summary>
    /// The checkerboard left eye is first
    /// </summary>
    CheckerboardLeft,

    /// <summary>
    /// The row interleaved right eye is first
    /// </summary>
    RowInterleavedRight,

    /// <summary>
    /// The row interleaved left eye is first
    /// </summary>
    RowInterleavedLeft,

    /// <summary>
    /// The column interleaved right eye is first
    /// </summary>
    ColumnInterleavedRight,

    /// <summary>
    /// The column interleaved left eye is first
    /// </summary>
    ColumnInterleavedLeft,

    /// <summary>
    /// The anaglyph cyan-red
    /// </summary>
    AnaglyphCyanRed,

    /// <summary>
    /// The side by side right eye is first
    /// </summary>
    SideBySideRight,

    /// <summary>
    /// The anaglyph green-magenta
    /// </summary>
    AnaglyphGreenMagenta,

    /// <summary>
    /// The both eyes laced left eye is first
    /// </summary>
    BothEyesLacedLeft,

    /// <summary>
    /// The both eyes laced right eye is first
    /// </summary>
    BothEyesLacedRight
  }
}