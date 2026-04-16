using MediatR;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

namespace SpotifyClone.Catalog.Application.EventHandlers.Tracks;

internal sealed class TrackMoodsUpdatedDomainEventHandler(
    ICatalogUnitOfWork unit)
    : INotificationHandler<TrackMoodsUpdatedDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task Handle(
        TrackMoodsUpdatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new TrackMoodsUpdatedIntegrationEvent(
                notification.Id.Value,
                notification.Moods.Select(m => m.Value));

        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);

        await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
