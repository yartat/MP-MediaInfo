#region Copyright (C) 2017-2020 Yaroslav Tatarenko

// Copyright (C) 2017-2020 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2020 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using Xunit.Abstractions;

namespace MediaInfo.Wrapper.Tests
{
  public class TestLogger : ILogger
  {
    private readonly ITestOutputHelper _testOutputHelper;

    public TestLogger(ITestOutputHelper testOutputHelper)
    {
      _testOutputHelper = testOutputHelper;
    }

    public void Log(LogLevel loglevel, string message, params object[] parameters)
    {
      _testOutputHelper.WriteLine($"{loglevel}: {string.Format(message, parameters)}");
    }
  }
}
