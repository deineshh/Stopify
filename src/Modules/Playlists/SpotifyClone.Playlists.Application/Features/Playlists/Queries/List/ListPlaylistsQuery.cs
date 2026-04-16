using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Playlists.Application.Features.Playlists.Queries.List;

public sealed record ListPlaylistsQuery(
    PlaylistFilterParams Filters,
    PaginationParams Pagination)
    : IQuery<PlaylistList>;
