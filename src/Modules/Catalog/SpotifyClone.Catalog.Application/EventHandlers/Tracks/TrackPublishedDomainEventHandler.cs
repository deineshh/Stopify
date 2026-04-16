using MediatR;
using Microsoft.Extensions.Logging;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Catalog.Application.Features.Albums.Queries;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

namespace SpotifyClone.Catalog.Application.EventHandlers.Tracks;

internal sealed class TrackPublishedDomainEventHandler(
    IAlbumReadService albumReadService,
    ICatalogUnitOfWork unit,
    ILogger<TrackPublishedDomainEventHandler> logger)
    : INotificationHandler<TrackPublishedDomainEvent>
{
    private readonly IAlbumReadService _albumReadService = albumReadService;
    private readonly ICatalogUnitOfWork _unit = unit;
    private readonly ILogger<TrackPublishedDomainEventHandler> _logger = logger;

    public async Task Handle(
        TrackPublishedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        AlbumSummary? album = await _albumReadService.GetSummary(notification.AlbumId, cancellationToken);
        if (album is null)
        {
            _logger.LogError("Album with id {AlbumId} not found.", notification.AlbumId.Value);
            throw new InvalidOperationException($"Album {notification.AlbumId.Value} not found.");
        }

        if (album.Cover is null)
        {
            _logger.LogError("Album with id {AlbumId} does not have a cover image.", notification.AlbumId.Value);
            throw new InvalidOperationException($"Album {notification.AlbumId.Value} does not have a cover image.");
        }

        var integrationEvent = new TrackPublishedIntegrationEvent(
            notification.Id.Value,
            notification.Title,
            notification.ContainsExplicitContent,
            notification.ReleaseDateUtc,
            album.Cover.ImageId,
            album.Id,
            notification.MainArtists.Select(a => a.Value),
            notification.FeaturedArtists.Select(a => a.Value),
            notification.Genres.Select(g => g.Value),
            notification.Moods.Select(m => m.Value));
        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);
        await _unit.OutboxMessages.AddAsync(message, cancellationToken);

        await _unit.CommitAsync(cancellationToken);
    }
}
