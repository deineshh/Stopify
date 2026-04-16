using MediatR;
using Microsoft.Extensions.Logging;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Albums;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.Events;
using SpotifyClone.Catalog.Domain.Services;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

namespace SpotifyClone.Catalog.Application.EventHandlers.Tracks;

internal sealed class TrackMarkedAsReadyToPublishDomainEventHandler(
    ICatalogUnitOfWork unit,
    AlbumTrackDomainService albumTrackDomainService,
    ILogger<TrackMarkedAsReadyToPublishDomainEventHandler> logger)
    : INotificationHandler<TrackMarkedAsReadyToPublishDomainEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;
    private readonly AlbumTrackDomainService _albumTrackDomainService = albumTrackDomainService;
    private readonly ILogger<TrackMarkedAsReadyToPublishDomainEventHandler> _logger = logger;

    public async Task Handle(
        TrackMarkedAsReadyToPublishDomainEvent notification,
        CancellationToken cancellationToken)
    {
        Album? album = await _unit.Albums.GetByIdAsync(notification.AlbumId, cancellationToken);
        if (album is null)
        {
            _logger.LogError(
                "Album {AlbumId} not found while refreshing it's status",
                notification.AlbumId.Value);

            return;
        }
        _albumTrackDomainService.TryMarkAlbumAsReadyToPublish(album);
        _albumTrackDomainService.ReevaluateAlbumType(album);

        var trackUnpublishedIntegrationEvent = new TrackUnpublishedIntegrationEvent(
            notification.TrackId.Value);
        await _unit.OutboxMessages.AddAsync(
            OutboxMessage.FromIntegrationEvent(trackUnpublishedIntegrationEvent),
            cancellationToken);

        var trackMarkedAsReadyIntegrationEvent = new TrackMarkedAsReadyIntegrationEvent(
                notification.TrackId.Value);
        await _unit.OutboxMessages.AddAsync(
            OutboxMessage.FromIntegrationEvent(trackMarkedAsReadyIntegrationEvent),
            cancellationToken);

        await _unit.CommitAsync(cancellationToken);
    }
}
