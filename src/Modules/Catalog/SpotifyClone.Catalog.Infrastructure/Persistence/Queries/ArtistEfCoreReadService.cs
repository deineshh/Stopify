using Microsoft.EntityFrameworkCore;
using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Catalog.Application.Features.Artists.Queries;
using SpotifyClone.Catalog.Application.Models;
using SpotifyClone.Catalog.Domain.Aggregates.Artists;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.Enums;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks;
using SpotifyClone.Catalog.Infrastructure.Persistence.Database;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Infrastructure.Persistence.Extensions;

namespace SpotifyClone.Catalog.Infrastructure.Persistence.Queries;

internal sealed class ArtistEfCoreReadService(
    CatalogAppDbContext context)
    : IArtistReadService
{
    private readonly CatalogAppDbContext _context = context;

    public async Task<bool> ExistsAsync(
        ArtistId id,
        CancellationToken cancellationToken = default)
        => await _context.Artists
        .AnyAsync(a => a.Id == id, cancellationToken);

    public async Task<ArtistDetails?> GetDetailsAsync(
        ArtistId id,
        CancellationToken cancellationToken = default)
        => await _context.Artists
        .Where(a => a.Id == id)
        .Select(a => new ArtistDetails(
            a.Id.Value,
            a.Name,
            a.Bio,
            a.OwnerId == null ? null : a.OwnerId.Value,
            a.Status.Value,
            a.Avatar == null ? null : new ImageMetadataDetails(
                a.Avatar.ImageId.Value,
                a.Avatar.Metadata.Width,
                a.Avatar.Metadata.Height,
                a.Avatar.Metadata.FileType.Value,
                a.Avatar.Metadata.SizeInBytes),
            a.Banner == null ? null : new ImageMetadataDetails(
                a.Banner.ImageId.Value,
                a.Banner.Metadata.Width,
                a.Banner.Metadata.Height,
                a.Banner.Metadata.FileType.Value,
                a.Banner.Metadata.SizeInBytes),
            a.Gallery.Select(img => new ImageMetadataDetails(
                img.ImageId!.Value,
                img.Metadata.Width,
                img.Metadata.Height,
                img.Metadata.FileType.Value,
                img.Metadata.SizeInBytes
            )).ToList()))
        .SingleOrDefaultAsync(cancellationToken);

    public async Task<ArtistSummary?> GetSummaryAsync(
        ArtistId id,
        CancellationToken cancellationToken = default)
        => await _context.Artists
        .Where(a => a.Id == id)
        .Select(a => new ArtistSummary(
            a.Id.Value,
            a.Name,
            a.Status.Value,
            a.OwnerId == null ? null : a.OwnerId.Value,
            a.Avatar == null ? null : new ImageMetadataDetails(
                a.Avatar.ImageId.Value,
                a.Avatar.Metadata.Width,
                a.Avatar.Metadata.Height,
                a.Avatar.Metadata.FileType.Value,
                a.Avatar.Metadata.SizeInBytes)))
        .SingleOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<ArtistSummary>> GetAllByIdsAsync(
        IEnumerable<ArtistId> artistIds,
        CancellationToken cancellationToken = default)
        => await _context.Artists
        .Where(a => artistIds.Any(id => id == a.Id))
        .Select(a => new ArtistSummary(
            a.Id.Value,
            a.Name,
            a.Status.Value,
            a.OwnerId == null ? null : a.OwnerId.Value,
            a.Avatar == null ? null : new ImageMetadataDetails(
                a.Avatar.ImageId.Value,
                a.Avatar.Metadata.Width,
                a.Avatar.Metadata.Height,
                a.Avatar.Metadata.FileType.Value,
                a.Avatar.Metadata.SizeInBytes)))
        .ToListAsync(cancellationToken);

    public async Task<PagedList<ArtistSummary>> ListAsync(
        bool includeBanned,
        ArtistFilterParams filters,
        PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Artist> query = _context.Artists
            .AsNoTracking();

        if (!includeBanned)
        {
            query = query.Where(a => a.Status != ArtistStatus.Banned);
        }
        if (filters.Name is not null)
        {
            string name = filters.Name.Trim();
            query = query.Where(a => EF.Functions.ILike(a.Name, name));
        }
        if (filters.Bio is not null)
        {
            string bio = filters.Bio.Trim();
            query = query.Where(a => a.Bio != null && EF.Functions.ILike(a.Bio, bio));
        }
        if (filters.Status is not null)
        {
            var status = ArtistStatus.From(filters.Status);
            query = query.Where(a => a.Status == status);
        }
        if ((filters.GenreIds?.Any() ?? false) ||
            (filters.MoodIds?.Any() ?? false))
        {
            IQueryable<Track> tracksQuery = _context.Tracks
                .AsNoTracking()
                .AsQueryable();

            if (filters.GenreIds?.Any() ?? false)
            {
                tracksQuery = tracksQuery
                    .Where(t => t.Genres
                        .Any(g => filters.GenreIds.Contains(g.Value)));
            }

            if (filters.MoodIds?.Any() ?? false)
            {
                tracksQuery = tracksQuery
                    .Where(t => t.Moods
                        .Any(m => filters.MoodIds.Contains(m.Value)));
            }

            List<Guid> artistIdValues = await tracksQuery
                .SelectMany(t => t.MainArtists)
                .Select(ma => ma.Value)
                .Distinct()
                .ToListAsync(cancellationToken);
            var artistIds = artistIdValues
                .Select(id => ArtistId.From(id))
                .ToList();

            if (artistIds.Count == 0)
            {
                return new PagedList<ArtistSummary>();
            }

            query = query.Where(a => artistIds.Contains(a.Id));
        }

        return await query
            .OrderBy(a => a.CreatedAtUtc)
            .Select(a => new ArtistSummary(
                a.Id.Value, a.Name, a.Status.Value,
                a.OwnerId == null ? null : a.OwnerId.Value,
                a.Avatar == null ? null : new ImageMetadataDetails(
                    a.Avatar.ImageId.Value,
                    a.Avatar.Metadata.Width,
                    a.Avatar.Metadata.Height,
                    a.Avatar.Metadata.FileType.Value,
                    a.Avatar.Metadata.SizeInBytes)))
        .ToPagedListAsync(pagination, cancellationToken);
    }

    public async Task<IEnumerable<ArtistSummary>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => await _context.Artists
        .Select(a => new ArtistSummary(
            a.Id.Value,
            a.Name,
            a.Status.Value,
            a.OwnerId == null ? null : a.OwnerId.Value,
            a.Avatar == null ? null : new ImageMetadataDetails(
                a.Avatar.ImageId.Value,
                a.Avatar.Metadata.Width,
                a.Avatar.Metadata.Height,
                a.Avatar.Metadata.FileType.Value,
                a.Avatar.Metadata.SizeInBytes)))
        .ToListAsync(cancellationToken);
}
