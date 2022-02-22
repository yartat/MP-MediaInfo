#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using ApiSample.Infrastructure.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IO;

namespace ApiSample.Infrastructure
{
    internal static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Includes the application XML comments to Swagger options.
        /// </summary>
        /// <param name="options">The Swagger options.</param>
        /// <param name="xmlFile">The application XML comments file.</param>
        public static SwaggerGenOptions IncludeApplicationXmlComments(this SwaggerGenOptions options, string xmlFile)
        {
            options.IncludeXmlComments(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, xmlFile));
            return options;
        }

        /// <summary>
        /// Adds the filters to DI container.
        /// </summary>
        /// <param name="services">The services instance.</param>
        public static IServiceCollection AddFilters(this IServiceCollection services) =>
            services
                .AddScoped<ValidateModelStateAttribute>();
    }
}