using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Accounts.Application.Features.Accounts.Commands.Delete;
using SpotifyClone.Accounts.Application.Features.Accounts.Commands.EditPersonalInfo;
using SpotifyClone.Accounts.Application.Features.Accounts.Commands.EditProfileDetails;
using SpotifyClone.Accounts.Application.Features.Accounts.Commands.LinkNewAvatar;
using SpotifyClone.Accounts.Application.Features.Accounts.Commands.UnlinkAvatar;
using SpotifyClone.Accounts.Application.Features.Accounts.Queries;
using SpotifyClone.Accounts.Application.Features.Accounts.Queries.GetCurrentDetails;
using SpotifyClone.Accounts.Application.Features.Accounts.Queries.GetProfileDetails;
using SpotifyClone.Accounts.Application.Features.Accounts.Queries.List;
using SpotifyClone.Api.Contracts.v1.Accounts.Profile.EditPersonalInfo;
using SpotifyClone.Api.Contracts.v1.Accounts.Profile.EditProfileDetails;
using SpotifyClone.Api.Contracts.v1.Accounts.Profile.LinkNewAvatar;
using SpotifyClone.Api.Mappers;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Api.Controllers.Accounts;

[Route("api/v1/users")]
public sealed class UsersController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("List Users")]
    [EndpointDescription("Returns a list of Users with pagination support.")]
    [ProducesResponseType(typeof(UserList), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult> List(
        [FromQuery] UserFilterParams filters,
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        Result<UserList> result = await Mediator.Send(
            new ListUsersQuery(filters, pagination),
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
            page = result.Value.Users.Page,
            pageSize = result.Value.Users.PageSize,
            hasPreviousPage = result.Value.Users.HasPreviousPage,
            hasNextPage = result.Value.Users.HasNextPage,
            totalCount = result.Value.Users.TotalCount,
        }));

        return Ok(result.Value.Users.Items);
    }

    [EndpointSummary("Get User's public profile")]
    [EndpointDescription("Returns a User's public profile.")]
    [ProducesResponseType(typeof(UserProfileDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetPublicProfile(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<UserProfileDetails> result = await Mediator.Send(
            new GetUserProfileDetailsQuery(id),
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

    [EndpointSummary("Get current User private profile")]
    [EndpointDescription("Returns the current User's private profile.")]
    [ProducesResponseType(typeof(UserProfileDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpGet("me")]
    public async Task<ActionResult> GetCurrentProfile(
        CancellationToken cancellationToken = default)
    {
        Result<CurrentUserDetails> result = await Mediator.Send(
            new GetCurrentUserDetailsQuery(),
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

    [EndpointSummary("Link User to new avatar image")]
    [EndpointDescription("Links a User to a new avatar image.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPut("{id:guid}/avatar")]
    public async Task<ActionResult> LinkNewAvatarImage(
        [FromRoute] Guid id,
        [FromBody] LinkNewAvatarToUserRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<LinkNewAvatarToUserCommandResult> result = await Mediator.Send(
            new LinkNewAvatarToUserCommand(
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

    [EndpointSummary("Unlink User from avatar image")]
    [EndpointDescription("Unlinks a User from it's avatar image.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpDelete("{id:guid}/avatar")]
    public async Task<ActionResult> UnlinkAvatarImage(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<UnlinkAvatarFromUserCommandResult> result = await Mediator.Send(
            new UnlinkAvatarFromUserCommand(id),
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

    [EndpointSummary("Edit User profile details")]
    [EndpointDescription("Edits basic User's profile details (display name).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPut("{id:guid}/profile")]
    public async Task<ActionResult> EditProfileDetails(
        [FromRoute] Guid id,
        [FromBody] EditUserProfileDetailsRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<EditUserProfileDetailsCommandResult> result = await Mediator.Send(
            new EditUserProfileDetailsCommand(
                id,
                request.DisplayName),
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

    [EndpointSummary("Edit User personal info")]
    [EndpointDescription("Edits User's personal information.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPut("{id:guid}/info")]
    public async Task<ActionResult> EditPersonalInfo(
        [FromRoute] Guid id,
        [FromBody] EditUserPersonalInfoRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<EditUserPersonalInfoCommandResult> result = await Mediator.Send(
            new EditUserPersonalInfoCommand(
                id,
                request.Email,
                request.Password,
                request.Gender,
                request.BirthDateUtc),
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

    [EndpointSummary("Delete User")]
    [EndpointDescription("Deletes a User and his entire profile.")]
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
        Result<DeleteUserCommandResult> result = await Mediator.Send(
            new DeleteUserCommand(id),
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
