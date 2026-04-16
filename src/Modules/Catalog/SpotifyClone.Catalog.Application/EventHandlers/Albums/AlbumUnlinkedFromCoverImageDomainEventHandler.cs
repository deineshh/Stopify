using MediatR;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

namespace SpotifyClone.Catalog.Application.EventHandlers.Albums;

internal sealed class AlbumUnlinkedFromCoverImageDomainEventHandler(
    ICatalogUnitOfWork unit)
    : INotificationHandler<AlbumUnlinkedFromCoverImageDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task Handle(
        AlbumUnlinkedFromCoverImageDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent1 = new ImageLinkRemovedIntegrationEvent(
            notification.ImageId.Value);
        var message1 = OutboxMessage.FromIntegrationEvent(integrationEvent1);
        await _unit.OutboxMessages.AddAsync(message1, cancellationToken);

        var integrationEvent2 = new AlbumUnlinkedFromCoverImageIntegrationEvent(
            notification.Id.Value,
            notification.Tracks.Select(t => t.Value));
        var message2 = OutboxMessage.FromIntegrationEvent(integrationEvent2);
        await _unit.OutboxMessages.AddAsync(message2, cancellationToken);

        await _unit.CommitAsync(cancellationToken);
    }
}
