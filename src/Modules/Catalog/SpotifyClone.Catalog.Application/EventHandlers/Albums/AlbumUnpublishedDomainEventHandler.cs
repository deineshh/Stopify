using MediatR;
using Microsoft.Extensions.Logging;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Albums;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.Events;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks;
using SpotifyClone.Catalog.Domain.Services;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

namespace SpotifyClone.Catalog.Application.EventHandlers.Albums;

internal sealed class AlbumUnpublishedDomainEventHandler(
    ICatalogUnitOfWork unit,
    AlbumTrackDomainService albumTrackDomainService,
    ILogger<AlbumUnpublishedDomainEventHandler> logger)
    : INotificationHandler<AlbumUnpublishedDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;
    private readonly AlbumTrackDomainService _albumTrackDomainService = albumTrackDomainService;
    private readonly ILogger<AlbumUnpublishedDomainEventHandler> _logger = logger;

    public async Task Handle(
        AlbumUnpublishedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        Album? album = await _unit.Albums.GetByIdAsync(notification.Id, cancellationToken);
        if (album is null)
        {
            _logger.LogError("Album {AlbumId} was not found.", notification.Id);
            throw new InvalidOperationException($"Album {notification.Id} was not found.");
        }

        IEnumerable<Track> tracks = await _unit.Tracks.GetAllByAlbumAsync(
            notification.Id,
            cancellationToken);

        foreach (Track track in tracks)
        {
            track.Unpublish();
        }

        _albumTrackDomainService.TryMarkAlbumAsReadyToPublish(album);

        var integrationEvent = new AlbumUnpublishedIntegrationEvent(
                notification.Id.Value);
        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);
        await _unit.OutboxMessages.AddAsync(message, cancellationToken);

        await _unit.CommitAsync(cancellationToken);
    }
}
