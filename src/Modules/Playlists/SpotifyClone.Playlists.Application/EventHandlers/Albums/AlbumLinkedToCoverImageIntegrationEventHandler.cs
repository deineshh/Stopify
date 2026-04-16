using MediatR;
using Microsoft.Extensions.Logging;
using SpotifyClone.Playlists.Application.Abstractions;
using SpotifyClone.Playlists.Application.Abstractions.Data;
using SpotifyClone.Playlists.Application.Features.Playlists.Queries;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;
using SpotifyClone.Shared.IntegrationEvents.Playlists;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Playlists.Application.EventHandlers.Albums;

internal sealed class AlbumLinkedToCoverImageIntegrationEventHandler(
    IPlaylistsUnitOfWork unit,
    IPlaylistReadService playlistReadService,
    ILogger<AlbumLinkedToCoverImageIntegrationEventHandler> logger)
    : INotificationHandler<AlbumLinkedToCoverImageIntegrationEvent>
{
    private readonly IPlaylistsUnitOfWork _unit = unit;
    private readonly IPlaylistReadService _playlistReadService = playlistReadService;
    private readonly ILogger<AlbumLinkedToCoverImageIntegrationEventHandler> _logger = logger;

    public async Task Handle(
        AlbumLinkedToCoverImageIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        if (!await _unit.TrackReferences.ExistsAsync(notification.Tracks, cancellationToken))
        {
            _logger.LogError("Track was not found in the Playlists module.");
            throw new InvalidOperationException($"Track was not found in the Playlists module.");
        }

        foreach (Guid trackId in notification.Tracks)
        {
            await _unit.TrackReferences.LinkCoverAsync(trackId, notification.CoverImageId, cancellationToken);
        }

        IEnumerable<PlaylistSummary> playlists = await _playlistReadService.GetAllByTracksAsync(
            notification.Tracks.Select(t => TrackId.From(t)), cancellationToken);
        foreach (PlaylistSummary playlist in playlists)
        {
            var integrationEvent = new PlaylistGeneratedCoverUpdatedIntegrationEvent(
                notification.Id,
                playlist.GeneratedCoverImageIds);
            var message = OutboxMessage.FromIntegrationEvent(integrationEvent);
            await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        }

        await _unit.CommitAsync(cancellationToken);
    }
}
