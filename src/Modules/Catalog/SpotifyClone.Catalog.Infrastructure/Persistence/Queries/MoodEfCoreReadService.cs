using Microsoft.EntityFrameworkCore;
using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Catalog.Application.Features.Moods.Queries;
using SpotifyClone.Catalog.Application.Models;
using SpotifyClone.Catalog.Domain.Aggregates.Moods.ValueObjects;
using SpotifyClone.Catalog.Infrastructure.Persistence.Database;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Infrastructure.Persistence.Extensions;

namespace SpotifyClone.Catalog.Infrastructure.Persistence.Queries;

internal sealed class MoodEfCoreReadService(
    CatalogAppDbContext context)
    : IMoodReadService
{
    private readonly CatalogAppDbContext _context = context;

    public async Task<bool> ExistsAsync(
        MoodId id,
        CancellationToken cancellationToken = default)
        => await _context.Moods
        .AnyAsync(m => m.Id == id, cancellationToken);

    public Task<MoodDetails?> GetDetailsAsync(
        MoodId id,
        CancellationToken cancellationToken = default)
        => _context.Moods
        .Where(m => m.Id == id)
        .Select(m => new MoodDetails(
            m.Id.Value,
            m.Name,
            m.Cover == null ? null : new ImageMetadataDetails(
                m.Cover.ImageId.Value,
                m.Cover.Metadata.Width,
                m.Cover.Metadata.Height,
                m.Cover.Metadata.FileType.Value,
                m.Cover.Metadata.SizeInBytes)))
        .SingleOrDefaultAsync(cancellationToken);

    public async Task<PagedList<MoodSummary>> ListAsync(
        PaginationParams pagination,
        CancellationToken cancellationToken = default)
        => await _context.Moods
        .AsNoTracking()
        .OrderBy(m => m.CreatedAtUtc)
        .Select(m => new MoodSummary(
            m.Id.Value,
            m.Name,
            m.Cover == null ? null : m.Cover.ImageId.Value))
        .ToPagedListAsync(pagination, cancellationToken);

    public async Task<IEnumerable<MoodSummary>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => await _context.Moods
        .AsNoTracking()
        .Select(m => new MoodSummary(
            m.Id.Value,
            m.Name,
            m.Cover == null ? null : m.Cover.ImageId.Value))
        .ToListAsync(cancellationToken);
}
