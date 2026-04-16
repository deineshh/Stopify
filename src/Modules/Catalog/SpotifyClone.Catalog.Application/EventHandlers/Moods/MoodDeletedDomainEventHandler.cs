using MediatR;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Moods.Events;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Moods;

namespace SpotifyClone.Catalog.Application.EventHandlers.Moods;

internal sealed class MoodDeletedDomainEventHandler(
    ICatalogUnitOfWork unit)
    : INotificationHandler<MoodDeletedDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task Handle(
        MoodDeletedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        IEnumerable<Track> tracks = await _unit.Tracks.GetAllByMoodAsync(
            notification.Id, cancellationToken);

        foreach (Track track in tracks)
        {
            track.RemoveMood(notification.Id);
        }

        var integrationEvent = new MoodDeletedIntegrationEvent(
                notification.Id.Value);
        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);
        await _unit.OutboxMessages.AddAsync(message, cancellationToken);

        await _unit.CommitAsync(cancellationToken);
    }
}
