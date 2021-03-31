#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MediaInfo.Model
{
    /// <summary>
    /// Provides properties and overridden methods for the analyze audio stream 
    /// and contains information about audio stream.
    /// </summary>
    /// <seealso cref="LanguageMediaStream" />
    [Serializable]
    [DataContract]
    public class AudioStream : LanguageMediaStream
  {
    #region matching dictionaries

    private static readonly Dictionary<AudioCodec, string> CodecFrendlyNames = new Dictionary<AudioCodec, string>
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
    [DataMember(Name = "codec")]
    public AudioCodec Codec { get; set; }

    /// <summary>
    /// Gets the codec friendly name.
    /// </summary>
    /// <value>
    /// The codec friendly name.
    /// </value>
    public string CodecFriendly
    {
      get => CodecFrendlyNames.TryGetValue(Codec, out var result) ? result : string.Empty;
    }

    /// <summary>
    /// Gets the stream duration.
    /// </summary>
    /// <value>
    /// The stream duration.
    /// </value>
    [DataMember(Name = "duration")]
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets the audio bitrate.
    /// </summary>
    /// <value>
    /// The audio bitrate.
    /// </value>
    [DataMember(Name = "bitrate")]
    public double Bitrate { get; set; }

    /// <summary>
    /// Gets the audio channel amount.
    /// </summary>
    /// <value>
    /// The audio channel amount.
    /// </value>
    [DataMember(Name = "channel")]
    public int Channel { get; set; }

    /// <summary>
    /// Gets the audio sampling rate.
    /// </summary>
    /// <value>
    /// The audio sampling rate.
    /// </value>
    [DataMember(Name = "samplingRate")]
    public double SamplingRate { get; set; }

    /// <summary>
    /// Gets the bit depth of stream.
    /// </summary>
    /// <value>
    /// The bit depth of stream.
    /// </value>
    [DataMember(Name = "bitDepth")]
    public int BitDepth { get; set; }

    /// <summary>
    /// Gets the bitrate mode of stream.
    /// </summary>
    /// <value>
    /// The bitrate mode of stream.
    /// </value>
    [DataMember(Name = "bitrateMode")]
    public BitrateMode BitrateMode { get; set; }

    /// <summary>
    /// Gets the audio format.
    /// </summary>
    /// <value>
    /// The audio format.
    /// </value>
    [DataMember(Name = "format")]
    public string Format { get; set; }

    /// <summary>
    /// Gets the audio codec name.
    /// </summary>
    /// <value>
    /// The audio codec name.
    /// </value>
    [DataMember(Name = "codecName")]
    public string CodecName { get; set; }

    /// <summary>
    /// Gets the audio codec description.
    /// </summary>
    /// <value>
    /// The audio codec description.
    /// </value>
    [DataMember(Name = "codecDescription")]
    public string CodecDescription { get; set; }

    /// <summary>
    /// Gets the audio channels friendly.
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

    private static string ConvertAudioChannels(int channels)
    {
      string result;
      return Channels.TryGetValue(channels, out result) ? result : "Unknown";
    }
  }
}