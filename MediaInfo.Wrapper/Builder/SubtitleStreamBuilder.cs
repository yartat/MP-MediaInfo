#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Collections.Generic;
using MediaInfo.Model;

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
      result.Format = Get((int)NativeMethods.Text.Text_Format, InfoKind.Text);
      result.Codec = Get<SubtitleCodec>((int)NativeMethods.Text.Text_CodecID, InfoKind.Text, TryGetCodec);
      return result;
    }

    private static bool TryGetCodec(string source, out SubtitleCodec result)
    {
      return SubtitleCodecs.TryGetValue(source.ToUpper(), out result);
    }
  }
}