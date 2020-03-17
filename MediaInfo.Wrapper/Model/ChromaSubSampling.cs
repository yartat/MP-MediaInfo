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

namespace MediaInfo.Model
{
  /// <summary>
  /// Describes video chroma sub sampling
  /// </summary>
  public enum ChromaSubSampling
  {
    /// <summary>
    /// 3:3:2
    /// </summary>
    Sampling332,

    /// <summary>
    /// 4:1:0
    /// </summary>
    Sampling410,

    /// <summary>
    /// 4:1:1
    /// </summary>
    Sampling411,

    /// <summary>
    /// 4:2:0
    /// </summary>
    Sampling420,

    /// <summary>
    /// 4:2:2
    /// </summary>
    Sampling422,

    /// <summary>
    /// 4:4:4
    /// </summary>
    Sampling444,

    /// <summary>
    /// 4:4:4:4
    /// </summary>
    Sampling4444,

    /// <summary>
    /// 5:5:5
    /// </summary>
    Sampling555,

    /// <summary>
    /// 5:6:5
    /// </summary>
    Sampling565,

    /// <summary>
    /// 8:8:8
    /// </summary>
    Sampling888
  }
}
