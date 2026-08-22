#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using MediaInfo.Model;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Builds a chapter stream representation from media information for use in media processing or analysis.
  /// </summary>
  /// <param name="info">The media information source containing metadata and stream details required to construct the chapter stream.</param>
  /// <param name="number">The stream number identifying the chapter stream within the media source.</param>
  /// <param name="position">The position of the stream within the media file, used to determine stream ordering or selection.</param>
  internal class ChapterStreamBuilder(IMediaInfoReader info, int number, int position) : MediaStreamBuilder<ChapterStream>(info, number, position)
  {
    /// <inheritdoc />
    public override MediaStreamKind Kind => MediaStreamKind.Menu;

    /// <inheritdoc />
    protected override StreamKind StreamKind => StreamKind.Other;
  }
}