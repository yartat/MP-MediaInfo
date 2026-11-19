#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MediaInfo.Analysis.Pipeline;
using MediaInfo.Analysis.Strategies.Selection;
using MediaInfo.Builder;
using MediaInfo.Model;
using Xunit;

namespace MediaInfo.Analysis.Tests;

/// <summary>Tests for the check of the loaded native library against the one the wrapper was built for.</summary>
public class LibraryCompatibilityTests
{
  [Theory]
  [InlineData("MediaInfoLib - v26.10")]
  [InlineData("MediaInfoLib - v26.10.1")]
  [InlineData("26.10")]
  [InlineData("")]
  [InlineData(null)]
  [InlineData("unknown build")]
  public void CheckVersion_MatchingOrUnknown_ReportsNothing(string? version) =>
    LibraryCompatibility.CheckVersion(version).Should().BeNull();

  [Theory]
  [InlineData("MediaInfoLib - v26.05", "26.05")]
  [InlineData("MediaInfoLib - v27.01", "27.01")]
  [InlineData("MediaInfoLib - v25.10", "25.10")]
  public void CheckVersion_OtherVersion_NamesBothVersions(string version, string found)
  {
    var message = LibraryCompatibility.CheckVersion(version);

    message.Should().Contain(found).And.Contain(LibraryCompatibility.ExpectedVersion);
  }

  [Fact]
  public void Inspect_MatchingLayout_ReportsNothing() =>
    new FieldLayoutInspector().Inspect(new LayoutReader(StreamKind.Image), StreamKind.Image).Should().BeNull();

  [Fact]
  public void Inspect_FieldsInsertedInTheMiddle_ReportsTheFirstDifference()
  {
    // What upstream MediaInfoLib after 26.10 does to the image table: two fields appear before MuxingMode.
    var reader = new LayoutReader(StreamKind.Image, insertAt: 43, "Format_Settings_SliceCount", "Format_Settings_SliceCount/String");

    var message = new FieldLayoutInspector().Inspect(reader, StreamKind.Image);

    message.Should()
      .Contain("Image")
      .And.Contain("#43")
      .And.Contain("Format_Settings_SliceCount")
      .And.Contain(LibraryCompatibility.ExpectedVersion);
  }

  [Fact]
  public void Inspect_LibraryAnswersNothing_IsInconclusiveAndNotRemembered()
  {
    var inspector = new FieldLayoutInspector();

    inspector.Inspect(new LayoutReader(StreamKind.Video, silent: true), StreamKind.Video).Should().BeNull();

    // A later, conclusive check is not masked by the earlier silence.
    inspector
      .Inspect(new LayoutReader(StreamKind.Video, insertAt: 10, "Extra"), StreamKind.Video)
      .Should().NotBeNull();
  }

  [Fact]
  public void Inspect_ConclusiveOutcome_IsKeptForTheLifetimeOfTheLibrary()
  {
    var inspector = new FieldLayoutInspector();
    inspector.Inspect(new LayoutReader(StreamKind.Audio), StreamKind.Audio).Should().BeNull();

    // The order of the fields belongs to the library, so a second media does not repeat the check.
    inspector
      .Inspect(new LayoutReader(StreamKind.Audio, insertAt: 5, "Extra"), StreamKind.Audio)
      .Should().BeNull();
  }

  [Fact]
  public void Collect_ReaderThatReportsWarnings_PutsThemInTheResult()
  {
    var reader = new WarningReader("The library does not match.");

    var result = new MediaStreamCollector(DefaultStreamSelectionStrategy.Instance).Collect(reader);

    result.Warnings.Should().Equal("The library does not match.");
  }

  [Fact]
  public void Collect_ReaderWithoutCompatibilityCheck_HasNoWarnings()
  {
    var result = new MediaStreamCollector(DefaultStreamSelectionStrategy.Instance).Collect(new LayoutReader(StreamKind.General, silent: true));

    result.Warnings.Should().BeEmpty();
  }

  [Theory]
  [InlineData("Dolby Vision / SMPTE ST 2086", Hdr.DolbyVision)]
  [InlineData("SMPTE ST 2094 App 4 / SMPTE ST 2086", Hdr.HDR10Plus)]
  [InlineData("SMPTE ST 2086 / Dolby Vision", Hdr.DolbyVision)]
  [InlineData("HDR10 / HDR10", Hdr.HDR10)]
  [InlineData("HLG / SMPTE ST 2086", Hdr.HLG)]
  [InlineData("Dolby Vision", Hdr.DolbyVision)]
  [InlineData("T/UWA 005 (HDR Vivid)", Hdr.HdrVivid)]
  public void TryGetHdr_ListOfFormats_ReturnsTheMostSpecific(string source, Hdr expected)
  {
    HdrUtils.TryGetHdr(source, out var result).Should().BeTrue();

    result.Should().Be(expected);
  }

  [Theory]
  [InlineData("")]
  [InlineData("Unknown / Other")]
  public void TryGetHdr_NothingRecognized_ReturnsFalse(string source)
  {
    HdrUtils.TryGetHdr(source, out var result).Should().BeFalse();

    result.Should().Be(Hdr.None);
  }

  private static Type EnumOf(StreamKind kind) => kind switch
  {
    StreamKind.General => typeof(NativeMethods.General),
    StreamKind.Video => typeof(NativeMethods.Video),
    StreamKind.Audio => typeof(NativeMethods.Audio),
    StreamKind.Text => typeof(NativeMethods.Text),
    StreamKind.Other => typeof(NativeMethods.Other),
    StreamKind.Image => typeof(NativeMethods.Image),
    _ => typeof(NativeMethods.Menu)
  };

  /// <summary>A reader that names the fields of one stream kind the way the library does, optionally with fields inserted.</summary>
  private class LayoutReader : EmptyReader
  {
    private readonly StreamKind _kind;
    private readonly bool _silent;
    private readonly List<string> _names;

    public LayoutReader(StreamKind kind, int insertAt = -1, params string[] inserted)
      : this(kind, false, insertAt, inserted)
    {
    }

    public LayoutReader(StreamKind kind, bool silent)
      : this(kind, silent, -1)
    {
    }

    private LayoutReader(StreamKind kind, bool silent, int insertAt, params string[] inserted)
    {
      _kind = kind;
      _silent = silent;
      _names = Enum.GetNames(EnumOf(kind)).Select(x => x.Substring(x.IndexOf('_') + 1)).ToList();
      if (insertAt >= 0)
      {
        _names.InsertRange(insertAt, inserted);
      }
    }

    public override string Get(StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo) =>
      !_silent && streamKind == _kind && kindOfInfo == InfoKind.Name && parameter >= 0 && parameter < _names.Count
        ? _names[parameter]
        : string.Empty;
  }

  private sealed class WarningReader(string warning) : EmptyReader, ILibraryCompatibilityProvider
  {
    public string[] GetCompatibilityWarnings() => [warning];
  }

  private abstract class EmptyReader : IMediaInfoReader
  {
    public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch) => string.Empty;

    public virtual string Get(StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo) => string.Empty;

    public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo) => string.Empty;

    public string Get(StreamKind streamKind, int streamNumber, string parameter) => string.Empty;

    public string Get(StreamKind streamKind, int streamNumber, int parameter) => string.Empty;

    public int CountGet(StreamKind streamKind, int streamNumber) => 0;

    public int CountGet(StreamKind streamKind) => 0;

    public string Option(string option, string value) => string.Empty;

    public string Option(string option) => string.Empty;

    public string Inform() => string.Empty;
  }
}
