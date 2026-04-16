using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Catalog.Application.Features.Tracks.Queries.List;

public sealed record ListTracksQuery(
    TrackFilterParams Filters,
    PaginationParams Pagination)
    : IQuery<TrackList>;
