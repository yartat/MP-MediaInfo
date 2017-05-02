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
      Assert.IsTrue(_mediaInfoWrapper.HasVideo, "Hasn't video stream");
      Assert.IsFalse(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
    }

    [Test]
    [TestCase(@".\Data\Test_H264_Atmos.m2ts")]
    public void LoadVideoWithDolbyAtmos(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      Assert.IsFalse(_mediaInfoWrapper.MediaInfoNotloaded, "InfoWrapper not loaded");
      Assert.IsTrue(_mediaInfoWrapper.HasVideo, "Hasn't video stream");
      Assert.IsFalse(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
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
      Assert.IsTrue(_mediaInfoWrapper.HasVideo, "Hasn't video stream");
      Assert.IsFalse(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
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
      Assert.IsTrue(_mediaInfoWrapper.HasVideo, "Hasn't video stream");
      Assert.IsFalse(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
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
      Assert.AreEqual(audioStreamCount, _mediaInfoWrapper.AudioStreams.Count);
      var dts = _mediaInfoWrapper.AudioStreams[dtsIndex];
      Assert.IsTrue(dts.Codec == AudioCodec.DtsHd);
    }

    [Test, Explicit]
    [TestCase(@"D:\Video\2016 DOLBY ATMOS DEMO DISC\BDMV\index.bdmv")]
    public void LoadBluRayWithMenuAndDolbyAtmos(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName);
      Assert.IsFalse(_mediaInfoWrapper.MediaInfoNotloaded, "InfoWrapper not loaded");
      Assert.IsTrue(_mediaInfoWrapper.HasVideo, "Hasn't video stream");
      Assert.IsTrue(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
      var atmos = _mediaInfoWrapper.AudioStreams[0];
      Assert.IsTrue(atmos.Codec == AudioCodec.TruehdAtmos);
      Assert.AreEqual(1, _mediaInfoWrapper.MenuStreams.Count);
    }
  }
}
