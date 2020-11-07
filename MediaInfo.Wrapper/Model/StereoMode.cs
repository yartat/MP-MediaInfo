#region Copyright (C) 2017-2020 Yaroslav Tatarenko

// Copyright (C) 2017-2020 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2020 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.Model
{
  /// <summary>
  /// Describes 3D stereo mode
  /// </summary>
  public enum StereoMode
  {
    /// <summary>
    /// No 3D (mono)
    /// </summary>
    Mono,

    /// <summary>
    /// Stereo mode without additional info
    /// </summary>
    Stereo,

    /// <summary>
    /// The side by side left eye is first
    /// </summary>
    SideBySideLeft,

    /// <summary>
    /// The top bottom right eye is first
    /// </summary>
    TopBottomRight,

    /// <summary>
    /// The top bottom left eye is first
    /// </summary>
    TopBottomLeft,

    /// <summary>
    /// The checkerboard right eye is first
    /// </summary>
    CheckerboardRight,

    /// <summary>
    /// The checkerboard left eye is first
    /// </summary>
    CheckerboardLeft,

    /// <summary>
    /// The row interleaved right eye is first
    /// </summary>
    RowInterleavedRight,

    /// <summary>
    /// The row interleaved left eye is first
    /// </summary>
    RowInterleavedLeft,

    /// <summary>
    /// The column interleaved right eye is first
    /// </summary>
    ColumnInterleavedRight,

    /// <summary>
    /// The column interleaved left eye is first
    /// </summary>
    ColumnInterleavedLeft,

    /// <summary>
    /// The anaglyph cyan-red
    /// </summary>
    AnaglyphCyanRed,

    /// <summary>
    /// The side by side right eye is first
    /// </summary>
    SideBySideRight,

    /// <summary>
    /// The anaglyph green-magenta
    /// </summary>
    AnaglyphGreenMagenta,

    /// <summary>
    /// The both eyes laced left eye is first
    /// </summary>
    BothEyesLacedLeft,

    /// <summary>
    /// The both eyes laced right eye is first
    /// </summary>
    BothEyesLacedRight
  }
}