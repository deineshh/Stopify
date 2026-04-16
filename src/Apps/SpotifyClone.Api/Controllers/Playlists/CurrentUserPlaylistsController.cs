using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Api.Mappers;
using SpotifyClone.Playlists.Application.Features.Playlists.Queries;
using SpotifyClone.Playlists.Application.Features.Playlists.Queries.List;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Api.Controllers.Playlists;

[Route("api/v1/me/playlists")]
public sealed class CurrentUserPlaylistsController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("Get current User Playlists")]
    [EndpointDescription("Returns the current User's playlists.")]
    [ProducesResponseType(typeof(PlaylistList), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpGet]
    public async Task<ActionResult<PlaylistList>> List(
        [FromQuery] PlaylistFilterParams filters,
        [FromQuery] PaginationParams pagination,
        ICurrentUser currentUser,
        CancellationToken cancellationToken = default)
    {
        PlaylistFilterParams filtersWithCurrentUser = filters with { OwnerId = currentUser.Id };

        Result<PlaylistList> result = await Mediator.Send(
            new ListPlaylistsQuery(filtersWithCurrentUser, pagination),
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
            page = result.Value.Playlists.Page,
            pageSize = result.Value.Playlists.PageSize,
            hasPreviousPage = result.Value.Playlists.HasPreviousPage,
            hasNextPage = result.Value.Playlists.HasNextPage,
            totalCount = result.Value.Playlists.TotalCount,
        }));

        return Ok(result.Value.Playlists.Items);
    }
}
