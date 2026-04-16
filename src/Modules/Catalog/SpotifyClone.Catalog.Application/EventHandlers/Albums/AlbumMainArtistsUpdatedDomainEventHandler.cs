using MediatR;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

namespace SpotifyClone.Catalog.Application.EventHandlers.Albums;

internal sealed class AlbumMainArtistsUpdatedDomainEventHandler(
    ICatalogUnitOfWork unit)
    : INotificationHandler<AlbumMainArtistsUpdatedDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task Handle(
        AlbumMainArtistsUpdatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new AlbumArtistsUpdatedIntegrationEvent(
                notification.Id.Value,
                notification.MainArtists.Select(a => a.Value));

        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);

        await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
