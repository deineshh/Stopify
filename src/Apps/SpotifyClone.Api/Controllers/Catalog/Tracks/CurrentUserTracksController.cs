using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Api.Mappers;
using SpotifyClone.Catalog.Application.Features.Tracks.Queries;
using SpotifyClone.Catalog.Application.Features.Tracks.Queries.List;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Api.Controllers.Catalog.Tracks;

[Tags("Catalog Module")]
[Route("api/v1/me/tracks")]
public sealed class CurrentUserTracksController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("List Tracks")]
    [EndpointDescription("Returns a list of current user's Tracks with pagination support.")]
    [ProducesResponseType(typeof(TrackList), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpGet]
    public async Task<ActionResult<TrackList>> List(
        [FromQuery] TrackFilterParams filters,
        [FromQuery] PaginationParams pagination,
        ICurrentUser currentUser,
        CancellationToken cancellationToken = default)
    {
        TrackFilterParams filtersWithCurrentUser = filters with { OwnerId = currentUser.Id };

        Result<TrackList> result = await Mediator.Send(
            new ListTracksQuery(filtersWithCurrentUser, pagination),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(new
        {
            page = result.Value.Tracks.Page,
            pageSize = result.Value.Tracks.PageSize,
            hasPreviousPage = result.Value.Tracks.HasPreviousPage,
            hasNextPage = result.Value.Tracks.HasNextPage,
            totalCount = result.Value.Tracks.TotalCount,
        }));

        return Ok(result.Value.Tracks.Items);
    }
}
