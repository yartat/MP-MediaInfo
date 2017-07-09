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
      var atmos = _mediaInfoWrapper.AudioStreams[0];
      Assert.IsTrue(atmos.Codec == AudioCodec.TruehdAtmos);
      Assert.AreEqual(1, _mediaInfoWrapper.MenuStreams.Count);
    }
  }
}
