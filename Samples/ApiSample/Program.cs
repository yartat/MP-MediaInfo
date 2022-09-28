#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace ApiSample;

/// <summary>
/// Entry point class
/// </summary>
public static class Program
{
    /// <summary>
    /// Entry point with arguments
    /// </summary>
    /// <param name="args">The application arguments.</param>
    public static async Task Main(string[] args)
    {
        var host = WebApplication
            .CreateBuilder(args)
            .ConfigureServices()
            .Build();

        await host
            .PrepareRuntime()
            .RunAsync();
    }
}
