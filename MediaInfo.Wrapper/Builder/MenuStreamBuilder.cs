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
        result.Chapters.Add(new MenuStream.Chapter
        {
                         Name = Get(i, InfoKind.Text),
                         Position = Get<TimeSpan>(i, InfoKind.NameText, TimeSpan.TryParse)
                       });
      }
      return result;
    }
  }
}