using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Catalog.Application.Features.Genres.Queries.List;

internal sealed class ListGenresQueryHandler(
    IGenreReadService genreReadService)
    : IQueryHandler<ListGenresQuery, GenreList>
{
    private readonly IGenreReadService _genreReadService = genreReadService;

    public async Task<Result<GenreList>> Handle(
        ListGenresQuery request,
        CancellationToken cancellationToken)
    {
        PagedList<GenreSummary> genres = await _genreReadService.ListAsync(
            request.Pagination, cancellationToken);

        return new GenreList(genres);
    }
}
