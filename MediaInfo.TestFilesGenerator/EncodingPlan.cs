#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using MediaInfo.TestFilesGenerator.Models;
using MediaToolkitNet.Abstractions.Formats;
using MediaToolkitNet.Abstractions.Recording;
using MediaToolkitNet.Abstractions.Transcoding;
using AudioFormat = MediaInfo.TestFilesGenerator.Models.AudioFormat;

namespace MediaInfo.TestFilesGenerator;

/// <summary>
/// Turns one set of <see cref="AudioParameters"/> into a transcoding job: the
/// silent master in, one Matroska audio file out.
/// </summary>
internal static class EncodingPlan
{
  /// <summary>
  /// Builds the job for one file.
  /// </summary>
  /// <param name="p">The parameters that were generated for it.</param>
  /// <param name="source">The silent master to cut it from.</param>
  /// <param name="output">The file to write.</param>
  /// <returns>Returns the request.</returns>
  /// <remarks>
  /// The request holds only what every backend understands, so any of them can
  /// run it. FLAC's compression level is the one parameter left out: libavcodec
  /// and GStreamer name it differently and it has no common setting, so it stays
  /// in the manifest but the encoder's default is used.
  /// </remarks>
  public static TranscodeRequest For(AudioParameters p, string source, string output)
  {
    // Rate, channel count and layout are stated rather than inherited, because
    // the master is mono at 48 kHz and every file is something else. The
    // transcoder resamples and remixes to them before the encoder sees a sample.
    var settings = new AudioOutputSettings(CodecFor(p))
    {
      SampleRate = (int)p.SampleRate,
      Channels = p.Channels,
      ChannelMask = LayoutFor(p.Format, p.Channels),
      SampleFormat = PinnedFormatFor(p),
      Quality = QualityFor(p),
      BitrateBitsPerSecond = BitrateFor(p),
    };

    return new TranscodeRequest(output)
    {
      Inputs = [source],
      Container = MediaContainer.Matroska,
      Streams = [OutputStream.Audio(StreamSource.First(MediaStreamKind.Audio), settings)],

      // The master is as long as the longest file; each is cut from its start.
      End = TimeSpan.FromSeconds(p.DurationSeconds),
      CopyChapters = false,
    };
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
      // Lossless: nothing to ask for.
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

  /// <summary>
  /// Chooses where the speakers are.
  /// </summary>
  /// <remarks>
  /// A channel count alone does not say: four channels are quad as often as they
  /// are 3.1. The layout named here is the one the remix aims at, so it has to be
  /// one the encoder takes: the DCA and TrueHD encoders take only the side
  /// arrangements, which is why those two differ.
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
