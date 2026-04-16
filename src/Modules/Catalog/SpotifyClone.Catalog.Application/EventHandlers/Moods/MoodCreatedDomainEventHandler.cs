using MediatR;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Moods.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Moods;

namespace SpotifyClone.Catalog.Application.EventHandlers.Moods;

internal sealed class MoodCreatedDomainEventHandler(
    ICatalogUnitOfWork unit)
    : INotificationHandler<MoodCreatedDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task Handle(
        MoodCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new MoodCreatedIntegrationEvent(
                notification.Id.Value,
                notification.Name);

        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);

        await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
