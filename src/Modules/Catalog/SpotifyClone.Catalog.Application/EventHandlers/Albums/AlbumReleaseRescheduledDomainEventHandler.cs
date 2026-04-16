using MediatR;
using Microsoft.Extensions.Logging;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Albums;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.Events;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Artists;

namespace SpotifyClone.Catalog.Application.EventHandlers.Albums;

internal sealed class AlbumReleaseRescheduledDomainEventHandler(
    ICatalogUnitOfWork unit,
    ILogger<AlbumReleaseRescheduledDomainEventHandler> logger)
    : INotificationHandler<AlbumReleaseRescheduledDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;
    private readonly ILogger<AlbumReleaseRescheduledDomainEventHandler> _logger = logger;

    public async Task Handle(
        AlbumReleaseRescheduledDomainEvent notification,
        CancellationToken cancellationToken)
    {
        Album? album = await _unit.Albums.GetByIdAsync(notification.Id, cancellationToken);
        if (album is null)
        {
            _logger.LogError(
                "Album with ID {AlbumId} not found while rescheduling release for all album tracks",
                notification.Id);
            return;
        }

        IEnumerable<Track> tracks = await _unit.Tracks.GetAllByAlbumAsync(album.Id, cancellationToken);
        foreach (Track track in tracks)
        {
            _logger.LogInformation(
                "Rescheduling release for track {Id} to {ReleaseDate}",
                track.Id, notification.ReleaseDateUtc);

            track.RescheduleRelease(notification.ReleaseDateUtc);

            _logger.LogInformation(
                "Track {Id} release rescheduled to {ReleaseDate}",
                track.Id, track.ReleaseDate);
        }

        var integrationEvent = new AlbumReleaseDateChangedIntegrationEvent(
                notification.Id.Value,
                notification.ReleaseDateUtc);
        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);
        await _unit.OutboxMessages.AddAsync(message, cancellationToken);

        await _unit.CommitAsync(cancellationToken);
    }
}
