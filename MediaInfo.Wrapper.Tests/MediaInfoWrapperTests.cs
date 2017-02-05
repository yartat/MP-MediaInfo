using System.IO;
using System.Reflection;

using NUnit.Framework;

namespace MediaInfo.Wrapper.Tests
{
  [TestFixture]
  public class MediaInfoWrapperTests
  {
    private MediaInfoWrapper _mediaInfoWrapper;

    [Test]
    [TestCase(".\\Data\\RTL_7_Darts_WK_2014-2013-12-23_1_h263.3gp")]
    public void LoadSimpleVideo(string fileName)
    {
      _mediaInfoWrapper = new MediaInfoWrapper(fileName, ".\\");
      Assert.IsFalse(_mediaInfoWrapper.MediaInfoNotloaded, "InfoWrapper not loaded");
      Assert.IsTrue(_mediaInfoWrapper.HasVideo, "Hasn't video stream");
      Assert.IsFalse(_mediaInfoWrapper.IsBluRay, "Is BluRay");
      Assert.IsFalse(_mediaInfoWrapper.IsDvd);
      Assert.IsFalse(_mediaInfoWrapper.IsInterlaced);
    }
  }
}
