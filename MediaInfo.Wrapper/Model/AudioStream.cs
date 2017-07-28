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

using JetBrains.Annotations;

namespace MediaInfo
{
  /// <summary>
  /// Provides properties and overridden methods for the analyze audio stream 
  /// and contains information about audio stream.
  /// </summary>
  /// <seealso cref="LanguageMediaStream" />
  [PublicAPI]
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
      { AudioCodec.Wma9, "Windows Audio Pro" },
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
    public AudioCodec Codec { get; set; }

    /// <summary>
    /// Gets the codec friendly name.
    /// </summary>
    /// <value>
    /// The codec friendly name.
    /// </value>
    public string CodecFriendly
    {
      get
      {
        string result;
        return CodecFrendlyNames.TryGetValue(Codec, out result) ? result : string.Empty;
      }
    }

    /// <summary>
    /// Gets the stream duration.
    /// </summary>
    /// <value>
    /// The stream duration.
    /// </value>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets the audio bitrate.
    /// </summary>
    /// <value>
    /// The audio bitrate.
    /// </value>
    public double Bitrate { get; set; }

    /// <summary>
    /// Gets the audio channel amount.
    /// </summary>
    /// <value>
    /// The audio channel amount.
    /// </value>
    public int Channel { get; set; }

    /// <summary>
    /// Gets the audio sampling rate.
    /// </summary>
    /// <value>
    /// The audio sampling rate.
    /// </value>
    public double SamplingRate { get; set; }

    /// <summary>
    /// Gets the bit depth of stream.
    /// </summary>
    /// <value>
    /// The bit depth of stream.
    /// </value>
    public int BitDepth { get; set; }

    /// <summary>
    /// Gets the audio format.
    /// </summary>
    /// <value>
    /// The audio format.
    /// </value>
    public string Format { get; set; }

    /// <summary>
    /// Gets the audio codec name.
    /// </summary>
    /// <value>
    /// The audio codec name.
    /// </value>
    public string CodecName { get; set; }

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
    public AudioTags Tags { get; internal set; } = new AudioTags();

    private static string ConvertAudioChannels(int channels)
    {
      string result;
      return Channels.TryGetValue(channels, out result) ? result : "Unknown";
    }
  }
}