using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Api.Contracts.v1.Catalog.Moods.Create;
using SpotifyClone.Api.Contracts.v1.Catalog.Moods.LinkNewCover;
using SpotifyClone.Api.Contracts.v1.Catalog.Moods.Rename;
using SpotifyClone.Api.Mappers;
using SpotifyClone.Catalog.Application.Features.Moods.Commands.Create;
using SpotifyClone.Catalog.Application.Features.Moods.Commands.Delete;
using SpotifyClone.Catalog.Application.Features.Moods.Commands.LinkNewCover;
using SpotifyClone.Catalog.Application.Features.Moods.Commands.Rename;
using SpotifyClone.Catalog.Application.Features.Moods.Commands.UnlinkCover;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Api.Controllers.Catalog.Moods;

[Tags("Catalog Module")]
[Route("api/v1/admin/moods")]
[Authorize(Roles = UserRoles.Admin)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class MoodsAdminController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("Create Mood")]
    [EndpointDescription("Creates a Mood without cover.")]
    [ProducesResponseType(typeof(CreateMoodResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<ActionResult<CreateMoodResponse>> Create(
        [FromBody] CreateMoodRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<CreateMoodCommandResult> createResult = await Mediator.Send(
            new CreateMoodCommand(request.Name),
            cancellationToken);
        if (createResult.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                createResult,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        CreateMoodCommandResult createResultData = createResult.Value;

        return CreatedAtAction(
            actionName: nameof(MoodsController.GetDetails),
            controllerName: "Moods",
            routeValues: new { id = createResultData.MoodId },
            value: new CreateMoodResponse(createResultData.MoodId));
    }

    [EndpointSummary("Link Mood to new cover image")]
    [EndpointDescription("Links a Mood to a new cover image.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpPut("{id:guid}/cover")]
    public async Task<ActionResult> LinkNewCoverImage(
        [FromRoute] Guid id,
        [FromBody] LinkNewCoverToMoodRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<LinkNewCoverToMoodCommandResult> result = await Mediator.Send(
            new LinkNewCoverToMoodCommand(
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

    [EndpointSummary("Unlink Mood from cover image")]
    [EndpointDescription("Unlinks a Mood from it's cover image.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id:guid}/cover")]
    public async Task<ActionResult> UnlinkCoverImage(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<UnlinkCoverFromMoodCommandResult> result = await Mediator.Send(
            new UnlinkCoverFromMoodCommand(id),
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

    [EndpointSummary("Delete Mood")]
    [EndpointDescription("Deletes a Mood, but also removes the link from the cover image asset and all tracks.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<DeleteMoodCommandResult> result = await Mediator.Send(
            new DeleteMoodCommand(id),
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

    [EndpointSummary("Rename Mood")]
    [EndpointDescription("Renames a Mood.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpPatch("{id:guid}/name")]
    public async Task<ActionResult> Rename(
        [FromRoute] Guid id,
        [FromBody] RenameMoodRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<RenameMoodCommandResult> result = await Mediator.Send(
            new RenameMoodCommand(
                id,
                request.Name),
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
