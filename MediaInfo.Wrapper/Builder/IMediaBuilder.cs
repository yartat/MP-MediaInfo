#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using MediaInfo.Model;

namespace MediaInfo.Builder
{
  /// <summary>
  /// Describes media builder interface
  /// </summary>
  /// <typeparam name="TStream">The type of the stream.</typeparam>
  internal interface IMediaBuilder<out TStream> where TStream : MediaStream
  {
    /// <summary>
    /// Builds media stream.
    /// </summary>
    /// <returns></returns>
    TStream Build();
  }
}