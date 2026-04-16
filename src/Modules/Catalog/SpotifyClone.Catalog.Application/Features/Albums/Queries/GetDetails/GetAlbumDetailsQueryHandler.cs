using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Catalog.Application.Errors;
using SpotifyClone.Catalog.Application.Features.Artists.Queries;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.Enums;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.ValueObjects;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Catalog.Application.Features.Albums.Queries.GetDetails;

internal sealed class GetAlbumDetailsQueryHandler(
    IAlbumReadService albumReadService,
    IArtistReadService artistReadService,
    ICurrentUser currentUser)
    : IQueryHandler<GetAlbumDetailsQuery, AlbumDetails>
{
    private readonly IAlbumReadService _albumReadService = albumReadService;
    private readonly IArtistReadService _artistReadService = artistReadService;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<AlbumDetails>> Handle(
        GetAlbumDetailsQuery request,
        CancellationToken cancellationToken)
    {
        AlbumDetails? album = await _albumReadService.GetDetailsAsync(
            AlbumId.From(request.AlbumId),
            cancellationToken);
        if (album is null)
        {
            return Result.Failure<AlbumDetails>(AlbumErrors.NotFound);
        }

        IEnumerable<ArtistSummary> artists = await _artistReadService.GetAllByIdsAsync(
            album.MainArtists.Select(a => ArtistId.From(a.Id)).ToList(),
            cancellationToken);

        if (album.Status != AlbumStatus.Published.Value &&
            (!_currentUser.IsAuthenticated || !artists.Any(a => a.OwnerId == _currentUser.Id)) &&
            !_currentUser.IsInRole(UserRoles.Admin))
        {
            return Result.Failure<AlbumDetails>(AlbumErrors.NotOwned);
        }

        return album;
    }
}
