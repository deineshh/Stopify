using Microsoft.EntityFrameworkCore;
using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Catalog.Application.Features.Albums.Queries;
using SpotifyClone.Catalog.Application.Features.Artists.Queries;
using SpotifyClone.Catalog.Application.Features.Tracks.Queries;
using SpotifyClone.Catalog.Application.Models;
using SpotifyClone.Catalog.Domain.Aggregates.Albums;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.Enums;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.ValueObjects;
using SpotifyClone.Catalog.Infrastructure.Persistence.Database;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Infrastructure.Persistence.Extensions;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Infrastructure.Persistence.Queries;

internal sealed class AlbumEfCoreReadService(
    CatalogAppDbContext context)
    : IAlbumReadService
{
    private readonly CatalogAppDbContext _context = context;

    public async Task<AlbumDetails?> GetDetailsAsync(
        AlbumId id,
        CancellationToken cancellationToken = default)
    {
        // Крок 1. Дістаємо "корінь" (Альбом) та його внутрішні колекції ID-шників.
        var albumBase = await _context.Albums
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Include(a => a.Tracks)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.ReleaseDate,
                a.Status,
                a.Type,
                a.Cover,
                ArtistIds = a.MainArtists,
                AlbumTracks = a.Tracks
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (albumBase == null)
        {
            return null;
        }

        // Крок 2. Дістаємо Артистів окремим запитом.
        var artistIds = albumBase.ArtistIds.ToList();

        List<ArtistSummary> artists = await _context.Artists
            .Where(artist => artistIds.Contains(artist.Id))
            .Select(artist => new ArtistSummary(
                artist.Id.Value,
                artist.Name,
                artist.Status.Value,
                artist.OwnerId == null ? null : artist.OwnerId.Value,
                artist.Avatar == null ? null : new ImageMetadataDetails(
                    artist.Avatar.ImageId.Value,
                    artist.Avatar.Metadata.Width,
                    artist.Avatar.Metadata.Height,
                    artist.Avatar.Metadata.FileType.Value,
                    artist.Avatar.Metadata.SizeInBytes)
            ))
            .ToListAsync(cancellationToken);

        // Крок 3. Дістаємо Треки окремим запитом.
        var trackIds = albumBase.AlbumTracks.Select(at => at.Id).ToList();

        var tracksDb = await _context.Tracks
            .Where(t => trackIds.Contains(t.Id))
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.ContainsExplicitContent,
                t.Status,
                t.Duration
            })
            .ToListAsync(cancellationToken);

        // Крок 4. Мапимо позиції з AlbumTracks на отримані дані Треків (In-Memory Join)
        var trackSummaries = tracksDb.Select(t =>
        {
            // Знаходимо позицію для цього конкретного треку з даних першого запиту
            int position = albumBase.AlbumTracks.First(at => at.Id == t.Id).Position;

            return new AlbumTrackSummary(
                t.Id.Value,
                t.Title,
                t.ContainsExplicitContent,
                t.Status.Value,
                t.Duration,
                position
            );
        })
        .OrderBy(t => t.Position) // Сортуємо вже в пам'яті сервера
        .ToList();

        // Крок 5. Збираємо фінальний DTO
        return new AlbumDetails(
            albumBase.Id.Value,
            albumBase.Title,
            albumBase.ReleaseDate,
            albumBase.Status.Value,
            albumBase.Type.Value,
            albumBase.Cover == null ? null : new ImageMetadataDetails(
                albumBase.Cover.ImageId.Value,
                albumBase.Cover.Metadata.Width,
                albumBase.Cover.Metadata.Height,
                albumBase.Cover.Metadata.FileType.Value,
                albumBase.Cover.Metadata.SizeInBytes),
            artists,
            trackSummaries
        );
    }

    public async Task<AlbumDetails?> GetDetailsByTrackIdAsync(
        TrackId trackId,
        CancellationToken cancellationToken = default)
    {
        // Крок 1. Дістаємо "корінь" (Альбом) та його внутрішні колекції ID-шників.
        var albumBase = await _context.Albums
            .AsNoTracking()
            .Where(a => a.Tracks.Any(t => t.Id.Value == trackId.Value))
            .Include(a => a.Tracks)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.ReleaseDate,
                a.Status,
                a.Type,
                a.Cover,
                ArtistIds = a.MainArtists,
                AlbumTracks = a.Tracks
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (albumBase == null)
        {
            return null;
        }

        // Крок 2. Дістаємо Артистів окремим запитом.
        var artistIds = albumBase.ArtistIds.ToList();

        List<ArtistSummary> artists = await _context.Artists
            .Where(artist => artistIds.Contains(artist.Id))
            .Select(artist => new ArtistSummary(
                artist.Id.Value,
                artist.Name,
                artist.Status.Value,
                artist.OwnerId == null ? null : artist.OwnerId.Value,
                artist.Avatar == null ? null : new ImageMetadataDetails(
                    artist.Avatar.ImageId.Value,
                    artist.Avatar.Metadata.Width,
                    artist.Avatar.Metadata.Height,
                    artist.Avatar.Metadata.FileType.Value,
                    artist.Avatar.Metadata.SizeInBytes)
            ))
            .ToListAsync(cancellationToken);

        // Крок 3. Дістаємо Треки окремим запитом.
        var trackIds = albumBase.AlbumTracks.Select(at => at.Id).ToList();

        var tracksDb = await _context.Tracks
            .Where(t => trackIds.Contains(t.Id))
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.ContainsExplicitContent,
                t.Status,
                t.Duration
            })
            .ToListAsync(cancellationToken);

        // Крок 4. Мапимо позиції з AlbumTracks на отримані дані Треків (In-Memory Join)
        var trackSummaries = tracksDb.Select(t =>
        {
            // Знаходимо позицію для цього конкретного треку з даних першого запиту
            int position = albumBase.AlbumTracks.First(at => at.Id == t.Id).Position;

            return new AlbumTrackSummary(
                t.Id.Value,
                t.Title,
                t.ContainsExplicitContent,
                t.Status.Value,
                t.Duration,
                position
            );
        })
        .OrderBy(t => t.Position) // Сортуємо вже в пам'яті сервера
        .ToList();

        // Крок 5. Збираємо фінальний DTO
        return new AlbumDetails(
            albumBase.Id.Value,
            albumBase.Title,
            albumBase.ReleaseDate,
            albumBase.Status.Value,
            albumBase.Type.Value,
            albumBase.Cover == null ? null : new ImageMetadataDetails(
                albumBase.Cover.ImageId.Value,
                albumBase.Cover.Metadata.Width,
                albumBase.Cover.Metadata.Height,
                albumBase.Cover.Metadata.FileType.Value,
                albumBase.Cover.Metadata.SizeInBytes),
            artists,
            trackSummaries
        );
    }

    public async Task<AlbumSummary?> GetSummary(
        AlbumId id,
        CancellationToken cancellationToken = default)
    {
        var albumData = await _context.Albums
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.ReleaseDate,
                Status = a.Status.Value,
                Type = a.Type.Value,
                a.Cover,
                MainArtistIds = a.MainArtists.ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (albumData == null)
        {
            return null;
        }

        List<ArtistSummary> artistSummaries = await _context.Artists
            .AsNoTracking()
            .Where(art => albumData.MainArtistIds.Contains(art.Id))
            .Select(art => new ArtistSummary(
                art.Id.Value,
                art.Name,
                art.Status.Value,
                art.OwnerId == null ? null : art.OwnerId.Value,
                art.Avatar == null ? null : new ImageMetadataDetails(
                    art.Avatar.ImageId.Value,
                    art.Avatar.Metadata.Width,
                    art.Avatar.Metadata.Height,
                    art.Avatar.Metadata.FileType.Value,
                    art.Avatar.Metadata.SizeInBytes)))
            .ToListAsync(cancellationToken);

        return new AlbumSummary(
            albumData.Id.Value,
            albumData.Title,
            albumData.ReleaseDate,
            albumData.Status,
            albumData.Type,
            albumData.Cover == null ? null : new ImageMetadataDetails(
                albumData.Cover.ImageId.Value,
                albumData.Cover.Metadata.Width,
                albumData.Cover.Metadata.Height,
                albumData.Cover.Metadata.FileType.Value,
                albumData.Cover.Metadata.SizeInBytes),
            artistSummaries);
    }

    public async Task<AlbumSummary?> GetSummaryByTrackId(
        TrackId trackId,
        CancellationToken cancellationToken = default)
    {
        var albumData = await _context.Albums
            .AsNoTracking()
            .Where(a => a.Tracks.Any(t => t.Id == trackId))
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.ReleaseDate,
                Status = a.Status.Value,
                Type = a.Type.Value,
                a.Cover,
                MainArtistIds = a.MainArtists.ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (albumData == null)
        {
            return null;
        }

        List<ArtistSummary> artistSummaries = await _context.Artists
            .AsNoTracking()
            .Where(art => albumData.MainArtistIds.Contains(art.Id))
            .Select(art => new ArtistSummary(
                art.Id.Value,
                art.Name,
                art.Status.Value,
                art.OwnerId == null ? null : art.OwnerId.Value,
                art.Avatar == null ? null : new ImageMetadataDetails(
                    art.Avatar.ImageId.Value,
                    art.Avatar.Metadata.Width,
                    art.Avatar.Metadata.Height,
                    art.Avatar.Metadata.FileType.Value,
                    art.Avatar.Metadata.SizeInBytes)))
            .ToListAsync(cancellationToken);

        return new AlbumSummary(
            albumData.Id.Value,
            albumData.Title,
            albumData.ReleaseDate,
            albumData.Status,
            albumData.Type,
            albumData.Cover == null ? null : new ImageMetadataDetails(
                albumData.Cover.ImageId.Value,
                albumData.Cover.Metadata.Width,
                albumData.Cover.Metadata.Height,
                albumData.Cover.Metadata.FileType.Value,
                albumData.Cover.Metadata.SizeInBytes),
            artistSummaries);
    }

    public async Task<PagedList<AlbumSummary>> ListAsync(
        UserId? ownerId,
        bool isAdmin,
        AlbumFilterParams filters,
        PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Album> query = _context.Albums.AsNoTracking();

        if (!isAdmin)
        {
            query = ownerId is null
                ? query
                    .Where(a => a.Status == AlbumStatus.Published)
                : query
                    .Where(a => a.Status == AlbumStatus.Published || a.MainArtists.Any(ma => _context.Artists
                        .Where(art => art.Id == ma)
                        .Any(art => art.OwnerId == ownerId)));
        }

        if (filters.Title is not null)
        {
            query = query.Where(a => EF.Functions.ILike(a.Title, filters.Title));
        }
        if (filters.ReleaseDate is not null)
        {
            query = query.Where(a => a.ReleaseDate == filters.ReleaseDate);
        }
        if (filters.Status is not null)
        {
            var status = AlbumStatus.From(filters.Status);
            query = query.Where(a => a.Status == status);
        }
        if (filters.Type is not null)
        {
            var type = AlbumType.From(filters.Type);
            query = query.Where(a => a.Type == type);
        }
        if (filters.MainArtistIds is not null && filters.MainArtistIds.Any())
        {
            query = query.Where(a => a.MainArtists.Any(art => filters.MainArtistIds.Any(id => id == art.Value)));
        }
        if (filters.TrackIds is not null && filters.TrackIds.Any())
        {
            query = query.Where(a => a.Tracks.Any(t => filters.TrackIds.Any(id => id == t.Id.Value)));
        }
        if (filters.GenreIds is not null && filters.GenreIds.Any())
        {
            query = query.Where(a => a.Tracks.Any(at =>
                _context.Tracks
                    .Where(t => t.Id == at.Id)
                    .Any(t => t.Genres.Any(g => filters.GenreIds.Contains(g.Value)))
            ));
        }
        if (filters.MoodIds is not null && filters.MoodIds.Any())
        {
            query = query.Where(a => a.Tracks.Any(at =>
                _context.Tracks
                    .Where(t => t.Id == at.Id)
                    .Any(t => t.Moods.Any(m => filters.MoodIds.Contains(m.Value)))
            ));
        }

        PagedList<Album> pagedAlbums = await query
        .OrderBy(a => a.CreatedAtUtc)
        .ToPagedListAsync(pagination, cancellationToken);

        // Якщо сторінка порожня - одразу повертаємо результат
        if (!pagedAlbums.Items.Any())
        {
            return new PagedList<AlbumSummary>([], pagedAlbums.TotalCount, pagination.Page, pagination.PageSize);
        }

        // Збираємо всі унікальні ID артистів ТІЛЬКИ для цієї сторінки
        var artistIdsForThisPage = pagedAlbums.Items
            .SelectMany(a => a.MainArtists)
            .Distinct()
            .ToList();

        // Порівнюємо прямо об'єкт з об'єктом
        Dictionary<Guid, ArtistSummary> artistSummaries = await _context.Artists
            .Where(art => artistIdsForThisPage.Contains(art.Id))
            .Select(art => new ArtistSummary(
                art.Id.Value,
                art.Name,
                art.Status.Value,
                art.OwnerId == null ? null : art.OwnerId.Value,
                art.Avatar == null ? null : new ImageMetadataDetails(
                    art.Avatar.ImageId.Value,
                    art.Avatar.Metadata.Width,
                    art.Avatar.Metadata.Height,
                    art.Avatar.Metadata.FileType.Value,
                    art.Avatar.Metadata.SizeInBytes)))
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        // Склеюємо дані в пам'яті (це відбувається миттєво)
        var finalSummaries = pagedAlbums.Items.Select(a => new AlbumSummary(
            a.Id.Value,
            a.Title,
            a.ReleaseDate,
            a.Status.Value,
            a.Type.Value,
            a.Cover == null ? null : new ImageMetadataDetails(
                a.Cover.ImageId.Value,
                a.Cover.Metadata.Width,
                a.Cover.Metadata.Height,
                a.Cover.Metadata.FileType.Value,
                a.Cover.Metadata.SizeInBytes),

            // Знаходимо артистів зі словника
            a.MainArtists
                .Where(ma => artistSummaries.ContainsKey(ma.Value))
                .Select(ma => artistSummaries[ma.Value])
                .ToList()
        )).ToList();

        // Повертаємо нову пагіновану колекцію
        return new PagedList<AlbumSummary>(
            finalSummaries,
            pagedAlbums.TotalCount,
            pagination.Page,
            pagination.PageSize);
    }

    public async Task<IEnumerable<AlbumSummary>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        List<Album> albums = await _context.Albums
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (albums.Count == 0)
        {
            return [];
        }

        var allArtistIds = albums
            .SelectMany(a => a.MainArtists)
            .Distinct()
            .ToList();

        Dictionary<Guid, ArtistSummary> artistSummaries = await _context.Artists
            .AsNoTracking()
            .Where(art => allArtistIds.Contains(art.Id))
            .Select(art => new ArtistSummary(
                art.Id.Value,
                art.Name,
                art.Status.Value,
                art.OwnerId == null ? null : art.OwnerId.Value,
                art.Avatar == null ? null : new ImageMetadataDetails(
                    art.Avatar.ImageId.Value,
                    art.Avatar.Metadata.Width,
                    art.Avatar.Metadata.Height,
                    art.Avatar.Metadata.FileType.Value,
                    art.Avatar.Metadata.SizeInBytes)))
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        return albums.Select(a => new AlbumSummary(
            a.Id.Value,
            a.Title,
            a.ReleaseDate,
            a.Status.Value,
            a.Type.Value,
            a.Cover == null ? null : new ImageMetadataDetails(
                a.Cover.ImageId.Value,
                a.Cover.Metadata.Width,
                a.Cover.Metadata.Height,
                a.Cover.Metadata.FileType.Value,
                a.Cover.Metadata.SizeInBytes),

            a.MainArtists
                .Where(ma => artistSummaries.ContainsKey(ma.Value))
                .Select(ma => artistSummaries[ma.Value])
                .ToList()
        )).ToList();
    }

    public async Task<IEnumerable<AlbumSummary>> GetAllByTracksAsync(
        IEnumerable<TrackId> trackIds,
        CancellationToken cancellationToken = default)
    {
        List<Album> albums = await _context.Albums
            .AsNoTracking()
            .Where(a => a.Tracks.Any(t => trackIds.Contains(t.Id)))
            .ToListAsync(cancellationToken);

        if (albums.Count == 0)
        {
            return [];
        }

        var allArtistIds = albums
            .SelectMany(a => a.MainArtists)
            .Distinct()
            .ToList();

        Dictionary<Guid, ArtistSummary> artistSummaries = await _context.Artists
            .AsNoTracking()
            .Where(art => allArtistIds.Contains(art.Id))
            .Select(art => new ArtistSummary(
                art.Id.Value,
                art.Name,
                art.Status.Value,
                art.OwnerId == null ? null : art.OwnerId.Value,
                art.Avatar == null ? null : new ImageMetadataDetails(
                    art.Avatar.ImageId.Value,
                    art.Avatar.Metadata.Width,
                    art.Avatar.Metadata.Height,
                    art.Avatar.Metadata.FileType.Value,
                    art.Avatar.Metadata.SizeInBytes)))
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        return albums.Select(a => new AlbumSummary(
            a.Id.Value,
            a.Title,
            a.ReleaseDate,
            a.Status.Value,
            a.Type.Value,
            a.Cover == null ? null : new ImageMetadataDetails(
                a.Cover.ImageId.Value,
                a.Cover.Metadata.Width,
                a.Cover.Metadata.Height,
                a.Cover.Metadata.FileType.Value,
                a.Cover.Metadata.SizeInBytes),

            a.MainArtists
                .Where(ma => artistSummaries.ContainsKey(ma.Value))
                .Select(ma => artistSummaries[ma.Value])
                .ToList()
        )).ToList();
    }
}
