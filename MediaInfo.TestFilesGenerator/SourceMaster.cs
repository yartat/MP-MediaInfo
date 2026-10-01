#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.IO;
using System.Text;

namespace MediaInfo.TestFilesGenerator;

/// <summary>
/// The one input every test file is transcoded from: a stretch of silence in a
/// plain WAV file.
/// </summary>
/// <remarks>
/// A transcoder reads files, so the silence has to exist as one before anything
/// can be made from it. It is written here byte by byte rather than through a
/// backend, which keeps it the same whichever backend then reads it. Mono at
/// 48 kHz is enough: every output is resampled and remixed to what its
/// parameters ask for, and silence stays silence through both. It is as long
/// as the longest file, and each output is cut from its start.
/// </remarks>
internal sealed class SourceMaster : IDisposable
{
  private const int SampleRate = 48000;
  private const short Channels = 1;
  private const short BitsPerSample = 16;

  private SourceMaster(string path) => Path = path;

  /// <summary>Gets the WAV file.</summary>
  public string Path { get; }

  /// <summary>
  /// Writes the silence to a temporary file.
  /// </summary>
  /// <param name="seconds">How long it has to be.</param>
  /// <returns>Returns the master, which deletes the file when disposed.</returns>
  public static SourceMaster Create(int seconds)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seconds);

    var path = System.IO.Path.Combine(
      System.IO.Path.GetTempPath(), $"mediainfo-testfiles-{Guid.NewGuid():n}.wav");

    const short blockAlign = Channels * BitsPerSample / 8;
    var dataSize = (uint)(SampleRate * blockAlign * seconds);

    using (var writer = new BinaryWriter(File.Create(path), Encoding.ASCII))
    {
      writer.Write("RIFF"u8);
      writer.Write(36u + dataSize);
      writer.Write("WAVE"u8);

      writer.Write("fmt "u8);
      writer.Write(16u);
      writer.Write((short)1);                        // PCM
      writer.Write(Channels);
      writer.Write(SampleRate);
      writer.Write(SampleRate * blockAlign);         // bytes per second
      writer.Write(blockAlign);
      writer.Write(BitsPerSample);

      writer.Write("data"u8);
      writer.Write(dataSize);
      writer.Write(new byte[dataSize]);
    }

    return new SourceMaster(path);
  }

  /// <inheritdoc />
  public void Dispose()
  {
    try
    {
      File.Delete(Path);
    }
    catch (IOException)
    {
      // A temporary file left behind is not worth failing a run over.
    }
  }
}
