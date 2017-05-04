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

using JetBrains.Annotations;

namespace MediaInfo
{
  /// <summary>
  /// Provides properties and overridden methods for the analyze stream 
  /// and contains information about media stream.
  /// </summary>
  /// <seealso cref="MediaStream" />
  public abstract class LanguageMediaStream : MediaStream
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageMediaStream"/> class.
    /// </summary>
    /// <param name="info">The media information.</param>
    /// <param name="number">The stream number.</param>
    /// <param name="position">The stream position.</param>
    protected LanguageMediaStream(MediaInfo info, int number, int position)
        : base(info, number, position)
    {
    }

    /// <summary>
    /// Gets the media stream language.
    /// </summary>
    /// <value>
    /// The media stream language.
    /// </value>
    [PublicAPI]
    public string Language { get; set; }

    /// <summary>
    /// Gets the media stream LCID.
    /// </summary>
    /// <value>
    /// The media stream LCID.
    /// </value>
    [PublicAPI]
    public int Lcid { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="LanguageMediaStream"/> is default.
    /// </summary>
    /// <value>
    ///   <c>true</c> if default; otherwise, <c>false</c>.
    /// </value>
    [PublicAPI]
    public bool Default { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="LanguageMediaStream"/> is forced.
    /// </summary>
    /// <value>
    ///   <c>true</c> if forced; otherwise, <c>false</c>.
    /// </value>
    [PublicAPI]
    public bool Forced { get; private set; }

    /// <inheritdoc />
    protected override void AnalyzeInternal()
    {
      base.AnalyzeInternal();
      var language = Get("Language").ToLower();
      Default = Get<bool>("Default", bool.TryParse);
      Forced = Get<bool>("Forced", bool.TryParse);
      Language = LanguageHelper.GetLanguageByShortName(language);
      Lcid = LanguageHelper.GetLcidByShortName(language);
    }
  }
}