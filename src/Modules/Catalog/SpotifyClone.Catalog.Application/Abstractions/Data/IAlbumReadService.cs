using SpotifyClone.Catalog.Application.Features.Albums.Queries;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Application.Abstractions.Data;

public interface IAlbumReadService
{
    Task<AlbumDetails?> GetDetailsAsync(
        AlbumId id,
        CancellationToken cancellationToken = default);

    Task<AlbumSummary?> GetSummary(
        AlbumId id,
        CancellationToken cancellationToken = default);

    Task<AlbumSummary?> GetSummaryByTrackId(
        TrackId trackId,
        CancellationToken cancellationToken = default);

    Task<AlbumDetails?> GetDetailsByTrackIdAsync(
        TrackId trackId,
        CancellationToken cancellationToken = default);

    Task<PagedList<AlbumSummary>> ListAsync(
        UserId? ownerId,
        bool isAdmin,
        AlbumFilterParams filters,
        PaginationParams pagination,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AlbumSummary>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AlbumSummary>> GetAllByTracksAsync(
        IEnumerable<TrackId> trackIds,
        CancellationToken cancellationToken = default);
}
