#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using MediaInfo.TestFilesGenerator.Models;

namespace MediaInfo.TestFilesGenerator;

/// <summary>
/// Generates <see cref="AudioParameters"/> using a seeded <see cref="Random"/>
/// so that the same seed always produces the same file set.
/// </summary>
/// <param name="seed">The RNG seed.</param>
internal sealed class ParameterGenerator(int seed)
{
  private static readonly AudioFormat[] AllFormats =
  {
      AudioFormat.AC3,
      AudioFormat.DTS,
      AudioFormat.AAC,
      AudioFormat.Wav,
      AudioFormat.Opus,
      AudioFormat.Flac,
      AudioFormat.Mp2,
      AudioFormat.Mp3,
      AudioFormat.Vorbis,
      AudioFormat.TrueHd,
      AudioFormat.RealAudio
  };

  private readonly Random _rng = new Random(seed);

  /// <summary>
  /// Picks a random format, then generates valid parameters for it.
  /// </summary>
  public AudioParameters GenerateRandom() =>
    Generate(AllFormats[_rng.Next(AllFormats.Length)]);

  public AudioParameters Generate(AudioFormat format) =>
    format switch
    {
      AudioFormat.AC3 => GenerateAc3(),
      AudioFormat.DTS => GenerateDts(),
      AudioFormat.AAC => GenerateAac(),
      AudioFormat.Wav => GenerateWav(),
      AudioFormat.Opus => GenerateOpus(),
      AudioFormat.Flac => GenerateFlac(),
      AudioFormat.Mp2 => GenerateMp2(),
      AudioFormat.Mp3 => GenerateMp3(),
      AudioFormat.Vorbis => GenerateVorbis(),
      AudioFormat.TrueHd => GenerateTrueHd(),
      AudioFormat.RealAudio => GenerateRealAudio(),
      _ => throw new ArgumentOutOfRangeException(nameof(format)),
    };

  #region Format-specific builders

  private AudioParameters GenerateAc3()
  {
    int channels = Pick(FormatConstraints.Ac3Channels);
    return new(
      AudioFormat.AC3,
      channels,
      16,
      Pick(GetAc3Bitrates(channels)),
      BitrateMode.CBR,
      Pick(FormatConstraints.Ac3SampleRates),
      Pick(FormatConstraints.Durations),
      0
    );
  }

  private AudioParameters GenerateDts()
  {
    int channels = Pick(FormatConstraints.DtsChannels);
    return new(
      AudioFormat.DTS,
      channels,
      16,
      Pick(GetDtsBitrates(channels)),
      BitrateMode.CBR,
      Pick(FormatConstraints.DtsSampleRates),
      Pick(FormatConstraints.Durations),
      0
    );
  }

  private AudioParameters GenerateAac()
  {
    bool isVbr = _rng.NextDouble() < 0.35; // ~35 % of AAC files use VBR
    return new AudioParameters(
      AudioFormat.AAC,
      Pick(FormatConstraints.AacChannels),
      16,
      isVbr ? 0 : Pick(FormatConstraints.AacBitrates),
      isVbr ? BitrateMode.VBR : BitrateMode.CBR,
      Pick(FormatConstraints.AacSampleRates),
      Pick(FormatConstraints.Durations),
      isVbr ? Pick(FormatConstraints.AacVbrQualities) : 0
    );
  }

  private AudioParameters GenerateWav()
  {
    var bitDepth = Pick(FormatConstraints.WavBitDepths);
    var channels = Pick(FormatConstraints.WavChannels);
    var sampleRate = Pick(FormatConstraints.WavSampleRates);
    return new AudioParameters(
      AudioFormat.Wav,
      channels,
      bitDepth,
      channels * bitDepth * (int)sampleRate / 1000, // kbps, calculated
      BitrateMode.CBR,
      sampleRate,
      Pick(FormatConstraints.Durations),
      0
    );
  }

  private AudioParameters GenerateOpus()
  {
    var channels = Pick(FormatConstraints.OpusChannels);
    return new AudioParameters(
      AudioFormat.Opus,
      channels,
      16,
      Pick(channels > 1 ? FormatConstraints.OpusWideBitrates : FormatConstraints.OpusBitrates),
      BitrateMode.CBR,
      Pick(FormatConstraints.OpusSampleRates),
      Pick(FormatConstraints.Durations),
      0
    );
  }

