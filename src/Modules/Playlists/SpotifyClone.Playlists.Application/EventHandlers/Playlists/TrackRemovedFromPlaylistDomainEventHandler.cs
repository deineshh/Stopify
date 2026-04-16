using MediatR;
using SpotifyClone.Playlists.Application.Abstractions;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Playlists;

namespace SpotifyClone.Playlists.Application.EventHandlers.Playlists;

internal sealed class TrackRemovedFromPlaylistDomainEventHandler(
    IPlaylistsUnitOfWork unit)
    : INotificationHandler<TrackRemovedFromPlaylistDomainEvent>
{
    private readonly IPlaylistsUnitOfWork _unit = unit;

    public async Task Handle(
        TrackRemovedFromPlaylistDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new TrackRemovedFromPlaylistIntegrationEvent(
                notification.Id.Value);

        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);

        await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
