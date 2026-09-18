#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using MediaInfo.TestFilesGenerator.Models;
using MediaToolkitNet.Abstractions.Formats;
using MediaToolkitNet.Abstractions.Recording;
using AudioFormat = MediaInfo.TestFilesGenerator.Models.AudioFormat;
using ToolkitAudioFormat = MediaToolkitNet.Abstractions.Formats.AudioFormat;

namespace MediaInfo.TestFilesGenerator;

/// <summary>
/// Turns one set of <see cref="AudioParameters"/> into what the recorder needs:
/// the format of the samples that will be pushed in, and how to encode them.
/// </summary>
internal static class EncodingPlan
{
  /// <summary>
  /// Builds the plan for one file.
  /// </summary>
  /// <param name="p">The parameters that were generated for it.</param>
  /// <returns>Returns the source format and the encoder settings.</returns>
  public static (ToolkitAudioFormat Format, AudioEncodingSettings Settings) For(AudioParameters p)
  {
    // Silence is pushed as interleaved 16-bit, and the recorder converts it to
    // whatever the encoder actually takes.
    var format = new ToolkitAudioFormat(
      (int)p.SampleRate,
      p.Channels,
      SampleFormat.S16,
      LayoutFor(p.Format, p.Channels));

    var settings = new AudioEncodingSettings(format, CodecFor(p), BitrateFor(p))
    {
      SampleFormat = PinnedFormatFor(p),
      Options = OptionsFor(p),
    };

    return QualityFor(p) is { } quality
      ? (format, settings with { Quality = quality })
      : (format, settings);
  }

  private static MediaCodec CodecFor(AudioParameters p) =>
    p.Format switch
    {
      AudioFormat.AC3 => MediaCodec.Ac3,
      AudioFormat.DTS => MediaCodec.Dts,
      AudioFormat.AAC => MediaCodec.Aac,
      AudioFormat.Wav => p.BitDepth switch
      {
        8 => MediaCodec.PcmU8,
        24 => MediaCodec.PcmS24,
        32 => MediaCodec.PcmS32,
        _ => MediaCodec.PcmS16,
      },
      AudioFormat.Opus => MediaCodec.Opus,
      AudioFormat.Flac => MediaCodec.Flac,
      AudioFormat.Mp2 => MediaCodec.Mp2,
      AudioFormat.Mp3 => MediaCodec.Mp3,
      AudioFormat.Vorbis => MediaCodec.Vorbis,
      AudioFormat.TrueHd => MediaCodec.TrueHd,
      AudioFormat.RealAudio => MediaCodec.RealAudio,
      _ => throw new ArgumentOutOfRangeException(nameof(p), p.Format, "Unknown audio format."),
    };

  // PCM and the lossless codecs carry whatever the sample width and rate imply,
  // so the rate that was generated for the manifest is not something to ask the
  // encoder for. RealAudio 1.0 has one rate and does not take it either.
  private static int BitrateFor(AudioParameters p) =>
    p.Format is AudioFormat.Wav or AudioFormat.Flac or AudioFormat.TrueHd or AudioFormat.RealAudio
      || p.BitrateMode == BitrateMode.VBR
        ? 0
        : p.Bitrate * 1000;

  /// <summary>
  /// The quality to ask for, on the scale the chosen encoder uses, or nothing
  /// when the stream is a constant bitrate one.
  /// </summary>
  private static double? QualityFor(AudioParameters p) =>
    p.Format switch
    {
      // Lossless: the compression level travels as an option instead.
      AudioFormat.Flac or AudioFormat.TrueHd => null,

      // The native AAC encoder wants a fraction of the way up its own scale.
      AudioFormat.AAC when p.BitrateMode == BitrateMode.VBR => p.VbrQuality * 0.4,

      // libmp3lame and libvorbis both take -q:a as it stands.
      AudioFormat.Mp3 or AudioFormat.Vorbis when p.BitrateMode == BitrateMode.VBR => p.VbrQuality,
      _ => null,
    };

  /// <summary>
  /// The format to open the encoder with, where the width is the point rather
  /// than an implementation detail.
  /// </summary>
  private static SampleFormat? PinnedFormatFor(AudioParameters p) =>
    p.Format switch
    {
      AudioFormat.Flac => p.BitDepth >= 24 ? SampleFormat.S32 : SampleFormat.S16,
      AudioFormat.TrueHd => p.BitDepth >= 24 ? SampleFormat.S32Planar : SampleFormat.S16Planar,
      _ => null,
    };

  private static IReadOnlyDictionary<string, string>? OptionsFor(AudioParameters p) =>
    p.Format == AudioFormat.Flac
      ? new Dictionary<string, string> { ["compression_level"] = p.VbrQuality.ToString(CultureInfo.InvariantCulture) }
      : null;

  /// <summary>
  /// Chooses where the speakers are.
  /// </summary>
  /// <remarks>
  /// A channel count alone does not say: four channels are quad as often as they
  /// are 3.1. The FFmpeg command line used to pick a layout and then quietly
  /// convert it to one the encoder accepted; asking the library directly means
  /// naming a layout the encoder takes. The DCA encoder takes only the side
  /// arrangements, which is why DTS differs here.
  /// </remarks>
  private static ulong LayoutFor(AudioFormat format, int channels) =>
    format switch
    {
      // The DCA and TrueHD encoders take the side arrangements alone.
      AudioFormat.DTS or AudioFormat.TrueHd => channels switch
      {
        1 => ChannelLayout.Mono,
        2 => ChannelLayout.Stereo,
        4 => ChannelLayout.QuadSide,
        6 => ChannelLayout.FivePoint1Side,
        _ => ChannelLayout.Default(channels),
      },
      _ => channels switch
      {
        1 => ChannelLayout.Mono,
        2 => ChannelLayout.Stereo,
        4 => ChannelLayout.Quad,
        6 => ChannelLayout.FivePoint1Back,
        8 => ChannelLayout.SevenPoint1,
        _ => ChannelLayout.Default(channels),
      },
    };
}
