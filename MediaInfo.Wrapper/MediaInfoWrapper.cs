#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using MediaInfo.Builder;
using MediaInfo.Model;
#if !NETFRAMEWORK
using Microsoft.Extensions.Logging;
#endif
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MediaInfo
{
  /// <summary>
  /// Provides data for the MediaInfo events.
  /// </summary>
  /// <remarks>
  /// This class is used as a parameter for events related to media information processing.
  /// It contains a reference to the MediaInfo instance that holds the media information data.
  /// </remarks>
  /// <seealso cref="EventArgs" />
  /// <remarks>
  /// Initializes a new instance of the <see cref="MediaInfoEventArgs"/> class.
  /// </remarks>
  /// <param name="mediaInfo">The MediaInfo instance containing the media information data.</param>
  public class MediaInfoEventArgs(MediaInfo? mediaInfo) : EventArgs
  {

    /// <summary>
    /// Gets the media information.
    /// </summary>
    public MediaInfo? MediaInfo { get; } = mediaInfo;
  }

  /// <summary>
  /// Represents a wrapper around the MediaInfo library to extract and provide media information in a structured way.
  /// </summary>
  /// <remarks>
  /// The <see cref="MediaInfoWrapper"/> class serves as a high-level interface to interact with the MediaInfo library,
  /// allowing users to easily access various properties of media files such as video and audio streams, subtitles,
  /// chapters, and more. It also provides logging capabilities to track the processing of media files and handle any
  /// potential errors that may arise during the extraction of media information. The class is designed to be flexible
  /// and can be initialized with either a file path or a stream, making it suitable for a wide range of applications
  /// that require media information extraction.
  /// </remarks>
  public class MediaInfoWrapper
  {
#region private vars

    private const int BufferSize = 64 * 1024;

    private static readonly Dictionary<string, bool> SubTitleExtensions = new(StringComparer.OrdinalIgnoreCase)
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

    private static readonly NumberFormatInfo _providerNumber = new() { NumberDecimalSeparator = "." };
    private readonly ILogger? _logger;
    private readonly string? _filePath;

#endregion

#region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaInfoWrapper"/> class with the specified media stream size and logger.
    /// </summary>
    /// <param name="size">The size of the media stream.</param>
    /// <param name="logger">The logger instance. Optional.</param>
    protected MediaInfoWrapper(long size, ILogger? logger = null)
    {
      _logger = logger;
      VideoStreams = new List<VideoStream>();
      AudioStreams = new List<AudioStream>();
      Subtitles = new List<SubtitleStream>();
      Chapters = new List<ChapterStream>();
      MenuStreams = new List<MenuStream>();
      Size = size;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaInfoWrapper"/> class with the specified media stream and logger.
    /// </summary>
    /// <param name="inputStream">The source media stream.</param>
    /// <param name="logger">The logger instance. Optional.</param>
    public MediaInfoWrapper(Stream inputStream, ILogger? logger = null)
#if NETFRAMEWORK
      : this(inputStream, Environment.Is64BitProcess ? ".\\x64" : ".\\x86", logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaInfoWrapper"/> class with the specified media stream, path to MediaInfo.dll, and logger.
    /// </summary>
    /// <param name="inputStream">The source media stream.</param>
    /// <param name="pathToDll">The path to MediaInfo.dll file.</param>
    /// <param name="logger">The logger instance. Optional.</param>
    public MediaInfoWrapper(Stream inputStream, string pathToDll, ILogger? logger)
#endif
      : this(inputStream.Length, logger)
    {
      try
      {
#if NETFRAMEWORK
        var realPathToDll = GetPathToMediaInfo(pathToDll, logger);
        if (string.IsNullOrEmpty(realPathToDll))
        {
          LogError(logger, "MediaInfo.dll was not found");
          return;
        }
        ParseMedia(inputStream, realPathToDll!);
#else
        ParseMedia(inputStream);
#endif
        LogDebug(
          logger,
          "Process {file} was completed successfully. Video={video}, Audio={audio}, Subtitle={subtitle}",
          inputStream,
          VideoStreams.Count,
          AudioStreams.Count,
          Subtitles.Count);
      }
      catch (Exception ex)
      {
        LogError(logger, ex, "Error processing media stream");
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaInfoWrapper"/> class with the specified file path and logger.
    /// </summary>
    /// <param name="filePath">The path to the media file.</param>
    /// <param name="logger">The logger instance. Optional.</param>
    public MediaInfoWrapper(string filePath, ILogger? logger = null)
#if NETFRAMEWORK
      : this (filePath, Environment.Is64BitProcess ? @".\x64" : @".\x86", logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaInfoWrapper"/> class with the specified file path, path to MediaInfo.dll, and logger.
    /// </summary>
    /// <param name="filePath">The path to the media file.</param>
    /// <param name="pathToDll">The path to MediaInfo.dll file.</param>
    /// <param name="logger">The logger instance. Optional.</param>
    public MediaInfoWrapper(string filePath, string pathToDll, ILogger? logger)
#endif
       : this(0L, logger)
    {
      _filePath = filePath;
      LogDebug(_logger, "Analyzing media {path}.", filePath);
      if (string.IsNullOrEmpty(filePath))
      {
        LogError(_logger, "Media file name to processing is null or empty");
        return;
      }

#if NETFRAMEWORK
      var realPathToDll = GetPathToMediaInfo(pathToDll, logger);
      if (string.IsNullOrEmpty(realPathToDll))
      {
        LogError(_logger, "MediaInfo library was not found");
        return;
      }
#endif

      var isTv = filePath.IsLiveTv();
      var isRadio = filePath.IsLastFmStream();
      var isRtsp = filePath.IsRtsp(); // RTSP for live TV and recordings.
      var isMms = filePath.IsMms(); // MMS streams
      var isRtmp = filePath.IsRtmp(); // RTMP streams
      var isAvStream = filePath.IsAvStream(); // other AV streams

      // currently disabled for all tv/radio
      if (isRtsp || isRtmp || isMms)
      {
        LogWarning(_logger, "Media file is live stream");
        return;
      }

      if (isRadio || isTv)
      {
        string path = Path.GetDirectoryName(filePath!)!;
        string fileName = Path.GetFileName(filePath);
        string[] files = Directory.GetFiles(path!, fileName + "*.ts");
        if (files.Length > 0)
        {
          filePath = files[0];
        }
        else
        {
          LogWarning(_logger, "Media file is {media}", GetMediaFileType(isTv, isRadio, isRtsp));
          return;
        }
      }

      try
      {
        // Analyze local file for DVD and BD
        if (!isAvStream)
        {
          if (filePath.IsDvD(out var resultIfoPath))
          {
            LogDebug(_logger, "Detects DVD. Processing DVD information");
#if NETFRAMEWORK
            filePath = ProcessDvd(resultIfoPath, realPathToDll!);
#else
            filePath = ProcessDvd(resultIfoPath);
#endif
          }
          else if (filePath.IsBluRay(out var resultBdmvPath))
          {
            LogDebug(_logger, "Detects BD.");
            IsBluRay = true;
            filePath = Path.GetDirectoryName(resultBdmvPath)!;
            Size = GetDirectorySize(filePath);
          }
          else
          {
            if (File.Exists(filePath))
            {
              Size = new FileInfo(filePath).Length;
            }
            else
            {
              LogWarning(_logger, "Media file {file} does not exist", filePath);
              return;
            }
          }

          HasExternalSubtitles = !string.IsNullOrEmpty(filePath) && CheckHasExternalSubtitles(filePath, _logger);
          if (HasExternalSubtitles)
          {
            LogDebug(_logger, "Found external subtitles");
          }
        }

#if NETFRAMEWORK
        ParseMedia(filePath, realPathToDll!);
#else
        ParseMedia(filePath!);
#endif
        LogDebug(
            _logger,
            "Process {file} was completed successfully. Video={videos}, Audio={audios}, Subtitle={subtitles}",
            filePath,
            VideoStreams.Count,
            AudioStreams.Count,
            Subtitles.Count);
      }
      catch (Exception exception)
      {
        LogError(_logger, exception, "Error processing media file");
      }

      static string GetMediaFileType(bool tv, bool radio, bool rtsp) =>
        (tv, radio, rtsp) switch
        {
          (true, _, _) => "TV",
          (false, true, _) => "radio",
          (false, false, true) => "RTSP",
          _ => string.Empty
        };
    }

    /// <summary>
    /// Writes the media information to the log.
    /// </summary>
    /// <remarks>
    /// This method logs detailed information about the media file, including general properties,
    /// video, audio, subtitles, and chapters if available.
    /// </remarks>
    public void WriteInfo()
    {
      LogInformation(_logger, "Inspecting media    : {path}", _filePath!);
      if (!Success)
      {
        LogWarning(_logger, "MediaInfo library was not loaded!");
      }
      else
      {
        LogDebug(_logger, "Library version      : {version}", Version!);

        // General
        LogDebug(_logger, "Media duration      : {duration}", TimeSpan.FromMilliseconds(Duration));
        LogDebug(_logger, "Has audio           : {audio}", (AudioStreams?.Count ?? 0) > 0);
        LogDebug(_logger, "Has video           : {video}", HasVideo);
        LogDebug(_logger, "Has subtitles       : {subtitles}", HasSubtitles);
        LogDebug(_logger, "Has chapters        : {chapters}", HasChapters);
        LogDebug(_logger, "Is DVD              : {dvd}", IsDvd);
        LogDebug(_logger, "Is Blu-Ray disk     : {bluRay}", IsBluRay);

        // Video
        if (HasVideo)
        {
          LogDebug(_logger, "Video duration      : {duration}", BestVideoStream?.Duration ?? TimeSpan.MinValue);
          LogDebug(_logger, "Video frame rate    : {rate}", Framerate);
          LogDebug(_logger, "Video width         : {width}", Width);
          LogDebug(_logger, "Video height        : {height}", Height);
          LogDebug(_logger, "Video aspect ratio  : {ratio}", AspectRatio);
          LogDebug(_logger, "Video codec         : {codec}", VideoCodec);
          LogDebug(_logger, "Video scan type     : {scanType}", ScanType);
          LogDebug(_logger, "Is video interlaced : {interlaced}", IsInterlaced);
          LogDebug(_logger, "Video resolution    : {resolution}", VideoResolution);
          LogDebug(_logger, "Video 3D mode       : {mode}", BestVideoStream?.Stereoscopic ?? StereoMode.Mono);
          LogDebug(_logger, "Video HDR standard  : {hdr}", BestVideoStream?.Hdr ?? Hdr.None);
        }

        // Audio
        if ((AudioStreams?.Count ?? 0) > 0)
        {
          LogDebug(_logger, "Audio duration      : {duration}", BestAudioStream?.Duration ?? TimeSpan.MinValue);
          LogDebug(_logger, "Audio rate          : {rate}", AudioRate);
          LogDebug(_logger, "Audio channels      : {channels}", AudioChannelsFriendly);
          LogDebug(_logger, "Audio codec         : {codec}", AudioCodec);
          LogDebug(_logger, "Audio bit depth     : {bits}", BestAudioStream?.BitDepth ?? 0);
        }

        // Subtitles
        if (HasSubtitles)
        {
          LogDebug(_logger, "Subtitles count     : {count}", Subtitles?.Count ?? 0);
        }

        // Chapters
        if (HasChapters)
        {
          LogDebug(_logger, "Chapters count      : {count}", Chapters?.Count ?? 0);
        }
      }
    }

    private unsafe void ParseStreamWithoutSeek(Stream stream, MediaInfo mediaInfo)
    {
      if (!stream.CanRead)
      {
        LogDebug(_logger, "Stream is not readable. Can't parse media stream.");
        return;
      }

      mediaInfo.Option("File_IsSeekable", "0");
      mediaInfo.OpenBufferInit(-1L, 0L);
      int byteRead;
      bool shouldContinue;
      var buffer = new byte[BufferSize];
#if !NETFRAMEWORK
      var bufferSpan = buffer.AsSpan();
#endif
      fixed (byte* pBuffer = buffer)
      {
        do
        {
#if !NETFRAMEWORK
          byteRead = stream.Read(bufferSpan);
#else
          byteRead = stream.Read(buffer, 0, BufferSize);
#endif
          shouldContinue = byteRead > 0 && (mediaInfo.OpenBufferContinue(pBuffer, byteRead) & 8) != 8;
        }
        while (shouldContinue);
      }
    }

    private unsafe void ParseStreamWithSeek(Stream stream, MediaInfo mediaInfo)
    {
      if (!stream.CanRead)
      {
        LogDebug(_logger, "Stream is not readable. Can't parse media stream.");
        return;
      }

      if (!stream.CanSeek)
      {
        LogDebug(_logger, "Stream is not seekable. Can't parse media stream.");
        return;
      }

      var buffer = new byte[BufferSize];
#if !NETFRAMEWORK
      var bufferSpan = buffer.AsSpan();
#endif
      mediaInfo.OpenBufferInit(stream.Length, 0L);
      int byteRead;
      fixed (byte* pBuffer = buffer)
      {
        bool mediaProcessed;
        do
        {
#if !NETFRAMEWORK
          byteRead = stream.Read(bufferSpan);
#else
          byteRead = stream.Read(buffer, 0, BufferSize);
#endif
          mediaProcessed = (mediaInfo.OpenBufferContinue(pBuffer, byteRead) & 8) != 8;

          if (mediaProcessed)
          {
            var fileOffsetToGet = mediaInfo.OpenBufferContinueGoToGet();
            if (fileOffsetToGet != -1L)
            {
              var fileOffset = stream.Seek(fileOffsetToGet, SeekOrigin.Begin);
              mediaInfo.OpenBufferInit(stream.Length, fileOffset);
            }
          }
        }
        while (mediaProcessed && byteRead > 0);
      }
    }

#if NETFRAMEWORK
    private static string? GetPathToMediaInfo(string path, ILogger? logger) =>
      default(string)
        .IfExistsPath("./", logger)
        .IfExistsPath(path, logger)
        .IfExistsPath(
          Path.GetDirectoryName(typeof(MediaInfoWrapper).Assembly.Location),
          logger)
        .IfExistsPath(
          Path.IsPathRooted(path) ?
            null :
            Path.Combine(Path.GetDirectoryName(typeof(MediaInfoWrapper).Assembly.Location), path),
          logger);
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

#if NETFRAMEWORK
    private string ProcessDvd(string filePath, string pathToDll)
#else
    private string ProcessDvd(string filePath)
#endif
    {
      LogDebug(_logger, "Processing DVD");
      IsDvd = true;
      var path = Path.GetDirectoryName(filePath) ?? string.Empty;
      Size = GetDirectorySize(path);
      var bups = Directory.GetFiles(path, "*.BUP", SearchOption.TopDirectoryOnly);
      LogDebug(_logger, "DVD directory size {size}", Size);
      var programBlocks = bups
        .Select(x =>
#if NETFRAMEWORK
        ProcessBupFile(x, pathToDll)
#else
        ProcessBupFile(x)
#endif
        )
        .Where(x => !string.IsNullOrEmpty(x.FileName))
        .ToList();

      // get all other info from main title's 1st vob
      if (programBlocks.Count > 0)
      {
        Duration = programBlocks.Max(x => x.Duration);
        return programBlocks.First(x => x.Duration == Duration).FileName!;
      }

      Duration = 0;
      return filePath;
    }

#if NETFRAMEWORK

    private (string? FileName, int Duration) ProcessBupFile(string file, string pathToDll)
#else
    private (string? FileName, int Duration) ProcessBupFile(string file)
#endif
    {
#if NETFRAMEWORK
      using var mi = new MediaInfo(pathToDll);
#else
      using var mi = new MediaInfo();
#endif
      LogDebug(_logger, "Opening {file}", file);
      Version = mi.Option("Info_Version");
      if (mi.Open(file) == IntPtr.Zero)
      {
        LogWarning(_logger, "MediaInfo library has not been opened media {path}", file);
        return (null, 0);
      }

      var profile = mi.Get(StreamKind.General, 0, "Format_Profile");
      if (profile == "Program")
      {
          double.TryParse(
            mi.Get(StreamKind.Video, 0, "Duration"),
            NumberStyles.AllowDecimalPoint,
            _providerNumber,
            out var duration);
          LogDebug(_logger, "Profile is program with {duration} sec", duration);
          return (file, (int)duration);
      }

      return (null, 0);
    }

#if NETFRAMEWORK
    private void ParseMedia(Stream stream, string pathToDll)
#else
    private void ParseMedia(Stream stream)
#endif
    {
#if NETFRAMEWORK
      using var mediaInfo = new MediaInfo(pathToDll);
#else
      using var mediaInfo = new MediaInfo();
#endif
      if (mediaInfo.Handle == IntPtr.Zero)
      {
        LogWarning(_logger, "MediaInfo library was not loaded!");
        return;
      }

      Version = mediaInfo.Option("Info_Version");
      LogDebug(_logger, "MediaInfo library was loaded. ({handle} Version={version}", mediaInfo.Handle, Version);
      if (!stream.CanSeek)
      {
        ParseStreamWithoutSeek(stream, mediaInfo);
      }
      else
      {
        ParseStreamWithSeek(stream, mediaInfo);
      }

      mediaInfo.OpenBufferFinalize();
      ParseMedia(mediaInfo);
    }

#if NETFRAMEWORK
    private void ParseMedia(string filePath, string pathToDll)
#else
    private void ParseMedia(string filePath)
#endif
    {
#if NETFRAMEWORK
      using var mediaInfo = new MediaInfo(pathToDll);
#else
      using var mediaInfo = new MediaInfo();
#endif
      if (mediaInfo.Handle == IntPtr.Zero)
      {
        LogWarning(_logger, "MediaInfo library was not loaded!");
        return;
      }
      else
      {
        Version = mediaInfo.Option("Info_Version");
        LogDebug(_logger, "MediaInfo library was loaded. (handle={handle}, version={version}", mediaInfo.Handle, Version);
      }

#if !NETFRAMEWORK
      if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        // On Linux/macOS, MediaInfoLib converts the wchar_t path passed to Open() into a
        // byte path via wcstombs(), which depends on the process' LC_CTYPE locale. .NET does
        // not call setlocale() itself, so without this the conversion runs in the "C" locale
        // and multi-byte (e.g. Unicode) file names fail to open.
        mediaInfo.Option("setlocale_LC_CTYPE", "C.UTF-8");
      }
#endif

      var fileProcessingHandle = mediaInfo.Open(filePath);
      if (fileProcessingHandle == IntPtr.Zero)
      {
        LogWarning(_logger, "MediaInfo library has not been opened media {path}", filePath);
        return;
      }
      else
      {
        LogDebug(_logger, "MediaInfo library successfully opened {path}. (handle={handle})", filePath, fileProcessingHandle);
      }

      ParseMedia(mediaInfo);
    }

    private void ParseMedia(MediaInfo mediaInfo)
    {
      if (mediaInfo is null)
      {
        throw new ArgumentNullException(nameof(mediaInfo));
      }

      Format = mediaInfo.Get(StreamKind.General, 0, "Format");
      IsStreamable = mediaInfo.Get(StreamKind.General, 0, (int)NativeMethods.General.General_IsStreamable).TryGetBool(out var streamable) && streamable;
      WritingApplication = mediaInfo.Get(StreamKind.General, 0, (int)NativeMethods.General.General_Encoded_Application);
      WritingLibrary = mediaInfo.Get(StreamKind.General, 0, (int)NativeMethods.General.General_Encoded_Library);
      Attachments = mediaInfo.Get(StreamKind.General, 0, "Attachments");
      Profile = mediaInfo.Get(StreamKind.General, 0, "Format_Profile");
      FormatVersion = mediaInfo.Get(StreamKind.General, 0, "Format_Version");
      Codec = mediaInfo.Get(StreamKind.General, 0, "CodecID");
      int audioChannelsTotal;
      AudioChannelsTotal = mediaInfo.Get(StreamKind.General, 0, "Audio_Channels_Total").TryGetInt(out audioChannelsTotal)
        ? audioChannelsTotal
        : 0;
      LogDebug(_logger, "Format=({format}, version={version}), Profile={profile}, Codec={Codec}", Format, FormatVersion, Profile, Codec);
      LogDebug(_logger, "Retrieving audio tags from stream position 0");
      Tags = new AudioTagBuilder(mediaInfo, 0).Build();
      if (Size == 0)
      {
        Size = mediaInfo.Get(StreamKind.General, 0, (int)NativeMethods.General.General_FileSize).TryGetLong(out long size) ? size : 0;
      }
      Text = mediaInfo.Inform();

      // Setup streams
      ProcessingMenuStreams(
        mediaInfo,
        ProcessingChapterStreams(
          mediaInfo,
          ProcessingSubtitleStreams(
            mediaInfo,
            ProcessingAudioStreams(
              mediaInfo,
              ProcessingVideoStreams(mediaInfo)))));

      Success = VideoStreams.Count != 0 || AudioStreams.Count != 0 || Subtitles.Count != 0;

      // Produce capability properties
      if (Success)
      {
        SetupProperties(mediaInfo);
      }
      else
      {
        LogWarning(_logger, "Can't find any video, audio or subtitles streams. Set Success to false");
        SetupPropertiesDefault();
      }
    }

    private int ProcessingVideoStreams(MediaInfo mediaInfo)
    {
      if (mediaInfo is null)
      {
        throw new ArgumentNullException(nameof(mediaInfo));
      }

      var result = 0;
      LogDebug(_logger, "Found {count} video streams.", mediaInfo.CountGet(StreamKind.Video));
      for (var i = 0; i < mediaInfo.CountGet(StreamKind.Video); ++i)
      {
        var stream = new VideoStreamBuilder(mediaInfo, result++, i).Build();
        LogDebug(_logger, "Add video stream #{i}: codec={codec}, profile={profile}", i, stream.CodecName, stream.CodecProfile);
        VideoStreams.Add(stream);
      }

      if (Duration == 0)
      {
          double.TryParse(
          mediaInfo.Get(StreamKind.Video, 0, (int)NativeMethods.Video.Video_Duration),
          NumberStyles.AllowDecimalPoint,
          _providerNumber,
          out var duration);
        Duration = (int)duration;
        LogDebug(_logger, "Set duration by video stream 0. Duration={duration}", TimeSpan.FromSeconds(Duration));
      }

      // Fix 3D for some containers
      if (VideoStreams.Count == 1 && Tags.GeneralTags.TryGetValue((NativeMethods.General)1000, out var isStereo))
      {
        LogDebug(_logger, "Check for stereoscopic mode");
        var video = VideoStreams[0];
        if (Tags.GeneralTags.TryGetValue((NativeMethods.General)1001, out var stereoMode))
        {
          video.Stereoscopic = (StereoMode) stereoMode;
        }
        else
        {
          video.Stereoscopic = (bool) isStereo ? StereoMode.Stereo : StereoMode.Mono;
        }

        LogDebug(_logger, "Stereoscopic mode={mode}", video.Stereoscopic);
      }

      return result;
    }

    private int ProcessingAudioStreams(MediaInfo mediaInfo, int streamNumber)
    {
      if (mediaInfo is null)
      {
        throw new ArgumentNullException(nameof(mediaInfo));
      }

      LogDebug(_logger, "Found {audios} audio streams.", mediaInfo.CountGet(StreamKind.Audio));
      for (var i = 0; i < mediaInfo.CountGet(StreamKind.Audio); ++i)
      {
        var stream = new AudioStreamBuilder(mediaInfo, streamNumber++, i).Build();
        LogDebug(_logger, "Add audio stream #{i}: codec={codec}, friendly name={friendlyName}", i, stream.CodecName, stream.CodecFriendly);
        AudioStreams.Add(stream);
      }

      if (Duration == 0)
      {
          double.TryParse(
              mediaInfo.Get(StreamKind.Audio, 0, (int)NativeMethods.Audio.Audio_Duration),
              NumberStyles.AllowDecimalPoint,
              _providerNumber,
              out var duration);
          Duration = (int)duration;
        LogDebug(_logger, "Set duration by audio stream 0. Duration={duration}", TimeSpan.FromSeconds(Duration));
      }

      return streamNumber;
    }

    private int ProcessingSubtitleStreams(MediaInfo mediaInfo, int streamNumber)
    {
      if (mediaInfo is null)
      {
        throw new ArgumentNullException(nameof(mediaInfo));
      }

      LogDebug(_logger, "Found {subtitles} subtitle streams.", mediaInfo.CountGet(StreamKind.Text));
      for (var i = 0; i < mediaInfo.CountGet(StreamKind.Text); ++i)
      {
        var stream = new SubtitleStreamBuilder(mediaInfo, streamNumber++, i).Build();
        LogDebug(_logger, "Add subtitle stream #{i}: format={format}", i, stream.Format);
        Subtitles.Add(stream);
      }

      return streamNumber;
    }

    private int ProcessingChapterStreams(MediaInfo mediaInfo, int streamNumber)
    {
      if (mediaInfo is null)
      {
        throw new ArgumentNullException(nameof(mediaInfo));
      }

      LogDebug(_logger, "Found {chapters} chapters.", mediaInfo.CountGet(StreamKind.Other));
      for (var i = 0; i < mediaInfo.CountGet(StreamKind.Other); ++i)
      {
        var chapter = new ChapterStreamBuilder(mediaInfo, streamNumber++, i).Build();
        LogDebug(_logger, "Add chapter #{i}: name={chapter}", i, chapter.Name);
        Chapters.Add(chapter);
      }

      return streamNumber;
    }

    private void ProcessingMenuStreams(MediaInfo mediaInfo, int streamNumber)
    {
      if (mediaInfo is null)
      {
        throw new ArgumentNullException(nameof(mediaInfo));
      }

      LogDebug(_logger, "Found {menus} menu items.", mediaInfo.CountGet(StreamKind.Menu));
      for (var i = 0; i < mediaInfo.CountGet(StreamKind.Menu); ++i)
      {
        var menu = new MenuStreamBuilder(mediaInfo, streamNumber++, i).Build();
        LogDebug(_logger, "Add menu #{i}: name={menu} duration={duration}", i, menu.Name, menu.Duration);
        MenuStreams.Add(menu);
      }
    }

    /// <summary>
    /// Sets the properties default values in case media was not loaded.
    /// </summary>
    private void SetupPropertiesDefault()
    {
      LogDebug(_logger, "Set default media properties");
      VideoCodec = string.Empty;
      VideoResolution = string.Empty;
      ScanType = string.Empty;
      AspectRatio = string.Empty;
      AudioCodec = string.Empty;
      AudioChannelsTotal = 0;
      AudioChannelsFriendly = string.Empty;

      OnSetupProperties(null);
    }

    /// <summary>
    /// Rise event in case the properties values was set.
    /// </summary>
    protected virtual void OnSetupProperties(MediaInfo? mediaInfo)
    {
      var @event = PropertiesInitialized;
      @event?.Invoke(this, new MediaInfoEventArgs(mediaInfo));
    }

    /// <summary>
    /// Sets the properties values in case media was loaded successfully.
    /// </summary>
    private void SetupProperties(MediaInfo mediaInfo)
    {
      LogDebug(_logger, "Set media properties by detected streams");
      BestVideoStream = VideoStreams.OrderByDescending(
          x => (long)x.Width * x.Height * x.BitDepth * (x.Stereoscopic == StereoMode.Mono ? 1L : 2L) * x.FrameRate * (x.Bitrate <= 1e-7 ? 1 : x.Bitrate))
        .FirstOrDefault();
      VideoCodec = BestVideoStream?.CodecName ?? string.Empty;
      VideoRate = (int?)BestVideoStream?.Bitrate ?? 0;
      VideoResolution = BestVideoStream?.Resolution ?? string.Empty;
      VideoRotation = (int?)BestVideoStream?.Rotation ?? 0;
      Width = BestVideoStream?.Width ?? 0;
      Height = BestVideoStream?.Height ?? 0;
      IsInterlaced = BestVideoStream?.Interlaced ?? false;
      Framerate = BestVideoStream?.FrameRate ?? 0;
      ScanType = BestVideoStream is not null
                   ? mediaInfo.Get(StreamKind.Video, BestVideoStream.StreamPosition, "ScanType").ToLowerInvariant()
                   : string.Empty;
      AspectRatio = BestVideoStream is not null
          ? GetAspectRatioText(mediaInfo.Get(StreamKind.Video, BestVideoStream.StreamPosition, "DisplayAspectRatio"))
          : string.Empty;

      BestAudioStream = AudioStreams.OrderByDescending(x => (x.Channel * 10000000) + x.Bitrate).FirstOrDefault();
      AudioCodec = BestAudioStream?.CodecName ?? string.Empty;
      AudioRate = (int?)BestAudioStream?.Bitrate ?? 0;
      AudioSampleRate = (int?)BestAudioStream?.SamplingRate ?? 0;
      AudioChannels = BestAudioStream?.Channel ?? 0;
      if (AudioChannelsTotal == 0)
      {
        AudioChannelsTotal = AudioStreams.Sum(x => x.Channel);
      }
      AudioChannelsFriendly = BestAudioStream?.AudioChannelsFriendly ?? string.Empty;

      OnSetupProperties(mediaInfo);

      static string GetAspectRatioText(string ratio) =>
        ratio switch
        {
          "4:3" or "1.333" => "fullscreen",
          "3:2" or "1.5" => "classic widescreen",
          "1:1" or "1.0" or "1" or "2.0" => "square",
          "5:4" or "1.25" => "classic",
          "4:5" or "0.8" or "9:16" => "vertical",
          "1.90:1" or "1.9" or "1.90" or "1.89" => "cinema",
          "2.39:1" or "2.39" or "2.40:1" or "2.40" or "2.35:1" or "2.35" => "scope",
          "2.76:1" or "2.76" => "ultra widescreen",
          _ => "widescreen"
        };
    }

#endregion

#region private methods

    private static void LogDebug(ILogger? logger, string message, params object[] args) =>
      logger?.LogDebug(message, args);

    private static void LogInformation(ILogger? logger, string message, params object[] args) =>
      logger?.LogInformation(message, args);

    private static void LogWarning(ILogger? logger, string message, params object[] args) =>
      logger?.LogWarning(message, args);

    private static void LogWarning(ILogger? logger, Exception? exception, string message, params object[] args) =>
      logger?.LogWarning(exception, message, args);

    private static void LogError(ILogger? logger, string message, params object[] args) =>
      logger?.LogError(message, args);

    private static void LogError(ILogger? logger, Exception? exception, string message, params object[] args) =>
      logger?.LogError(exception, message, args);

    private static void LogCritical(ILogger? logger, string message, params object[] args) =>
      logger?.LogCritical(message, args);

    private static void LogCritical(ILogger? logger, Exception? exception, string message, params object[] args) =>
      logger?.LogCritical(exception, message, args);

    private static bool CheckHasExternalSubtitles(string strFile, ILogger? logger)
    {
      if (string.IsNullOrEmpty(strFile))
      {
        return false;
      }

      var filenameNoExt = Path.GetFileNameWithoutExtension(strFile);
      try
      {
        return Directory.GetFiles(Path.GetDirectoryName(strFile) ?? string.Empty, filenameNoExt + "*")
          .Any(x => SubTitleExtensions.ContainsKey(Path.GetExtension(x)));
      }
      catch (Exception ex)
      {
        LogWarning(logger, ex, "Error while checking for external subtitles. Path: {path}", strFile);
        return false;
      }
    }

#endregion

#region public video related properties

    /// <summary>
    /// A value indicating whether media has at least one video stream.
    /// </summary>
    /// <value>
    ///   <c>true</c> if media has at least one video stream; otherwise, <c>false</c>.
    /// </value>
    public bool HasVideo => VideoStreams.Count > 0;

    /// <summary>
    /// A value indicating whether media has at least one video stream with 3D effect.
    /// </summary>
    /// <value>
    ///   <c>true</c> if media has at least one video stream with 3D effect; otherwise, <c>false</c>.
    /// </value>
    public bool Is3D => VideoStreams.Any(x => x.Stereoscopic != StereoMode.Mono);

    /// <summary>
    /// A value indicating whether media has at least one video stream with HDR effect.
    /// </summary>
    /// <value>
    ///   <c>true</c> if media has at least one video stream with HDR effect; otherwise, <c>false</c>.
    /// </value>
    public bool IsHdr => VideoStreams.Any(x => x.Hdr != Hdr.None);

    /// <summary>
    /// A list of video streams. The list is empty if media has no video streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The list is ordered by the stream position in the media file.
    /// </remarks>
    public IList<VideoStream> VideoStreams { get; }

    /// <summary>
    /// Gets the best video stream based on resolution, bitrate, frame rate, bit depth and 3D effect. The value is null if media has no video streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The best video stream is determined by the following criteria (in order of importance):
    /// 1. Resolution
    /// 2. Bitrate
    /// 3. Frame rate
    /// 4. Bit depth
    /// 5. 3D effect
    /// If multiple streams have the same resolution, bitrate, frame rate, bit depth and 3D effect, the first one in the media file is selected as the best stream.
    /// In case of Blu-Ray media, the best stream is selected based on the highest resolution and bitrate, as Blu-Ray media typically contains multiple video streams with different resolutions and bitrates.
    /// For example, if a media file contains two video streams with the same resolution and frame rate, but one has a higher bitrate than the other, the stream with the higher bitrate will be selected as the best stream. If both streams have the same bitrate, the first one in the media file will be selected as the best stream.
    /// In case of 3D media, the stream with the highest resolution and bitrate will be selected as the best stream, regardless of the frame rate and bit depth, as 3D effect is considered more important than frame rate and bit depth for 3D media.
    /// In case of HDR media, the stream with the highest resolution and bitrate will be selected as the best stream, regardless of the frame rate and bit depth, as HDR effect is considered more important than frame rate and bit depth for HDR media.
    /// In case of Blu-Ray media with 3D effect, the stream with the highest resolution and bitrate will be selected as the best stream, regardless of the frame rate and bit depth, as 3D effect is considered more important than frame rate and bit depth for Blu-Ray media with 3D effect.
    /// In case of Blu-Ray media with HDR effect, the stream with the highest resolution and bitrate will be selected as the best stream, regardless of the frame rate and bit depth, as HDR effect is considered more important than frame rate and bit depth for Blu-Ray media with HDR effect.
    /// In case of Blu-Ray media with 3D and HDR effect, the stream with the highest resolution and bitrate will be selected as the best stream, regardless of the frame rate and bit depth, as 3D and HDR effects are considered more important than frame rate and bit depth for Blu-Ray media with 3D and HDR effect.
    /// If media has no video streams, the value is null.
    /// </remarks>
    public VideoStream? BestVideoStream { get; private set; }

    /// <summary>
    /// Gets the video codec of the best video stream. The value is empty string if media has no video streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The video codec is determined based on the <see cref="BestVideoStream">best video stream</see>. If media has no video streams or media was not loaded successfully, the value is an empty string.
    /// </remarks>
    public string VideoCodec { get; private set; } = default!;

    /// <summary>
    /// Gets the video frame rate of the best video stream. The value is 0 if media has no video streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The video frame rate is determined based on the <see cref="BestVideoStream">best video stream</see>. If media has no video streams or media was not loaded successfully, the value is 0.
    /// </remarks>
    public double Framerate { get; private set; }

    /// <summary>
    /// Gets the video width of the best video stream. The value is 0 if media has no video streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The video width is determined based on the <see cref="BestVideoStream">best video stream</see>. If media has no video streams or media was not loaded successfully, the value is 0.
    /// </remarks>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the video height of the best video stream. The value is 0 if media has no video streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The video height is determined based on the <see cref="BestVideoStream">best video stream</see>. If media has no video streams or media was not loaded successfully, the value is 0.
    /// </remarks>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the video aspect ratio of the best video stream. The value is an empty string if media has no video streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The video aspect ratio is determined based on the <see cref="BestVideoStream">best video stream</see>. If media has no video streams or media was not loaded successfully, the value is an empty string.
    /// </remarks>
    public string AspectRatio { get; private set; } = default!;

    /// <summary>
    /// Gets the type of the scan. The value is an empty string if media has no video streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The type of the scan is determined based on the <see cref="BestVideoStream">best video stream</see>. If media has no video streams or media was not loaded successfully, the value is an empty string.
    /// </remarks>
    public string ScanType { get; private set; } = default!;

    /// <summary>
    /// Gets a value indicating whether video is interlaced. The value is <c>false</c> if media has no video streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The interlaced status is determined based on the <see cref="BestVideoStream">best video stream</see>. If media has no video streams or media was not loaded successfully, the value is <c>false</c>.
    /// </remarks>
    public bool IsInterlaced { get; private set; }

    /// <summary>
    /// Gets the video resolution of the best video stream. The value is an empty string if media has no video streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The video resolution is determined based on the <see cref="BestVideoStream">best video stream</see>. If media has no video streams or media was not loaded successfully, the value is an empty string.
    /// </remarks>
    public string VideoResolution { get; private set; } = default!;

    /// <summary>
    /// Gets the video bitrate. The value is 0 if media has no video streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The video bitrate is determined based on the <see cref="BestVideoStream">best video stream</see>. If media has no video streams or media was not loaded successfully, the value is 0.
    /// </remarks>
    public int VideoRate { get; private set; }

#endregion

#region public audio related properties

    /// <summary>
    /// Gets the list of audio streams. The list is empty if media has no audio streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The audio streams are determined based on the media file. If media has no audio streams or media was not loaded successfully, the value is an empty list.
    /// </remarks>
    public IList<AudioStream> AudioStreams { get; }

    /// <summary>
    /// Gets the best audio stream. The value is <c>null</c> if media has no audio streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The best audio stream is determined based on the media file. If media has no audio streams or media was not loaded successfully, the value is <c>null</c>.
    /// </remarks>
    public AudioStream? BestAudioStream { get; private set; }


    /// <summary>
    /// Gets the audio codec. The value is an empty string if media has no audio streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The audio codec is determined based on the <see cref="BestAudioStream">best audio stream</see>. If media has no audio streams or media was not loaded successfully, the value is an empty string.
    /// </remarks>
    public string AudioCodec { get; private set; } = default!;

    /// <summary>
    /// Gets the audio bitrate. The value is 0 if media has no audio streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The audio bitrate is determined based on the <see cref="BestAudioStream">best audio stream</see>. If media has no audio streams or media was not loaded successfully, the value is 0.
    /// </remarks>
    public int AudioRate { get; private set; }

    /// <summary>
    /// Gets the audio sample rate. The value is 0 if media has no audio streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The audio sample rate is determined based on the <see cref="BestAudioStream">best audio stream</see>. If media has no audio streams or media was not loaded successfully, the value is 0.
    /// </remarks>
    public int AudioSampleRate { get; private set; }

    /// <summary>
    /// Gets the count of audio channels. The value is 0 if media has no audio streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The count of audio channels is determined based on the <see cref="BestAudioStream">best audio stream</see>. If media has no audio streams or media was not loaded successfully, the value is 0.
    /// </remarks>
    public int AudioChannels { get; private set; }

    /// <summary>
    /// Gets the total count of audio channels across all audio streams.
    /// </summary>
    /// <remarks>
    /// The total audio channel count is taken from the general MediaInfo field when available, otherwise it is computed from detected audio streams.
    /// </remarks>
    public int AudioChannelsTotal { get; private set; }

    /// <summary>
    /// Gets the audio channels friendly name. The value is an empty string if media has no audio streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The audio channels friendly name is determined based on the <see cref="BestAudioStream">best audio stream</see>. If media has no audio streams or media was not loaded successfully, the value is an empty string.
    /// </remarks>
    public string AudioChannelsFriendly { get; private set; } = default!;

#endregion

#region public subtitles related properties

    /// <summary>
    /// Gets the list of media subtitles. The list is empty if media has no subtitles or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media subtitles are determined based on the media file. If media has no subtitles or media was not loaded successfully, the value is an empty list.
    /// </remarks>
    public IList<SubtitleStream> Subtitles { get; }

    /// <summary>
    /// A value indicating whether media has subtitles. The value is <c>false</c> if media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media has subtitles if it has at least one subtitle stream or at least one external subtitle file. If media was not loaded successfully, the value is <c>false</c>.
    /// </remarks>
    public bool HasSubtitles => HasExternalSubtitles || Subtitles.Count > 0;

    /// <summary>
    /// A value indicating whether media has external subtitles. The value is <c>false</c> if media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The instance has external subtitles if it has at least one external subtitle file. If media was not loaded successfully, the value is <c>false</c>.
    /// </remarks>
    public bool HasExternalSubtitles { get; }

#endregion

#region public chapters related properties

    /// <summary>
    /// Gets the media chapters. The list is empty if media has no chapters or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media chapters are determined based on the media file. If media has no chapters or media was not loaded successfully, the value is an empty list.
    /// </remarks>
    public IList<ChapterStream> Chapters { get; }

    /// <summary>
    /// Gets a value indicating whether media has chapters. The value is <c>false</c> if media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media has chapters if it has at least one chapter stream. If media was not loaded successfully, the value is <c>false</c>.
    /// </remarks>
    public bool HasChapters => Chapters.Count > 0;

#endregion

#region public menu related properties

    /// <summary>
    /// Gets the menu streams from media. The list is empty if media has no menu streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The menu streams are determined based on the media file. If media has no menu streams or media was not loaded successfully, the value is an empty list.
    /// </remarks>
    public IList<MenuStream> MenuStreams { get; }

    /// <summary>
    /// A value indicating whether media has menu. The value is <c>false</c> if media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media has menu if it has at least one menu stream. If media was not loaded successfully, the value is <c>false</c>.
    /// </remarks>
    public bool HasMenu => MenuStreams.Count > 0;

#endregion

#region public common properties

    /// <summary>
    /// A value indicating whether media is DVD. The value is <c>false</c> if media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media is DVD if it has a DVD structure. If media was not loaded successfully, the value is <c>false</c>.
    /// </remarks>
    public bool IsDvd { get; private set; }

    /// <summary>
    /// Gets the media (container) format. The value is an empty string if media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media (container) format is determined based on the media file. If media was not loaded successfully, the value is an empty string.
    /// </remarks>
    public string Format { get; private set; } = default!;

    /// <summary>
    /// A value indicating whether media is streamable. The value is <c>false</c> if media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media is streamable if it can be played over a network. If media was not loaded successfully, the value is <c>false</c>.
    /// </remarks>
    public bool IsStreamable { get; private set; }

    /// <summary>
    /// Gets the writing media application. The value is an empty string if media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The writing media application is determined based on the media file. If media was not loaded successfully, the value is an empty string.
    /// </remarks>
    public string WritingApplication { get; private set; } = default!;

    /// <summary>
    /// Gets the writing media library. The value is an empty string if media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The writing media library is determined based on the media file. If media was not loaded successfully, the value is an empty string.
    /// </remarks>
    public string WritingLibrary { get; private set; } = default!;

    /// <summary>
    /// Gets the media attachments. The value is an empty string if media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media attachments are determined based on the media file. If media was not loaded successfully, the value is an empty string.
    /// </remarks>
    public string Attachments { get; private set; } = default!;

    /// <summary>
    /// Gets the media (container) format version. The value is an empty string if media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media (container) format version is determined based on the media file. If media was not loaded successfully, the value is an empty string.
    /// </remarks>
    public string FormatVersion { get; private set; } = default!;

    /// <summary>
    /// Gets the media (container) format profile. The value is an empty string if media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media (container) format profile is determined based on the media file. If media was not loaded successfully, the value is an empty string.
    /// </remarks>
    public string Profile { get; private set; } = default!;

    /// <summary>
    /// Gets the media (container) codec. The value is an empty string if media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media (container) codec is determined based on the media file. If media was not loaded successfully, the value is an empty string.
    /// </remarks>
    public string Codec { get; private set; } = default!;

    /// <summary>
    /// A value indicating whether media is BluRay. The value is <c>false</c> if media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media is BluRay if it has a BluRay structure. If media was not loaded successfully, the value is <c>false</c>.
    /// </remarks>
    public bool IsBluRay { get; }

    /// <summary>
    /// A value indicating whether media information was loaded successfully. The value is <c>false</c> if media information was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The value is <c>true</c> if media information was loaded successfully; otherwise, <c>false</c>.
    /// </remarks>
    public bool Success { get; private set; }

    /// <summary>
    /// Gets the duration of the media in seconds. The value is 0 if media has no video and audio streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media duration is determined based on the video and audio streams. If media has no video and audio streams or media was not loaded successfully, the value is 0.
    /// </remarks>
    public int Duration { get; private set; }

    /// <summary>
    /// Gets the version of the mediainfo.dll library. The value is <c>null</c> if mediainfo.dll was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The mediainfo.dll version is determined based on the loaded mediainfo.dll. If mediainfo.dll was not loaded successfully, the value is <c>null</c>.
    /// </remarks>
    public string? Version { get; private set; }

    /// <summary>
    /// Gets the media size in bytes. The value is 0 if media has no video and audio streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media size is determined based on the video and audio streams. If media has no video and audio streams or media was not loaded successfully, the value is 0.
    /// </remarks>
    public long Size { get; private set; }

    /// <summary>
    /// Gets the media audio tags. The value is an empty string if media has no audio streams or media was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The media audio tags are determined based on the media file. If media was not loaded successfully, the value is an empty string.
    /// </remarks>
    public AudioTags Tags { get; private set; } = default!;

#endregion

    /// <summary>
    /// Occurs when properties initialized.
    /// </summary>
    public event EventHandler? PropertiesInitialized;

    /// <summary>
    /// Gets the text representation of the media information. The value is an empty string if media information was not loaded successfully.
    /// </summary>
    /// <remarks>
    /// The text representation of the media information is determined based on the loaded media information. If media information was not loaded successfully, the value is an empty string.
    /// </remarks>
    public string Text { get; private set; } = default!;

    /// <summary>
    /// Gets the video rotation
    /// </summary>
    public int VideoRotation { get; private set; }
  }

#if NETFRAMEWORK
  internal static class PathExtensions
  {
    /// <summary>
    /// Returns the source path if it is not null or empty; otherwise, checks the another path for mediaInfo.dll file and returns it if the file exists; otherwise, returns null.
    /// </summary>
    /// <param name="sourcePath">The source path to check.</param>
    /// <param name="anotherPath">The another path to check if the source path is
    /// null or empty.</param>
    /// <param name="logger">The logger to log the information about the paths checking.</param>
    /// <returns>
    /// The source path if it is not null or empty; otherwise, the another path if mediaInfo.dll file exists in it; otherwise, null.
    /// </returns>
    /// <remarks>
    /// This method is used to check the paths for mediaInfo.dll file. It first checks the source path and returns it if it is not null or empty. If the source path is null or empty, it checks the another path for mediaInfo.dll file and returns it if the file exists; otherwise, it returns null.
    /// </remarks>
    public static string? IfExistsPath(this string? sourcePath, string? anotherPath, ILogger? logger)
    {
      if (!string.IsNullOrEmpty(sourcePath))
      {
        return sourcePath;
      }

      if (string.IsNullOrEmpty(anotherPath))
      {
        return null;
      }

      logger?.LogDebug("Check MediaInfo.dll from {0}.", anotherPath!);
      if (!anotherPath!.MediaInfoExist())
      {
        logger?.LogWarning("Library MediaInfo.dll was not found at {path}", anotherPath!);
        return null;
      }

      logger?.LogInformation("Library MediaInfo.dll was found at {path}", anotherPath!);
      return anotherPath;
    }

    /// <summary>
    /// Checks the specified path for mediaInfo.dll file and returns <b>true</b> if the file exists; otherwise, returns <b>false</b>.
    /// </summary>
    /// <param name="pathToDll">The path to check for mediaInfo.dll file. The path should not contain the mediaInfo.dll file name, but only the directory path.</param>
    /// <returns>
    /// <b>true</b> if the mediaInfo.dll file exists in the specified path; otherwise, <b>false</b>.</returns>
    /// <remarks>
    /// This method is used to check if the mediaInfo.dll file exists in the specified path. The path should not include the mediaInfo.dll file name, only the directory path.
    /// </remarks>
    public static bool MediaInfoExist(this string pathToDll) =>
      File.Exists(Path.Combine(pathToDll, "MediaInfo.dll"));
  }
#endif
}