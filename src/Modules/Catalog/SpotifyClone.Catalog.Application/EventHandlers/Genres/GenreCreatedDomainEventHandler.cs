using MediatR;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Genres.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Genres;

namespace SpotifyClone.Catalog.Application.EventHandlers.Genres;

internal sealed class GenreCreatedDomainEventHandler(
    ICatalogUnitOfWork unit)
    : INotificationHandler<GenreCreatedDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task Handle(
        GenreCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new GenreCreatedIntegrationEvent(
                notification.Id.Value,
                notification.Name);

        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);

        await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
