#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using MediaInfo.Model;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Describes method to build chapter stream.
  /// </summary>
  internal class ChapterStreamBuilder : MediaStreamBuilder<ChapterStream>
  {
    public ChapterStreamBuilder(MediaInfo info, int number, int position)
      : base(info, number, position)
    {
    }

    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Menu;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Other;
  }
}