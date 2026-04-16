using MediatR;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Artists;

namespace SpotifyClone.Catalog.Application.EventHandlers.Artists;

internal sealed class ArtistLinkedToAvatarImageDomainEventHandler(
    ICatalogUnitOfWork unit)
    : INotificationHandler<ArtistLinkedToAvatarImageDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task Handle(
        ArtistLinkedToAvatarImageDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent1 = new ArtistAvatarImageChangedIntegrationEvent(
                notification.Id.Value,
                notification.ImageId.Value);
        var message1 = OutboxMessage.FromIntegrationEvent(integrationEvent1);
        await _unit.OutboxMessages.AddAsync(message1, cancellationToken);

        var integrationEvent2 = new ImageLinkAddedIntegrationEvent(
                notification.ImageId.Value);
        var message2 = OutboxMessage.FromIntegrationEvent(integrationEvent2);
        await _unit.OutboxMessages.AddAsync(message2, cancellationToken);

        await _unit.CommitAsync(cancellationToken);
    }
}
