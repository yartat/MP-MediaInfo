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
using System.Collections.Generic;

using JetBrains.Annotations;

namespace MediaInfo
{
  /// <summary>
  /// Describes properties of the menu
  /// </summary>
  /// <seealso cref="MediaStream" />
  [PublicAPI]
  public class MenuStream : MediaStream
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuStream"/> class.
    /// </summary>
    public MenuStream()
    {
      Chapters = new List<Chapter>();
    }

    /// <summary>
    /// Gets or sets the menu duration.
    /// </summary>
    /// <value>
    /// The menu duration.
    /// </value>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets the chapters.
    /// </summary>
    /// <value>
    /// The chapters.
    /// </value>
    public IList<Chapter> Chapters { get; }

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Menu;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Menu;

    /// <summary>
    /// Describes properties of the menu chapter
    /// </summary>
    [PublicAPI]
    public sealed class Chapter
    {
      /// <summary>
      /// Gets or sets the menu position.
      /// </summary>
      /// <value>
      /// The menu position.
      /// </value>
      public TimeSpan Position { get; set; }

      /// <summary>
      /// Gets or sets the menu chapter name.
      /// </summary>
      /// <value>
      /// The menu chapter name.
      /// </value>
      public string Name { get; set; }
    }
  }
}