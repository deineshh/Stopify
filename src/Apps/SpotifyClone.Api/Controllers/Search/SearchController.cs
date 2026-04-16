using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Api.Contracts.v1.Search.GlobalSearch;
using SpotifyClone.Api.Mappers;
using SpotifyClone.Search.Application.Features.GlobalSearch;
using SpotifyClone.Search.Application.Jobs;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Services;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Api.Controllers.Search;

[Tags("Search Module")]
[Route("api/v1/search")]
public sealed class PlaylistsController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("Global Search")]
    [EndpointDescription("Returns a list of Users, Genres, Moods, Artists, Albums, Tracks, and Playlists " +
                         "based on the search term.")]
    [ProducesResponseType(typeof(GlobalSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<GlobalSearchQuery>> GlobalSearch(
        [FromQuery] GlobalSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<GlobalSearchResponse> result = await Mediator.Send(
            new GlobalSearchQuery(
                request.SearchTerm, request.Limit),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result, HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return Ok(result.Value);
    }

    [EndpointSummary("Trigger Search Reindex")]
    [EndpointDescription("Triggers a reindexing of all searchable entities in the system.")]
    [ProducesResponseType(typeof(GlobalSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpPost("reindex")]
    public async Task<ActionResult<GlobalSearchQuery>> TriggerReindex(
        IBackgroundJobService jobService,
        CancellationToken cancellationToken = default)
    {
        jobService.Enqueue<FullReindexJob>(job => job.ProcessAsync(cancellationToken));

        return Accepted(new { Message = "Reindexing searchable entities started in the background..." });
    }
}
