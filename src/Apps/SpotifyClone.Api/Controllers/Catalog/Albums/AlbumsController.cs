using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Api.Contracts.v1.Catalog.Albums.AddTrackToAlbum;
using SpotifyClone.Api.Contracts.v1.Catalog.Albums.CorrectTitle;
using SpotifyClone.Api.Contracts.v1.Catalog.Albums.Create;
using SpotifyClone.Api.Contracts.v1.Catalog.Albums.LinkNewCover;
using SpotifyClone.Api.Contracts.v1.Catalog.Albums.MoveTrack;
using SpotifyClone.Api.Contracts.v1.Catalog.Albums.PublishAlbum;
using SpotifyClone.Api.Contracts.v1.Catalog.Albums.RescheduleRelease;
using SpotifyClone.Api.Contracts.v1.Catalog.Albums.UpdateMainArtists;
using SpotifyClone.Api.Mappers;
using SpotifyClone.Catalog.Application.Features.Albums.Commands.AddTrack;
using SpotifyClone.Catalog.Application.Features.Albums.Commands.CorrectTitle;
using SpotifyClone.Catalog.Application.Features.Albums.Commands.Create;
using SpotifyClone.Catalog.Application.Features.Albums.Commands.Delete;
using SpotifyClone.Catalog.Application.Features.Albums.Commands.LinkNewCover;
using SpotifyClone.Catalog.Application.Features.Albums.Commands.MoveTrack;
using SpotifyClone.Catalog.Application.Features.Albums.Commands.PublishAlbum;
using SpotifyClone.Catalog.Application.Features.Albums.Commands.RemoveTrack;
using SpotifyClone.Catalog.Application.Features.Albums.Commands.RescheduleRelease;
using SpotifyClone.Catalog.Application.Features.Albums.Commands.UnpublishAlbum;
using SpotifyClone.Catalog.Application.Features.Albums.Commands.UpdateMainArtists;
using SpotifyClone.Catalog.Application.Features.Albums.Queries;
using SpotifyClone.Catalog.Application.Features.Albums.Queries.GetDetails;
using SpotifyClone.Catalog.Application.Features.Albums.Queries.List;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Api.Controllers.Catalog.Albums;

