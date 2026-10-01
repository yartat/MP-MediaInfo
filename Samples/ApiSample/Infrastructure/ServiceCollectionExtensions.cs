#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using ApiSample.Infrastructure.Filters;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using System;
using System.Threading.Tasks;

namespace ApiSample.Infrastructure;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Describes how one CLR type appears in the document, wherever it appears.
    /// </summary>
    /// <typeparam name="T">The type to describe. Its nullable form is covered too.</typeparam>
    /// <param name="options">The OpenAPI options.</param>
    /// <param name="configureSchema">Fills in the schema for that type.</param>
    /// <returns>Returns the options, so calls chain.</returns>
    /// <remarks>
    /// This is the replacement for Swashbuckle's MapType. The transformer runs for
    /// every schema the generator builds, so the match is on the type the schema
    /// was built from rather than on a parameter: a TimeSpan is described the same
    /// way whether it arrives in a query string or leaves in a response body.
    /// </remarks>
    public static OpenApiOptions MapType<T>(this OpenApiOptions options, Action<OpenApiSchema> configureSchema) =>
        options.AddSchemaTransformer((schema, context, _) =>
        {
            var type = context.JsonTypeInfo.Type;
            if (type == typeof(T) || Nullable.GetUnderlyingType(type) == typeof(T))
            {
                configureSchema(schema);
            }

            return Task.CompletedTask;
        });

    /// <summary>
    /// Adds the filters to DI container.
    /// </summary>
    /// <param name="services">The services instance.</param>
    public static IServiceCollection AddFilters(this IServiceCollection services) =>
        services
            .AddScoped<ValidateModelStateAttribute>();
}
