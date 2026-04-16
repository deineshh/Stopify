using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Api.Mappers;
using SpotifyClone.Catalog.Application.Features.Moods.Queries;
using SpotifyClone.Catalog.Application.Features.Moods.Queries.GetDetails;
using SpotifyClone.Catalog.Application.Features.Moods.Queries.List;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Api.Controllers.Catalog.Moods;

[Tags("Catalog Module")]
[Route("api/v1/moods")]
public sealed class MoodsController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("List Moods")]
    [EndpointDescription("Returns a list of Moods with pagination support.")]
    [ProducesResponseType(typeof(MoodList), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<MoodList>> List(
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        Result<MoodList> result = await Mediator.Send(
            new ListMoodsQuery(pagination),
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
            page = result.Value.Moods.Page,
            pageSize = result.Value.Moods.PageSize,
            hasPreviousPage = result.Value.Moods.HasPreviousPage,
            hasNextPage = result.Value.Moods.HasNextPage,
            totalCount = result.Value.Moods.TotalCount,
        }));

        return Ok(result.Value.Moods.Items);
    }

    [EndpointSummary("Get Mood Details")]
    [EndpointDescription("Returns all the necessary Mood details.")]
    [ProducesResponseType(typeof(MoodDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MoodDetails>> GetDetails(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<MoodDetails> result = await Mediator.Send(
            new GetMoodDetailsQuery(id),
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
}
