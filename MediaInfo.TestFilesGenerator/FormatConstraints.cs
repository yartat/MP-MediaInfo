#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.TestFilesGenerator;

/// <summary>
/// Static tables of valid parameter values per audio format.
/// </summary>
internal static class FormatConstraints
{
  // AC3 (Dolby Digital)
  // Standard ATSC bitrate set; only 48 kHz is valid for AC3.
  internal static readonly int[] Ac3Channels = { 1, 2, 4, 6 };
  internal static readonly double[] Ac3SampleRates = { 48000.0 };
  internal static readonly int[] Ac3Bitrates =
  {
    32, 40, 48, 56, 64, 80, 96, 112, 128,
    160, 192, 224, 256, 320, 384, 448, 512, 576, 640
  };

  // DTS
  // FFmpeg DCA encoder; valid standard CBR bitrates.
  internal static readonly int[] DtsChannels = { 1, 2, 4, 6 };
  internal static readonly double[] DtsSampleRates = { 44100.0, 48000.0 };
  internal static readonly int[] DtsBitrates =
  {
      128, 192, 256, 320, 384, 448, 512,
      576, 640, 768, 960, 1024, 1152, 1280, 1344, 1409, 1509
  };

  // AAC
  internal static readonly int[] AacChannels = { 1, 2, 4, 6, 8 };
  internal static readonly double[] AacSampleRates = { 22050.0, 32000.0, 44100.0, 48000.0 };
  internal static readonly int[] AacBitrates =
  {
      32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320
  };
  /// <summary>VBR quality levels for FFmpeg native aac encoder.</summary>
  internal static readonly int[] AacVbrQualities = { 1, 2, 3, 4, 5 };

  // WAV / PCM in MKA
  internal static readonly int[] WavChannels = { 1, 2, 4, 6, 8 };
  internal static readonly int[] WavBitDepths = { 8, 16, 24, 32 };
  internal static readonly double[] WavSampleRates =
  {
    8000.0, 11025.0, 16000.0, 22050.0, 32000.0, 44100.0, 48000.0, 96000.0
  };

  // Opus (libopus)
  // The encoder resamples everything to 48 kHz internally and the container says
  // so, so asking for anything else would put a rate in the manifest that no
  // reader will ever report.
  internal static readonly int[] OpusChannels = { 1, 2, 4, 6, 8 };
  internal static readonly double[] OpusSampleRates = { 48000.0 };
  internal static readonly int[] OpusBitrates = { 16, 32, 48, 64, 96, 128, 160, 192, 256 };
  /// <summary>Above two channels libopus accepts up to 510 kbps; mono stops at 256.</summary>
  internal static readonly int[] OpusWideBitrates = { 96, 128, 160, 192, 256, 320, 384, 450, 510 };

  // FLAC — lossless, so the bit depth is the interesting axis rather than a rate.
  internal static readonly int[] FlacChannels = { 1, 2, 4, 6, 8 };
  internal static readonly int[] FlacBitDepths = { 16, 24 };
  internal static readonly double[] FlacSampleRates = { 22050.0, 44100.0, 48000.0, 88200.0, 96000.0 };
  /// <summary>The <c>compression_level</c> the FLAC encoder takes.</summary>
  internal static readonly int[] FlacCompressionLevels = { 0, 5, 8, 12 };

  // MPEG audio layer II
  internal static readonly int[] Mp2Channels = { 1, 2 };
  internal static readonly double[] Mp2Mpeg1SampleRates = { 32000.0, 44100.0, 48000.0 };
  internal static readonly double[] Mp2Mpeg2SampleRates = { 16000.0, 22050.0, 24000.0 };
  internal static readonly int[] Mp2Mpeg1Bitrates =
  {
    32, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 384
  };
  /// <summary>Layer II at the half rates tops out at 160 kbps.</summary>
  internal static readonly int[] Mp2Mpeg2Bitrates = { 32, 48, 56, 64, 80, 96, 112, 128, 160 };

  // MPEG audio layer III (libmp3lame, which clamps rather than refusing)
  internal static readonly int[] Mp3Channels = { 1, 2 };
  internal static readonly double[] Mp3SampleRates =
  {
    8000.0, 11025.0, 12000.0, 16000.0, 22050.0, 24000.0, 32000.0, 44100.0, 48000.0
  };
  internal static readonly int[] Mp3Bitrates =
  {
    32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320
  };
  /// <summary>VBR quality levels for libmp3lame, where 0 is best.</summary>
  internal static readonly int[] Mp3VbrQualities = { 0, 2, 4, 6, 9 };

  // Vorbis (libvorbis), which is asked for a quality rather than a rate.
  internal static readonly int[] VorbisChannels = { 1, 2, 4, 6, 8 };
  internal static readonly double[] VorbisSampleRates = { 22050.0, 32000.0, 44100.0, 48000.0 };
  internal static readonly int[] VorbisVbrQualities = { 0, 2, 4, 6, 8, 10 };

  // Dolby TrueHD — lossless, and the encoder takes neither quad nor 7.1.
  internal static readonly int[] TrueHdChannels = { 1, 2, 6 };
  internal static readonly int[] TrueHdBitDepths = { 16, 24 };
  internal static readonly double[] TrueHdSampleRates = { 44100.0, 48000.0, 88200.0, 96000.0, 176400.0, 192000.0 };

  // RealAudio 1.0 is 8 kHz mono at a fixed rate and offers nothing to vary.
  internal static readonly double[] RealAudioSampleRates = { 8000.0 };

  // Shared
  internal static readonly int[] Durations = { 3, 5, 7, 10, 15, 20, 30 };
}
