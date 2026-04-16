using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Api.Contracts.v1.Catalog.Tracks.CorrectTitle;
using SpotifyClone.Api.Contracts.v1.Catalog.Tracks.Create;
using SpotifyClone.Api.Contracts.v1.Catalog.Tracks.UpdateFeaturedArtists;
using SpotifyClone.Api.Contracts.v1.Catalog.Tracks.UpdateGenres;
using SpotifyClone.Api.Contracts.v1.Catalog.Tracks.UpdateMainArtists;
using SpotifyClone.Api.Contracts.v1.Catalog.Tracks.UpdateMoods;
using SpotifyClone.Api.Mappers;
using SpotifyClone.Catalog.Application.Features.Tracks.Commands.CorrectTitle;
using SpotifyClone.Catalog.Application.Features.Tracks.Commands.Create;
using SpotifyClone.Catalog.Application.Features.Tracks.Commands.Delete;
using SpotifyClone.Catalog.Application.Features.Tracks.Commands.MarkAsExplicit;
using SpotifyClone.Catalog.Application.Features.Tracks.Commands.MarkAsNotExplicit;
using SpotifyClone.Catalog.Application.Features.Tracks.Commands.UnlinkFromAudioFile;
using SpotifyClone.Catalog.Application.Features.Tracks.Commands.UpdateFeaturedArtists;
using SpotifyClone.Catalog.Application.Features.Tracks.Commands.UpdateGenres;
using SpotifyClone.Catalog.Application.Features.Tracks.Commands.UpdateMainArtists;
using SpotifyClone.Catalog.Application.Features.Tracks.Commands.UpdateMoods;
using SpotifyClone.Catalog.Application.Features.Tracks.Queries;
using SpotifyClone.Catalog.Application.Features.Tracks.Queries.GetDetails;
using SpotifyClone.Catalog.Application.Features.Tracks.Queries.List;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Api.Controllers.Catalog.Tracks;

[Tags("Catalog Module")]
[Route("api/v1/tracks")]
public sealed class TracksController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("List Tracks")]
    [EndpointDescription("Returns a list of Tracks with pagination support.")]
    [ProducesResponseType(typeof(TrackList), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<TrackList>> List(
        [FromQuery] TrackFilterParams filters,
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        Result<TrackList> result = await Mediator.Send(
            new ListTracksQuery(filters, pagination),
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

    [EndpointSummary("Get Track Details")]
    [EndpointDescription("Returns all the necessary Track details.")]
    [ProducesResponseType(typeof(TrackDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrackDetails>> GetDetails(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<TrackDetails> result = await Mediator.Send(
            new GetTrackDetailsQuery(id),
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

    [EndpointSummary("Create Track")]
    [EndpointDescription("Creates a Track in a Draft state.")]
    [ProducesResponseType(typeof(CreateTrackResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPost]
    public async Task<ActionResult<CreateTrackResponse>> Create(
        [FromBody] CreateTrackRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<CreateTrackCommandResult> createResult = await Mediator.Send(
            new CreateTrackCommand(
                request.Title,
                request.ContainsExplicitContent,
                request.AlbumId,
                request.MainArtists,
                request.FeaturedArtists,
                request.Genres,
                request.Moods),
            cancellationToken);
        if (createResult.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                createResult,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        CreateTrackCommandResult createResultData = createResult.Value;

        return CreatedAtAction(nameof(TracksController.GetDetails),
            new { id = createResultData.TrackId },
            new CreateTrackResponse(
                createResultData.TrackId));
    }

    [EndpointSummary("Update Track's main artists")]
    [EndpointDescription("Updates the list of track's main artists.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPut("{id:guid}/main-artists")]
    public async Task<ActionResult> UpdateMainArtists(
        [FromRoute] Guid id,
        [FromBody] UpdateTrackMainArtistsRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<UpdateTrackMainArtistsCommandResult> result = await Mediator.Send(
            new UpdateTrackMainArtistsCommand(
                id,
                request.MainArtistIds),
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

    [EndpointSummary("Update Track's featured artists")]
    [EndpointDescription("Updates the list of track's featured artists.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPut("{id:guid}/feat-artists")]
    public async Task<ActionResult> UpdateFeaturedArtists(
        [FromRoute] Guid id,
        [FromBody] UpdateTrackFeaturedArtistsRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<UpdateTrackFeaturedArtistsCommandResult> result = await Mediator.Send(
            new UpdateTrackFeaturedArtistsCommand(
                id,
                request.FeaturedArtistIds),
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

    [EndpointSummary("Update Track's genres")]
    [EndpointDescription("Updates the list of track's genres.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPut("{id:guid}/genres")]
    public async Task<ActionResult> UpdateGenres(
        [FromRoute] Guid id,
        [FromBody] UpdateTrackGenresRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<UpdateTrackGenresCommandResult> result = await Mediator.Send(
            new UpdateTrackGenresCommand(
                id,
                request.GenreIds),
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

    [EndpointSummary("Update Track's moods")]
    [EndpointDescription("Updates the list of track's moods.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPut("{id:guid}/moods")]
    public async Task<ActionResult> UpdateMoods(
        [FromRoute] Guid id,
        [FromBody] UpdateTrackMoodsRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<UpdateTrackMoodsCommandResult> result = await Mediator.Send(
            new UpdateTrackMoodsCommand(
                id,
                request.MoodIds),
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

    [EndpointSummary("Unlink Audio file")]
    [EndpointDescription("Unlinks the audio file from the Track if it's not yet published. " +
        "The Track will return to a Draft state. The audio content will be permanently deleted. " +
        "Note: This operation is eventually consistent; " +
        "the physical file deletion happens in the background.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPost("{id:guid}/unlink-audio-file")]
    public async Task<ActionResult> UnlinkFromAudioFile(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<UnlinkTrackFromAudioFileCommandResult> result = await Mediator.Send(
            new UnlinkTrackFromAudioFileCommand(id),
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

    [EndpointSummary("Correct Track title")]
    [EndpointDescription("Corrects the track title.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPatch("{id:guid}/title")]
    public async Task<ActionResult> CorrectTitle(
        [FromRoute] Guid id,
        [FromBody] CorrectTrackTitleRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<CorrectTrackTitleCommandResult> result = await Mediator.Send(
            new CorrectTrackTitleCommand(
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

    [EndpointSummary("Mark Track as Explicit")]
    [EndpointDescription("Flags the track as containing explicit content.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPost("{id:guid}/explicit")]
    public async Task<ActionResult> MarkAsExplicit(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<MarkTrackAsExplicitCommandResult> result = await Mediator.Send(
            new MarkTrackAsExplicitCommand(id),
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

    [EndpointSummary("Unmark Track as Explicit")]
    [EndpointDescription("Flags the track as containing NO explicit content.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpDelete("{id:guid}/explicit")]
    public async Task<ActionResult> MarkAsNotExplicit(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<MarkTrackAsNotExplicitCommandResult> result = await Mediator.Send(
            new MarkTrackAsNotExplicitCommand(id),
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

    [EndpointSummary("Delete Track")]
    [EndpointDescription("Completely deletes a Track, " +
        "unlinks the Audio asset from it and removes it from all albums and playlists.")]
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
        Result<DeleteTrackCommandResult> result = await Mediator.Send(
            new DeleteTrackCommand(id),
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
