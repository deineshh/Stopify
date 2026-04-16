using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Shared.IntegrationEvents.Playlists;

namespace SpotifyClone.Search.Application.EventHandlers.Playlists;

internal sealed class PlaylistDeletedIntegrationEventHandler(
    ISearchIndexer searchIndexer)
    : INotificationHandler<PlaylistDeletedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;

    public async Task Handle(
        PlaylistDeletedIntegrationEvent notification,
        CancellationToken cancellationToken)
        => await _searchIndexer.DeleteDocumentAsync(
            SearchIndexNames.Playlists, notification.Id.ToString(), cancellationToken);
}
