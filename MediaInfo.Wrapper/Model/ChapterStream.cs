#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Diagnostics.CodeAnalysis;

namespace MediaInfo.Model
{
  /// <summary>
  /// Provides properties and overridden methods for the analyze chapter in media
  /// and contains information about chapter.
  /// </summary>
  /// <seealso cref="MediaStream" />
  public class ChapterStream : MediaStream
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ChapterStream"/> class.
    /// </summary>
    /// <param name="offset">The offset.</param>
    /// <param name="description">The description.</param>
    [SetsRequiredMembers]
    public ChapterStream(double offset, string description)
    {
      Offset = offset;
      Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChapterStream"/> class.
    /// </summary>
    [SetsRequiredMembers]
    public ChapterStream()
      : this(0, string.Empty)
    {
    }

      /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Chapter;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Other;

    /// <summary>
    /// Gets the chapter offset.
    /// </summary>
    /// <value>
    /// The chapter offset.
    /// </value>
    public required double Offset { get; init;}

    /// <summary>
    /// Gets the chapter description.
    /// </summary>
    /// <value>
    /// The chapter description.
    /// </value>
    public required string Description { get; init; }
  }
}