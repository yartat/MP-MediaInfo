#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using Microsoft.Extensions.Logging;
using System;

namespace ApiSample.Infrastructure
{
    /// <summary>
    /// ASP.NET Core implementation of the logger
    /// Implements the <see cref="MediaInfo.ILogger" />
    /// </summary>
    /// <seealso cref="MediaInfo.ILogger" />
    public class MediaInfoLogger : MediaInfo.ILogger
    {
        private readonly ILogger<MediaInfoLogger> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaInfoLogger"/> class.
        /// </summary>
        /// <param name="logger">The ASP.NET Core logger instance.</param>
        /// <exception cref="ArgumentNullException">logger</exception>
        public MediaInfoLogger(ILogger<MediaInfoLogger> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public void Log(MediaInfo.LogLevel loglevel, string message, params object[] parameters) =>
            _logger.Log(loglevel.ToMicrosoftLogLevel(), message, parameters);
    }

    internal static class LoggerExtensions
    {
        public static LogLevel ToMicrosoftLogLevel(this MediaInfo.LogLevel loglevel) =>
            loglevel switch
            {
                MediaInfo.LogLevel.Critical => LogLevel.Critical,
                MediaInfo.LogLevel.Debug => LogLevel.Debug,
                MediaInfo.LogLevel.Error => LogLevel.Error,
                MediaInfo.LogLevel.Information => LogLevel.Information,
                MediaInfo.LogLevel.Verbose => LogLevel.Trace,
                MediaInfo.LogLevel.Warning => LogLevel.Warning,
                _ => LogLevel.Trace
            };
    }
}
