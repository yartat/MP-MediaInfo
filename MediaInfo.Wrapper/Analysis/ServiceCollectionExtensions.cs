#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace MediaInfo.Analysis;

/// <summary>
/// Registers the media analyzer with a dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Registers <see cref="IMediaInfoAnalyzer"/> as a singleton.
  /// </summary>
  /// <remarks>
  /// The analyzer is a singleton because it holds no per-request state and because a concurrency limit is only
  /// meaningful when every caller shares one. A logger is taken from the container when logging has been registered,
  /// unless the configuration supplies one of its own.
  /// </remarks>
  /// <param name="services">The container to register with.</param>
  /// <param name="configure">The configuration to apply to the builder.</param>
  /// <returns>Returns the container.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
  public static IServiceCollection AddMediaInfoAnalyzer(
    this IServiceCollection services,
    Action<MediaInfoAnalyzerBuilder>? configure = null)
  {
    if (services is null)
    {
      throw new ArgumentNullException(nameof(services));
    }

    services.TryAddSingleton(provider =>
    {
      var builder = MediaInfoAnalyzerBuilder.Create();

      var logger = provider.GetService<ILoggerFactory>()?.CreateLogger("MediaInfo.Analysis");
      if (logger is not null)
      {
        builder.UseLogger(logger);
      }

      configure?.Invoke(builder);
      return builder.Build();
    });

    return services;
  }

  /// <summary>
  /// Registers <see cref="IMediaInfoAnalyzer"/> as a singleton with the specified options.
  /// </summary>
  /// <param name="services">The container to register with.</param>
  /// <param name="configureOptions">The adjustment to apply to the analysis options.</param>
  /// <returns>Returns the container.</returns>
  public static IServiceCollection AddMediaInfoAnalyzer(
    this IServiceCollection services,
    Action<MediaAnalysisOptions> configureOptions) =>
    services.AddMediaInfoAnalyzer(builder => builder.Configure(configureOptions));
}
