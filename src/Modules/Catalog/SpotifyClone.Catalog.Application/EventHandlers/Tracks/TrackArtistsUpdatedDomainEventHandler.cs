using MediatR;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

namespace SpotifyClone.Catalog.Application.EventHandlers.Tracks;

internal sealed class TrackArtistsUpdatedDomainEventHandler(
    ICatalogUnitOfWork unit)
    : INotificationHandler<TrackArtistsUpdatedDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task Handle(
        TrackArtistsUpdatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new TrackArtistsUpdatedIntegrationEvent(
                notification.Id.Value,
                notification.MainArtists.Select(a => a.Value),
                notification.FeaturedArtists.Select(a => a.Value));

        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);

        await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
