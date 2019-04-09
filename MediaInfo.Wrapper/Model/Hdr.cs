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
  public enum Hdr
  {
    /// <summary>
    /// No HDR 
    /// </summary>
    None,

    /// <summary>
    /// HDR10
    /// </summary>
    HDR10,

    /// <summary>
    /// HDR10+
    /// </summary>
    HDR10Plus,

    /// <summary>
    /// Dolby Vision
    /// </summary>
    DolbyVision,

    /// <summary>
    /// Hybrid Log Gamma 
    /// </summary>
    HLG,

    /// <summary>
    /// Advanced HDR by Technicolor (SL-HDR1)
    /// </summary>
    SLHDR1,

    /// <summary>
    /// Advanced HDR by Technicolor (SL-HDR2)
    /// </summary>
    SLHDR2,

    /// <summary>
    /// Advanced HDR by Technicolor (SL-HDR3)
    /// </summary>
    SLHDR3
  }
}
