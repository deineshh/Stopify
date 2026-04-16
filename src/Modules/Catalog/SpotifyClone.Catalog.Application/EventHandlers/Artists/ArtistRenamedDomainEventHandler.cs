using MediatR;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Artists;

namespace SpotifyClone.Catalog.Application.EventHandlers.Artists;

internal sealed class ArtistRenamedDomainEventHandler(
    ICatalogUnitOfWork unit)
    : INotificationHandler<ArtistRenamedDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task Handle(
        ArtistRenamedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new ArtistNameChangedIntegrationEvent(
                notification.Id.Value,
                notification.Name);

        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);

        await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
