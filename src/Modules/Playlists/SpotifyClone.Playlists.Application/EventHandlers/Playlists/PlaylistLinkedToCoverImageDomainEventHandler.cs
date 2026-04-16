using MediatR;
using SpotifyClone.Playlists.Application.Abstractions;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;
using SpotifyClone.Shared.IntegrationEvents.Playlists;

namespace SpotifyClone.Playlists.Application.EventHandlers.Playlists;

internal sealed class PlaylistLinkedToCoverImageDomainEventHandler(
    IPlaylistsUnitOfWork unit)
    : INotificationHandler<PlaylistLinkedToCoverImageDomainEvent>
{
    private readonly IPlaylistsUnitOfWork _unit = unit;

    public async Task Handle(
        PlaylistLinkedToCoverImageDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent1 = new ImageLinkAddedIntegrationEvent(
                notification.ImageId.Value);
        var message1 = OutboxMessage.FromIntegrationEvent(integrationEvent1);
        await _unit.OutboxMessages.AddAsync(message1, cancellationToken);

        var integrationEvent2 = new PlaylistCoverChangedIntegrationEvent(
                notification.Id.Value,
                notification.ImageId.Value, []);
        var message2 = OutboxMessage.FromIntegrationEvent(integrationEvent2);
        await _unit.OutboxMessages.AddAsync(message2, cancellationToken);

        await _unit.CommitAsync(cancellationToken);
    }
}
