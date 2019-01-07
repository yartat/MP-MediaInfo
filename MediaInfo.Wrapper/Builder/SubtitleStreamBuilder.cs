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

using System.Collections.Generic;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Describes base methods to build subtitle stream.
  /// </summary>
  internal class SubtitleStreamBuilder : LanguageMediaStreamBuilder<SubtitleStream>
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
    /// Initializes a new instance of the <see cref="SubtitleStreamBuilder"/> class.
    /// </summary>
    /// <param name="info">The media info object.</param>
    /// <param name="number">The stream number.</param>
    /// <param name="position">The stream position.</param>
    public SubtitleStreamBuilder(MediaInfo info, int number, int position)
      : base(info, number, position)
    {
    }

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Text;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Text;

    /// <inheritdoc />
    public override SubtitleStream Build()
    {
      var result = base.Build();
      result.Format = Get("Format");
      result.Codec = Get<SubtitleCodec>("CodecID", TryGetCodec);
      return result;
    }

    private static bool TryGetCodec(string source, out SubtitleCodec result)
    {
      return SubtitleCodecs.TryGetValue(source.ToUpper(), out result);
    }
  }
}