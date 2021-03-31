#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

namespace MediaInfo.Model
{
  /// <summary>
  /// Describes video chroma sub sampling
  /// </summary>
  public enum ChromaSubSampling
  {
    /// <summary>
    /// 3:3:2
    /// </summary>
    Sampling332,

    /// <summary>
    /// 4:1:0
    /// </summary>
    Sampling410,

    /// <summary>
    /// 4:1:1
    /// </summary>
    Sampling411,

    /// <summary>
    /// 4:2:0
    /// </summary>
    Sampling420,

    /// <summary>
    /// 4:2:2
    /// </summary>
    Sampling422,

    /// <summary>
    /// 4:4:4
    /// </summary>
    Sampling444,

    /// <summary>
    /// 4:4:4:4
    /// </summary>
    Sampling4444,

    /// <summary>
    /// 5:5:5
    /// </summary>
    Sampling555,

    /// <summary>
    /// 5:6:5
    /// </summary>
    Sampling565,

    /// <summary>
    /// 8:8:8
    /// </summary>
    Sampling888
  }
}
