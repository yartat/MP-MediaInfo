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

using System.Collections.Generic;

using JetBrains.Annotations;

namespace MediaInfo
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

  /// <summary>
  /// Provides properties and overridden methods for the analyze subtitle stream 
  /// and contains information about subtitle.
  /// </summary>
  /// <seealso cref="LanguageMediaStream" />
  public class SubtitleStream : LanguageMediaStream
  {
    #region match dictionary

    private static readonly Dictionary<string, SubtitleCodec> SubtitleCodecs = new Dictionary<string, SubtitleCodec>
    {
        { "S_ASS", SubtitleCodec.Ass },
        { "S_IMAGE/BMP", SubtitleCodec.ImageBmp },
        { "S_SSA", SubtitleCodec.Ssa },
        { "S_TEXT/ASS", SubtitleCodec.TextAss },
        { "S_TEXT/SSA", SubtitleCodec.TextSsa },
        { "S_TEXT/USF", SubtitleCodec.TextUsf },
        { "S_TEXT/UTF8", SubtitleCodec.TextUtf8 },
        { "S_USF", SubtitleCodec.Usf },
        { "S_UTF8", SubtitleCodec.Utf8 },
        { "S_VOBSUB", SubtitleCodec.Vobsub },
        { "S_HDMV/PGS", SubtitleCodec.HdmvPgs },
        { "S_HDMV/TEXTST", SubtitleCodec.HdmvTextst }
    };

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="SubtitleStream"/> class.
    /// </summary>
    /// <param name="info">The media information.</param>
    /// <param name="number">The stream number.</param>
    /// <param name="position">The stream position.</param>
    public SubtitleStream(MediaInfo info, int number, int position)
        : base(info, number, position)
    {
    }

    /// <summary>
    /// Gets the subtitle format.
    /// </summary>
    /// <value>
    /// The subtitle format.
    /// </value>
    [PublicAPI]
    public string Format { get; private set; }

    /// <summary>
    /// Gets the subtitle codec.
    /// </summary>
    /// <value>
    /// The subtitle codec.
    /// </value>
    [PublicAPI]
    public SubtitleCodec Codec { get; private set; }

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Text;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Text;

    /// <inheritdoc />
    protected override void AnalyzeStreamInternal(MediaInfo info)
    {
      base.AnalyzeStreamInternal(info);
      Format = Get(info, "Format");
      Codec = GetCodec(Get(info, "CodecID").ToUpper());
    }

    private static SubtitleCodec GetCodec(string source)
    {
      SubtitleCodec result;
      return SubtitleCodecs.TryGetValue(source, out result) ? result : SubtitleCodec.Undefined;
    }
  }
}