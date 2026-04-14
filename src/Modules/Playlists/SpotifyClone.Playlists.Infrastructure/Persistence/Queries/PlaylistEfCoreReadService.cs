using Microsoft.EntityFrameworkCore;
using SpotifyClone.Playlists.Application.Abstractions.Clients;
using SpotifyClone.Playlists.Application.Abstractions.Data;
using SpotifyClone.Playlists.Application.Features.Playlists.Queries;
using SpotifyClone.Playlists.Application.Models;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.Entities;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.Enums;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.ValueObjects;
using SpotifyClone.Playlists.Infrastructure.Persistence.Database;
using SpotifyClone.Playlists.Infrastructure.Persistence.Entities;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Infrastructure.Persistence.Extensions;
using SpotifyClone.Shared.Kernel.Contracts.Catalog;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Playlists.Infrastructure.Persistence.Queries;

internal sealed class PlaylistEfCoreReadService(
    PlaylistsAppDbContext context,
    ICatalogModulePlaylistsClient catalogClient)
    : IPlaylistReadService
{
    private readonly PlaylistsAppDbContext _context = context;
    private readonly ICatalogModulePlaylistsClient _catalogClient = catalogClient;

    public async Task<PlaylistDetails?> GetDetailsAsync(
        PlaylistId id,
        CancellationToken cancellationToken = default)
    {
        var header = await _context.Playlists
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.OwnerId,
                p.IsPublic,
                p.Cover,
                TrackInfos = p.Tracks.OrderBy(t => t.Position)
                    .Select(t => new { t.Id, t.Position }).ToList(),
                CollaboratorIds = p.Collaborators.Select(c => c.Value).ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (header == null)
        {
            return null;
        }

        var top4Ids = header.TrackInfos.Take(4).Select(x => x.Id.Value).ToList();

        List<CollaboratorSummary> collaboratorsTask = await _context.UserReferences
            .AsNoTracking()
            .Where(u => header.CollaboratorIds.Contains(u.Id))
            .Select(u => new CollaboratorSummary(u.Id, u.Name, u.AvatarImageId))
            .ToListAsync(cancellationToken);

        List<Guid> validCoverIdsTask = await _context.TrackReferences
            .AsNoTracking()
            .Where(t => top4Ids.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        return new PlaylistDetails(
            header.Id.Value,
            header.Name,
            header.Description,
            header.OwnerId.Value,
            header.IsPublic,
            header.Cover == null ? null : new ImageMetadataDetails(
                header.Cover.ImageId.Value,
                header.Cover.Metadata.Width,
                header.Cover.Metadata.Height,
                header.Cover.Metadata.FileType.Value,
                header.Cover.Metadata.SizeInBytes),
            validCoverIdsTask
                .OrderBy(id => header.TrackInfos.FindIndex(x => x.Id.Value == id))
                .ToList(),
            collaboratorsTask,
            header.TrackInfos.Select(t => new PlaylistTrackSummary(t.Id.Value, t.Position)).ToList()
        );
    }

    public async Task<PagedList<PlaylistSummary>> ListAsync(
        UserId? ownerId,
        bool isAdmin,
        PlaylistFilterParams filters,
        PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        IQueryable<Playlist> query = _context.Playlists.AsNoTracking();
        
        if (!isAdmin)
        {
            query = ownerId is null
                ? query.Where(p => p.IsPublic)
                : query.Where(p => p.IsPublic || p.OwnerId == ownerId);
        }

        if (filters.Name is not null)
        {
            query = query.Where(p => EF.Functions.ILike(p.Name, filters.Name));
        }
        if (filters.Description is not null)
        {
            query = query.Where(p =>
                p.Description != null &&
                EF.Functions.ILike(p.Description, filters.Description));
        }
        if (filters.OwnerId is not null)
        {
            var owner = UserId.From(filters.OwnerId.Value);
            query = query.Where(p => p.OwnerId == owner);
        }
        if (filters.Type is not null)
        {
            PlaylistType type = Enum.Parse<PlaylistType>(filters.Type);
            query = query.Where(p => p.Type == type);
        }
        if (filters.IsPublic is not null)
        {
            query = query.Where(p => p.IsPublic == filters.IsPublic);
        }
        if (filters.CollaboratorIds is not null)
        {
            query = query.Where(p => p.Collaborators.Any(c => filters.CollaboratorIds.Any(id => id == c.Value)));
        }
        if (filters.TrackIds is not null)
        {
            query = query.Where(p => p.Tracks.Any(t => filters.TrackIds.Any(id => id == t.Id.Value)));
        }
        if (filters.GenreIds is not null && filters.GenreIds.Any() ||
            filters.MoodIds is not null && filters.MoodIds.Any())
        {
            IEnumerable<TrackSharedDto> tracks =
                await _catalogClient.GetAllTracksAsync(cancellationToken);

            var matchingTrackGuids = tracks
                .Where(tr =>
                    (filters.GenreIds == null || !filters.GenreIds.Any() ||
                        tr.GenreIds.Any(g => filters.GenreIds.Contains(g)))
                    &&
                    (filters.MoodIds == null || !filters.MoodIds.Any() ||
                        tr.MoodIds.Any(m => filters.MoodIds.Contains(m)))
                )
                .Select(tr => tr.Id)
                .Distinct()
                .ToList();

            if (matchingTrackGuids.Count == 0)
            {
                query = query.Where(p => false);
            }
            else
            {
                var matchingTrackIds = matchingTrackGuids
                    .Select(id => TrackId.From(id))
                    .ToList();

                List<PlaylistId> matchingPlaylistIds = await _context.PlaylistTracks
                    .AsNoTracking()
                    .Where(pt => matchingTrackIds.Contains(pt.Id))
                    .Select(pt => pt.PlaylistId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                query = matchingPlaylistIds.Count == 0
                    ? query.Where(p => false)
                    : query.Where(p => matchingPlaylistIds.Contains(p.Id));
            }
        }

        var playlists = query.Select(p => new
        {
            p.Id,
            p.Name,
            p.Description,
            p.IsPublic,
            p.OwnerId,
            p.Cover
        });

        var playlistIds = playlists.Select(p => p.Id).ToList();

        List<PlaylistTrack> playlistTracks = await _context.PlaylistTracks
            .Where(pt => playlistIds.Contains(pt.PlaylistId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        List<TrackReference> trackRefs = await _context.TrackReferences
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var trackLookup = playlistTracks
            .Join(trackRefs,
                pt => pt.Id.Value,
                tr => tr.Id,
                (pt, tr) => new { pt.PlaylistId, pt.Position, tr.Id })
            .GroupBy(x => x.PlaylistId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.Position).Take(4).Select(x => x.Id).ToList());

        return await playlists.Select(p => new PlaylistSummary(
            p.Id.Value,
            p.Name,
            p.Description,
            p.IsPublic,
            p.OwnerId.Value,
            p.Cover == null ? null : new ImageMetadataDetails(
                p.Cover.ImageId.Value,
                p.Cover.Metadata.Width,
                p.Cover.Metadata.Height,
                p.Cover.Metadata.FileType.Value,
                p.Cover.Metadata.SizeInBytes),
            trackLookup.GetValueOrDefault(p.Id) ?? new List<Guid>()
        )).ToPagedListAsync(pagination, cancellationToken);
    }

    public async Task<IEnumerable<PlaylistSummary>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var playlists = await _context.Playlists
            .AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.IsPublic,
                OwnerId = p.OwnerId.Value,
                p.Cover
            })
            .ToListAsync(cancellationToken);

        if (playlists.Count <= 0)
        {
            return Enumerable.Empty<PlaylistSummary>();
        }

        var playlistIds = playlists
            .Select(p => p.Id)
            .ToList();

        var trackLookup = await _context.PlaylistTracks
            .AsNoTracking()
            .Where(pt => playlistIds.Contains(pt.PlaylistId))
            .Select(pt => new
            {
                PlaylistId = pt.PlaylistId.Value,
                pt.Position,
                TrackId = pt.Id.Value
            })
            .ToListAsync(cancellationToken);

        var trackIds = trackLookup
            .Select(t => t.TrackId)
            .Distinct()
            .ToList();

        var trackCovers = await _context.TrackReferences
            .AsNoTracking()
            .Where(tr => trackIds.Contains(tr.Id))
            .Select(tr => new
            {
                TrackId = tr.Id,
                tr.CoverImageId
            })
            .ToListAsync(cancellationToken);

        var coverMap = trackCovers
            .ToDictionary(x => x.TrackId, x => x.CoverImageId);

        var groupedCovers = trackLookup
            .OrderBy(x => x.Position)
            .GroupBy(x => x.PlaylistId)
            .ToDictionary(
                g => g.Key,
                g => g
                .Select(x => coverMap.GetValueOrDefault(x.TrackId))
                .Where(x => x != null)
                .Take(4)
                .Select(x => x!.Value)
                .ToList());

        return playlists.Select(p => new PlaylistSummary(
            p.Id.Value,
            p.Name,
            p.Description,
            p.IsPublic,
            p.OwnerId,
            p.Cover == null
                ? null
                : new ImageMetadataDetails(
                    p.Cover.ImageId.Value,
                    p.Cover.Metadata.Width,
                    p.Cover.Metadata.Height,
                    p.Cover.Metadata.FileType.Value,
                    p.Cover.Metadata.SizeInBytes),
            groupedCovers.GetValueOrDefault(p.Id.Value) ?? new List<Guid>()
        )).ToList();
    }

    public async Task<IEnumerable<PlaylistSummary>> GetAllByTracksAsync(
        IEnumerable<TrackId> trackIds,
        CancellationToken cancellationToken)
    {
        IQueryable<PlaylistId> targetPlaylistIdsQuery = _context.PlaylistTracks
            .AsNoTracking()
            .Where(pt => trackIds.Contains(pt.Id))
            .Select(pt => pt.PlaylistId)
            .Distinct();

        var playlistsInfo = await _context.Playlists
            .AsNoTracking()
            .Where(p => targetPlaylistIdsQuery.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.IsPublic,
                p.OwnerId,
                p.Cover
            })
            .ToListAsync(cancellationToken);

        if (playlistsInfo.Count <= 0)
        {
            return Enumerable.Empty<PlaylistSummary>();
        }

        var playlistIds = playlistsInfo.Select(p => p.Id).ToList();

        var trackLookup = await _context.PlaylistTracks
            .AsNoTracking()
            .Where(pt => playlistIds.Contains(pt.PlaylistId))
            .OrderBy(pt => pt.Position)
            .Select(pt => new
            {
                pt.PlaylistId,
                pt.Position,
                TrackIdGuid = pt.Id.Value
            })
            .Join(_context.TrackReferences,
                pt => pt.TrackIdGuid,
                tr => tr.Id,
                (pt, tr) => new { pt.PlaylistId, tr.CoverImageId })
            .ToListAsync(cancellationToken);

        var groupedCovers = trackLookup
            .GroupBy(x => x.PlaylistId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .Where(x => x.CoverImageId is not null)
                    .Select(x => x.CoverImageId!.Value)
                    .Take(4)
                    .ToList());

        return playlistsInfo.Select(p => new PlaylistSummary(
            p.Id.Value,
            p.Name,
            p.Description,
            p.IsPublic,
            p.OwnerId.Value,
            p.Cover == null ? null : new ImageMetadataDetails(
                p.Cover.ImageId.Value,
                p.Cover.Metadata.Width,
                p.Cover.Metadata.Height,
                p.Cover.Metadata.FileType.Value,
                p.Cover.Metadata.SizeInBytes),
            groupedCovers.GetValueOrDefault(p.Id) ?? new List<Guid>()
        )).ToList();
    }
}
