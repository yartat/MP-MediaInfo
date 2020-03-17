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
  /// Provides properties and overridden methods for the analyze chapter in media 
  /// and contains information about chapter.
  /// </summary>
  /// <seealso cref="MediaStream" />
  public class ChapterStream : MediaStream
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ChapterStream"/> class.
    /// </summary>
    /// <param name="offset">The offset.</param>
    /// <param name="description">The description.</param>
    public ChapterStream(double offset, string description)
    {
      Offset = offset;
      Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChapterStream"/> class.
    /// </summary>
    public ChapterStream()
    {
    }

      /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Menu;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Other;

    /// <summary>
    /// Gets the chapter offset.
    /// </summary>
    /// <value>
    /// The chapter offset.
    /// </value>
    public double Offset { get; }

    /// <summary>
    /// Gets the chapter description.
    /// </summary>
    /// <value>
    /// The chapter description.
    /// </value>
    public string Description { get; }
  }
}