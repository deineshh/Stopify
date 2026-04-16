using MediatR;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

namespace SpotifyClone.Catalog.Application.EventHandlers.Albums;

internal sealed class AlbumTitleCorrectedDomainEventHandler(
    ICatalogUnitOfWork unit)
    : INotificationHandler<AlbumTitleCorrectedDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task Handle(
        AlbumTitleCorrectedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new AlbumTitleChangedIntegrationEvent(
                notification.Id.Value,
                notification.Title);

        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);

        await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
