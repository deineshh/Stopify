using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Application.Features.Tracks.Queries.List;

internal sealed class ListTracksQueryHandler(
    ITrackReadService trackReadService,
     ICurrentUser currentUser)
      : IQueryHandler<ListTracksQuery, TrackList>
{
    private readonly ITrackReadService _trackReadService = trackReadService;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<TrackList>> Handle(
        ListTracksQuery request,
        CancellationToken cancellationToken)
    {
        PagedList<TrackSummary> tracks;

        if (_currentUser.IsAuthenticated)
        {
            bool isAdmin = _currentUser.IsInRole(UserRoles.Admin);
            tracks = await _trackReadService.ListAsync(
                UserId.From(_currentUser.Id), isAdmin, request.Filters, request.Pagination, cancellationToken);
        }
        else
        {
            tracks = await _trackReadService.ListAsync(
                null, false, request.Filters, request.Pagination, cancellationToken);
        }

        return new TrackList(tracks);
    }
}
