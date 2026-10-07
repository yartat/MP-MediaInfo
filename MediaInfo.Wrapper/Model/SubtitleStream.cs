#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;

namespace MediaInfo.Model
{
  /// <summary>
  /// Represents a subtitle or text stream detected by MediaInfo.
  /// </summary>
  /// <remarks>
  /// This model contains codec, duration, frame rate, language, and time code metadata for a single
  /// subtitle or text stream.
  /// </remarks>
  /// <seealso cref="LanguageMediaStream" />
  public class SubtitleStream : LanguageMediaStream
  {
    /// <summary>
    /// Gets the subtitle format.
    /// </summary>
    /// <value>
    /// The subtitle format.
    /// </value>
    public string Format { get; set; } = default!;

    /// <summary>
    /// Gets or sets the subtitle duration.
    /// </summary>
    /// <value>
    /// The subtitle duration.
    /// </value>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the subtitle frame rate reported for the stream.
    /// </summary>
    /// <value>
    /// The calculated or container-reported frame rate for the subtitle stream.
    /// </value>
    public double FrameRate { get; set; }

    /// <summary>
    /// Gets or sets the numerator component of the subtitle frame rate.
    /// </summary>
    /// <value>
    /// The integer numerator used for exact frame rate representation.
    /// </value>
    public int FrameRateNumerator { get; set; }

    /// <summary>
    /// Gets or sets the denominator component of the subtitle frame rate.
    /// </summary>
    /// <value>
    /// The integer denominator used for exact frame rate representation.
    /// </value>
    public int FrameRateDenominator { get; set; }

    /// <summary>
    /// Gets or sets the time code associated with the first subtitle event or frame.
    /// </summary>
    /// <value>
    /// The first subtitle time code in MediaInfo text form, usually `HH:MM:SS:FF`.
    /// </value>
    public string TimeCodeFirstFrame { get; set; } = default!;

    /// <summary>
    /// Gets or sets the time code associated with the last subtitle event or frame.
    /// </summary>
    /// <value>
    /// The last subtitle time code in MediaInfo text form, usually `HH:MM:SS:FF`.
    /// </value>
    public string TimeCodeLastFrame { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the subtitle time code uses drop-frame notation.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the subtitle time code uses drop-frame notation; otherwise, <c>false</c>.
    /// </value>
    public bool TimeCodeDropFrame { get; set; }

    /// <summary>
    /// Gets or sets additional MediaInfo time code settings for the subtitle stream.
    /// </summary>
    /// <value>
    /// A raw string describing extra time code flags or configuration reported by MediaInfo.
    /// </value>
    public string TimeCodeSettings { get; set; } = default!;

    /// <summary>
    /// Gets or sets the source from which the subtitle time code was obtained.
    /// </summary>
    /// <value>
    /// The raw MediaInfo source description, such as container or embedded stream metadata.
    /// </value>
    public string TimeCodeSource { get; set; } = default!;

    /// <summary>
    /// Gets or sets the detected subtitle codec.
    /// </summary>
    /// <value>
    /// The subtitle codec.
    /// </value>
    public SubtitleCodec Codec { get; set; }

    /// <summary>
    /// Gets or sets the maximum count of characters per line.
    /// </summary>
    /// <value>
    /// The longest line, in characters, or 0 when the format does not report it (TTML, EIA-608/708 and PAC do).
    /// </value>
    public int MaxCharactersPerLine { get; set; }

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Text;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Text;
  }
}