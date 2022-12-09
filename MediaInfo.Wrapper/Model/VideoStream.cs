#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Drawing;

namespace MediaInfo.Model;

/// <summary>
/// Describes properties of the video stream and method to analyze stream
/// </summary>
/// <seealso cref="LanguageMediaStream" />
public class VideoStream : LanguageMediaStream
{
    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Video;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Video;

    /// <summary>
    /// The video frame rate.
    /// </summary>
    public double FrameRate { get; set; }

    /// <summary>
    /// The video frame rate mode.
    /// </summary>
    public FrameRateMode FrameRateMode { get; set; }

    /// <summary>
    /// The video width.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// The video height.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// The video bitrate.
    /// </summary>
    public double Bitrate { get; set; }

    /// <summary>
    /// The video aspect ratio.
    /// </summary>
    public AspectRatio AspectRatio { get; set; }

    /// <summary>
    /// A value indicating whether this <see cref="VideoStream"/> is interlaced.
    /// </summary>
    /// <value>
    ///   <c>true</c> if interlaced; otherwise, <c>false</c>.
    /// </value>
    public bool Interlaced { get; set; }

    /// <summary>
    /// The video stereoscopic mode.
    /// </summary>
    public StereoMode Stereoscopic { get; set; }

    /// <summary>
    /// The video format.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// The video codec.
    /// </summary>
    public VideoCodec Codec { get; set; }

    /// <summary>
    /// The video codec profile.
    /// </summary>
    public string? CodecProfile { get; set; }

    /// <summary>
    /// The video standard.
    /// </summary>
    /// <value>
    /// Possible values:
    /// <list type="bullet">
    /// <item>PAL</item>
    /// <item>NTSC</item>
    /// </list>
    /// </value>
    public VideoStandard Standard { get; set; }

    /// <summary>
    /// The video color space.
    /// </summary>
    public ColorSpace ColorSpace { get; set; }

    /// <summary>
    /// The video transfer characteristics.
    /// </summary>
    public TransferCharacteristic TransferCharacteristics { get; set; }

    /// <summary>
    /// The video chroma sub-sampling.
    /// </summary>
    public ChromaSubSampling SubSampling { get; set; }

    /// <summary>
    /// The stream duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// The video HDR type.
    /// </summary>
    public Hdr Hdr { get; set; }

    /// <summary>
    /// The video bit depth.
    /// </summary>
    public int BitDepth { get; set; }

    /// <summary>
    /// The name of the video codec.
    /// </summary>
    public string CodecName { get; set; } = default!;

    /// <summary>
    /// The video resolution.
    /// </summary>
    public string Resolution => GetVideoResolution();

    /// <summary>
    /// The video size.
    /// </summary>
    public Size Size => new(Width, Height);

    /// <summary>
    /// The video stream tags.
    /// </summary>
    public VideoTags Tags { get; internal set; } = new();

    private string GetVideoResolution()
    {
        string result;
        if (Width >= 7680 || Height >= 4320)
        {
            result = "4320";
        }
        else if (Width >= 3200 || Height >= 2100)
        {
            result = "2160";
        }
        else if (Width >= 1800 || Height >= 1000)
        {
            result = "1080";
        }
        else if (Width >= 1200 || Height >= 700)
        {
            result = "720";
        }
        else if (Height >= 560)
        {
            result = "576";
        }
        else if (Height >= 460)
        {
            result = "480";
        }
        else if (Height >= 350)
        {
            result = "360";
        }
        else if (Height >= 230)
        {
            result = "240";
        }
        else
        {
            result = "SD";
        }

        if (result != "SD")
        {
            result += Interlaced ? "I" : "P";
        }

        return result;
    }
}