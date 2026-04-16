using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Playlists.Application.Features.Playlists.Queries;

public sealed record PlaylistList(
    PagedList<PlaylistSummary> Playlists);
