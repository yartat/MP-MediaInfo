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

using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaInfo.Model
{
  /// <summary>
  /// Base class to read tags from stream
  /// </summary>
  public abstract class BaseTags
  {
    /// <summary>
    /// Gets or sets the general tags.
    /// </summary>
    /// <value>
    /// The general tags.
    /// </value>
    internal IDictionary<NativeMethods.General, object> GeneralTags { get; } = new Dictionary<NativeMethods.General, object>();

    /// <summary>
    /// Gets the title of the media.
    /// </summary>
    /// <value>
    /// The title of the media.
    /// </value>
    public string Title => GeneralTags.TryGetValue(NativeMethods.General.General_Title, out var result) ? (string)result : null;

    /// <summary>
    /// Gets a short description of the contents, such as "Two birds flying".
    /// </summary>
    /// <value>
    /// A short description of the contents, such as "Two birds flying".
    /// </value>
    public string Description => GeneralTags.TryGetValue(NativeMethods.General.General_Description, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the keywords to the item separated by a comma, used for searching.
    /// </summary>
    /// <value>
    /// The keywords to the item separated by a comma, used for searching.
    /// </value>
    public string[] Keywords => GeneralTags.TryGetValue(NativeMethods.General.General_Keywords, out var result) ? ((string)result).Split(new[]  { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray() : null;

    /// <summary>
    /// Gets the country.
    /// </summary>
    /// <value>
    /// The country.
    /// </value>
    public string Country => GeneralTags.TryGetValue(NativeMethods.General.General_Country, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the time that the item was originally released.
    /// </summary>
    /// <value>
    /// The time that the item was originally released.
    /// </value>
    public DateTime? ReleasedDate => GeneralTags.TryGetValue(NativeMethods.General.General_Released_Date, out var result) ? (DateTime?)result : null;

    /// <summary>
    /// Gets the time that the encoding of this item was completed began.
    /// </summary>
    /// <value>
    /// The time that the encoding of this item was completed began.
    /// </value>
    public DateTime? EncodedDate => GeneralTags.TryGetValue(NativeMethods.General.General_Encoded_Date, out var result) ? (DateTime?)result : null;

    /// <summary>
    /// Gets the time that the tags were done for this item.
    /// </summary>
    /// <value>
    /// The time that the tags were done for this item.
    /// </value>
    public DateTime? TaggedDate => GeneralTags.TryGetValue(NativeMethods.General.General_Tagged_Date, out var result) ? (DateTime?)result : null;

    /// <summary>
    /// Gets any comment related to the content.
    /// </summary>
    /// <value>
    /// Any comment related to the content.
    /// </value>
    public string Comment => GeneralTags.TryGetValue(NativeMethods.General.General_Comment, out var result) ? (string)result : null;

    /// <summary>
    /// Gets a numeric value defining how much a person likes the song/movie. The number is between 0 and 5 with decimal values possible (e.g. 2.7), 5(.0) being the highest possible rating.
    /// </summary>
    /// <value>
    /// a numeric value defining how much a person likes the song/movie. The number is between 0 and 5 with decimal values possible (e.g. 2.7), 5(.0) being the highest possible rating.
    /// </value>
    public double? Rating => GeneralTags.TryGetValue(NativeMethods.General.General_Rating, out var result) ? (double?)result : null;

    /// <summary>
    /// Gets the copyright attribution.
    /// </summary>
    /// <value>
    /// The copyright attribution.
    /// </value>
    public string Copyright => GeneralTags.TryGetValue(NativeMethods.General.General_Copyright, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the organization producing the track (i.e. the 'record label').
    /// </summary>
    /// <value>
    /// The name of the organization producing the track (i.e. the 'record label').
    /// </value>
    public string Publisher => GeneralTags.TryGetValue(NativeMethods.General.General_Publisher, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the publishers official web page.
    /// </summary>
    /// <value>
    /// The publishers official web page.
    /// </value>
    public string PublisherUrl => GeneralTags.TryGetValue(NativeMethods.General.General_Publisher_URL, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the organization distributing track.
    /// </summary>
    /// <value>
    /// The name of the organization distributing track.
    /// </value>
    public string DistributedBy => GeneralTags.TryGetValue(NativeMethods.General.General_DistributedBy, out var result) ? (string)result : null;

    /// <summary>
    /// Gets the average number of beats per minute in the complete target.
    /// </summary>
    /// <value>
    /// The average number of beats per minute in the complete target.
    /// </value>
    public int? Bpm => GeneralTags.TryGetValue(NativeMethods.General.General_BPM, out var result) ? (int?)result : null;

    /// <summary>
    /// Gets the cover media.
    /// </summary>
    /// <value>
    /// The cover media.
    /// </value>
    public IEnumerable<CoverInfo> Covers { get; set; }

    /// <summary>
    /// Describes properties of the cover tags
    /// </summary>
    public class CoverInfo
    {
      /// <summary>
      /// Gets a value indicating whether this <see cref="CoverInfo"/> is exists.
      /// </summary>
      /// <value>
      /// <c>true</c> if exists; otherwise, <c>false</c>.
      /// </value>
      public bool Exists { get; internal set; }

      /// <summary>
      /// Gets the description of the cover.
      /// </summary>
      /// <value>
      /// The description of the cover.
      /// </value>
      public string Description { get; internal set; }

      /// <summary>
      /// Gets the type of the cover.
      /// </summary>
      /// <value>
      /// The type of the cover.
      /// </value>
      public string Type { get; internal set; }

      /// <summary>
      /// Gets the MIME of the cover.
      /// </summary>
      /// <value>
      /// The MIME of the cover.
      /// </value>
      public string Mime { get; internal set; }

      /// <summary>
      /// Gets the cover data.
      /// </summary>
      /// <value>
      /// The cover data.
      /// </value>
      public byte[] Data { get; internal set; }
    }
  }
}