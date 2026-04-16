using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Api.Contracts.v1.Playlists.AddCollaborator;
using SpotifyClone.Api.Contracts.v1.Playlists.AddTrack;
using SpotifyClone.Api.Contracts.v1.Playlists.Create;
using SpotifyClone.Api.Contracts.v1.Playlists.EditDetails;
using SpotifyClone.Api.Contracts.v1.Playlists.LinkNewCover;
using SpotifyClone.Api.Contracts.v1.Playlists.MoveTrack;
using SpotifyClone.Api.Mappers;
using SpotifyClone.Playlists.Application.Features.Playlists.Commands.AddCollaborator;
using SpotifyClone.Playlists.Application.Features.Playlists.Commands.AddTrack;
using SpotifyClone.Playlists.Application.Features.Playlists.Commands.Create;
using SpotifyClone.Playlists.Application.Features.Playlists.Commands.Delete;
using SpotifyClone.Playlists.Application.Features.Playlists.Commands.EditDetails;
using SpotifyClone.Playlists.Application.Features.Playlists.Commands.LinkNewCover;
using SpotifyClone.Playlists.Application.Features.Playlists.Commands.MoveTrack;
using SpotifyClone.Playlists.Application.Features.Playlists.Commands.RemoveCollaborator;
using SpotifyClone.Playlists.Application.Features.Playlists.Commands.RemoveTrackFromPlaylist;
using SpotifyClone.Playlists.Application.Features.Playlists.Queries;
using SpotifyClone.Playlists.Application.Features.Playlists.Queries.GetDetails;
using SpotifyClone.Playlists.Application.Features.Playlists.Queries.List;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Api.Controllers.Playlists;

[Tags("Playlists Module")]
[Route("api/v1/playlists")]
public sealed class PlaylistsController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("List Playlists")]
    [EndpointDescription("Returns a list of Playlists with pagination support.")]
    [ProducesResponseType(typeof(PlaylistList), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PlaylistList>> List(
        [FromQuery] PlaylistFilterParams filters,
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        Result<PlaylistList> result = await Mediator.Send(
            new ListPlaylistsQuery(filters, pagination),
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

    [EndpointSummary("Get Playlist Details")]
    [EndpointDescription("Returns all the necessary Playlist details.")]
    [ProducesResponseType(typeof(PlaylistDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlaylistDetails>> GetDetails(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<PlaylistDetails> result = await Mediator.Send(
            new GetPlaylistDetailsQuery(id),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return Ok(result.Value);
    }

    [EndpointSummary("Create Playlist")]
    [EndpointDescription("Creates a private Playlist.")]
    [ProducesResponseType(typeof(CreatePlaylistResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPost]
    public async Task<ActionResult<CreatePlaylistResponse>> CreatePlaylist(
        CancellationToken cancellationToken = default)
    {
        Result<CreatePlaylistCommandResult> createResult = await Mediator.Send(
            new CreatePlaylistCommand(),
            cancellationToken);
        if (createResult.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                createResult,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return CreatedAtAction(nameof(GetDetails),
            new { id = createResult.Value.PlaylistId },
            new CreatePlaylistResponse(
                createResult.Value.PlaylistId));
    }

    [EndpointSummary("Link Playlist to new cover image")]
    [EndpointDescription("Links a Playlist to a new cover image.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPut("{id:guid}/cover")]
    public async Task<ActionResult> LinkNewCoverImage(
        [FromRoute] Guid id,
        [FromBody] LinkNewCoverToPlaylistRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<LinkNewCoverToPlaylistCommandResult> result = await Mediator.Send(
            new LinkNewCoverToPlaylistCommand(
                id,
                request.ImageId,
                request.ImageWidth,
                request.ImageHeight,
                request.ImageFileType,
                request.ImageSizeInBytes),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }

    [EndpointSummary("Edit Playlist details")]
    [EndpointDescription("Edits playlist details (name, description, visibility).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> EditDetails(
        [FromRoute] Guid id,
        [FromBody] EditPlaylistDetailsRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<EditPlaylistDetailsCommandResult> result = await Mediator.Send(
            new EditPlaylistDetailsCommand(
                id,
                request.Name,
                request.Description,
                request.IsPublic),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }

    [EndpointSummary("Delete Playlist")]
    [EndpointDescription("Deletes a Playlist.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<DeletePlaylistCommandResult> result = await Mediator.Send(
            new DeletePlaylistCommand(id),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }

    [EndpointSummary("Add collaborator to Playlist")]
    [EndpointDescription("Adds a collaborator to a playlist.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPost("{id:guid}/collaborators")]
    public async Task<ActionResult> AddCollaborator(
        [FromRoute] Guid id,
        [FromBody] AddCollaboratorToPlaylistRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<AddCollaboratorToPlaylistCommandResult> result = await Mediator.Send(
            new AddCollaboratorToPlaylistCommand(
                id,
                request.CollaboratorId),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }

    [EndpointSummary("Remove collaborator from Playlist")]
    [EndpointDescription("Removes a collaborator from a playlist.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpDelete("{id:guid}/collaborators/{collaboratorId:guid}")]
    public async Task<ActionResult> RemoveCollaborator(
        [FromRoute] Guid id,
        [FromRoute] Guid collaboratorId,
        CancellationToken cancellationToken = default)
    {
        Result<RemoveCollaboratorFromPlaylistCommandResult> result = await Mediator.Send(
            new RemoveCollaboratorFromPlaylistCommand(id, collaboratorId),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }

    [EndpointSummary("Add track to Playlist")]
    [EndpointDescription("Adds a track to a playlist.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPost("{id:guid}/tracks")]
    public async Task<ActionResult> AddTrack(
        [FromRoute] Guid id,
        [FromBody] AddTrackToPlaylistRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<AddTrackToPlaylistCommandResult> result = await Mediator.Send(
            new AddTrackToPlaylistCommand(
                id,
                request.TrackId),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }

    [EndpointSummary("Remove track from Playlist")]
    [EndpointDescription("Removes a track from a playlist.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpDelete("{id:guid}/tracks/{trackId:guid}")]
    public async Task<ActionResult> RemoveTrack(
        [FromRoute] Guid id,
        [FromRoute] Guid trackId,
        CancellationToken cancellationToken = default)
    {
        Result<RemoveTrackFromPlaylistCommandResult> result = await Mediator.Send(
            new RemoveTrackFromPlaylistCommand(id, trackId),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }

    [EndpointSummary("Move track in Playlist")]
    [EndpointDescription("Moves a track in a playlist.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPost("{id:guid}/tracks/{trackId:guid}/move")]
    public async Task<ActionResult> MoveTracks(
        [FromRoute] Guid id,
        [FromRoute] Guid trackId,
        [FromBody] MoveTrackInPlaylistRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<MoveTrackInPlaylistCommandResult> result = await Mediator.Send(
            new MoveTrackInPlaylistCommand(
                id,
                trackId,
                request.TargetPositionIndex),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }
}
