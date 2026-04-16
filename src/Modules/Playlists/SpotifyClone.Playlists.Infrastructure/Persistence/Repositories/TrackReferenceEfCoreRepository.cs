using Microsoft.EntityFrameworkCore;
using SpotifyClone.Playlists.Application.Abstractions.Repositories;
using SpotifyClone.Playlists.Infrastructure.Persistence.Database;
using SpotifyClone.Playlists.Infrastructure.Persistence.Entities;
using SpotifyClone.Playlists.Infrastructure.Persistence.Entities.Enums;

namespace SpotifyClone.Playlists.Infrastructure.Persistence.Repositories;

internal sealed class TrackReferenceEfCoreRepository(
    PlaylistsAppDbContext context)
    : ITrackReferenceRepository
{
    private readonly DbSet<TrackReference> _tracks = context.Set<TrackReference>();

    public Task<bool> ExistsAsync(
        Guid trackId,
        CancellationToken cancellationToken)
        => _tracks.AnyAsync(x => x.Id == trackId, cancellationToken);

    public Task<bool> ExistsAsync(
        IEnumerable<Guid> trackIds,
        CancellationToken cancellationToken)
        => _tracks.AnyAsync(x => trackIds.Contains(x.Id), cancellationToken);

    public Task<bool> IsPublishedAsync(
        Guid trackId,
        CancellationToken cancellationToken)
        => _tracks.AnyAsync(
            x => x.Id == trackId && x.Status == TrackReferenceStatus.Published, cancellationToken);

    public async Task AddAsync(
        Guid trackId,
        CancellationToken cancellationToken = default)
        => await _tracks.AddAsync(
            new TrackReference(trackId, TrackReferenceStatus.NotPublished, null),
            cancellationToken);

    public async Task LinkCoverAsync(
        Guid trackId,
        Guid coverImageId,
        CancellationToken cancellationToken = default)
        => await _tracks.Where(x => x.Id == trackId)
        .ExecuteUpdateAsync(
            x => x.SetProperty(t => t.CoverImageId, coverImageId),
            cancellationToken);

    public async Task UnlinkCoverAsync(
        Guid trackId,
        CancellationToken cancellationToken = default)
        => await _tracks.Where(x => x.Id == trackId)
        .ExecuteUpdateAsync(
            x => x.SetProperty(t => t.CoverImageId, (Guid?)null),
            cancellationToken);

    public async Task MarkAsPublishedAsync(
        Guid trackId,
        CancellationToken cancellationToken = default)
        => await _tracks.Where(x => x.Id == trackId)
        .ExecuteUpdateAsync(
            x => x.SetProperty(t => t.Status, TrackReferenceStatus.Published),
            cancellationToken);

    public async Task MarkAsNotPublishedAsync(
        Guid trackId,
        CancellationToken cancellationToken = default)
        => await _tracks.Where(x => x.Id == trackId)
        .ExecuteUpdateAsync(x => x.SetProperty(
            t => t.Status, TrackReferenceStatus.NotPublished), cancellationToken);

    public async Task MarkAsArchivedAsync(
        Guid trackId,
        CancellationToken cancellationToken = default)
        => await _tracks.Where(x => x.Id == trackId)
        .ExecuteUpdateAsync(x => x.SetProperty(
            t => t.Status, TrackReferenceStatus.Archived), cancellationToken);

    public async Task DeleteAsync(
        Guid trackId,
        CancellationToken cancellationToken = default)
        => await _tracks.Where(x => x.Id == trackId)
        .ExecuteDeleteAsync(cancellationToken);
}
