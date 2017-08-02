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

using NUnit.Framework;

namespace MediaInfo.Wrapper.Tests
{
  [TestFixture]
  public class MediaInfoWrapperTests
  {
    private MediaInfoWrapper _mediaInfoWrapper;

    [Test]
    [TestCase(@".\Data\RTL_7_Darts_WK_2014-2013-12-23_1_h263.3gp")]
    public void LoadSimpleVideo(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      Assert.IsFalse(_mediaInfoWrapper.MediaInfoNotloaded, "InfoWrapper not loaded");
      Assert.AreEqual(1371743L, _mediaInfoWrapper.Size);
      Assert.IsTrue(_mediaInfoWrapper.HasVideo, "Hasn't video stream");
      Assert.AreEqual(310275, _mediaInfoWrapper.VideoRate);
      Assert.IsFalse(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
      Assert.IsFalse(_mediaInfoWrapper.Is3D);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.EncodedDate);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.TaggedDate);
      Assert.AreEqual(1, _mediaInfoWrapper.AudioStreams.Count);
      Assert.IsNotNull(_mediaInfoWrapper.AudioStreams[0].Tags);
      Assert.IsNotEmpty(_mediaInfoWrapper.AudioStreams[0].Tags.Tags);
      Assert.IsNotEmpty(_mediaInfoWrapper.VideoStreams[0].Tags.Tags);
      Assert.IsNotNullOrEmpty(_mediaInfoWrapper.VideoStreams[0].Tags.EncodedLibrary);
    }

