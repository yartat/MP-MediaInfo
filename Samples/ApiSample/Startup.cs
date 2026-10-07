#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ApiSample.Infrastructure;
using MediaInfo.Analysis;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace ApiSample;

/// <summary>
/// Startup application
/// </summary>
public static class Startup
{
    /// <summary>
    /// Configures the services to add services to the container.
    /// </summary>
    /// <param name="webApplicationBuilder">The WEB application builder instance.</param>
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder webApplicationBuilder)
    {
        var services = webApplicationBuilder.Services;
        var configuration = webApplicationBuilder.Configuration;
        services
            .AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(Startup).Assembly);
        config.Compile();

        services
            .AddSingleton(config)
            .AddScoped<IMapper, ServiceMapper>();

        // One analyzer for the whole application: it holds no per-request state, and the concurrency limit only
        // means anything when every request shares it.
        services.AddMediaInfoAnalyzer(builder => builder
            .WithValidation()
            .WithCaching(TimeSpan.FromMinutes(10))
            .WithTimeout(TimeSpan.FromMinutes(2))
            .WithConcurrencyLimit(Environment.ProcessorCount)
            .WithExternalSubtitles());

        services
            .AddFilters()
            .AddOpenApi(options =>
            {
                // A TimeSpan is a string in JSON, and saying so keeps the document
                // from describing it as the object its properties would suggest.
                options.MapType<TimeSpan>(schema =>
                {
                    schema.Type = JsonSchemaType.String;
                    schema.Format = "duration";
                    schema.Default = JsonValue.Create("00:01:00");
                    schema.Examples = [JsonValue.Create("00:01:00")];
                });

                options.AddDocumentTransformer((document, _, _) =>
                {
                    document.Info = new OpenApiInfo
                    {
                        Title = "MP-MediaInfo API sample",
                        Version = "v1",
                        Description =
                            "Reads what MediaInfo knows about a media file or a disc folder. " +
                            "Give a path the server can reach.",
                    };

                    return Task.CompletedTask;
                });
            });

        return webApplicationBuilder;
    }

    /// <summary>
    /// Configures HTTP request pipeline and prepares an application runtime.
    /// </summary>
    /// <param name="app">The Web application instance.</param>
    public static WebApplication PrepareRuntime(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app
            .UseHttpsRedirection()
            .UseRouting()
            .UseAuthorization();
        app.MapControllers();

        // The document at /openapi/v1.json, and Scalar reading it at /scalar. Both
        // stay on outside development on purpose: an unreachable sample teaches
        // nobody anything, and there is nothing here worth hiding.
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "MP-MediaInfo API sample";
            options.Theme = ScalarTheme.BluePlanet;
            options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        return app;
    }
}
