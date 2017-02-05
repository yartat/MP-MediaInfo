#region Copyright (C) 2005-2017 Team MediaPortal

// Copyright (C) 2005-2017 Team MediaPortal
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

namespace MediaInfo
{
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

    private readonly List<VideoStream> _videoStreams;
    private readonly List<AudioStream> _audioStreams;
    private readonly VideoStream _bestVideo;
    private readonly AudioStream _bestAudio;
    private readonly List<SubtitleStream> _subtitleStreams;
    private readonly List<Chapter> _chapters;

    //Subtitles
    private readonly bool _hasExternalSubtitles;

    #endregion

    #region ctor's

    public MediaInfoWrapper(string filePath, string pathToDll)
    {
      MediaInfoNotloaded = false;
      _videoStreams = new List<VideoStream>();
      _audioStreams = new List<AudioStream>();
      _subtitleStreams = new List<SubtitleStream>();
      _chapters = new List<Chapter>();

      if (!MediaInfoExist(pathToDll))
      {
        MediaInfoNotloaded = true;
        return;
      }

      if (string.IsNullOrEmpty(filePath))
      {
        MediaInfoNotloaded = true;
        return;
      }

      var isTv = filePath.IsLiveTv();
      var isRadio = filePath.IsLiveTv();
      var isRTSP = filePath.IsRTSP(); //rtsp for live TV and recordings.
      var isAvStream = filePath.IsAvStream(); //other AV streams
      var isNetwork = filePath.IsNetwork();

      //currently disabled for all tv/radio
      if (isTv || isRadio || isRTSP)
      {
        MediaInfoNotloaded = true;
        return;
      }

      NumberFormatInfo providerNumber = new NumberFormatInfo { NumberDecimalSeparator = "." };

      try
      {
        // Analyze local file for DVD and BD
        if (!(isAvStream || isNetwork))
        {
          if (filePath.EndsWith(".ifo", StringComparison.OrdinalIgnoreCase))
          {
            IsDvd = true;
            var path = Path.GetDirectoryName(filePath);
            var bups = Directory.GetFiles(path, "*.BUP", SearchOption.TopDirectoryOnly);
            var programBlocks = new List<Tuple<string, int>>();
            foreach (var bupFile in bups)
            {
              using (var mi = new MediaInfo())
              {
                mi.Open(bupFile);
                var profile = mi.Get(StreamKind.General, 0, "Format_Profile");
                if (profile == "Program")
                {
                  double duration;
                  double.TryParse(mi.Get(StreamKind.Video, 0, "Duration"), NumberStyles.AllowDecimalPoint, providerNumber, out duration);
                  programBlocks.Add(new Tuple<string, int>(bupFile, (int)duration));
                }
              }
            }
            // get all other info from main title's 1st vob
            if (programBlocks.Any())
            {
              VideoDuration = programBlocks.Max(x => x.Item2);
              filePath = programBlocks.First(x => x.Item2 == VideoDuration).Item1;
            }
          }
          else if (filePath.EndsWith(".bdmv", StringComparison.OrdinalIgnoreCase))
          {
            IsBluRay = true;
            var path = Path.GetDirectoryName(filePath) + @"\STREAM";
            filePath = GetLargestFileInDirectory(path, "*.m2ts");
          }

          _hasExternalSubtitles = !string.IsNullOrEmpty(filePath) && CheckHasExternalSubtitles(filePath);
        }

        using (var mediaInfo = new MediaInfo())
        {
          mediaInfo.Open(filePath);

          var streamNumber = 0;
          //Video
          var videoStreamCount = mediaInfo.CountGet(StreamKind.Video);
          for (var i = 0; i < videoStreamCount; ++i)
          {
            _videoStreams.Add(new VideoStream(mediaInfo, streamNumber++, i));
          }

          if (VideoDuration == 0)
          {
            double duration;
            double.TryParse(
              mediaInfo.Get(StreamKind.Video, 0, "Duration"),
              NumberStyles.AllowDecimalPoint,
              providerNumber,
              out duration);
            VideoDuration = (int)duration;
          }

          //Audio
          var iAudioStreams = mediaInfo.CountGet(StreamKind.Audio);
          for (var i = 0; i < iAudioStreams; ++i)
          {
            _audioStreams.Add(new AudioStream(mediaInfo, streamNumber++, i));
          }

          //Subtitles
          var numsubtitles = mediaInfo.CountGet(StreamKind.Text);

          for (var i = 0; i < numsubtitles; ++i)
          {
            _subtitleStreams.Add(new SubtitleStream(mediaInfo, streamNumber++, i));
          }

          var numChapters = mediaInfo.CountGet(StreamKind.Other);

          for (var i = 0; i < numChapters; ++i)
          {
            _chapters.Add(new Chapter(mediaInfo, streamNumber++, i));
          }

          MediaInfoNotloaded = _videoStreams.Count == 0 && _audioStreams.Count == 0 && _subtitleStreams.Count == 0;

          // Produce copability properties
          if (!MediaInfoNotloaded)
          {
            _bestVideo =
              _videoStreams.OrderByDescending(
                x =>
                  (long)x.Width * x.Height * x.BitDepth * (x.Stereoscopic == StereoMode.Mono ? 1L : 2L)
                  * x.FrameRate).FirstOrDefault();
            VideoCodec = _bestVideo != null ? _bestVideo.CodecName : string.Empty;
            VideoResolution = _bestVideo != null ? _bestVideo.Resolution : string.Empty;
            Width = _bestVideo?.Width ?? 0;
            Height = _bestVideo?.Height ?? 0;
            IsInterlaced = _bestVideo != null && _bestVideo.Interlaced;
            Framerate = _bestVideo?.FrameRate ?? 0;
            ScanType = _bestVideo != null
                         ? mediaInfo.Get(StreamKind.Video, _bestVideo.StreamPosition, "ScanType").ToLower()
                         : string.Empty;
            AspectRatio = _bestVideo != null
                            ? mediaInfo.Get(StreamKind.Video, _bestVideo.StreamPosition, "DisplayAspectRatio")
                            : string.Empty;
            AspectRatio = AspectRatio == "4:3" || AspectRatio == "1.333" ? "fullscreen" : "widescreen";

            _bestAudio = _audioStreams.OrderByDescending(x => x.Channel * 10000000 + x.Bitrate).FirstOrDefault();
            AudioCodec = _bestAudio != null ? _bestAudio.CodecName : string.Empty;
            AudioRate = _bestAudio != null ? (int)_bestAudio.Bitrate : 0;
            AudioChannels = _bestAudio?.Channel ?? 0;
            AudioChannelsFriendly = _bestAudio != null ? _bestAudio.AudioChannelsFriendly : string.Empty;
          }
          else
          {
            VideoCodec = string.Empty;
            VideoResolution = string.Empty;
            ScanType = string.Empty;
            AspectRatio = string.Empty;

            AudioCodec = string.Empty;
            AudioChannelsFriendly = string.Empty;
          }
        }
      }
      catch  { }
    }

