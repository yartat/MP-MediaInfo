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

  // The names a library gives to the fields of a stream kind, written the way the library writes them.
  private static List<string> NamesOf(Type enumeration, params string[] without) =>
    Enum.GetNames(enumeration)
      .Select(x => x.Substring(x.IndexOf('_') + 1))
      .Where(x => !without.Contains(x))
      .ToList();

  private static Func<int, string> Reading(List<string> names) =>
    index => index >= 0 && index < names.Count ? names[index] : string.Empty;

  [Fact]
  public void FieldLayout_SameOrderAsTheWrapper_NeedsNoTranslation()
  {
    var layout = new FieldLayout();

    layout.GetTable(StreamKind.Image, Reading(NamesOf(typeof(NativeMethods.Image)))).Should().BeNull();

    layout.TryGetKnown(StreamKind.Image, out var table).Should().BeTrue();
    table.Should().BeNull();
  }

  [Fact]
  public void FieldLayout_LibraryWithoutAFieldTheWrapperKnows_FindsTheOthersByName()
  {
    // The library of 26.10: there is no CompliantWith, so every general field after it is one number lower.
    var layout = new FieldLayout();
    var library = NamesOf(typeof(NativeMethods.General), "CompliantWith");

    var table = layout.GetTable(StreamKind.General, Reading(library));

    FieldLayout.Apply(table, (int)NativeMethods.General.General_CompliantWith).Should().Be(-1);
    FieldLayout.Apply(table, (int)NativeMethods.General.General_Format).Should().Be((int)NativeMethods.General.General_Format);
    var after = (int)NativeMethods.General.General_InternetMediaType;
    after.Should().BeGreaterThan((int)NativeMethods.General.General_CompliantWith);
    FieldLayout.Apply(table, after).Should().Be(after - 1);
    library[after - 1].Should().Be("InternetMediaType");
  }

  [Fact]
  public void FieldLayout_LibraryWithAFieldTheWrapperDoesNotKnow_SkipsIt()
  {
    var layout = new FieldLayout();
    var library = NamesOf(typeof(NativeMethods.Video));
    library.Insert(10, "NotInTheWrapper");

    var table = layout.GetTable(StreamKind.Video, Reading(library));

    FieldLayout.Apply(table, 9).Should().Be(9);
    FieldLayout.Apply(table, 10).Should().Be(11);
    FieldLayout.Apply(table, (int)NativeMethods.Video.Video_Format).Should().Be((int)NativeMethods.Video.Video_Format + 1);
    library[(int)NativeMethods.Video.Video_Format + 1].Should().Be("Format");
  }

  [Fact]
  public void FieldLayout_NumberBeyondTheEnumeration_IsPassedOn()
  {
    var layout = new FieldLayout();
    var library = NamesOf(typeof(NativeMethods.Menu));
    library.Insert(3, "Another");
    var table = layout.GetTable(StreamKind.Menu, Reading(library));

    // A field the media added to its own stream, such as a chapter, is numbered by the library and is not translated.
    FieldLayout.Apply(table, 5000).Should().Be(5000);
    FieldLayout.Apply(table, -1).Should().Be(-1);
  }

  [Fact]
  public void FieldLayout_NamesWithSlashes_MatchTheNamesOfTheEnumeration()
  {
    var layout = new FieldLayout();
    var library = NamesOf(typeof(NativeMethods.Image));
    library.Insert(0, "Zero");
    library.Remove("Zero");
    var name = library.Single(x => x == "Format_Settings_SliceCount_String");
    library[library.IndexOf(name)] = "Format_Settings_SliceCount/String";

    layout.GetTable(StreamKind.Image, Reading(library)).Should().BeNull("a slash in a name is the same as an underscore");
  }

  [Fact]
  public void FieldLayout_LibraryAnswersNothing_IsInconclusiveAndNotRemembered()
  {
    var layout = new FieldLayout();

    layout.GetTable(StreamKind.Video, _ => string.Empty).Should().BeNull();
    layout.TryGetKnown(StreamKind.Video, out _).Should().BeFalse();

    var library = NamesOf(typeof(NativeMethods.Video));
    library.Insert(10, "NotInTheWrapper");
    layout.GetTable(StreamKind.Video, Reading(library)).Should().NotBeNull();
  }

  [Fact]
  public void FieldLayout_ConclusiveOutcome_IsKeptForTheLifetimeOfTheLibrary()
  {
    var layout = new FieldLayout();
    layout.GetTable(StreamKind.Audio, Reading(NamesOf(typeof(NativeMethods.Audio)))).Should().BeNull();

    // The order of the fields belongs to the library, so a second media does not repeat the look.
    var other = NamesOf(typeof(NativeMethods.Audio));
    other.Insert(5, "Extra");
    layout.GetTable(StreamKind.Audio, Reading(other)).Should().BeNull();
  }

  [Fact]
  public void FieldLayout_EveryMemberOfTheEnumerationsHasAName()
  {
    // The wrapper's own enumerations must be consistent with themselves: no two members may stand for one field.
    foreach (var type in new[]
             {
               typeof(NativeMethods.General), typeof(NativeMethods.Video), typeof(NativeMethods.Audio),
               typeof(NativeMethods.Text), typeof(NativeMethods.Other), typeof(NativeMethods.Image), typeof(NativeMethods.Menu)
             })
    {
      var names = NamesOf(type).Select(FieldLayout.Normalize).ToList();

      names.Should().OnlyHaveUniqueItems(type.Name);
    }
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
