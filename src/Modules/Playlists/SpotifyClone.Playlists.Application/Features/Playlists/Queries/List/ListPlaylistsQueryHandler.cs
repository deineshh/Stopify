using SpotifyClone.Playlists.Application.Abstractions.Data;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Playlists.Application.Features.Playlists.Queries.List;

internal sealed class ListPlaylistsQueryHandler(
    IPlaylistReadService playlistReadService,
    ICurrentUser currentUser)
    : IQueryHandler<ListPlaylistsQuery, PlaylistList>
{
    private readonly IPlaylistReadService _playlistReadService = playlistReadService;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<PlaylistList>> Handle(
        ListPlaylistsQuery request,
        CancellationToken cancellationToken)
    {
        PagedList<PlaylistSummary> playlists;
        if (_currentUser.IsAuthenticated)
        {
            bool isAdmin = _currentUser.IsInRole(UserRoles.Admin);
            playlists = await _playlistReadService.ListAsync(
                UserId.From(_currentUser.Id), isAdmin, request.Filters, request.Pagination, cancellationToken);
        }
        else
        {
            playlists = await _playlistReadService.ListAsync(
                null, false, request.Filters, request.Pagination, cancellationToken);
        }

        return new PlaylistList(playlists);
    }
}
