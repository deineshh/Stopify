using Microsoft.EntityFrameworkCore;
using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Catalog.Application.Features.Artists.Queries;
using SpotifyClone.Catalog.Application.Features.Genres.Queries;
using SpotifyClone.Catalog.Application.Features.Moods.Queries;
using SpotifyClone.Catalog.Application.Features.Tracks.Queries;
using SpotifyClone.Catalog.Application.Models;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.ValueObjects;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.Enums;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.ValueObjects;
using SpotifyClone.Catalog.Infrastructure.Persistence.Database;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Infrastructure.Persistence.Extensions;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Infrastructure.Persistence.Queries;

internal sealed class TrackEfCoreReadService(
    CatalogAppDbContext context)
    : ITrackReadService
{
    private readonly CatalogAppDbContext _context = context;

    public async Task<bool> ExistsAsync(
        TrackId id,
        CancellationToken cancellationToken = default)
        => await _context.Tracks.AnyAsync(t => t.Id == id, cancellationToken);

    public async Task<TrackDetails?> GetDetailsAsync(
        TrackId id,
        CancellationToken cancellationToken = default)
    {
        var trackInfo = await _context.Tracks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Duration,
                t.ReleaseDate,
                t.ContainsExplicitContent,
                t.Status,
                t.AudioFileId,
                t.AlbumId,
                MainArtistIds = t.MainArtists.ToList(),
                FeaturedArtistIds = t.FeaturedArtists.ToList(),
                GenreIds = t.Genres.ToList(),
                MoodIds = t.Moods.ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (trackInfo == null)
        {
            return null;
        }

        List<ArtistSummary> mainArtists = await _context.Artists
            .AsNoTracking()
            .Where(a => trackInfo.MainArtistIds.Contains(a.Id))
            .Select(a => new ArtistSummary(
                a.Id.Value, a.Name,
                a.Status.Value,
                a.OwnerId == null ? null : a.OwnerId.Value,
                a.Avatar == null ? null : new ImageMetadataDetails(
                    a.Avatar.ImageId.Value,
                    a.Avatar.Metadata.Width,
                    a.Avatar.Metadata.Height,
                    a.Avatar.Metadata.FileType.Value,
                    a.Avatar.Metadata.SizeInBytes)))
            .ToListAsync(cancellationToken);

        List<ArtistSummary> featuredArtists = await _context.Artists
            .AsNoTracking()
            .Where(a => trackInfo.FeaturedArtistIds.Contains(a.Id))
            .Select(a => new ArtistSummary(
                a.Id.Value, a.Name,
                a.Status.Value,
                a.OwnerId == null ? null : a.OwnerId.Value,
                a.Avatar == null ? null : new ImageMetadataDetails(
                    a.Avatar.ImageId.Value,
                    a.Avatar.Metadata.Width,
                    a.Avatar.Metadata.Height,
                    a.Avatar.Metadata.FileType.Value,
                    a.Avatar.Metadata.SizeInBytes)))
            .ToListAsync(cancellationToken);

        List<GenreSummary> genres = await _context.Genres
            .AsNoTracking()
            .Where(g => trackInfo.GenreIds.Any(id => id == g.Id))
            .Select(g => new GenreSummary(
                g.Id.Value,
                g.Name,
                g.Cover == null ? null : g.Cover.ImageId.Value))
            .ToListAsync(cancellationToken);

        List<MoodSummary> moods = await _context.Moods
            .AsNoTracking()
            .Where(m => trackInfo.MoodIds.Any(id => id == m.Id))
            .Select(m => new MoodSummary(
                m.Id.Value,
                m.Name,
                m.Cover == null ? null : m.Cover.ImageId.Value))
            .ToListAsync(cancellationToken);

        return new TrackDetails(
            trackInfo.Id.Value,
            trackInfo.Title,
            trackInfo.Duration,
            trackInfo.ReleaseDate,
            trackInfo.ContainsExplicitContent,
            trackInfo.Status.Value,
            trackInfo.AudioFileId?.Value,
            trackInfo.AlbumId?.Value,
            mainArtists,
            featuredArtists,
            genres,
            moods
        );
    }

    public async Task<TrackSummary?> GetSummaryAsync(
        TrackId id,
        CancellationToken cancellationToken = default)
    {
        var trackInfo = await _context.Tracks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Duration,
                t.ReleaseDate,
                t.ContainsExplicitContent,
                t.Status,
                t.AudioFileId,
                t.AlbumId,
                MainArtistIds = t.MainArtists.ToList(),
                FeaturedArtistIds = t.FeaturedArtists.ToList(),
                GenreIds = t.Genres.ToList(),
                MoodIds = t.Moods.ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (trackInfo == null)
        {
            return null;
        }

        List<ArtistSummary> mainArtists = await _context.Artists
            .AsNoTracking()
            .Where(a => trackInfo.MainArtistIds.Contains(a.Id))
            .Select(a => new ArtistSummary(
                a.Id.Value, a.Name,
                a.Status.Value,
                a.OwnerId == null ? null : a.OwnerId.Value,
                a.Avatar == null ? null : new ImageMetadataDetails(
                    a.Avatar.ImageId.Value,
                    a.Avatar.Metadata.Width,
                    a.Avatar.Metadata.Height,
                    a.Avatar.Metadata.FileType.Value,
                    a.Avatar.Metadata.SizeInBytes)))
            .ToListAsync(cancellationToken);

        List<ArtistSummary> featuredArtists = await _context.Artists
            .AsNoTracking()
            .Where(a => trackInfo.FeaturedArtistIds.Contains(a.Id))
            .Select(a => new ArtistSummary(
                a.Id.Value, a.Name,
                a.Status.Value,
                a.OwnerId == null ? null : a.OwnerId.Value,
                a.Avatar == null ? null : new ImageMetadataDetails(
                    a.Avatar.ImageId.Value,
                    a.Avatar.Metadata.Width,
                    a.Avatar.Metadata.Height,
                    a.Avatar.Metadata.FileType.Value,
                    a.Avatar.Metadata.SizeInBytes)))
            .ToListAsync(cancellationToken);

        return new TrackSummary(
            trackInfo.Id.Value,
            trackInfo.Title,
            trackInfo.Duration,
            trackInfo.ReleaseDate,
            trackInfo.ContainsExplicitContent,
            trackInfo.Status.Value,
            trackInfo.AudioFileId?.Value,
            trackInfo.AlbumId?.Value,
            mainArtists,
            featuredArtists,
            trackInfo.GenreIds.Select(g => g.Value),
            trackInfo.MoodIds.Select(m => m.Value)
        );
    }

    public async Task<PagedList<TrackSummary>> ListAsync(
        UserId? ownerId,
        bool isAdmin,
        TrackFilterParams filters,
        PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Track> query = _context.Tracks.AsNoTracking();

        if (!isAdmin)
        {
            if (ownerId is null)
            {
                query = query.Where(t => t.Status == TrackStatus.Published);
            }
            else
            {
                List<Guid> userArtistIds = await _context.Artists
                    .Where(art => art.OwnerId == ownerId)
                    .Select(art => art.Id.Value)
                    .ToListAsync(cancellationToken);

                query = query.Where(t =>
                    t.Status == TrackStatus.Published ||
                    t.MainArtists.Any(ma => userArtistIds.Contains(ma.Value)) ||
                    t.FeaturedArtists.Any(fa => userArtistIds.Contains(fa.Value))
                );
            }
        }

        if (filters.Title is not null)
        {
            query = query.Where(t => EF.Functions.ILike(t.Title, filters.Title));
        }
        if (filters.Duration is not null)
        {
            query = query.Where(t => t.Duration == filters.Duration);
        }
        if (filters.ReleaseDate is not null)
        {
            query = query.Where(t => t.ReleaseDate == filters.ReleaseDate);
        }
        if (filters.Explicit is not null)
        {
            query = query.Where(t => t.ContainsExplicitContent == filters.Explicit);
        }
        if (filters.Status is not null)
        {
            var status = TrackStatus.From(filters.Status);
            query = query.Where(t => t.Status == status);
        }
        if (filters.AudioFileId is not null)
        {
            var audioFileId = AudioFileId.From(filters.AudioFileId.Value);
            query = query.Where(t => t.AudioFileId == audioFileId);
        }
        if (filters.AlbumId is not null)
        {
            var albumId = AlbumId.From(filters.AlbumId.Value);
            query = query.Where(t => t.AlbumId == albumId);
        }
        if (filters.OwnerId is not null)
        {
            var owner = UserId.From(filters.OwnerId.Value);

            List<Guid> ownedArtistIds = await _context.Artists
                .Where(a => a.OwnerId == owner)
                .Select(a => a.Id.Value)
                .ToListAsync(cancellationToken);

            if (ownedArtistIds.Count > 0)
            {
                query = query.Where(t => t.MainArtists.Any(ma => ownedArtistIds.Contains(ma.Value)));
            }
            else
            {
                // Якщо у власника немає артистів, він не може мати треків.
                // Замість ігнорування фільтра, ми робимо запит, який нічого не поверне.
                query = query.Where(t => false);
            }
        }
        if (filters.MainArtistIds is not null && filters.MainArtistIds.Any())
        {
            query = query.Where(t => t.MainArtists.Any(a => filters.MainArtistIds.Contains(a.Value)));
        }
        if (filters.FeaturedArtistIds is not null && filters.FeaturedArtistIds.Any())
        {
            query = query.Where(t => t.FeaturedArtists.Any(a => filters.FeaturedArtistIds.Contains(a.Value)));
        }
        if (filters.GenreIds is not null && filters.GenreIds.Any())
        {
            query = query.Where(t => t.Genres.Any(a => filters.GenreIds.Contains(a.Value)));
        }
        if (filters.MoodIds is not null && filters.MoodIds.Any())
        {
            query = query.Where(t => t.Moods.Any(a => filters.MoodIds.Contains(a.Value)));
        }

        var pagedData = await query
        .OrderBy(t => t.CreatedAtUtc)
        .Select(t => new
        {
            t.Id,
            t.Title,
            t.Duration,
            t.ReleaseDate,
            t.ContainsExplicitContent,
            t.Status,
            t.AudioFileId,
            t.AlbumId,
            MainArtistIds = t.MainArtists,
            FeaturedArtistIds = t.FeaturedArtists,
            GenreIds = t.Genres,
            MoodIds = t.Moods,
        })
        .ToPagedListAsync(pagination, cancellationToken);

        if (!pagedData.Items.Any())
        {
            return new PagedList<TrackSummary>([], pagedData.TotalCount, pagination.Page, pagination.PageSize);
        }

        var allArtistIds = pagedData.Items
            .SelectMany(t => t.MainArtistIds)
            .Concat(pagedData.Items.SelectMany(t => t.FeaturedArtistIds))
            .Distinct()
            .ToList();

        Dictionary<Guid, ArtistSummary> artistMap = await _context.Artists
            .AsNoTracking()
            .Where(a => allArtistIds.Any(id => id == a.Id))
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
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        var finalItems = pagedData.Items.Select(t => new TrackSummary(
            t.Id.Value,
            t.Title,
            t.Duration,
            t.ReleaseDate,
            t.ContainsExplicitContent,
            t.Status.Value,
            t.AudioFileId?.Value,
            t.AlbumId?.Value,
            t.MainArtistIds
                .Where(id => artistMap.ContainsKey(id.Value))
                .Select(id => artistMap[id.Value])
                .ToList(),
            t.FeaturedArtistIds
                .Where(id => artistMap
                .ContainsKey(id.Value))
                .Select(id => artistMap[id.Value])
                .ToList(),
            t.GenreIds.Select(g => g.Value),
            t.MoodIds.Select(m => m.Value))).ToList();

        return new PagedList<TrackSummary>(
            finalItems,
            pagedData.TotalCount,
            pagination.Page,
            pagination.PageSize);
    }

    public async Task<IEnumerable<TrackSummary>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        List<Track> tracks = await _context.Tracks
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (tracks.Count <= 0)
        {
            return Enumerable.Empty<TrackSummary>();
        }

        var allArtistIds = tracks
            .SelectMany(t => t.MainArtists)
            .Concat(tracks.SelectMany(t => t.FeaturedArtists))
            .Distinct()
            .ToList();

        Dictionary<Guid, ArtistSummary> artistMap = await _context.Artists
            .AsNoTracking()
            .Where(a => allArtistIds.Any(id => id == a.Id))
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
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        return tracks.Select(t => new TrackSummary(
            t.Id.Value,
            t.Title,
            t.Duration,
            t.ReleaseDate,
            t.ContainsExplicitContent,
            t.Status.Value,
            t.AudioFileId?.Value,
            t.AlbumId?.Value,
            t.MainArtists
                .Where(id => artistMap.ContainsKey(id.Value))
                .Select(id => artistMap[id.Value])
                .ToList(),
            t.FeaturedArtists
                .Where(id => artistMap.ContainsKey(id.Value))
                .Select(id => artistMap[id.Value])
                .ToList(),
            t.Genres.Select(g => g.Value),
            t.Moods.Select(m => m.Value)));
    }

    public async Task<IEnumerable<TrackSummary>> GetAllByIdsAsync(
        IEnumerable<TrackId> ids,
        CancellationToken cancellationToken = default)
    {
        List<Track> tracks = await _context.Tracks
            .AsNoTracking()
            .Where(t => ids.Any(id => id == t.Id))
            .ToListAsync(cancellationToken);

        if (tracks.Count <= 0)
        {
            return Enumerable.Empty<TrackSummary>();
        }

        var allArtistIds = tracks
            .SelectMany(t => t.MainArtists)
            .Concat(tracks.SelectMany(t => t.FeaturedArtists))
            .Distinct()
            .ToList();

        Dictionary<Guid, ArtistSummary> artistMap = await _context.Artists
            .AsNoTracking()
            .Where(a => allArtistIds.Any(id => id == a.Id))
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
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        return tracks.Select(t => new TrackSummary(
            t.Id.Value,
            t.Title,
            t.Duration,
            t.ReleaseDate,
            t.ContainsExplicitContent,
            t.Status.Value,
            t.AudioFileId?.Value,
            t.AlbumId?.Value,
            t.MainArtists
                .Where(id => artistMap.ContainsKey(id.Value))
                .Select(id => artistMap[id.Value])
                .ToList(),
            t.FeaturedArtists
                .Where(id => artistMap.ContainsKey(id.Value))
                .Select(id => artistMap[id.Value])
                .ToList(),
            t.Genres.Select(g => g.Value),
            t.Moods.Select(m => m.Value)));
    }
}
