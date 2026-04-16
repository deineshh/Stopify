using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Catalog.Application.Features.Genres.Queries.List;

public sealed record ListGenresQuery(
    PaginationParams Pagination)
    : IQuery<GenreList>;
