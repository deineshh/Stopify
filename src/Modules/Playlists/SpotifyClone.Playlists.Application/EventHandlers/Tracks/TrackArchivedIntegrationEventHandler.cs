using MediatR;
using Microsoft.Extensions.Logging;
using SpotifyClone.Playlists.Application.Abstractions;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.Enums;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Playlists.Application.EventHandlers.Tracks;

internal sealed class TrackArchivedIntegrationEventHandler(
    IPlaylistsUnitOfWork unit,
    ICurrentUser currentUser,
    ILogger<TrackArchivedIntegrationEventHandler> logger)
    : INotificationHandler<TrackArchivedIntegrationEvent>
{
    private readonly IPlaylistsUnitOfWork _unit = unit;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly ILogger<TrackArchivedIntegrationEventHandler> _logger = logger;

    public async Task Handle(
        TrackArchivedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        if (!await _unit.TrackReferences.ExistsAsync(notification.Id, cancellationToken))
        {
            _logger.LogError(
                "Track {TrackId} was not found in the Playlists module.",
                notification.Id);
            throw new InvalidOperationException(
                $"Track {notification.Id} was not found in the Playlists module.");
        }
        await _unit.TrackReferences.MarkAsArchivedAsync(notification.Id, cancellationToken);

        if (!_currentUser.IsAuthenticated ||
            !await _unit.UserReferences.ExistsAsync(_currentUser.Id, cancellationToken))
        {
            _logger.LogError("No authenticated user was found in the Playlists module.");
            throw new InvalidOperationException("No authenticated user was found in the Playlists module.");
        }

        var ownerId = UserId.From(_currentUser.Id);
        Playlist playlist = await _unit.Playlists.GetArchivedTracksAsync(
            UserId.From(_currentUser.Id),
            cancellationToken)
        ?? Playlist.CreateSystemPlaylist(
            PlaylistId.New(),
            ownerId,
            "Archived Tracks",
            null,
            PlaylistType.ArchivedTracks);

        playlist.AddTrack(TrackId.From(notification.Id), ownerId, true);

        await _unit.Playlists.AddAsync(playlist, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
