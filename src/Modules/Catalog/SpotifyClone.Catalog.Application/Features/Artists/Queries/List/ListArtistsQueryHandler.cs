using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Catalog.Application.Features.Artists.Queries.List;

internal sealed class ListArtistsQueryHandler(
    IArtistReadService artistReadService,
    ICurrentUser currentUser)
     : IQueryHandler<ListArtistsQuery, ArtistList>
{
    private readonly IArtistReadService _artistReadService = artistReadService;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<ArtistList>> Handle(
        ListArtistsQuery request,
        CancellationToken cancellationToken)
    {
        bool includeBanned = _currentUser.IsAuthenticated && _currentUser.IsInRole(UserRoles.Admin);

        PagedList<ArtistSummary> artists = await _artistReadService.ListAsync(
            includeBanned, request.Filters, request.Pagination, cancellationToken);

        return new ArtistList(artists);
    }
}
