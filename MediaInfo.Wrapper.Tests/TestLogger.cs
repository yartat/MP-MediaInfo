#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2020 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

#if NET5_0_OR_GREATER
using System;
using Microsoft.Extensions.Logging;
#else
using System.Text.RegularExpressions;
#endif
using Xunit.Abstractions;

namespace MediaInfo.Wrapper.Tests
{
  public class TestLogger : ILogger
  {
    private readonly ITestOutputHelper _testOutputHelper;
#if !NET5_0_OR_GREATER
    private readonly Regex _regex = new(@"\{(?<logValue>[^\}]+)\}", RegexOptions.Singleline | RegexOptions.Compiled);
#endif

        public TestLogger(ITestOutputHelper testOutputHelper)
    {
      _testOutputHelper = testOutputHelper;
    }

#if NET5_0_OR_GREATER
    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      _testOutputHelper.WriteLine($"{logLevel}: {formatter(state, exception)}");
    }
#else
    public void Log(LogLevel loglevel, string message, params object[] parameters)
    {
      var processedMessage = message;
      var position = 0;
      var index = 0;
      foreach (var parameter in parameters)
      {
        var result = _regex.Match(processedMessage, position);
        if (result.Success)
        {
          processedMessage = processedMessage.Replace(result.Value, $"{{{index}}}");
          position = result.Index + 1;
        }

        index++;
      }
      _testOutputHelper.WriteLine($"{loglevel}: {string.Format(processedMessage, parameters)}");
    }
#endif
  }
}
