using SpotifyClone.Catalog.Application.Features.Artists.Queries;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Catalog.Application.Abstractions.Data;

public interface IArtistReadService
{
    Task<bool> ExistsAsync(
        ArtistId id,
        CancellationToken cancellationToken = default);

    Task<ArtistDetails?> GetDetailsAsync(
        ArtistId id,
        CancellationToken cancellationToken = default);

    Task<ArtistSummary?> GetSummaryAsync(
        ArtistId id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ArtistSummary>> GetAllByIdsAsync(
        IEnumerable<ArtistId> artistIds,
        CancellationToken cancellationToken = default);

    Task<PagedList<ArtistSummary>> ListAsync(
        bool includeBanned,
        ArtistFilterParams filters,
        PaginationParams pagination,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ArtistSummary>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
