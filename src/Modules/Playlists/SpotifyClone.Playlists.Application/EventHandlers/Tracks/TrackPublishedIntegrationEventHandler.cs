using MediatR;
using Microsoft.Extensions.Logging;
using SpotifyClone.Playlists.Application.Abstractions;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

namespace SpotifyClone.Playlists.Application.EventHandlers.Tracks;

internal sealed class TrackPublishedIntegrationEventHandler(
    IPlaylistsUnitOfWork unit,
    ILogger<TrackPublishedIntegrationEventHandler> logger)
    : INotificationHandler<TrackPublishedIntegrationEvent>
{
    private readonly IPlaylistsUnitOfWork _unit = unit;
    private readonly ILogger<TrackPublishedIntegrationEventHandler> _logger = logger;

    public async Task Handle(
        TrackPublishedIntegrationEvent notification,
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

        await _unit.TrackReferences.MarkAsPublishedAsync(
            notification.Id,
            cancellationToken);

        await _unit.CommitAsync(cancellationToken);
    }
}
