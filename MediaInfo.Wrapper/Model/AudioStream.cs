#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;

namespace MediaInfo.Model;

/// <summary>
/// Provides properties and overridden methods for the analyze audio stream
/// and contains information about audio stream.
/// </summary>
/// <seealso cref="LanguageMediaStream" />
public class AudioStream : LanguageMediaStream
{
    #region matching dictionaries

    private static readonly Dictionary<AudioCodec, string> CodecFrendlyNames = new()
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
        { AudioCodec.Dts, "DTS" },
        { AudioCodec.DtsHd, "DTS-HD" },
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
    };

    private static readonly Dictionary<int, string> Channels = new()
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
    };

    #endregion

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Audio;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Audio;

    /// <summary>
    /// The audio codec.
    /// </summary>
    public AudioCodec Codec { get; set; }

    /// <summary>
    /// The codec friendly name.
    /// </summary>
    public string CodecFriendly
    {
        get => CodecFrendlyNames.TryGetValue(Codec, out var result) ? result : string.Empty;
    }

    /// <summary>
    /// The stream duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// The audio bitrate.
    /// </summary>
    public double Bitrate { get; set; }

    /// <summary>
    /// The audio channel amount.
    /// </summary>
    public int Channel { get; set; }

    /// <summary>
    /// The audio sampling rate.
    /// </summary>
    public double SamplingRate { get; set; }

    /// <summary>
    /// The bit depth of stream.
    /// </summary>
    public int BitDepth { get; set; }

    /// <summary>
    /// The bitrate mode of stream.
    /// </summary>
    public BitrateMode BitrateMode { get; set; }

    /// <summary>
    /// The audio format.
    /// </summary>
    public string Format { get; set; } = default!;

    /// <summary>
    /// The audio codec name.
    /// </summary>
    public string CodecName { get; set; } = default!;

    /// <summary>
    /// The audio codec description.
    /// </summary>
    public string? CodecDescription { get; set; }

    /// <summary>
    /// The audio channels friendly.
    /// </summary>
    public string AudioChannelsFriendly => ConvertAudioChannels(Channel);

    /// <summary>
    /// The stream tags.
    /// </summary>
    public AudioTags Tags { get; internal set; } = new AudioTags();

    private static string ConvertAudioChannels(int channels) =>
        Channels.TryGetValue(channels, out var result) ?
            result :
            "Unknown";
}