    #endregion

    public static bool MediaInfoExist(string pathToDll)
    {
      return File.Exists(Path.Combine(pathToDll, "MediaInfo.dll"));
    }

    #region private methods

    private static string GetLargestFileInDirectory(string targetDir, string fileMask)
    {
      string largestFile = null;
      long largestSize = 0;
      var dir = new DirectoryInfo(targetDir);
      try
      {
        var files = dir.GetFiles(fileMask, SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
          var fileSize = file.Length;
          if (fileSize > largestSize)
          {
            largestSize = fileSize;
            largestFile = file.FullName;
          }
        }
      }
      catch { }

      return largestFile;
    }

    private static bool CheckHasExternalSubtitles(string strFile)
    {
      var filenameNoExt = Path.GetFileNameWithoutExtension(strFile);
      try
      {
        if (Directory
              .GetFiles(Path.GetDirectoryName(strFile), filenameNoExt + "*")
              .Select(file => new FileInfo(file))
              .Any(fi => SubTitleExtensions.ContainsKey(fi.Extension.ToUpper())))
        {
          return true;
        }
      }
      catch { }

      return false;
    }

    #endregion

    #region public video related properties

    public int VideoDuration { get; }

    public bool HasVideo
    {
      get { return _videoStreams.Count > 0; }
    }

    public IList<VideoStream> VideoStreams
    {
      get { return _videoStreams; }
    }

    public VideoStream BestVideoStream
    {
      get { return _bestVideo; }
    }

    public string VideoCodec { get; private set; }

    public double Framerate { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public string AspectRatio { get; }

    public string ScanType { get; private set; }

    public bool IsInterlaced { get; private set; }

    public string VideoResolution { get; private set; }

    #endregion

    #region public audio related properties

    public IList<AudioStream> AudioStreams
    {
      get { return _audioStreams; }
    }

    public AudioStream BestAudioStream
    {
      get { return _bestAudio; }
    }

    public string AudioCodec { get; private set; }

    public int AudioRate { get; private set; }

    public int AudioChannels { get; private set; }

    public string AudioChannelsFriendly { get; private set; }

    #endregion

    #region public subtitles related properties

    public IList<SubtitleStream> Subtitles
    {
      get { return _subtitleStreams; }
    }

    public bool HasSubtitles
    {
      get { return _hasExternalSubtitles || _subtitleStreams.Count > 0; }
    }

    public bool HasExternalSubtitles
    {
      get { return _hasExternalSubtitles; }
    }

    #endregion

    public bool IsDvd { get; private set; }

    public bool IsBluRay { get; private set; }

    public bool MediaInfoNotloaded { get; }
  }
}