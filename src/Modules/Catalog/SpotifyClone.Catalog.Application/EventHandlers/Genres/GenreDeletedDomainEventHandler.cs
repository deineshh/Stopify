using MediatR;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Genres.Events;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Genres;

namespace SpotifyClone.Catalog.Application.EventHandlers.Genres;

internal sealed class GenreDeletedDomainEventHandler(
    ICatalogUnitOfWork unit)
    : INotificationHandler<GenreDeletedDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task Handle(
        GenreDeletedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        IEnumerable<Track> tracks = await _unit.Tracks.GetAllByGenreAsync(
            notification.Id, cancellationToken);
        foreach (Track track in tracks)
        {
            track.RemoveGenre(notification.Id);
        }

        var integrationEvent = new GenreDeletedIntegrationEvent(
                notification.Id.Value);
        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);
        await _unit.OutboxMessages.AddAsync(message, cancellationToken);

        await _unit.CommitAsync(cancellationToken);
    }
}
