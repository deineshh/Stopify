using MediatR;
using Microsoft.Extensions.Logging;
using SpotifyClone.Playlists.Application.Abstractions;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

namespace SpotifyClone.Playlists.Application.EventHandlers.Albums;

internal sealed class AlbumUnlinkedFromCoverImageIntegrationEventHandler(
    IPlaylistsUnitOfWork unit,
    ILogger<AlbumUnlinkedFromCoverImageIntegrationEventHandler> logger)
    : INotificationHandler<AlbumUnlinkedFromCoverImageIntegrationEvent>
{
    private readonly IPlaylistsUnitOfWork _unit = unit;
    private readonly ILogger<AlbumUnlinkedFromCoverImageIntegrationEventHandler> _logger = logger;

    public async Task Handle(
        AlbumUnlinkedFromCoverImageIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        if (!await _unit.TrackReferences.ExistsAsync(notification.Tracks, cancellationToken))
        {
            _logger.LogError("Track was not found in the Playlists module.");
            throw new InvalidOperationException($"Track was not found in the Playlists module.");
        }

        foreach (Guid trackId in notification.Tracks)
        {
            await _unit.TrackReferences.UnlinkCoverAsync(trackId, cancellationToken);
        }

        await _unit.CommitAsync(cancellationToken);
    }
}