  private AudioParameters GenerateFlac() =>
    new(
      AudioFormat.Flac,
      Pick(FormatConstraints.FlacChannels),
      Pick(FormatConstraints.FlacBitDepths),
      0,
      BitrateMode.VBR,
      Pick(FormatConstraints.FlacSampleRates),
      Pick(FormatConstraints.Durations),
      // Lossless, so the quality column carries the compression level instead.
      Pick(FormatConstraints.FlacCompressionLevels)
    );

  private AudioParameters GenerateMp2()
  {
    var mpeg1 = _rng.NextDouble() < 0.5;
    return new AudioParameters(
      AudioFormat.Mp2,
      Pick(FormatConstraints.Mp2Channels),
      16,
      Pick(mpeg1 ? FormatConstraints.Mp2Mpeg1Bitrates : FormatConstraints.Mp2Mpeg2Bitrates),
      BitrateMode.CBR,
      Pick(mpeg1 ? FormatConstraints.Mp2Mpeg1SampleRates : FormatConstraints.Mp2Mpeg2SampleRates),
      Pick(FormatConstraints.Durations),
      0
    );
  }

  private AudioParameters GenerateMp3()
  {
    var isVbr = _rng.NextDouble() < 0.35; // ~35 % of MP3 files use VBR
    return new AudioParameters(
      AudioFormat.Mp3,
      Pick(FormatConstraints.Mp3Channels),
      16,
      isVbr ? 0 : Pick(FormatConstraints.Mp3Bitrates),
      isVbr ? BitrateMode.VBR : BitrateMode.CBR,
      Pick(FormatConstraints.Mp3SampleRates),
      Pick(FormatConstraints.Durations),
      isVbr ? Pick(FormatConstraints.Mp3VbrQualities) : 0
    );
  }

  private AudioParameters GenerateVorbis() =>
    new(
      AudioFormat.Vorbis,
      Pick(FormatConstraints.VorbisChannels),
      16,
      0,
      BitrateMode.VBR,
      Pick(FormatConstraints.VorbisSampleRates),
      Pick(FormatConstraints.Durations),
      Pick(FormatConstraints.VorbisVbrQualities)
    );

  private AudioParameters GenerateTrueHd() =>
    new(
      AudioFormat.TrueHd,
      Pick(FormatConstraints.TrueHdChannels),
      Pick(FormatConstraints.TrueHdBitDepths),
      0,
      BitrateMode.VBR,
      Pick(FormatConstraints.TrueHdSampleRates),
      // TrueHD is lossless and its files are large, so they stay short.
      Pick(FormatConstraints.Durations) / 3 + 3,
      0
    );

  private AudioParameters GenerateRealAudio() =>
    new(
      AudioFormat.RealAudio,
      1,
      16,
      8,
      BitrateMode.CBR,
      Pick(FormatConstraints.RealAudioSampleRates),
      Pick(FormatConstraints.Durations),
      0
    );

  #endregion

  #region Helpers

  private T Pick<T>(T[] arr) => arr[_rng.Next(arr.Length)];

  /// <summary>
  /// Returns the subset of AC3 bitrates that are valid for the given
  /// channel count (encoder rejects too-low bitrates for wide configs).
  /// </summary>
  private static int[] GetAc3Bitrates(int channels) =>
    channels switch
    {
        1 => [32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192],
        2 => [32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 384],
        4 => [96, 112, 128, 160, 192, 224, 256, 320, 384, 448],
        _ => [192, 224, 256, 320, 384, 448, 512, 576, 640],// 6ch (5.1)
    };

  /// <summary>
  /// Returns the subset of DTS bitrates that are valid for the given channel
  /// count. The DCA encoder has a floor per configuration and rejects anything
  /// under it: 192 kbps for mono, 320 for stereo, 576 for quad and 768 for 5.1.
  /// </summary>
  private static int[] GetDtsBitrates(int channels) =>
    channels switch
    {
        1 => [192, 256, 320, 384, 448, 512, 576, 640, 768, 960, 1024, 1152, 1280, 1344, 1409, 1509],
        2 => [320, 384, 448, 512, 576, 640, 768, 960, 1024, 1152, 1280, 1344, 1409, 1509],
        4 => [576, 640, 768, 960, 1024, 1152, 1280, 1344, 1409, 1509],
        _ => [768, 960, 1024, 1152, 1280, 1344, 1409, 1509],// 6ch (5.1)
    };

  #endregion
}
