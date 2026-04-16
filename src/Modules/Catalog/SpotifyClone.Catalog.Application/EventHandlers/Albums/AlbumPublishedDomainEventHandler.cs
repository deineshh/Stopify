using MediatR;
using Microsoft.Extensions.Logging;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Albums;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.Events;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

namespace SpotifyClone.Catalog.Application.EventHandlers.Albums;

internal sealed class AlbumPublishedDomainEventHandler(
    ICatalogUnitOfWork unit,
    ILogger<AlbumPublishedDomainEventHandler> logger)
    : INotificationHandler<AlbumPublishedDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;
    private readonly ILogger<AlbumPublishedDomainEventHandler> _logger = logger;

    public async Task Handle(
        AlbumPublishedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        Album? album = await _unit.Albums.GetByIdAsync(notification.Id, cancellationToken);
        if (album is null)
        {
            _logger.LogError(
                "Album {Id} was not found while publishing it's tracks.",
                notification.Id);
            return;
        }

        IEnumerable<Track> tracks = await _unit.Tracks.GetByIdsAsync(
            notification.Tracks, cancellationToken);

        foreach (Track track in tracks)
        {
            track.Publish(notification.ReleaseDate);
        }

        var integrationEvent = new AlbumPublishedIntegrationEvent(
                notification.Id.Value,
                notification.Title,
                notification.ReleaseDate,
                notification.Type.Value,
                notification.CoverImageId.Value,
                notification.MainArtists.Select(a => a.Value));
        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);
        await _unit.OutboxMessages.AddAsync(message, cancellationToken);

        await _unit.CommitAsync(cancellationToken);
    }
}
