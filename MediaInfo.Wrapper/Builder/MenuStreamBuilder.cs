#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using MediaInfo.Model;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Builds a menu stream representation from the provided media information, including associated chapters and their
  /// positions.
  /// </summary>
  /// <param name="info">The media information source used to extract menu stream data.</param>
  /// <param name="number">The stream number identifying the specific menu stream within the media information.</param>
  /// <param name="position">The position index of the stream within the media information.</param>
  internal class MenuStreamBuilder(IMediaInfoReader info, int number, int position) : MediaStreamBuilder<MenuStream>(info, number, position)
  {
    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Menu;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Menu;

    /// <inheritdoc />
    public override MenuStream Build()
    {
      var result = base.Build();
      var chapterStartId = Get<int>((int)NativeMethods.Menu.Menu_Chapters_Pos_Begin, InfoKind.Text, TagBuilderHelper.TryGetInt);
      var chapterEndId = Get<int>((int)NativeMethods.Menu.Menu_Chapters_Pos_End, InfoKind.Text, TagBuilderHelper.TryGetInt);
      for (var i = chapterStartId; i < chapterEndId; ++i)
      {
        result.Chapters.Add(new Chapter
        {
          Name = Get(i, InfoKind.Text),
          Position = Get<TimeSpan>(i, InfoKind.NameText, TimeSpan.TryParse)
        });
      }

      return result;
    }
  }
}