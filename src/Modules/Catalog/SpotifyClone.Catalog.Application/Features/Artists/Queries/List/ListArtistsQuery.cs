using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Catalog.Application.Features.Artists.Queries.List;

public sealed record ListArtistsQuery(
    ArtistFilterParams Filters,
    PaginationParams Pagination)
    : IQuery<ArtistList>;
