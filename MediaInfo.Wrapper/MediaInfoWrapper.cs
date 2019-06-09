#region Copyright (C) 2005-2019 Team MediaPortal

// Copyright (C) 2005-2019 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;

using MediaInfo.Builder;
using MediaInfo.Model;

namespace MediaInfo
{
  /// <summary>
  /// Describes method and properties to retrieve information from media source
  /// </summary>
  public class MediaInfoWrapper
  {
    #region private vars

    private static readonly Dictionary<string, bool> SubTitleExtensions = new Dictionary<string, bool> 
    {
      { ".AQT", true },
      { ".ASC", true },
      { ".ASS", true },
      { ".DAT", true },
      { ".DKS", true },
      { ".IDX", true },
      { ".JS", true },
      { ".JSS", true },
      { ".LRC", true },
      { ".MPL", true },
      { ".OVR", true },
      { ".PAN", true },
      { ".PJS", true },
      { ".PSB", true },
      { ".RT", true },
      { ".RTF", true },
      { ".S2K", true },
      { ".SBT", true },
      { ".SCR", true },
      { ".SMI", true },
      { ".SON", true },
      { ".SRT", true },
      { ".SSA", true },
      { ".SST", true },
      { ".SSTS", true },
      { ".STL", true },
      { ".SUB", true },
      { ".TXT", true },
      { ".VKT", true },
      { ".VSF", true },
      { ".ZEG", true },
    };

