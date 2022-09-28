#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using MediaInfo.Model;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Describes method to build menu stream.
  /// </summary>
  internal class MenuStreamBuilder : MediaStreamBuilder<MenuStream>
  {
    public MenuStreamBuilder(MediaInfo info, int number, int position)
      : base(info, number, position)
    {
    }

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