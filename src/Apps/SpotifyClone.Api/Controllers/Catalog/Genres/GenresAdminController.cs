using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Api.Contracts.v1.Catalog.Genres.Create;
using SpotifyClone.Api.Contracts.v1.Catalog.Genres.LinkNewCover;
using SpotifyClone.Api.Contracts.v1.Catalog.Genres.Rename;
using SpotifyClone.Api.Mappers;
using SpotifyClone.Catalog.Application.Features.Genres.Commands.Create;
using SpotifyClone.Catalog.Application.Features.Genres.Commands.Delete;
using SpotifyClone.Catalog.Application.Features.Genres.Commands.LinkNewCover;
using SpotifyClone.Catalog.Application.Features.Genres.Commands.Rename;
using SpotifyClone.Catalog.Application.Features.Genres.Commands.UnlinkCover;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Api.Controllers.Catalog.Genres;

[Tags("Catalog Module")]
[Route("api/v1/admin/genres")]
[Authorize(Roles = UserRoles.Admin)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class GenresAdminController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("Create Genre")]
    [EndpointDescription("Creates a Genre without cover.")]
    [ProducesResponseType(typeof(CreateGenreResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<ActionResult<CreateGenreResponse>> Create(
        [FromBody] CreateGenreRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<CreateGenreCommandResult> createResult = await Mediator.Send(
            new CreateGenreCommand(request.Name),
            cancellationToken);
        if (createResult.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                createResult,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        CreateGenreCommandResult createResultData = createResult.Value;

        return CreatedAtAction(
            actionName: nameof(GenresController.GetDetails),
            controllerName: "Genres",
            routeValues: new { id = createResultData.GenreId },
            value: new CreateGenreResponse(createResultData.GenreId));
    }

    [EndpointSummary("Link Genre to new cover image")]
    [EndpointDescription("Links a Genre to a new cover image.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpPut("{id:guid}/cover")]
    public async Task<ActionResult> LinkNewCoverImage(
        [FromRoute] Guid id,
        [FromBody] LinkNewCoverToGenreRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<LinkNewCoverToGenreCommandResult> result = await Mediator.Send(
            new LinkNewCoverToGenreCommand(
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

    [EndpointSummary("Unlink Genre from cover image")]
    [EndpointDescription("Unlinks a Genre from it's cover image.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id:guid}/cover")]
    public async Task<ActionResult> UnlinkCoverImage(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<UnlinkCoverFromGenreCommandResult> result = await Mediator.Send(
            new UnlinkCoverFromGenreCommand(id),
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

    [EndpointSummary("Delete Genre")]
    [EndpointDescription("Deletes a Genre, but also removes the link from the cover image asset and all tracks.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<DeleteGenreCommandResult> result = await Mediator.Send(
            new DeleteGenreCommand(id),
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

    [EndpointSummary("Rename Genre")]
    [EndpointDescription("Renames a Genre.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpPatch("{id:guid}/name")]
    public async Task<ActionResult> Rename(
        [FromRoute] Guid id,
        [FromBody] RenameGenreRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<RenameGenreCommandResult> result = await Mediator.Send(
            new RenameGenreCommand(
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
