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
  /// Defines constants for different kind of subtitles.
  /// </summary>
  public enum SubtitleCodec
  {

    /// <summary>
    /// The undefined type.
    /// </summary>
    Undefined,

    /// <summary>
    /// The Advanced SubStation Alpha subtitles.
    /// </summary>
    Ass,

    /// <summary>
    /// The BMP image subtitles.
    /// </summary>
    ImageBmp,

    /// <summary>
    /// The  SubStation Alpha subtitles.
    /// </summary>
    Ssa,

    /// <summary>
    /// The Advanced SubStation Alpha text subtitles.
    /// </summary>
    TextAss,

    /// <summary>
    /// The SubStation Alpha text subtitles.
    /// </summary>
    TextSsa,

    /// <summary>
    /// The Universal Subtitle Format text subtitles.
    /// </summary>
    TextUsf,

    /// <summary>
    /// The Unicode text subtitles.
    /// </summary>
    TextUtf8,

    /// <summary>
    /// The Universal Subtitle Format subtitles.
    /// </summary>
    Usf,

    /// <summary>
    /// The Unicode subtitles.
    /// </summary>
    Utf8,

    /// <summary>
    /// The VOB SUB subtitles (DVD subtitles).
    /// </summary>
    Vobsub,

    /// <summary>
    /// The Presentation Grapic Stream Subtitle Format subtitles
    /// </summary>
    HdmvPgs,

    /// <summary>
    /// The HDMV Text Subtitle Format subtitles
    /// </summary>
    HdmvTextst
  }
}