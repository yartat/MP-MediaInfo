#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

namespace MediaInfo
{
  /// <summary>
  /// Describes read-only access to the properties of an opened media.
  /// </summary>
  /// <remarks>
  /// This is the surface the stream and tag builders need. It is implemented by <see cref="MediaInfo"/> itself and by
  /// the analysis layer adapter, which allows the builders to be driven by a test double that has no native library
  /// behind it.
  /// </remarks>
  public interface IMediaInfoReader
  {
    /// <summary>
    /// Gets the specified parameter value in the stream by parameter name, kind of information and kind of search.
    /// </summary>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The parameter name.</param>
    /// <param name="kindOfInfo">The kind of information.</param>
    /// <param name="kindOfSearch">The kind of search.</param>
    /// <returns>Returns the parameter value, or <see cref="string.Empty"/> when it is not available.</returns>
    string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch);

    /// <summary>
    /// Gets the specified parameter value in the stream by parameter index and kind of information.
    /// </summary>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The parameter index.</param>
    /// <param name="kindOfInfo">The kind of information.</param>
    /// <returns>Returns the parameter value, or <see cref="string.Empty"/> when it is not available.</returns>
    string Get(StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo);

    /// <summary>
    /// Gets the specified parameter value in the stream by parameter name and kind of information.
    /// </summary>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The parameter name.</param>
    /// <param name="kindOfInfo">The kind of information.</param>
    /// <returns>Returns the parameter value, or <see cref="string.Empty"/> when it is not available.</returns>
    string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo);

    /// <summary>
    /// Gets the specified parameter value in the stream by parameter name.
    /// </summary>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The parameter name.</param>
    /// <returns>Returns the parameter value, or <see cref="string.Empty"/> when it is not available.</returns>
    string Get(StreamKind streamKind, int streamNumber, string parameter);

    /// <summary>
    /// Gets the specified parameter value in the stream by parameter index.
    /// </summary>
    /// <param name="streamKind">Kind of the stream.</param>
    /// <param name="streamNumber">The stream number.</param>
    /// <param name="parameter">The parameter index.</param>
    /// <returns>Returns the parameter value, or <see cref="string.Empty"/> when it is not available.</returns>
    string Get(StreamKind streamKind, int streamNumber, int parameter);

    /// <summary>
    /// Gets the count of the specified kind of stream in the media.
    /// </summary>
    /// <param name="streamKind">Kind of the streams.</param>
    /// <param name="streamNumber">The stream number, or -1 to count all streams of the kind.</param>
    /// <returns>Returns the count of the specified kind of streams, or 0 when the media is not available.</returns>
    int CountGet(StreamKind streamKind, int streamNumber);

    /// <summary>
    /// Gets the count of the specified kind of stream in the media.
    /// </summary>
    /// <param name="streamKind">Kind of the streams.</param>
    /// <returns>Returns the count of the specified kind of streams, or 0 when the media is not available.</returns>
    int CountGet(StreamKind streamKind);

    /// <summary>
    /// Gets or sets the specified library option.
    /// </summary>
    /// <param name="option">The option name.</param>
    /// <param name="value">The option value.</param>
    /// <returns>Returns the option value, or <see cref="string.Empty"/> when the library is not available.</returns>
    string Option(string option, string value);

    /// <summary>
    /// Gets the specified library option.
    /// </summary>
    /// <param name="option">The option name.</param>
    /// <returns>Returns the option value, or <see cref="string.Empty"/> when the library is not available.</returns>
    string Option(string option);

    /// <summary>
    /// Gets all the media details as a single formatted report.
    /// </summary>
    /// <returns>Returns the report, or <see cref="string.Empty"/> when the media is not available.</returns>
    string Inform();
  }
}
