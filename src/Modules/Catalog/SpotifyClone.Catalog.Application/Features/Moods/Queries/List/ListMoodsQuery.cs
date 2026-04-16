using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Catalog.Application.Features.Moods.Queries.List;

public sealed record ListMoodsQuery(
    PaginationParams Pagination)
    : IQuery<MoodList>;
