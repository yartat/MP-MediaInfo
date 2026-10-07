#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Net;

namespace MediaInfo.Analysis.Rtsp;

/// <summary>
/// Describes how much of an RTSP stream is captured before it is described.
/// </summary>
/// <remarks>
/// A live stream has no end, so the capture has to be bounded. It stops at whichever limit is reached first, and the
/// defaults are chosen to be long enough for a camera to send a key frame and short enough that describing a stream
/// still feels immediate.
/// </remarks>
public sealed class RtspAnalysisOptions
{
  /// <summary>
  /// Gets the default options.
  /// </summary>
  public static RtspAnalysisOptions Default { get; } = new();

  /// <summary>
  /// Gets or sets how long media is captured for.
  /// </summary>
  /// <value>The default value is three seconds.</value>
  public TimeSpan CaptureDuration { get; set; } = TimeSpan.FromSeconds(3);

  /// <summary>
  /// Gets or sets the greatest number of bytes of elementary stream to capture.
  /// </summary>
  /// <value>The default value is 8 megabytes.</value>
  public int MaximumCaptureBytes { get; set; } = 8 * 1024 * 1024;

  /// <summary>
  /// Gets or sets the number of complete access units after which the capture may stop early.
  /// </summary>
  /// <remarks>
  /// A handful of frames is enough for the library to describe the video. Set the value to 0 to always capture for
  /// the whole duration, which measures the frame rate and bitrate more accurately.
  /// </remarks>
  /// <value>The default value is 60.</value>
  public int SufficientFrameCount { get; set; } = 60;

  /// <summary>
  /// Gets or sets how long the connection may take to open.
  /// </summary>
  /// <value>The default value is five seconds.</value>
  public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

  /// <summary>
  /// Gets or sets how long a single read may take before the stream is considered dead.
  /// </summary>
  /// <value>The default value is five seconds.</value>
  public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(5);

  /// <summary>
  /// Gets or sets the credentials to answer an authentication challenge with.
  /// </summary>
  /// <remarks>
  /// Credentials given in the location itself are used when this is not set, so that a caller can pass one URL and
  /// nothing else.
  /// </remarks>
  public NetworkCredential? Credentials { get; set; }

  /// <summary>
  /// Gets or sets the user agent the server is told about.
  /// </summary>
  public string UserAgent { get; set; } = "MP-MediaInfo";
}
