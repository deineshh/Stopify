using SpotifyClone.Catalog.Application.Features.Moods.Queries;
using SpotifyClone.Catalog.Domain.Aggregates.Moods.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Catalog.Application.Abstractions.Data;

public interface IMoodReadService
{
    Task<bool> ExistsAsync(
        MoodId id,
        CancellationToken cancellationToken = default);

    Task<MoodDetails?> GetDetailsAsync(
        MoodId id,
        CancellationToken cancellationToken = default);

    Task<PagedList<MoodSummary>> ListAsync(
        PaginationParams pagination,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<MoodSummary>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