    private readonly ILogger _logger;
    private readonly string _filePath;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaInfoWrapper"/> class.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="logger">The logger instance.</param>
    public MediaInfoWrapper(string filePath, ILogger logger = null)
#if (NET40 || NET45)
      : this (filePath, Environment.Is64BitProcess ? @".\x64" : @".\x86", logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaInfoWrapper"/> class.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="pathToDll">The path to DLL.</param>
    /// <param name="logger">the logger instance.</param>
    protected MediaInfoWrapper(string filePath, string pathToDll, ILogger logger)
#endif
    {
      _filePath = filePath;
      _logger = logger;
      logger.LogDebug("Analyzing media {0}.", filePath);
      VideoStreams = new List<VideoStream>();
      AudioStreams = new List<AudioStream>();
      Subtitles = new List<SubtitleStream>();
      Chapters = new List<ChapterStream>();
      MenuStreams = new List<MenuStream>();

      if (string.IsNullOrEmpty(filePath))
      {
        MediaInfoNotloaded = true;
        logger.LogError("Media file name to processing is null or empty");
        return;
      }

#if (NET40 || NET45)
      var realPathToDll = IfExistsPath(logger, ".\\", () => IfExistsPath(logger, pathToDll, () => null));
#endif

      var isTv = filePath.IsLiveTv();
      var isRadio = filePath.IsLiveTv();
      var isRtsp = filePath.IsRtsp(); // RTSP for live TV and recordings.
      var isAvStream = filePath.IsAvStream(); //other AV streams

      //currently disabled for all tv/radio
      if (isTv || isRadio || isRtsp)
      {
        MediaInfoNotloaded = true;
        logger.LogError($"Media file is {(isTv ? "TV" : isRadio ? "radio" : isRtsp ? "RTSP" : string.Empty)}");
        return;
      }

      NumberFormatInfo providerNumber = new NumberFormatInfo { NumberDecimalSeparator = "." };

      try
      {
        // Analyze local file for DVD and BD
        if (!isAvStream)
        {
          if (filePath.EndsWith(".ifo", StringComparison.OrdinalIgnoreCase))
          {
#if (NET40 || NET45)
            filePath = ProcessDvd(filePath, realPathToDll, providerNumber);
#else
            filePath = ProcessDvd(filePath, providerNumber);
#endif
          }
          else if (filePath.EndsWith(".bdmv", StringComparison.OrdinalIgnoreCase))
          {
            IsBluRay = true;
            filePath = Path.GetDirectoryName(filePath);
            Size = GetDirectorySize(filePath);
          }
          else
          {
            Size = new FileInfo(filePath).Length;
          }

          HasExternalSubtitles = !string.IsNullOrEmpty(filePath) && CheckHasExternalSubtitles(filePath);
        }

#if (NET40 || NET45)
        ExtractInfo(filePath, realPathToDll, providerNumber);
#else
        ExtractInfo(filePath, providerNumber);
#endif
        logger.LogDebug($"Process file {filePath} was completed successfully. Video={VideoStreams.Count}, Audio={AudioStreams.Count}, Subtitle={Subtitles.Count}");
      }
      catch (Exception exception)
      {
        logger.LogError(exception, "Error processing media file");
      }
    }

    /// <summary>
    /// Writes the media information data to log.
    /// </summary>
    public void WriteInfo()
    {
      _logger.LogInformation($"Inspecting media    : {_filePath}");
      if (MediaInfoNotloaded)
      { 
        _logger.LogWarning($"MediaInfo.dll was not loaded!");
      }
      else
      {
        _logger.LogDebug($"DLL version         : {Version}");
        
        // General
        _logger.LogDebug($"Media duration      : {TimeSpan.FromMilliseconds(Duration)}");
        _logger.LogDebug($"Has audio           : {(AudioStreams?.Count ?? 0) > 0}");
        _logger.LogDebug($"Has video           : {HasVideo}");
        _logger.LogDebug($"Has subtitles       : {HasSubtitles}");
        _logger.LogDebug($"Has chapters        : {HasChapters}");
        _logger.LogDebug($"Is DVD              : {IsDvd}");
        _logger.LogDebug($"Is Blu-Ray disk     : {IsBluRay}");

        // Video
        if (HasVideo)
        {
          _logger.LogDebug($"Video duration      : {BestVideoStream?.Duration ?? TimeSpan.MinValue}");
          _logger.LogDebug($"Video frame rate    : {Framerate}");
          _logger.LogDebug($"Video width         : {Width}");
          _logger.LogDebug($"Video height        : {Height}");
          _logger.LogDebug($"Video aspect ratio  : {AspectRatio}");
          _logger.LogDebug($"Video codec         : {VideoCodec}");
          _logger.LogDebug($"Video scan type     : {ScanType}");
          _logger.LogDebug($"Is video interlaced : {IsInterlaced}");
          _logger.LogDebug($"Video resolution    : {VideoResolution}");
          _logger.LogDebug($"Video 3D mode       : {BestVideoStream?.Stereoscopic ?? StereoMode.Mono}");
          _logger.LogDebug($"Video HDR standard  : {BestVideoStream?.Hdr ?? Hdr.None}");
        }

        // Audio
        if ((AudioStreams?.Count ?? 0) > 0)
        {
          _logger.LogDebug($"Audio duration      : {BestAudioStream?.Duration ?? TimeSpan.MinValue}");
          _logger.LogDebug($"Audio rate          : {AudioRate}");
          _logger.LogDebug($"Audio channels      : {AudioChannelsFriendly}");
          _logger.LogDebug($"Audio codec         : {AudioCodec}");
          _logger.LogDebug($"Audio bit depth     : {BestAudioStream?.BitDepth ?? 0}");
        }

        // Subtitles
        if (HasSubtitles)
        { 
          _logger.LogDebug($"Subtitles count     : {Subtitles?.Count ?? 0}");
        }

        // Chapters
        if (HasChapters)
        {
          _logger.LogDebug($"Chapters count      : {Chapters?.Count ?? 0}");
        }
      }
    }

#if (NET40 || NET45)
    private string IfExistsPath(ILogger logger, string pathToDll, Func<string> anotherPath)
    {
      var result = anotherPath();
      if (!string.IsNullOrEmpty(result))
      { 
        return result;
      }

      logger.LogDebug("Check MediaInfo.dll from {0}.", pathToDll);
      if (!MediaInfoExist(pathToDll))
      {
        MediaInfoNotloaded = true;
        logger.LogWarning($"Library MediaInfo.dll was not found at {pathToDll}");
        return null;
      }

      return pathToDll;
    }
#endif

    private static long GetDirectorySize(string folderName)
    {
      if (!Directory.Exists(folderName))
      {
        return 0L;
      }

      var result = Directory.GetFiles(folderName).Sum(x => new FileInfo(x).Length);
      result += Directory.GetDirectories(folderName).Sum(x => GetDirectorySize(x));
      return result;
    }

#if (NET40 || NET45)

    private string ProcessDvd(string filePath, string pathToDll, NumberFormatInfo providerNumber)
#else
    private string ProcessDvd(string filePath, NumberFormatInfo providerNumber)
#endif
    {
      IsDvd = true;
      var path = Path.GetDirectoryName(filePath) ?? string.Empty;
      Size = GetDirectorySize(path);
      var bups = Directory.GetFiles(path, "*.BUP", SearchOption.TopDirectoryOnly);
      var programBlocks = new List<Tuple<string, int>>();
      foreach (var bupFile in bups)
      {
#if (NET40 || NET45)
        using (var mi = new MediaInfo(pathToDll))
#else
        using (var mi = new MediaInfo())
#endif
        {
          Version = mi.Option("Info_Version");
          mi.Open(bupFile);
          var profile = mi.Get(StreamKind.General, 0, "Format_Profile");
          if (profile == "Program")
          {
              double.TryParse(
              mi.Get(StreamKind.Video, 0, "Duration"),
              NumberStyles.AllowDecimalPoint,
              providerNumber,
              out var duration);
            programBlocks.Add(new Tuple<string, int>(bupFile, (int)duration));
          }
        }
      }

      // get all other info from main title's 1st vob
      if (programBlocks.Any())
      {
        Duration = programBlocks.Max(x => x.Item2);
        filePath = programBlocks.First(x => x.Item2 == Duration).Item1;
      }

      return filePath;
    }

#if (NET40 || NET45)
    private void ExtractInfo(string filePath, string pathToDll, NumberFormatInfo providerNumber)
#else
    private void ExtractInfo(string filePath, NumberFormatInfo providerNumber)
#endif
    {
#if (NET40 || NET45)
      using (var mediaInfo = new MediaInfo(pathToDll))
#else
      using (var mediaInfo = new MediaInfo())
#endif
      {
        if (mediaInfo.Handle == IntPtr.Zero)
        {
          MediaInfoNotloaded = true;
          _logger.LogWarning("MediaInfo library was not loaded!");
          return;
        }
        else
        {
          Version = mediaInfo.Option("Info_Version");
          _logger.LogDebug($"MediaInfo library was loaded. Handle={mediaInfo.Handle} Version is {Version}");
        }

        var filePricessingHandle = mediaInfo.Open(filePath);
        if (filePricessingHandle == IntPtr.Zero)
        {
          MediaInfoNotloaded = true;
          _logger.LogWarning($"MediaInfo library has not been opened media {filePath}");
          return;
        }
        else
        {
          _logger.LogDebug($"MediaInfo library successfully opened {filePath}. Handle={filePricessingHandle}");
        }

        var streamNumber = 0;

        Tags = new AudioTagBuilder(mediaInfo, 0).Build();

        // Setup videos
        for (var i = 0; i < mediaInfo.CountGet(StreamKind.Video); ++i)
        {
          VideoStreams.Add(new VideoStreamBuilder(mediaInfo, streamNumber++, i).Build());
        }

        if (Duration == 0)
        {
            double.TryParse(
            mediaInfo.Get(StreamKind.Video, 0, (int)NativeMethods.Video.Video_Duration),
            NumberStyles.AllowDecimalPoint,
            providerNumber,
            out var duration);
          Duration = (int)duration;
        }

        // Fix 3D for some containers
        if (VideoStreams.Count == 1 && Tags.GeneralTags.TryGetValue((NativeMethods.General)1000, out var isStereo))
        {
          var video = VideoStreams.First();
          if (Tags.GeneralTags.TryGetValue((NativeMethods.General)1001, out var stereoMode))
          { 
            video.Stereoscopic = (StereoMode) stereoMode;
          }
          else
          { 
            video.Stereoscopic = (bool) isStereo ? StereoMode.Stereo : StereoMode.Mono;
          }
        }

        // Setup audios
        for (var i = 0; i < mediaInfo.CountGet(StreamKind.Audio); ++i)
        {
          AudioStreams.Add(new AudioStreamBuilder(mediaInfo, streamNumber++, i).Build());
        }

        if (Duration == 0)
        {
            double.TryParse(
                mediaInfo.Get(StreamKind.Audio, 0, (int)NativeMethods.Audio.Audio_Duration),
                NumberStyles.AllowDecimalPoint,
                providerNumber,
                out var duration);
            Duration = (int)duration;
        }

        // Setup subtitles
        for (var i = 0; i < mediaInfo.CountGet(StreamKind.Text); ++i)
        {
          Subtitles.Add(new SubtitleStreamBuilder(mediaInfo, streamNumber++, i).Build());
        }

        // Setup chapters
        for (var i = 0; i < mediaInfo.CountGet(StreamKind.Other); ++i)
        {
          Chapters.Add(new ChapterStreamBuilder(mediaInfo, streamNumber++, i).Build());
        }

        // Setup menus
        for (var i = 0; i < mediaInfo.CountGet(StreamKind.Menu); ++i)
        {
          MenuStreams.Add(new MenuStreamBuilder(mediaInfo, streamNumber++, i).Build());
        }

        MediaInfoNotloaded = VideoStreams.Count == 0 && AudioStreams.Count == 0 && Subtitles.Count == 0;

        // Produce capability properties
        if (MediaInfoNotloaded)
        {
          SetPropertiesDefault();
        }
        else
        {
          SetupProperties(mediaInfo);
        }
      }
    }

    private void SetPropertiesDefault()
    {
      VideoCodec = string.Empty;
      VideoResolution = string.Empty;
      ScanType = string.Empty;
      AspectRatio = string.Empty;
      AudioCodec = string.Empty;
      AudioChannelsFriendly = string.Empty;
    }

    private void SetupProperties(MediaInfo mediaInfo)
    {
      BestVideoStream = VideoStreams.OrderByDescending(
          x => (long)x.Width * x.Height * x.BitDepth * (x.Stereoscopic == StereoMode.Mono ? 1L : 2L) * x.FrameRate * (x.Bitrate <= 1e-7 ? 1 : x.Bitrate))
        .FirstOrDefault();
      VideoCodec = BestVideoStream?.CodecName ?? string.Empty;
      VideoRate = (int?)BestVideoStream?.Bitrate ?? 0;
      VideoResolution = BestVideoStream?.Resolution ?? string.Empty;
      Width = BestVideoStream?.Width ?? 0;
      Height = BestVideoStream?.Height ?? 0;
      IsInterlaced = BestVideoStream?.Interlaced ?? false;
      Framerate = BestVideoStream?.FrameRate ?? 0;
      ScanType = BestVideoStream != null
                   ? mediaInfo.Get(StreamKind.Video, BestVideoStream.StreamPosition, "ScanType").ToLower()
                   : string.Empty;
      AspectRatio = BestVideoStream != null
                      ? mediaInfo.Get(StreamKind.Video, BestVideoStream.StreamPosition, "DisplayAspectRatio")
                      : string.Empty;
      AspectRatio = BestVideoStream != null ?
          AspectRatio == "4:3" || AspectRatio == "1.333" ? "fullscreen" : "widescreen" : 
          string.Empty;

      BestAudioStream = AudioStreams.OrderByDescending(x => x.Channel * 10000000 + x.Bitrate).FirstOrDefault();
      AudioCodec = BestAudioStream?.CodecName ?? string.Empty;
      AudioRate = (int?)BestAudioStream?.Bitrate ?? 0;
      AudioSampleRate = (int?)BestAudioStream?.SamplingRate ?? 0;
      AudioChannels = BestAudioStream?.Channel ?? 0;
      AudioChannelsFriendly = BestAudioStream?.AudioChannelsFriendly ?? string.Empty;
    }

#endregion

    /// <summary>
    /// Checks if mediaInfo.dll file exist.
    /// </summary>
    /// <param name="pathToDll">The path to mediaInfo.dll</param>
    /// <returns>Returns <b>true</b> if mediaInfo.dll is exists; elsewhere <b>false</b>.</returns>
    public static bool MediaInfoExist(string pathToDll)
    {
      return File.Exists(Path.Combine(pathToDll, "MediaInfo.dll"));
    }

#region private methods

    private static bool CheckHasExternalSubtitles(string strFile)
    {
      if (string.IsNullOrEmpty(strFile))
      {
        return false;
      }

      var filenameNoExt = Path.GetFileNameWithoutExtension(strFile);
      try
      {
        return Directory.GetFiles(Path.GetDirectoryName(strFile) ?? string.Empty, filenameNoExt + "*")
          .Select(file => new FileInfo(file))
          .Any(fi => SubTitleExtensions.ContainsKey(fi.Extension.ToUpper()));
      }
      catch
      {
        return false;
      }
    }

#endregion

#region public video related properties

    /// <summary>
    /// Gets a value indicating whether this instance has video.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance has video; otherwise, <c>false</c>.
    /// </value>
    public bool HasVideo => VideoStreams.Count > 0;

    /// <summary>
    /// Gets a value indicating whether media has at least one video stream with stereoscopic effect.
    /// </summary>
    /// <value>
    ///   <c>true</c> if is3d; otherwise, <c>false</c>.
    /// </value>
    public bool Is3D => VideoStreams.Any(x => x.Stereoscopic != StereoMode.Mono);

    /// <summary>
    /// Gets a value indicating whether media has at least one video stream with HDR effect.
    /// </summary>
    /// <value>
    ///   <c>true</c> if video stream with HDR effect; otherwise, <c>false</c>.
    /// </value>
    public bool IsHdr => VideoStreams.Any(x => x.Hdr != Hdr.None);

    /// <summary>
    /// Gets the video streams.
    /// </summary>
    /// <value>
    /// The video streams.
    /// </value>
    public IList<VideoStream> VideoStreams { get; }

    /// <summary>
    /// Gets the best video stream.
    /// </summary>
    /// <value>
    /// The best video stream.
    /// </value>
    public VideoStream BestVideoStream { get; private set; }

    /// <summary>
    /// Gets the video codec.
    /// </summary>
    /// <value>
    /// The video codec.
    /// </value>
    public string VideoCodec { get; private set; }

    /// <summary>
    /// Gets the video frame rate.
    /// </summary>
    /// <value>
    /// The video frame rate.
    /// </value>
    public double Framerate { get; private set; }

    /// <summary>
    /// Gets the video width.
    /// </summary>
    /// <value>
    /// The video width.
    /// </value>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the video height.
    /// </summary>
    /// <value>
    /// The video height.
    /// </value>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the video aspect ratio.
    /// </summary>
    /// <value>
    /// The video aspect ratio.
    /// </value>
    public string AspectRatio { get; private set; }

    /// <summary>
    /// Gets the type of the scan.
    /// </summary>
    /// <value>
    /// The type of the scan.
    /// </value>
    public string ScanType { get; private set; }

    /// <summary>
    /// Gets a value indicating whether video is interlaced.
    /// </summary>
    /// <value>
    ///   <c>true</c> if video is interlaced; otherwise, <c>false</c>.
    /// </value>
    public bool IsInterlaced { get; private set; }

    /// <summary>
    /// Gets the video resolution.
    /// </summary>
    /// <value>
    /// The video resolution.
    /// </value>
    public string VideoResolution { get; private set; }

    /// <summary>
    /// Gets the video bitrate.
    /// </summary>
    /// <value>
    /// The video bitrate.
    /// </value>
    public int VideoRate { get; private set; }

#endregion

#region public audio related properties

    /// <summary>
    /// Gets the audio streams.
    /// </summary>
    /// <value>
    /// The audio streams.
    /// </value>
    public IList<AudioStream> AudioStreams { get; }

    /// <summary>
    /// Gets the best audio stream.
    /// </summary>
    /// <value>
    /// The best audio stream.
    /// </value>
    public AudioStream BestAudioStream { get; private set; }

    /// <summary>
    /// Gets the audio codec.
    /// </summary>
    /// <value>
    /// The audio codec.
    /// </value>
    public string AudioCodec { get; private set; }

    /// <summary>
    /// Gets the audio bitrate.
    /// </summary>
    /// <value>
    /// The audio bitrate.
    /// </value>
    public int AudioRate { get; private set; }

    /// <summary>
    /// Gets the audio sample rate.
    /// </summary>
    /// <value>
    /// The audio sample rate.
    /// </value>
    public int AudioSampleRate { get; private set; }

    /// <summary>
    /// Gets the count of audio channels.
    /// </summary>
    /// <value>
    /// The count of audio channels.
    /// </value>
    public int AudioChannels { get; private set; }

    /// <summary>
    /// Gets the audio channels friendly name.
    /// </summary>
    /// <value>
    /// The audio channels friendly name.
    /// </value>
    public string AudioChannelsFriendly { get; private set; }

#endregion

#region public subtitles related properties

    /// <summary>
    /// Gets the list of media subtitles.
    /// </summary>
    /// <value>
    /// The media subtitles.
    /// </value>
    public IList<SubtitleStream> Subtitles { get; }

    /// <summary>
    /// Gets a value indicating whether media has internal or external subtitles.
    /// </summary>
    /// <value>
    ///   <c>true</c> if media has subtitles; otherwise, <c>false</c>.
    /// </value>
    public bool HasSubtitles => HasExternalSubtitles || Subtitles.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this instance has external subtitles.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance has external subtitles; otherwise, <c>false</c>.
    /// </value>
    public bool HasExternalSubtitles { get; }

#endregion

#region public chapters related properties

    /// <summary>
    /// Gets the media chapters.
    /// </summary>
    /// <value>
    /// The media chapters.
    /// </value>
    public IList<ChapterStream> Chapters { get; }

    /// <summary>
    /// Gets a value indicating whether media has chapters.
    /// </summary>
    /// <value>
    ///   <c>true</c> if media has chapters; otherwise, <c>false</c>.
    /// </value>
    public bool HasChapters => Chapters.Count > 0;

#endregion

#region public menu related properties

    /// <summary>
    /// Gets the menu streams from media.
    /// </summary>
    /// <value>
    /// The menu streams.
    /// </value>
    public IList<MenuStream> MenuStreams { get; }

    /// <summary>
    /// Gets a value indicating whether media has menu.
    /// </summary>
    /// <value>
    ///   <c>true</c> if media has menu; otherwise, <c>false</c>.
    /// </value>
    public bool HasMenu => MenuStreams.Count > 0;

#endregion

    /// <summary>
    /// Gets a value indicating whether media is DVD.
    /// </summary>
    /// <value>
    ///   <c>true</c> if media is DVD; otherwise, <c>false</c>.
    /// </value>
    public bool IsDvd { get; private set; }

    /// <summary>
    /// Gets a value indicating whether media is BluRay.
    /// </summary>
    /// <value>
    ///   <c>true</c> if media is BluRay; otherwise, <c>false</c>.
    /// </value>
    public bool IsBluRay { get; }

    /// <summary>
    /// Gets a value indicating whether media information was not loaded.
    /// </summary>
    /// <value>
    ///   <c>true</c> if media information was not loaded; otherwise, <c>false</c>.
    /// </value>
    public bool MediaInfoNotloaded { get; private set; }

    /// <summary>
    /// Gets the duration of the media.
    /// </summary>
    /// <value>
    /// The duration of the media.
    /// </value>
    public int Duration { get; private set; }

    /// <summary>
    /// Gets the mediainfo.dll version.
    /// </summary>
    /// <value>
    /// The mediainfo.dll version.
    /// </value>
    public string Version { get; private set; }

    /// <summary>
    /// Gets the media size.
    /// </summary>
    /// <value>
    /// The media size.
    /// </value>
    public long Size { get; private set; }

    /// <summary>
    /// Gets the tags.
    /// </summary>
    /// <value>
    /// The tags.
    /// </value>
    public AudioTags Tags { get; private set; }
  }
}