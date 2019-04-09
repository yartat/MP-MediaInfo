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

namespace MediaInfo.Model
{
  /// <summary>
  /// Describes video color space
  /// </summary>
  public enum ColorSpace
  {
    /// <summary>
    /// Generic film
    /// </summary>
    Generic,

    /// <summary>
    /// Printing density
    /// </summary>
    PrintingDensity,

    /// <summary>
    /// BT.601 NTSC
    /// </summary>
    NTSC,

    /// <summary>
    /// BT.601 PAL
    /// </summary>
    PAL,

    /// <summary>
    /// ADX
    /// </summary>
    ADX,

    /// <summary>
    /// BT.470 System M
    /// </summary>
    BT470M,

    /// <summary>
    /// BT.470 System B/G
    /// </summary>
    BT470BG,

    /// <summary>
    /// BT.601 PAL or NTSC
    /// </summary>
    BT601,

    /// <summary>
    /// BT.709
    /// </summary>
    BT709,

    /// <summary>
    /// BT.1361
    /// </summary>
    BT1361,

    /// <summary>
    /// BT.2020 (10 bit or 12 bit)
    /// </summary>
    BT2020,

    /// <summary>
    /// BT.2100
    /// </summary>
    BT2100,

    /// <summary>
    /// EBU Tech 3213
    /// </summary>
    EBUTech3213,

    /// <summary>
    /// SMPTE 240M
    /// </summary>
    SMPTE240M,

    /// <summary>
    /// SMPTE 274M
    /// </summary>
    SMPTE274M,

    /// <summary>
    /// SMPTE 428M
    /// </summary>
    SMPTE428M,

    /// <summary>
    ///  SMPTE ST 2065-1
    /// </summary>
    ACES,

    /// <summary>
    /// SMPTE ST 2067-40 / ISO 11664-3
    /// </summary>
    XYZ,

    /// <summary>
    /// DCI-P3
    /// </summary>
    DCIP3,

    /// <summary>
    /// Display P3
    /// </summary>
    DisplayP3
  }
}
