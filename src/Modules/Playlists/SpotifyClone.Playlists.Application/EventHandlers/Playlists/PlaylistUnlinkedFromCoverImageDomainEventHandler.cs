using MediatR;
using Microsoft.Extensions.Logging;
using SpotifyClone.Playlists.Application.Abstractions;
using SpotifyClone.Playlists.Application.Abstractions.Data;
using SpotifyClone.Playlists.Application.Features.Playlists.Queries;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;
using SpotifyClone.Shared.IntegrationEvents.Playlists;

namespace SpotifyClone.Playlists.Application.EventHandlers.Playlists;

internal sealed class PlaylistUnlinkedFromCoverImageDomainEventHandler(
    IPlaylistReadService playlistReadService,
    IPlaylistsUnitOfWork unit,
    ILogger<PlaylistUnlinkedFromCoverImageDomainEventHandler> logger)
    : INotificationHandler<PlaylistUnlinkedFromCoverImageDomainEvent>
{
    private readonly IPlaylistReadService _playlistReadService = playlistReadService;
    private readonly IPlaylistsUnitOfWork _unit = unit;
    private readonly ILogger<PlaylistUnlinkedFromCoverImageDomainEventHandler> _logger = logger;

    public async Task Handle(
        PlaylistUnlinkedFromCoverImageDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent1 = new ImageLinkRemovedIntegrationEvent(
                notification.ImageId.Value);
        var message1 = OutboxMessage.FromIntegrationEvent(integrationEvent1);
        await _unit.OutboxMessages.AddAsync(message1, cancellationToken);

        PlaylistDetails? playlist = await _playlistReadService.GetDetailsAsync(
            notification.Id, cancellationToken);
        if (playlist is null)
        {
            _logger.LogError("Playlist with ID {PlaylistId} not found.", notification.Id.Value);
            throw new InvalidOperationException($"Playlist {notification.Id.Value} not found.");
        }

        var integrationEvent2 = new PlaylistCoverChangedIntegrationEvent(
                notification.Id.Value,
                null, playlist.GeneratedCoverImageIds);
        var message2 = OutboxMessage.FromIntegrationEvent(integrationEvent2);
        await _unit.OutboxMessages.AddAsync(message2, cancellationToken);

        await _unit.CommitAsync(cancellationToken);
    }
}
