using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Application.Features.Albums.Queries.List;

internal sealed class ListAlbumsQueryHandler(
    IAlbumReadService albumReadService,
    ICurrentUser currentUser)
     : IQueryHandler<ListAlbumsQuery, AlbumList>
{
    private readonly IAlbumReadService _albumReadService = albumReadService;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<AlbumList>> Handle(
        ListAlbumsQuery request,
        CancellationToken cancellationToken)
    {
        PagedList<AlbumSummary> albums;

        if (_currentUser.IsAuthenticated)
        {
            bool isAdmin = _currentUser.IsInRole(UserRoles.Admin);
            albums = await _albumReadService.ListAsync(
                UserId.From(_currentUser.Id), isAdmin, request.Filters, request.Pagination, cancellationToken);
        }
        else
        {
            albums = await _albumReadService.ListAsync(
                null, false, request.Filters, request.Pagination, cancellationToken);
        }

        return new AlbumList(albums);
    }
}
