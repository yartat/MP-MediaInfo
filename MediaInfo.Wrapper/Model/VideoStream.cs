#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Drawing;

namespace MediaInfo.Model;

/// <summary>
/// Represents a video stream detected by MediaInfo and exposes its technical characteristics.
/// </summary>
/// <remarks>
/// This model contains geometry, frame rate, codec, HDR, stereoscopic, colour, and time code metadata
/// for a single video stream.
/// </remarks>
/// <seealso cref="LanguageMediaStream" />
public class VideoStream : LanguageMediaStream
{
  /// <inheritdoc />
  public override MediaStreamKind Kind => MediaStreamKind.Video;

  /// <inheritdoc />
  protected override StreamKind StreamKind => StreamKind.Video;

  /// <summary>
  /// Gets or sets the video frame rate.
  /// </summary>
  /// <value>
  /// The video frame rate.
  /// </value>
  public double FrameRate { get; set; }

  /// <summary>
  /// Gets or sets the numerator component of the video frame rate.
  /// </summary>
  /// <value>
  /// The integer numerator used for exact frame rate representation.
  /// </value>
  public int FrameRateNumerator { get; set; }

  /// <summary>
  /// Gets or sets the denominator component of the video frame rate.
  /// </summary>
  /// <value>
  /// The integer denominator used for exact frame rate representation.
  /// </value>
  public int FrameRateDenominator { get; set; }

  /// <summary>
  /// Gets or sets the video frame rate mode.
  /// </summary>
  /// <value>
  /// The video frame rate mode.
  /// </value>
  public FrameRateMode FrameRateMode { get; set; }

  /// <summary>
  /// Gets or sets the video width.
  /// </summary>
  /// <value>
  /// The video width.
  /// </value>
  public int Width { get; set; }

  /// <summary>
  /// Gets or sets the video height.
  /// </summary>
  /// <value>
  /// The video height.
  /// </value>
  public int Height { get; set; }

  /// <summary>
  /// Gets or sets the video bitrate.
  /// </summary>
  /// <value>
  /// The video bitrate.
  /// </value>
  public double Bitrate { get; set; }

  /// <summary>
  /// Gets or sets the video aspect ratio.
  /// </summary>
  /// <value>
  /// The video aspect ratio.
  /// </value>
  public AspectRatio AspectRatio { get; set; }

  /// <summary>
  /// Gets or sets a value indicating whether this <see cref="VideoStream"/> is interlaced.
  /// </summary>
  /// <value>
  ///   <c>true</c> if interlaced; otherwise, <c>false</c>.
  /// </value>
  public bool Interlaced { get; set; }

  /// <summary>
  /// Gets or sets the video stereoscopic mode.
  /// </summary>
  /// <value>
  /// The video stereoscopic mode.
  /// </value>
  public StereoMode Stereoscopic { get; set; }

  /// <summary>
  /// Gets or sets the video format.
  /// </summary>
  /// <value>
  /// The video format.
  /// </value>
  public string Format { get; set; } = default!;

  /// <summary>
  /// Gets or sets the video codec.
  /// </summary>
  /// <value>
  /// The video codec.
  /// </value>
  public VideoCodec Codec { get; set; }

  /// <summary>
  /// Gets or sets the video codec profile.
  /// </summary>
  /// <value>
  /// The video codec profile.
  /// </value>
  public string CodecProfile { get; set; } = default!;

  /// <summary>
  /// Gets or sets the video standard.
  /// </summary>
  /// <value>
  /// Possible values:
  /// PAL
  /// NTSC
  /// </value>
  public VideoStandard Standard { get; set; }

  /// <summary>
  /// Gets or sets the video color space.
  /// </summary>
  /// <value>
  /// The video color space.
  /// </value>
  public ColorSpace ColorSpace { get; set; }

  /// <summary>
  /// Gets or sets the video transfer characteristics.
  /// </summary>
  /// <value>
  /// The video transfer characteristics.
  /// </value>
  public TransferCharacteristic TransferCharacteristics { get; set; }

  /// <summary>
  /// Gets or sets the video chroma sub-sampling.
  /// </summary>
  /// <value>
  /// The video chroma sub-sampling.
  /// </value>
  public ChromaSubSampling SubSampling { get; set; }

