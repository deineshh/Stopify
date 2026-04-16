using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Api.Contracts.v1.Catalog.Artists.AddGalleryImage;
using SpotifyClone.Api.Contracts.v1.Catalog.Artists.Create;
using SpotifyClone.Api.Contracts.v1.Catalog.Artists.EditProfile;
using SpotifyClone.Api.Contracts.v1.Catalog.Artists.LinkNewAvatar;
using SpotifyClone.Api.Contracts.v1.Catalog.Artists.LinkNewBanner;
using SpotifyClone.Api.Mappers;
using SpotifyClone.Catalog.Application.Features.Artists.Commands.AddGalleryImage;
using SpotifyClone.Catalog.Application.Features.Artists.Commands.Create;
using SpotifyClone.Catalog.Application.Features.Artists.Commands.EditProfile;
using SpotifyClone.Catalog.Application.Features.Artists.Commands.LinkNewAvatar;
using SpotifyClone.Catalog.Application.Features.Artists.Commands.LinkNewBanner;
using SpotifyClone.Catalog.Application.Features.Artists.Commands.RemoveGalleryImageFromArtist;
using SpotifyClone.Catalog.Application.Features.Artists.Commands.UnlinkAvatar;
using SpotifyClone.Catalog.Application.Features.Artists.Commands.UnlinkBanner;
using SpotifyClone.Catalog.Application.Features.Artists.Queries;
using SpotifyClone.Catalog.Application.Features.Artists.Queries.GetDetails;
using SpotifyClone.Catalog.Application.Features.Artists.Queries.List;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Api.Controllers.Catalog.Artists;

[Tags("Catalog Module")]
[Route("api/v1/artists")]
public sealed class ArtistsController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("List Artists")]
    [EndpointDescription("Returns a list of Artists with pagination support.")]
    [ProducesResponseType(typeof(ArtistList), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ArtistList>> List(
        [FromQuery] ArtistFilterParams filters,
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        Result<ArtistList> result = await Mediator.Send(
            new ListArtistsQuery(filters, pagination),
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
            page = result.Value.Artists.Page,
            pageSize = result.Value.Artists.PageSize,
            hasPreviousPage = result.Value.Artists.HasPreviousPage,
            hasNextPage = result.Value.Artists.HasNextPage,
            totalCount = result.Value.Artists.TotalCount,
        }));

        return Ok(result.Value.Artists.Items);
    }

    [EndpointSummary("Get Artist Details")]
    [EndpointDescription("Returns all the necessary Artist details.")]
    [ProducesResponseType(typeof(ArtistDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArtistDetails>> GetDetails(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<ArtistDetails> result = await Mediator.Send(
            new GetArtistDetailsQuery(id),
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

    [EndpointSummary("Create Artist")]
    [EndpointDescription("Creates an Artist in a non-verified state. " +
        "Note: Non-verified artists cannot have custom bio, banner or gallery.")]
    [ProducesResponseType(typeof(CreateArtistResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPost]
    public async Task<ActionResult<CreateArtistResponse>> Create(
        [FromBody] CreateArtistRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<CreateArtistCommandResult> createResult = await Mediator.Send(
            new CreateArtistCommand(
                request.Name),
            cancellationToken);
        if (createResult.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                createResult,
                HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        CreateArtistCommandResult createResultData = createResult.Value;

        return CreatedAtAction(nameof(GetDetails),
            new { id = createResultData.ArtistId },
            new CreateArtistResponse(
                createResultData.ArtistId));
    }

    [EndpointSummary("Link Artist to new avatar image")]
    [EndpointDescription("Links an Artist to a new avatar image.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPut("{id:guid}/avatar")]
    public async Task<ActionResult> LinkNewAvatarImage(
        [FromRoute] Guid id,
        [FromBody] LinkNewAvatarToArtistRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<LinkNewAvatarToArtistCommandResult> result = await Mediator.Send(
            new LinkNewAvatarToArtistCommand(
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

    [EndpointSummary("Unlink Artist from avatar image")]
    [EndpointDescription("Unlinks an Artist from it's avatar image.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpDelete("{id:guid}/avatar")]
    public async Task<ActionResult> UnlinkAvatarImage(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<UnlinkAvatarFromArtistCommandResult> result = await Mediator.Send(
            new UnlinkAvatarFromArtistCommand(id),
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

    [EndpointSummary("Link Artist to new banner image")]
    [EndpointDescription("Links an Artist to a new banner image.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPut("{id:guid}/banner")]
    public async Task<ActionResult> LinkNewBannerImage(
        [FromRoute] Guid id,
        [FromBody] LinkNewBannerToArtistRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<LinkNewBannerToArtistCommandResult> result = await Mediator.Send(
            new LinkNewBannerToArtistCommand(
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

    [EndpointSummary("Unlink Artist from banner image")]
    [EndpointDescription("Unlinks an Artist from it's banner image.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpDelete("{id:guid}/banner")]
    public async Task<ActionResult> UnlinkBannerImage(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<UnlinkBannerFromArtistCommandResult> result = await Mediator.Send(
            new UnlinkBannerFromArtistCommand(id),
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

    [EndpointSummary("Edit Artist profile")]
    [EndpointDescription("Edits an Artist's profile info.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPut("{id:guid}/profile")]
    public async Task<ActionResult> EditProfile(
        [FromRoute] Guid id,
        [FromBody] EditArtistProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<EditArtistProfileCommandResult> result = await Mediator.Send(
            new EditArtistProfileCommand(
                id,
                request.Name,
                request.Bio),
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

    [EndpointSummary("Add image to Artist's gallery")]
    [EndpointDescription("Adds an image to a verified artist's gallery.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpPost("{id:guid}/gallery")]
    public async Task<ActionResult> AddGalleryImage(
        [FromRoute] Guid id,
        [FromBody] AddGalleryImageToArtistRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<AddGalleryImageToArtistCommandResult> result = await Mediator.Send(
            new AddGalleryImageToArtistCommand(
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

    [EndpointSummary("Remove image from Artist's gallery")]
    [EndpointDescription("Removes an image from a verified artist's gallery.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Creator)]
    [HttpDelete("{id:guid}/gallery/{imageId:guid}")]
    public async Task<ActionResult> RemoveGalleryImage(
        [FromRoute] Guid id,
        [FromRoute] Guid imageId,
        CancellationToken cancellationToken = default)
    {
        Result<RemoveGalleryImageFromArtistCommandResult> result = await Mediator.Send(
            new RemoveGalleryImageFromArtistCommand(id, imageId),
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
