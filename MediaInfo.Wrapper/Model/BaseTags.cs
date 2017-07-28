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

using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaInfo
{
  /// <summary>
  /// Base class to read tags from stream
  /// </summary>
  public abstract class BaseTags
  {
    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    /// <value>
    /// The tags.
    /// </value>
    internal IDictionary<string, object> Tags { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a short description of the contents, such as "Two birds flying".
    /// </summary>
    /// <value>
    /// A short description of the contents, such as "Two birds flying".
    /// </value>
    public string Description => Tags.TryGetValue("Description", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the keywords to the item separated by a comma, used for searching.
    /// </summary>
    /// <value>
    /// The keywords to the item separated by a comma, used for searching.
    /// </value>
    public string[] Keywords => Tags.TryGetValue("Keywords", out var result) ? ((string)result).Split(new[]  { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray() : null;

    /// <summary>
    /// Gets the country.
    /// </summary>
    /// <value>
    /// The country.
    /// </value>
    public string Country => Tags.TryGetValue("Country", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the time that the item was originally released.
    /// </summary>
    /// <value>
    /// The time that the item was originally released.
    /// </value>
    public DateTime? ReleasedDate => Tags.TryGetValue("Released_Date", out var result) ? (DateTime?)result : null;

    /// <summary>
    /// Gets the time that the encoding of this item was completed began.
    /// </summary>
    /// <value>
    /// The time that the encoding of this item was completed began.
    /// </value>
    public DateTime? EncodedDate => Tags.TryGetValue("Encoded_Date", out var result) ? (DateTime?)result : null;

    /// <summary>
    /// Gets the time that the tags were done for this item.
    /// </summary>
    /// <value>
    /// The time that the tags were done for this item.
    /// </value>
    public DateTime? TaggedDate => Tags.TryGetValue("Tagged_Date", out var result) ? (DateTime?)result : null;

    /// <summary>
    /// Gets any comment related to the content.
    /// </summary>
    /// <value>
    /// Any comment related to the content.
    /// </value>
    public string Comment => Tags.TryGetValue("Comment", out var result) ? (string)result : null;

    /// <summary>
    /// Gets a numeric value defining how much a person likes the song/movie. The number is between 0 and 5 with decimal values possible (e.g. 2.7), 5(.0) being the highest possible rating.
    /// </summary>
    /// <value>
    /// a numeric value defining how much a person likes the song/movie. The number is between 0 and 5 with decimal values possible (e.g. 2.7), 5(.0) being the highest possible rating.
    /// </value>
    public double? Rating => Tags.TryGetValue("Rating", out var result) ? (double?)result : null;

    /// <summary>
    /// Gets the copyright attribution.
    /// </summary>
    /// <value>
    /// The copyright attribution.
    /// </value>
    public string Copyright => Tags.TryGetValue("Copyright", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the organization producing the track (i.e. the 'record label').
    /// </summary>
    /// <value>
    /// The name of the organization producing the track (i.e. the 'record label').
    /// </value>
    public string Publisher => Tags.TryGetValue("Publisher", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the publishers official web page.
    /// </summary>
    /// <value>
    /// The publishers official web page.
    /// </value>
    public string PublisherUrl => Tags.TryGetValue("Publisher/URL", out var result) ? (string)result : null;

    /// <summary>
    /// Gets the name of the organization distributing track.
    /// </summary>
    /// <value>
    /// The name of the organization distributing track.
    /// </value>
    public string DistributedBy => Tags.TryGetValue("DistributedBy", out var result) ? (string)result : null;
  }
}