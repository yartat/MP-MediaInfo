#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Text.Json.Serialization;
using ApiSample.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

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

        services
            .AddFilters()
            .AddAutoMapper(MapperExtensions.ConfigureMapper)
            .AddSwaggerGen(options =>
            {
                options.MapType<TimeSpan>(() => new OpenApiSchema { Type = "string", Format = "duration", Default = new OpenApiString("00:01:00"), Example = new OpenApiString("00:01:00") });
                options.MapType<TimeSpan?>(() => new OpenApiSchema { Type = "string", Format = "duration", Default = new OpenApiString("00:01:00"), Example = new OpenApiString("00:01:00") });
                options
                    .IncludeApplicationXmlComments("ApiSample.xml")
                    .EnableAnnotations();
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
        app
            .UseSwagger()
            .UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Sample");
                options.DisplayRequestDuration();
            });

        return app;
    }
}
