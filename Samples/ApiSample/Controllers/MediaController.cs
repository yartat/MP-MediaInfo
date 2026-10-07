#region Copyright (C) 2017-2026 Yaroslav Tatarenko

// Copyright (C) 2017-2026 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2026 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using ApiSample.Infrastructure.Filters;
using ApiSample.Models;
using MapsterMapper;
using MediaInfo.Analysis;
using MediaInfo.Analysis.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApiSample.Controllers;

/// <summary>
/// Test controller to show how media info works.
/// Implements the <see cref="ControllerBase" />
/// </summary>
/// <seealso cref="ControllerBase" />
[ApiController]
[Route("[controller]")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[ValidateModelState]
public class MediaController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IMediaInfoAnalyzer _analyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaController"/> class.
    /// </summary>
    /// <param name="mapper">The mapper instance.</param>
    /// <param name="analyzer">The media analyzer.</param>
    /// <exception cref="System.ArgumentNullException">mapper</exception>
    /// <exception cref="System.ArgumentNullException">analyzer</exception>
    public MediaController(IMapper mapper, IMediaInfoAnalyzer analyzer)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
    }

    /// <summary>
    /// Gets the media information by specified location.
    /// </summary>
    /// <param name="request">The location request.</param>
    /// <returns>Returns media info.</returns>
    /// <response code="200">Returns information about media.</response>
    /// <response code="400">Input parameters is null, empty or incorrect.</response>
    /// <response code="404">The media was not found.</response>
    /// <response code="500">Internal service error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Models.MediaInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Dictionary<string, string[]>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Models.MediaInfo>> GetMediaInfo(
        [FromBody, Required(ErrorMessage = "REQUEST_REQUIRED")] MediaInfoRequest request)
    {
        // The request token is forwarded, so a client that gives up stops the analysis instead of leaving it running.
        var result = await _analyzer.AnalyzeAsync(request.Location.OriginalString, HttpContext.RequestAborted);
        return result.Success
            ? _mapper.Map<Models.MediaInfo>(result.AsLegacy())
            : NotFound(new { reason = result.Failure?.Reason.ToString(), message = result.Failure?.Message });
    }

    /// <summary>
    /// Gets the structure of the DVD or Blu-ray disc at the specified location.
    /// </summary>
    /// <param name="request">The location request.</param>
    /// <returns>Returns every title the disc holds.</returns>
    /// <response code="200">Returns the structure of the disc.</response>
    /// <response code="400">Input parameters is null, empty or incorrect.</response>
    /// <response code="404">The location does not hold a disc structure.</response>
    [HttpPost("disc")]
    [ProducesResponseType(typeof(DiscInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Dictionary<string, string[]>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DiscInfo>> GetDiscInfo(
        [FromBody, Required(ErrorMessage = "REQUEST_REQUIRED")] MediaInfoRequest request)
    {
        var result = await _analyzer.AnalyzeAsync(request.Location.OriginalString, HttpContext.RequestAborted);
        if (result.Disc is not { } disc)
        {
            return NotFound(new { message = "The location does not hold a DVD or Blu-ray structure." });
        }

        return new DiscInfo
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
        };
    }
}
