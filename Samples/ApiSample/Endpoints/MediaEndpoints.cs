#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL.
// https://mediaarea.net

#endregion

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiSample.Models;
using MapsterMapper;
using MediaInfo.Analysis;
using MediaInfo.Analysis.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ApiSample.Endpoints;

/// <summary>
/// The endpoints that show how media info works.
/// </summary>
public static class MediaEndpoints
{
    /// <summary>
    /// Maps the <c>/media</c> endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/media")
            .WithTags("Media");

        group
            .MapPost("/", GetMediaInfo)
            .WithName("GetMediaInfo")
            .WithSummary("Gets the media information by specified location.")
            .WithDescription("Returns what MediaInfo knows about the file, or the folder of a disc, at the location.");

        group
            .MapPost("/disc", GetDiscInfo)
            .WithName("GetDiscInfo")
            .WithSummary("Gets the structure of the DVD or Blu-ray disc at the specified location.")
            .WithDescription("Returns every title the disc holds.");

        return app;
    }

    // The request token is forwarded, so a client that gives up stops the analysis instead of leaving it running.
    private static async Task<Results<Ok<Models.MediaInfo>, NotFound<AnalysisFailure>>> GetMediaInfo(
        MediaInfoRequest request,
        IMediaInfoAnalyzer analyzer,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var result = await analyzer.AnalyzeAsync(request.Location.OriginalString, cancellationToken);
        return result.Success
            ? TypedResults.Ok(mapper.Map<Models.MediaInfo>(result.AsLegacy()))
            : TypedResults.NotFound(new AnalysisFailure(result.Failure?.Reason.ToString(), result.Failure?.Message));
    }

    private static async Task<Results<Ok<DiscInfo>, NotFound<AnalysisFailure>>> GetDiscInfo(
        MediaInfoRequest request,
        IMediaInfoAnalyzer analyzer,
        CancellationToken cancellationToken)
    {
        var result = await analyzer.AnalyzeAsync(request.Location.OriginalString, cancellationToken);
        if (result.Disc is not { } disc)
        {
            return TypedResults.NotFound(new AnalysisFailure(null, "The location does not hold a DVD or Blu-ray structure."));
        }

        return TypedResults.Ok(new DiscInfo
        {
            Kind = disc.Kind.ToString(),
            RootPath = disc.RootPath,
            TotalSize = disc.TotalSize,
            MainTitleNumber = disc.MainTitle?.Number,
            Titles = disc.Titles
                .Select(x => new DiscTitleInfo
                {
                    Number = x.Number,
                    Duration = x.Duration,
                    Size = x.Size,
                    PrimaryFile = x.PrimaryFile,
                    FileCount = x.Files.Count,

                    // Only a DVD title carries them: the chapters come out of the
                    // navigation tables, and a Blu-ray playlist has no equivalent
                    // that this API reads.
                    Chapters = x is DvdTitle dvd
                        ? dvd.Chapters
                            .Select(c => new DiscChapterInfo
                            {
                                Number = c.Number,
                                Start = c.Start,
                                Duration = c.Duration,
                                Name = c.Name
                            })
                            .ToList()
                        : null
                })
                .ToList()
        });
    }
}

/// <summary>
/// Why the analysis of a location gave nothing.
/// </summary>
/// <param name="Reason">The reason of the failure, when the analyzer named one.</param>
/// <param name="Message">What went wrong.</param>
public sealed record AnalysisFailure(string? Reason, string? Message);
