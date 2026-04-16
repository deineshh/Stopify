using MediatR;
using Microsoft.Extensions.Logging;
using SpotifyClone.Playlists.Application.Abstractions;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

namespace SpotifyClone.Playlists.Application.EventHandlers.Tracks;

internal sealed class TrackUnpublishedIntegrationEventHandler(
    IPlaylistsUnitOfWork unit,
    ILogger<TrackUnpublishedIntegrationEventHandler> logger)
    : INotificationHandler<TrackUnpublishedIntegrationEvent>
{
    private readonly IPlaylistsUnitOfWork _unit = unit;
    private readonly ILogger<TrackUnpublishedIntegrationEventHandler> _logger = logger;

    public async Task Handle(
        TrackUnpublishedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        if (!await _unit.TrackReferences.ExistsAsync(notification.Id, cancellationToken))
        {
            return;
        }

        await _unit.TrackReferences.MarkAsNotPublishedAsync(notification.Id, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
