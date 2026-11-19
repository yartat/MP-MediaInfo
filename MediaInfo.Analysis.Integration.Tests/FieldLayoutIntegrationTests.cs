#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using FluentAssertions;
using Xunit;

namespace MediaInfo.Analysis.Integration.Tests;

/// <summary>
/// Checks, against the native library that is actually loaded, that a field read by its number in the enumerations of
/// the wrapper is the field that was meant, whatever order the library keeps its fields in.
/// </summary>
[Collection(NativeMediaCollection.Name)]
public class FieldLayoutIntegrationTests
{
  [Theory]
  [InlineData("Test_MP3Tags.mp3")]
  [InlineData("Test_H264_AC3.m2ts")]
  public void Get_ByNumber_AgreesWithGetByName(string fileName)
  {
    using var mediaInfo = new MediaInfo();
    mediaInfo.Open(TestMedia.Path(fileName)).Should().NotBe(System.IntPtr.Zero);

    (int Number, string Name, StreamKind Kind)[] fields =
    [
      ((int)NativeMethods.General.General_Format, "Format", StreamKind.General),
      ((int)NativeMethods.General.General_FileSize, "FileSize", StreamKind.General),
      ((int)NativeMethods.General.General_Duration, "Duration", StreamKind.General),
      ((int)NativeMethods.General.General_OverallBitRate, "OverallBitRate", StreamKind.General),
      ((int)NativeMethods.General.General_Encoded_Library, "Encoded_Library", StreamKind.General),
      ((int)NativeMethods.Audio.Audio_Format, "Format", StreamKind.Audio),
      ((int)NativeMethods.Audio.Audio_BitRate, "BitRate", StreamKind.Audio),
      ((int)NativeMethods.Audio.Audio_SamplingRate, "SamplingRate", StreamKind.Audio),
      ((int)NativeMethods.Audio.Audio_Channel_s_, "Channel(s)", StreamKind.Audio)
    ];

    var compared = 0;
    foreach (var (number, name, kind) in fields)
    {
      var byName = mediaInfo.Get(kind, 0, name);
      var byNumber = mediaInfo.Get(kind, 0, number);

      byNumber.Should().Be(byName, $"{kind} field '{name}' read by its number");
      compared += byName.Length > 0 ? 1 : 0;
    }

    compared.Should().BeGreaterThan(3, "the file has to have the fields for the comparison to say anything");
  }

  [Fact]
  public void Get_FieldAfterTheOnesTheLibraryDoesNotHave_IsStillTheRightOne()
  {
    using var mediaInfo = new MediaInfo();
    mediaInfo.Open(TestMedia.Path("Test_MP3Tags.mp3")).Should().NotBe(System.IntPtr.Zero);

    // Title comes after the general fields that newer libraries insert, so a wrong number would read another field.
    var byName = mediaInfo.Get(StreamKind.General, 0, "Title");

    byName.Should().NotBeEmpty();
    mediaInfo.Get(StreamKind.General, 0, (int)NativeMethods.General.General_Title).Should().Be(byName);
  }

  [Fact]
  public void Get_FieldTheLibraryDoesNotHave_IsEmptyInsteadOfAnotherField()
  {
    using var mediaInfo = new MediaInfo();
    mediaInfo.Open(TestMedia.Path("Test_MP3Tags.mp3")).Should().NotBe(System.IntPtr.Zero);

    // Whether the loaded library knows the field or not, the answer for a file that does not state it is empty.
    mediaInfo.Get(StreamKind.General, 0, (int)NativeMethods.General.General_CompliantWith).Should().BeEmpty();
  }
}
