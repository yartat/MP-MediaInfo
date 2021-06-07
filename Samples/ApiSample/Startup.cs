#region Copyright (C) 2017-2021 Yaroslav Tatarenko

// Copyright (C) 2017-2021 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using ApiSample.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System;

namespace ApiSample
{
    /// <summary>
    /// Startup application
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class with host environment.
        /// </summary>
        /// <param name="configuration">The host configuration.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures the services to add services to the container.
        /// </summary>
        /// <param name="services">The services collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers()
                    .AddJsonOptions(options => options.JsonSerializerOptions.IgnoreNullValues = true);

            services
                .AddFilters()
                .AddMediaInfoLogger()
                .AddAutoMapper(MapperExtensions.ConfigureMapper)
                .AddSwaggerGen(options =>
                {
                    options.MapType<TimeSpan>(() => new OpenApiSchema { Type = "string", Format = "duration", Default = new OpenApiString("00:01:00"), Example = new OpenApiString("00:01:00") });
                    options.MapType<TimeSpan?>(() => new OpenApiSchema { Type = "string", Format = "duration", Default = new OpenApiString("00:01:00"), Example = new OpenApiString("00:01:00") });
                    options
                        .IncludeApplicationXmlComments("ApiSample.xml")
                        .EnableAnnotations();
                });
        }

        /// <summary>
        /// Configures the specified application runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder instance.</param>
        /// <param name="env">The host environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app
                .UseHttpsRedirection()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints => endpoints.MapControllers())
                .UseSwagger()
                .UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Sample");
                    options.DisplayRequestDuration();
                });
        }
    }
}