[Tags("Catalog Module")]
[Route("api/v1/albums")]
public sealed class AlbumsController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("List Albums")]
    [EndpointDescription("Returns a list of Albums with pagination support.")]
    [ProducesResponseType(typeof(AlbumList), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<AlbumList>> List(
        [FromQuery] AlbumFilterParams filters,
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        Result<AlbumList> result = await Mediator.Send(
            new ListAlbumsQuery(filters, pagination),
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
            page = result.Value.Albums.Page,
            pageSize = result.Value.Albums.PageSize,
            hasPreviousPage = result.Value.Albums.HasPreviousPage,
            hasNextPage = result.Value.Albums.HasNextPage,
            totalCount = result.Value.Albums.TotalCount,
        }));

        return Ok(result.Value.Albums.Items);
    }

    [EndpointSummary("Get Album Details")]
    [EndpointDescription("Returns all the necessary Album details.")]
    [ProducesResponseType(typeof(AlbumDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AlbumDetails>> GetDetails(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<AlbumDetails> result = await Mediator.Send(
            new GetAlbumDetailsQuery(id),
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

    [EndpointSummary("Create Album")]
    [EndpointDescription("Creates an Album in a Draft state.")]
    [ProducesResponseType(typeof(CreateAlbumResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPost]
    public async Task<ActionResult<CreateAlbumResponse>> Create(
        [FromBody] CreateAlbumRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<CreateAlbumCommandResult> createResult = await Mediator.Send(
            new CreateAlbumCommand(
                request.Title,
                request.MainArtistIds),
            cancellationToken);
        if (createResult.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                createResult,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        CreateAlbumCommandResult createResultData = createResult.Value;

        return CreatedAtAction(nameof(GetDetails),
            new { id = createResultData.AlbumId },
            new CreateAlbumResponse(
                createResultData.AlbumId));
    }

    [EndpointSummary("Update Album's main artists")]
    [EndpointDescription("Updates the list of album's main artists.")]
    [ProducesResponseType(typeof(CreateAlbumResponse), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPut("{id:guid}/main-artists")]
    public async Task<ActionResult<CreateAlbumResponse>> UpdateMainArtists(
        [FromRoute] Guid id,
        [FromBody] UpdateAlbumMainArtistsRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<UpdateAlbumMainArtistsCommandResult> createResult = await Mediator.Send(
            new UpdateAlbumMainArtistsCommand(
                id,
                request.MainArtistIds),
            cancellationToken);
        if (createResult.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                createResult,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }

    [EndpointSummary("Delete Album")]
    [EndpointDescription("Deletes an Album if it's not yet published.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<DeleteAlbumCommandResult> result = await Mediator.Send(
            new DeleteAlbumCommand(id),
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

    [EndpointSummary("Link Album to new Cover image")]
    [EndpointDescription("Links an Album to a new Cover image if it's not yet published.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPut("{id:guid}/cover")]
    public async Task<ActionResult> LinkNewCoverImage(
        [FromRoute] Guid id,
        [FromBody] LinkNewCoverToAlbumRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<LinkNewCoverToAlbumCommandResult> result = await Mediator.Send(
            new LinkNewCoverToAlbumCommand(
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

    [EndpointSummary("Publish Album")]
    [EndpointDescription("Publishes an Album and all of it's Tracks if the Album is ready to publish.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult> Publish(
        [FromRoute] Guid id,
        [FromBody] PublishAlbumRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<PublishAlbumCommandResult> result = await Mediator.Send(
            new PublishAlbumCommand(
                id,
                request.ReleaseDate),
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

    [EndpointSummary("Unpublish Album")]
    [EndpointDescription("Unpublishes an Album and all of it's Tracks if the Album is published.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPost("{id:guid}/unpublish")]
    public async Task<ActionResult> Unpublish(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<UnpublishAlbumCommandResult> result = await Mediator.Send(
            new UnpublishAlbumCommand(id),
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

    [EndpointSummary("Add Track to Album")]
    [EndpointDescription("Adds a new Track to an Album if the Track is not yet attached to a different album.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPost("{id:guid}/tracks")]
    public async Task<ActionResult> AddTrack(
        [FromRoute] Guid id,
        [FromBody] AddTrackToAlbumRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<AddTrackToAlbumCommandResult> result = await Mediator.Send(
            new AddTrackToAlbumCommand(
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

    [EndpointSummary("Remove Track from Album")]
    [EndpointDescription("Removes an existing Track from an Album and archive the removed Track.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpDelete("{id:guid}/tracks/{trackId:guid}")]
    public async Task<ActionResult> RemoveFromAlbum(
        [FromRoute] Guid id,
        [FromRoute] Guid trackId,
        CancellationToken cancellationToken = default)
    {
        Result<RemoveTrackFromAlbumCommandResult> result = await Mediator.Send(
            new RemoveTrackFromAlbumCommand(id, trackId),
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

    [EndpointSummary("Move Track in Album")]
    [EndpointDescription("Move an existing Track in an Album.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPost("{id:guid}/tracks/{trackId:guid}/move")]
    public async Task<ActionResult> MoveTrack(
        [FromRoute] Guid id,
        [FromRoute] Guid trackId,
        [FromBody] MoveTrackInAlbumRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<MoveTrackInAlbumCommandResult> result = await Mediator.Send(
            new MoveTrackInAlbumCommand(
                id, trackId, request.TargetPositionIndex),
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

    [EndpointSummary("Correct Album title")]
    [EndpointDescription("Corrects the Album title.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPatch("{id:guid}/title")]
    public async Task<ActionResult> CorrectTitle(
        [FromRoute] Guid id,
        [FromBody] CorrectAlbumTitleRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<CorrectAlbumTitleCommandResult> result = await Mediator.Send(
            new CorrectAlbumTitleCommand(
                id,
                request.Title),
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

    [EndpointSummary("Reschedule Album Release")]
    [EndpointDescription("Reschedules Album release if it's published but not released yet. " +
        "Automatically does the same for all it's Tracks.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPatch("{id:guid}/release")]
    public async Task<ActionResult> RescheduleRelease(
        [FromRoute] Guid id,
        [FromBody] RescheduleAlbumReleaseRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<RescheduleAlbumReleaseCommandResult> result = await Mediator.Send(
            new RescheduleAlbumReleaseCommand(
                id,
                request.ReleaseDate),
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
