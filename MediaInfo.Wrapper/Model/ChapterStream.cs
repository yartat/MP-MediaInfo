#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

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
    public ChapterStream(double offset, string description)
    {
      Offset = offset;
      Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChapterStream"/> class.
    /// </summary>
    public ChapterStream()
    {
    }

      /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Menu;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Other;

    /// <summary>
    /// Gets the chapter offset.
    /// </summary>
    /// <value>
    /// The chapter offset.
    /// </value>
    public double Offset { get; }

    /// <summary>
    /// Gets the chapter description.
    /// </summary>
    /// <value>
    /// The chapter description.
    /// </value>
    public string Description { get; }
  }
}