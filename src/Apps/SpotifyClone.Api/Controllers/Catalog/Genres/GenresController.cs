using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Api.Mappers;
using SpotifyClone.Catalog.Application.Features.Genres.Queries;
using SpotifyClone.Catalog.Application.Features.Genres.Queries.GetDetails;
using SpotifyClone.Catalog.Application.Features.Genres.Queries.List;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Api.Controllers.Catalog.Genres;

[Tags("Catalog Module")]
[Route("api/v1/genres")]
public sealed class GenresController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("Get Genre List")]
    [EndpointDescription("Returns a list of Genres with pagination support.")]
    [ProducesResponseType(typeof(GenreList), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<GenreList>> List(
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        Result<GenreList> result = await Mediator.Send(
            new ListGenresQuery(pagination),
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
            page = result.Value.Genres.Page,
            pageSize = result.Value.Genres.PageSize,
            hasPreviousPage = result.Value.Genres.HasPreviousPage,
            hasNextPage = result.Value.Genres.HasNextPage,
            totalCount = result.Value.Genres.TotalCount,
        }));

        return Ok(result.Value.Genres.Items);
    }

    [EndpointSummary("Get Genre Details")]
    [EndpointDescription("Returns all the necessary Genre details.")]
    [ProducesResponseType(typeof(GenreDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GenreDetails>> GetDetails(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        Result<GenreDetails> result = await Mediator.Send(
            new GetGenreDetailsQuery(id),
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
