#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ApiSample
{
    /// <summary>
    /// Entry point class
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Entry point with arguments
        /// </summary>
        /// <param name="args">The application arguments.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the Web host builder.
        /// </summary>
        /// <param name="args">The application arguments.</param>
        /// <returns>Returns <see cref="IHostBuilder"/> instance.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
