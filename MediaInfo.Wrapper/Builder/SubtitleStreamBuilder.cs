#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using MediaInfo.Model;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Builds a subtitle stream by extracting and mapping subtitle-specific metadata from the provided media information.
  /// </summary>
  /// <param name="info">The media information source containing metadata for the subtitle stream. Cannot be null.</param>
  /// <param name="number">The stream number within the media file used to identify the specific subtitle stream.</param>
  /// <param name="position">The position of the stream in the media file, used to determine stream ordering.</param>
  internal class SubtitleStreamBuilder(IMediaInfoReader info, int number, int position) : LanguageMediaStreamBuilder<SubtitleStream>(info, number, position)
  {
    #region match dictionary

    private static readonly Dictionary<string, SubtitleCodec> SubtitleCodecs = new Dictionary<string, SubtitleCodec>
    {
        { "S_ASS", SubtitleCodec.Ass },
        { "ASS", SubtitleCodec.Ass },
        { "S_IMAGE/BMP", SubtitleCodec.ImageBmp },
        { "N19", SubtitleCodec.N19 },
        { "PAC", SubtitleCodec.Pac },
        { "S_SSA", SubtitleCodec.Ssa },
        { "SSA", SubtitleCodec.Ssa },
        { "S_TEXT/ASS", SubtitleCodec.TextAss },
        { "S_TEXT/SSA", SubtitleCodec.TextSsa },
        { "S_TEXT/TTML", SubtitleCodec.Ttml },
        { "S_TEXT/USF", SubtitleCodec.TextUsf },
        { "S_TEXT/UTF8", SubtitleCodec.TextUtf8 },
        { "TTML", SubtitleCodec.Ttml },
        { "S_USF", SubtitleCodec.Usf },
        { "S_UTF8", SubtitleCodec.Utf8 },
        { "S_VOBSUB", SubtitleCodec.Vobsub },
        { "S_HDMV/PGS", SubtitleCodec.HdmvPgs },
        { "PGS", SubtitleCodec.HdmvPgs },
        { "S_HDMV/TEXTST", SubtitleCodec.HdmvTextst },
        { "WEBVTT", SubtitleCodec.WebVtt },
        { "S_DVBSUB", SubtitleCodec.DvbSubtitle },
        { "DVB SUBTITLE", SubtitleCodec.DvbSubtitle },
        { "S_KATE", SubtitleCodec.Kate },
        { "KATE", SubtitleCodec.Kate },
        { "C608", SubtitleCodec.Eia608 },
        { "EIA-608", SubtitleCodec.Eia608 },
        { "C708", SubtitleCodec.Eia708 },
        { "EIA-708", SubtitleCodec.Eia708 },
        { "APPLE TEXT", SubtitleCodec.AppleText },
        { "TX3G", SubtitleCodec.TimedText },
        { "TIMED TEXT", SubtitleCodec.TimedText },
        { "DXSB", SubtitleCodec.DivxSubtitle },
        { "DIVX SUBTITLE", SubtitleCodec.DivxSubtitle },
        { "VOBSUB", SubtitleCodec.Vobsub }
    };

        #endregion

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Text;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Text;

    /// <inheritdoc />
    public override SubtitleStream Build()
    {
      var result = base.Build();
      result.Format = Get((int)NativeMethods.Text.Text_Format, InfoKind.Text);
      result.Duration = TimeSpan.FromMilliseconds(Get<double>((int)NativeMethods.Text.Text_Duration, InfoKind.Text, TagBuilderHelper.TryGetDouble));
      result.FrameRate = Get<double>((int)NativeMethods.Text.Text_FrameRate, InfoKind.Text, TagBuilderHelper.TryGetDouble);
      result.FrameRateNumerator = Get<int>((int)NativeMethods.Text.Text_FrameRate_Num, InfoKind.Text, TagBuilderHelper.TryGetInt);
      result.FrameRateDenominator = Get<int>((int)NativeMethods.Text.Text_FrameRate_Den, InfoKind.Text, TagBuilderHelper.TryGetInt);
      result.TimeCodeFirstFrame = Get((int)NativeMethods.Text.Text_TimeCode_FirstFrame, InfoKind.Text);
      result.TimeCodeLastFrame = Get((int)NativeMethods.Text.Text_TimeCode_LastFrame, InfoKind.Text);
      result.TimeCodeDropFrame = Get<bool>((int)NativeMethods.Text.Text_TimeCode_DropFrame, InfoKind.Text, TagBuilderHelper.TryGetBool);
      result.TimeCodeSettings = Get((int)NativeMethods.Text.Text_TimeCode_Settings, InfoKind.Text);
      result.TimeCodeSource = Get((int)NativeMethods.Text.Text_TimeCode_Source, InfoKind.Text);
      result.MaxCharactersPerLine = Get<int>((int)NativeMethods.Text.Text_Lines_MaxCharacterCount, InfoKind.Text, TagBuilderHelper.TryGetInt);
      result.Codec = Get<SubtitleCodec>((int)NativeMethods.Text.Text_CodecID, InfoKind.Text, TryGetCodec);
      if (result.Codec == SubtitleCodec.Undefined)
      {
        result.Codec = Get<SubtitleCodec>((int)NativeMethods.Text.Text_Format, InfoKind.Text, TryGetCodec);
      }

      return result;
    }

    private static bool TryGetCodec(string source, out SubtitleCodec result) =>
      SubtitleCodecs.TryGetValue(source.ToUpper(), out result);
  }
}