  /// <summary>
  /// Gets or sets the stream duration.
  /// </summary>
  /// <value>
  /// The stream duration.
  /// </value>
  public TimeSpan Duration { get; set; }

  /// <summary>
  /// Gets or sets the time code associated with the first decoded video frame.
  /// </summary>
  /// <value>
  /// The first frame time code in MediaInfo text form, usually `HH:MM:SS:FF`.
  /// </value>
  public string TimeCodeFirstFrame { get; set; } = default!;

  /// <summary>
  /// Gets or sets the time code associated with the last decoded video frame.
  /// </summary>
  /// <value>
  /// The last frame time code in MediaInfo text form, usually `HH:MM:SS:FF`.
  /// </value>
  public string TimeCodeLastFrame { get; set; } = default!;

  /// <summary>
  /// Gets or sets a value indicating whether the video time code uses drop-frame notation.
  /// </summary>
  /// <value>
  ///   <c>true</c> if the video time code uses drop-frame notation; otherwise, <c>false</c>.
  /// </value>
  public bool TimeCodeDropFrame { get; set; }

  /// <summary>
  /// Gets or sets additional MediaInfo time code settings for the video stream.
  /// </summary>
  /// <value>
  /// A raw string describing extra time code flags or configuration reported by MediaInfo.
  /// </value>
  public string TimeCodeSettings { get; set; } = default!;

  /// <summary>
  /// Gets or sets the source from which the video time code was obtained.
  /// </summary>
  /// <value>
  /// The raw MediaInfo source description, such as container or embedded stream metadata.
  /// </value>
  public string TimeCodeSource { get; set; } = default!;

  /// <summary>
  /// Gets or sets the detected HDR format for the video stream.
  /// </summary>
  /// <value>
  /// The HDR format derived from MediaInfo metadata, including HDR10, Dolby Vision, HLG, and HDR Vivid.
  /// </value>
  public Hdr Hdr { get;set; }

  /// <summary>
  /// Gets or sets the video bit depth.
  /// </summary>
  /// <value>
  /// The video bit depth.
  /// </value>
  public int BitDepth { get; set; }

  /// <summary>
  /// Gets or sets the name of the video codec.
  /// </summary>
  /// <value>
  /// The name of the video codec.
  /// </value>
  public string CodecName { get; set; } = default!;

  /// <summary>
  /// Gets the video resolution.
  /// </summary>
  /// <value>
  /// The video resolution.
  /// </value>
  public string Resolution => GetVideoResolution();

  /// <summary>
  /// Gets or sets the scan type of the video stream as the library reports it, for example <c>Progressive</c>,
  /// <c>Interlaced</c> or <c>MBAFF</c>.
  /// </summary>
  /// <value>
  /// The scan type of the video stream.
  /// </value>
  public string ScanType { get; set; } = default!;

  /// <summary>
  /// Gets or sets the display aspect ratio of the video stream as the library reports it, for example <c>16:9</c>
  /// or <c>1.778</c>.
  /// </summary>
  /// <value>
  /// The display aspect ratio of the video stream.
  /// </value>
  public string DisplayAspectRatio { get; set; } = default!;

  /// <summary>
  /// Gets the video size.
  /// </summary>
  /// <value>
  /// The video size.
  /// </value>
  public Size Size => new(Width, Height);

  /// <summary>
  /// Gets the video stream tags.
  /// </summary>
  /// <value>
  /// The video stream tags.
  /// </value>
  public VideoTags Tags { get; internal set; } = new();

  /// <summary>
  /// Gets the video rotation in degrees.
  /// </summary>
  /// <value>
  /// The video rotation in degrees.
  /// </value>
  public double Rotation { get; set; }

  private string GetVideoResolution()
  {
    var result = (Width, Height) switch
    {
      var (width, height) when width >= 7680 || height >= 4320 => "4320",
      var (width, height) when width >= 3200 || height >= 2100 => "2160",
      var (width, height) when width >= 1800 || height >= 1000 => "1080",
      var (width, height) when width >= 1200 || height >= 700 => "720",
      (_, var height) when height >= 560 => "576",
      (_, var height) when height >= 460 => "480",
      (_, var height) when height >= 350 => "360",
      (_, var height) when height >= 230 => "240",
      _ => "SD"
    };

    return result != "SD"
      ? result + (Interlaced ? "I" : "P")
      : result;
  }
}
