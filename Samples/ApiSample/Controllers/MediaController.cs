#region Copyright (C) 2017-2022 Yaroslav Tatarenko

// Copyright (C) 2017-2022 Yaroslav Tatarenko
// This product uses MediaInfo library, Copyright (c) 2002-2021 MediaArea.net SARL. 
// https://mediaarea.net

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using ApiSample.Infrastructure.Filters;
using ApiSample.Models;
using AutoMapper;
using MediaInfo;
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
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="mapper">The mapper instance.</param>
    /// <exception cref="System.ArgumentNullException">logger</exception>
    /// <exception cref="System.ArgumentNullException">mapper</exception>
    public MediaController(ILogger<MediaController> logger, IMapper mapper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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
    public ActionResult<Models.MediaInfo> GetMediaInfo([FromBody, Required(ErrorMessage = "REQUEST_REQUIRED")] MediaInfoRequest request)
    {
        var media = new MediaInfoWrapper(request.Location.OriginalString, _logger);
        return media.Success ?
            _mapper.Map<Models.MediaInfo>(media) :
            NotFound();
    }
}
