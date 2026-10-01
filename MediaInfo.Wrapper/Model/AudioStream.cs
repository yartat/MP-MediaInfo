#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MediaInfo.Model
{
    /// <summary>
    /// Represents an audio stream detected by MediaInfo and exposes its technical metadata.
    /// </summary>
    /// <remarks>
    /// This model contains codec, bitrate, sampling, channel layout, timing, and time code information
    /// for a single audio stream.
    /// </remarks>
    /// <seealso cref="LanguageMediaStream" />
    public class AudioStream : LanguageMediaStream
  {
    #region matching dictionaries

    private static readonly Dictionary<AudioCodec, string> CodecFriendlyNames = new()
    {
      { AudioCodec.Undefined, "" },
      { AudioCodec.MpegLayer1, "MPEG Layer 1" },
      { AudioCodec.MpegLayer2, "MPEG Layer 2" },
      { AudioCodec.MpegLayer3, "MPEG Layer 3" },
      { AudioCodec.PcmIntBig, "PCM" },
      { AudioCodec.PcmIntLit, "PCM" },
      { AudioCodec.PcmFloatIeee, "PCM" },
      { AudioCodec.Ac3, "Dolby Digital" },
      { AudioCodec.Ac3Atmos, "Dolby Atmos" },
      { AudioCodec.Ac3Bsid9, "DolbyNet" },
      { AudioCodec.Ac3Bsid10, "DolbyNet" },
      { AudioCodec.Ac4, "Dolby AC-4" },
      { AudioCodec.Apac, "Apple Positional Audio Codec" },
      { AudioCodec.AuroCx, "Auro-Cx" },
      { AudioCodec.Dts, "DTS" },
      { AudioCodec.DtsHd, "DTS-HD" },
      { AudioCodec.DtsHdMa, "DTS-HD MA" },
      { AudioCodec.DtsUhd, "DTS-UHD" },
      { AudioCodec.DtsX, "DTS:X" },
      { AudioCodec.DolbyE, "Dolby E" },
      { AudioCodec.DolbyEd2, "Dolby ED2" },
      { AudioCodec.Eac3, "Dolby Digital Plus" },
      { AudioCodec.Eac3Atmos, "Dolby Atmos" },
      { AudioCodec.Flac, "FLAC" },
      { AudioCodec.Opus, "OPUS" },
      { AudioCodec.Tta1, "True Audio" },
      { AudioCodec.Vorbis, "Vorbis" },
      { AudioCodec.WavPack4, "WavPack" },
      { AudioCodec.WavPack, "WavPack" },
      { AudioCodec.Wave, "Wave" },
      { AudioCodec.Wave64, "Wave" },
      { AudioCodec.Real14_4, "Real Audio" },
      { AudioCodec.Real28_8, "Real Audio" },
      { AudioCodec.RealCook, "Real Audio" },
      { AudioCodec.RealSipr, "Real Audio" },
      { AudioCodec.RealRalf, "Real Audio" },
      { AudioCodec.RealAtrc, "Real Audio" },
      { AudioCodec.Truehd, "Dolby TrueHD" },
      { AudioCodec.TruehdAtmos, "Dolby TrueHD Atmos" },
      { AudioCodec.Mlp, "Meridian Lossless" },
      { AudioCodec.Aac, "AAC" },
      { AudioCodec.AacMpeg2Main, "AAC" },
      { AudioCodec.AacMpeg2Lc, "AAC" },
      { AudioCodec.AacMpeg2LcSbr, "AAC" },
      { AudioCodec.AacMpeg2Ssr, "AAC" },
      { AudioCodec.AacMpeg4Main, "AAC" },
      { AudioCodec.AacMpeg4Lc, "AAC" },
      { AudioCodec.AacMpeg4LcSbr, "AAC" },
      { AudioCodec.AacMpeg4LcSbrPs, "AAC" },
      { AudioCodec.AacMpeg4Ssr, "AAC" },
      { AudioCodec.AacMpeg4Ltp, "AAC" },
      { AudioCodec.Alac, "Apple Lossless" },
      { AudioCodec.Ape, "Monkey's Audio" },
      { AudioCodec.Wma1, "Windows Audio" },
      { AudioCodec.Wma2, "Windows Audio" },
      { AudioCodec.WmaPro, "Windows Audio Pro" },
      { AudioCodec.Adpcm, "ADPCM" },
      { AudioCodec.Amr, "Adaptive Multi-Rate" },
      { AudioCodec.Atrac1, "SDSS" },
      { AudioCodec.Atrac3, "ATRAC3" },
      { AudioCodec.Atrac3Plus, "ATRAC3plus" },
      { AudioCodec.AtracLossless, "ATRAC Advanced Lossless" },
      { AudioCodec.Atrac9, "ATRAC9" },
      { AudioCodec.Aptx100, "aptX100" },
    };

    private static readonly Dictionary<int, string> Channels = new Dictionary<int, string>
    {
      { 1, "Mono" },
      { 2, "Stereo" },
      { 3, "2.1" },
      { 4, "4.0" },
      { 5, "5.0" },
      { 6, "5.1" },
      { 7, "6.1" },
      { 8, "7.1" },
      { 9, "7.2" },
      { 10, "7.2.1" },
      { 12, "7.1.4" },
    };

    #endregion

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Audio;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Audio;

    /// <summary>
    /// Gets the audio codec.
    /// </summary>
    /// <value>
    /// The audio codec.
    /// </value>
    public AudioCodec Codec { get; set; }

    /// <summary>
    /// Gets the codec friendly name.
    /// </summary>
    /// <value>
    /// The codec friendly name.
    /// </value>
    public string CodecFriendly
    {
      get => CodecFriendlyNames.TryGetValue(Codec, out var result) ? result : string.Empty;
    }

    /// <summary>
    /// Gets the stream duration.
    /// </summary>
    /// <value>
    /// The stream duration.
    /// </value>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the audio frame rate reported for the stream.
    /// </summary>
    /// <value>
    /// The calculated or container-reported frame rate for the audio stream.
    /// </value>
    public double FrameRate { get; set; }

    /// <summary>
    /// Gets or sets the numerator component of the audio frame rate.
    /// </summary>
    /// <value>
    /// The integer numerator used for exact frame rate representation.
    /// </value>
    public int FrameRateNumerator { get; set; }

    /// <summary>
    /// Gets or sets the denominator component of the audio frame rate.
    /// </summary>
    /// <value>
    /// The integer denominator used for exact frame rate representation.
    /// </value>
    public int FrameRateDenominator { get; set; }

    /// <summary>
    /// Gets or sets the time code associated with the first audio frame.
    /// </summary>
    /// <value>
    /// The first frame time code in MediaInfo text form, usually `HH:MM:SS:FF`.
    /// </value>
    public string TimeCodeFirstFrame { get; set; } = default!;

    /// <summary>
    /// Gets or sets the time code associated with the last audio frame.
    /// </summary>
    /// <value>
    /// The last frame time code in MediaInfo text form, usually `HH:MM:SS:FF`.
    /// </value>
    public string TimeCodeLastFrame { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the audio time code uses drop-frame notation.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the audio time code uses drop-frame notation; otherwise, <c>false</c>.
    /// </value>
    public bool TimeCodeDropFrame { get; set; }

    /// <summary>
    /// Gets or sets additional MediaInfo time code settings for the audio stream.
    /// </summary>
    /// <value>
    /// A raw string describing extra time code flags or configuration reported by MediaInfo.
    /// </value>
    public string TimeCodeSettings { get; set; } = default!;

    /// <summary>
    /// Gets or sets the source from which the audio time code was obtained.
    /// </summary>
    /// <value>
    /// The raw MediaInfo source description, such as container or embedded stream metadata.
    /// </value>
    public string TimeCodeSource { get; set; } = default!;

    /// <summary>
    /// Gets or sets the audio bitrate.
    /// </summary>
    /// <value>
    /// The audio bitrate.
    /// </value>
    public double Bitrate { get; set; }

    /// <summary>
    /// Gets or sets the number of audio channels.
    /// </summary>
    /// <value>
    /// The audio channel amount.
    /// </value>
    public int Channel { get; set; }

    /// <summary>
    /// Gets or sets the number of dynamic objects of object-based audio (Dolby Atmos, DTS:X).
    /// </summary>
    /// <value>
    /// The number of dynamic objects, or 0 when the stream carries none or does not say.
    /// </value>
    public int DynamicObjects { get; set; }

    /// <summary>
    /// Gets or sets the audio sampling rate.
    /// </summary>
    /// <value>
    /// The audio sampling rate.
    /// </value>
    public double SamplingRate { get; set; }

    /// <summary>
    /// Gets or sets the audio bit depth.
    /// </summary>
    /// <value>
    /// The bit depth of stream.
    /// </value>
    public int BitDepth { get; set; }

    /// <summary>
    /// Gets or sets the bitrate mode of the stream.
    /// </summary>
    /// <value>
    /// The bitrate mode of stream.
    /// </value>
    [DataMember(Name = "bitrateMode")]
    public BitrateMode BitrateMode { get; set; }

    /// <summary>
    /// Gets or sets the raw MediaInfo audio format name.
    /// </summary>
    /// <value>
    /// The audio format.
    /// </value>
    public string Format { get; set; } = default!;

    /// <summary>
    /// Gets or sets the normalized audio codec name.
    /// </summary>
    /// <value>
    /// The audio codec name.
    /// </value>
    public string CodecName { get; set; } = default!;

    /// <summary>
    /// Gets or sets the commercial or descriptive codec name.
    /// </summary>
    /// <value>
    /// The audio codec description.
    /// </value>
    public string CodecDescription { get; set; } = default!;

    /// <summary>
    /// Gets a human-readable audio channel layout name.
    /// </summary>
    /// <value>
    /// The audio channels friendly.
    /// </value>
    public string AudioChannelsFriendly => ConvertAudioChannels(Channel);

    /// <summary>
    /// Gets the stream tags.
    /// </summary>
    /// <value>
    /// The stream tags.
    /// </value>
    [DataMember(Name = "tags")]
    public AudioTags Tags { get; internal set; } = new AudioTags();

    private static string ConvertAudioChannels(int channels) =>
      Channels.TryGetValue(channels, out var result) ? result : "Unknown";
  }
}