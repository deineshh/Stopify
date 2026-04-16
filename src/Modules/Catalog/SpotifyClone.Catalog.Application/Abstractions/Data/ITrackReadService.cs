using SpotifyClone.Catalog.Application.Features.Tracks.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Application.Abstractions.Data;

public interface ITrackReadService
{
    Task<bool> ExistsAsync(
        TrackId id,
        CancellationToken cancellationToken = default);

    Task<TrackDetails?> GetDetailsAsync(
        TrackId id,
        CancellationToken cancellationToken = default);

    Task<TrackSummary?> GetSummaryAsync(
        TrackId id,
        CancellationToken cancellationToken = default);

    Task<PagedList<TrackSummary>> ListAsync(
        UserId? ownerId,
        bool isAdmin,
        TrackFilterParams filters,
        PaginationParams pagination,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TrackSummary>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TrackSummary>> GetAllByIdsAsync(
        IEnumerable<TrackId> ids,
        CancellationToken cancellationToken = default);
}