    [Test]
    [TestCase(@".\Data\Test_H264_Atmos.m2ts")]
    public void LoadVideoWithDolbyAtmos(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      Assert.IsFalse(_mediaInfoWrapper.MediaInfoNotloaded, "InfoWrapper not loaded");
      Assert.AreEqual(503808L, _mediaInfoWrapper.Size);
      Assert.IsTrue(_mediaInfoWrapper.HasVideo, "Hasn't video stream");
      Assert.AreEqual(24000000, _mediaInfoWrapper.VideoRate);
      Assert.IsFalse(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
      Assert.IsFalse(_mediaInfoWrapper.Is3D);
      Assert.AreEqual(2, _mediaInfoWrapper.AudioStreams.Count);
      var atmos = _mediaInfoWrapper.AudioStreams[0];
      Assert.AreEqual(AudioCodec.TruehdAtmos, atmos.Codec);
      Assert.IsEmpty(_mediaInfoWrapper.Tags.Tags);
      Assert.IsEmpty(_mediaInfoWrapper.AudioStreams[0].Tags.Tags);
      Assert.IsEmpty(_mediaInfoWrapper.AudioStreams[1].Tags.Tags);
      Assert.IsEmpty(_mediaInfoWrapper.VideoStreams[0].Tags.Tags);
    }

    [Test]
    [TestCase(@".\Data\Test_H264_Ac3.m2ts")]
    public void LoadVideoWithDolbyDigital(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      Assert.IsFalse(_mediaInfoWrapper.MediaInfoNotloaded, "InfoWrapper not loaded");
      Assert.AreEqual(86016L, _mediaInfoWrapper.Size);
      Assert.IsTrue(_mediaInfoWrapper.HasVideo, "Hasn't video stream");
      Assert.IsFalse(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
      Assert.IsFalse(_mediaInfoWrapper.Is3D);
      Assert.AreEqual(1, _mediaInfoWrapper.AudioStreams.Count);
      var ac3 = _mediaInfoWrapper.AudioStreams[0];
      Assert.AreEqual(AudioCodec.Ac3, ac3.Codec);
      Assert.IsEmpty(_mediaInfoWrapper.Tags.Tags);
      Assert.IsEmpty(_mediaInfoWrapper.AudioStreams[0].Tags.Tags);
      Assert.IsEmpty(_mediaInfoWrapper.VideoStreams[0].Tags.Tags);
    }

    [Test]
    [TestCase(@".\Data\Test_H264.m2ts")]
    public void LoadVideoWithoutAudio(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      Assert.IsFalse(_mediaInfoWrapper.MediaInfoNotloaded, "InfoWrapper not loaded");
      Assert.AreEqual(18432L, _mediaInfoWrapper.Size);
      Assert.IsTrue(_mediaInfoWrapper.HasVideo, "Hasn't video stream");
      Assert.AreEqual(5000000, _mediaInfoWrapper.VideoRate);
      Assert.IsFalse(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
      Assert.IsFalse(_mediaInfoWrapper.Is3D);
      Assert.AreEqual(0, _mediaInfoWrapper.AudioStreams.Count);
      Assert.IsEmpty(_mediaInfoWrapper.Tags.Tags);
      Assert.IsNotEmpty(_mediaInfoWrapper.VideoStreams[0].Tags.Tags);
      Assert.IsNotNullOrEmpty(_mediaInfoWrapper.VideoStreams[0].Tags.EncodedLibrary);
    }

    [Test]
    [TestCase(@".\Data\Test_H264_DTS1.m2ts", 9, 1)]
    [TestCase(@".\Data\Test_H264_DTS2.m2ts", 6, 0)]
    public void LoadVideoWithDtsMa(string fileName, int audioStreamCount, int dtsIndex)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      Assert.IsFalse(_mediaInfoWrapper.MediaInfoNotloaded, "InfoWrapper not loaded");
      Assert.IsTrue(_mediaInfoWrapper.HasVideo, "Hasn't video stream");
      Assert.IsFalse(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
      Assert.IsFalse(_mediaInfoWrapper.Is3D);
      Assert.AreEqual(audioStreamCount, _mediaInfoWrapper.AudioStreams.Count);
      var dts = _mediaInfoWrapper.AudioStreams[dtsIndex];
      Assert.IsTrue(dts.Codec == AudioCodec.DtsHd);
      Assert.IsEmpty(_mediaInfoWrapper.Tags.Tags);
      Assert.IsEmpty(_mediaInfoWrapper.VideoStreams[0].Tags.Tags);
    }

    [Test, Explicit]
    [TestCase(@"D:\Video\2016 DOLBY ATMOS DEMO DISC\BDMV\index.bdmv")]
    public void LoadBluRayWithMenuAndDolbyAtmos(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      Assert.AreEqual(24716230397L, _mediaInfoWrapper.Size);
      Assert.IsFalse(_mediaInfoWrapper.MediaInfoNotloaded, "InfoWrapper not loaded");
      Assert.IsTrue(_mediaInfoWrapper.HasVideo, "Hasn't video stream");
      Assert.AreEqual(33837116, _mediaInfoWrapper.VideoRate);
      Assert.IsFalse(_mediaInfoWrapper.Is3D);
      Assert.IsTrue(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
      Assert.AreEqual(22, _mediaInfoWrapper.AudioStreams.Count);
      var atmos = _mediaInfoWrapper.AudioStreams[0];
      Assert.IsTrue(atmos.Codec == AudioCodec.TruehdAtmos);
      Assert.AreEqual(1, _mediaInfoWrapper.MenuStreams.Count);
      Assert.IsEmpty(_mediaInfoWrapper.Tags.Tags);
    }

    [Test]
    [TestCase(@".\Data\Test_MP3Tags.mp3", 74406L)]
    [TestCase(@".\Data\Test_MP3Tags_2.mp3", 212274L)]
    public void LoadMp3FileWithTags(string fileName, long size)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      Assert.IsFalse(_mediaInfoWrapper.MediaInfoNotloaded, "InfoWrapper not loaded");
      Assert.AreEqual(size, _mediaInfoWrapper.Size);
      Assert.IsFalse(_mediaInfoWrapper.HasVideo, "Video stream does not supported int the MP3!");
      Assert.IsFalse(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
      Assert.IsFalse(_mediaInfoWrapper.Is3D);
      Assert.AreEqual(1, _mediaInfoWrapper.AudioStreams.Count);
      // MP3 file contains all tags in general stream
      Assert.IsNotEmpty(_mediaInfoWrapper.Tags.Tags);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.Album);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.Track);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.TrackPosition);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.Artist);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.RecordedDate);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.Genre);
      Assert.IsEmpty(_mediaInfoWrapper.AudioStreams[0].Tags.Tags);
    }

    [Test, Explicit]
    [TestCase(@"E:\Music\Anugama\Healing\01 - Healing Earth.flac")]
    public void LoadFlacWithCover(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      Assert.IsFalse(_mediaInfoWrapper.MediaInfoNotloaded, "InfoWrapper not loaded");
      Assert.AreEqual(141439565L, _mediaInfoWrapper.Size);
      Assert.IsFalse(_mediaInfoWrapper.HasVideo, "Video stream does not supported int the MP3!");
      Assert.IsFalse(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
      Assert.IsFalse(_mediaInfoWrapper.Is3D);
      Assert.AreEqual(1, _mediaInfoWrapper.AudioStreams.Count);
      // MP3 file contains all tags in general stream
      Assert.IsNotEmpty(_mediaInfoWrapper.Tags.Tags);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.Album);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.Track);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.TrackPosition);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.Artist);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.RecordedDate);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.Genre);
      Assert.IsTrue(_mediaInfoWrapper.Tags.Cover.Exists);
      Assert.AreEqual("image/jpeg", _mediaInfoWrapper.Tags.Cover.Mime);
      Assert.IsNotEmpty(_mediaInfoWrapper.AudioStreams[0].Tags.Tags);
    }

    [Test]
    [TestCase(@".\Data\Test_MP3Tags.mka")]
    public void LoadMultiStreamContainer(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      Assert.IsFalse(_mediaInfoWrapper.MediaInfoNotloaded, "InfoWrapper not loaded");
      Assert.AreEqual(135172L, _mediaInfoWrapper.Size);
      Assert.IsFalse(_mediaInfoWrapper.HasVideo, "Video stream does not supported int the MKA!");
      Assert.IsFalse(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
      Assert.IsFalse(_mediaInfoWrapper.Is3D);
      Assert.AreEqual(2, _mediaInfoWrapper.AudioStreams.Count);
      // MP3 file contains all tags in general stream
      Assert.IsNotEmpty(_mediaInfoWrapper.Tags.Tags);
      Assert.IsNotNull(_mediaInfoWrapper.Tags.EncodedDate);
      Assert.IsNotEmpty(_mediaInfoWrapper.AudioStreams[0].Tags.Tags);
      Assert.IsNotNullOrEmpty(_mediaInfoWrapper.AudioStreams[0].Tags.Album);
      Assert.IsNotNullOrEmpty(_mediaInfoWrapper.AudioStreams[0].Tags.Track);
      Assert.IsNotNullOrEmpty(_mediaInfoWrapper.AudioStreams[0].Tags.Artist);
      Assert.IsNotNull(_mediaInfoWrapper.AudioStreams[0].Tags.ReleasedDate);
      Assert.IsNotEmpty(_mediaInfoWrapper.AudioStreams[1].Tags.Tags);
      Assert.IsNotNullOrEmpty(_mediaInfoWrapper.AudioStreams[1].Tags.Album);
      Assert.IsNotNullOrEmpty(_mediaInfoWrapper.AudioStreams[1].Tags.Track);
      Assert.IsNotNullOrEmpty(_mediaInfoWrapper.AudioStreams[1].Tags.Artist);
    }
  }
}
