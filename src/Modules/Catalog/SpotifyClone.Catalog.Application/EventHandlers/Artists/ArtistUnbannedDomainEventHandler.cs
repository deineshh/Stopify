using MediatR;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Artists;

namespace SpotifyClone.Catalog.Application.EventHandlers.Artists;

internal sealed class ArtistUnbannedDomainEventHandler(
    ICatalogUnitOfWork unit)
    : INotificationHandler<ArtistUnbannedDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task Handle(
        ArtistUnbannedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new ArtistUnbannedIntegrationEvent(
            notification.Id.Value,
            notification.Name,
            notification.Status.IsVerified,
            notification.AvatarImageId?.Value);

        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);

        await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
