using MediatR;
using Microsoft.Extensions.Logging;
using SpotifyClone.Playlists.Application.Abstractions;
using SpotifyClone.Playlists.Application.Abstractions.Data;
using SpotifyClone.Playlists.Application.Features.Playlists.Queries;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Playlists;

namespace SpotifyClone.Playlists.Application.EventHandlers.Playlists;

internal sealed class PlaylistDetailsEditedDomainEventHandler(
    IPlaylistsUnitOfWork unit,
    IPlaylistReadService playlistReadService,
    ILogger<PlaylistDetailsEditedDomainEventHandler> logger)
    : INotificationHandler<PlaylistDetailsEditedDomainEvent>
{
    private readonly IPlaylistsUnitOfWork _unit = unit;
    private readonly IPlaylistReadService _playlistReadService = playlistReadService;
    private readonly ILogger<PlaylistDetailsEditedDomainEventHandler> _logger = logger;

    public async Task Handle(
        PlaylistDetailsEditedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        PlaylistDetails? playlist = await _playlistReadService.GetDetailsAsync(
            notification.Id, cancellationToken);
        if (playlist is null)
        {
            _logger.LogWarning(
                "Playlist {PlaylistId} not found. Skipping integration event.",
                notification.Id.Value);
            throw new InvalidOperationException($"Playlist {notification.Id.Value} not found.");
        }

        var integrationEvent = new PlaylistDetailsChangedIntegrationEvent(
            notification.Id.Value,
            notification.Name,
            notification.IsPublic,
            notification.OwnerId.Value,
            notification.CoverImageId?.Value,
            playlist.GeneratedCoverImageIds);

        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);

        await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
