using MediatR;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

namespace SpotifyClone.Catalog.Application.EventHandlers.Tracks;

internal sealed class TrackMarkedAsNotExplicitDomainEventHandler(
    ICatalogUnitOfWork unit)
    : INotificationHandler<TrackMarkedAsNotExplicitDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task Handle(
        TrackMarkedAsNotExplicitDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new TrackExplicityChangedIntegrationEvent(
                notification.Id.Value, false);

        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);

        await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
