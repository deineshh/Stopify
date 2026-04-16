using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Api.Mappers;
using SpotifyClone.Catalog.Application.Features.Artists.Commands.Ban;
using SpotifyClone.Catalog.Application.Features.Artists.Commands.Unban;
using SpotifyClone.Catalog.Application.Features.Artists.Commands.Unverify;
using SpotifyClone.Catalog.Application.Features.Artists.Commands.Verify;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Api.Controllers.Catalog.Artists;

[Tags("Catalog Module")]
[Route("api/v1/admin/artists")]
[Authorize(Roles = UserRoles.Admin)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class ArtistsAdminController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("Ban Artist")]
    [EndpointDescription("Bans an artist and unpublishes all it's releases (albums & tracks).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpPost("{id:guid}/ban")]
    public async Task<ActionResult> Ban(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<BanArtistCommandResult> result = await Mediator.Send(
            new BanArtistCommand(id),
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

    [EndpointSummary("Unban Artist")]
    [EndpointDescription("Unbans an artist but not publishes back it's releases (albums & tracks).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpPost("{id:guid}/unban")]
    public async Task<ActionResult> Unban(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<UnbanArtistCommandResult> result = await Mediator.Send(
            new UnbanArtistCommand(id),
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

    [EndpointSummary("Verify Artist")]
    [EndpointDescription("Verifies an artist to unlock new features (bio, banner, gallery etc).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpPost("{id:guid}/verify")]
    public async Task<ActionResult> Verify(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<VerifyArtistCommandResult> result = await Mediator.Send(
            new VerifyArtistCommand(id),
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

    [EndpointSummary("Unverify Artist")]
    [EndpointDescription("Unverifies a verified artist to prevent from additional features.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id:guid}/verify")]
    public async Task<ActionResult> Unverify(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<UnverifyArtistCommandResult> result = await Mediator.Send(
            new UnverifyArtistCommand(id),
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
