using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

namespace SpotifyClone.Search.Application.EventHandlers.Tracks;

internal sealed class TrackArchivedIntegrationEventHandler(
    ISearchIndexer searchIndexer)
    : INotificationHandler<TrackArchivedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;

    public async Task Handle(
        TrackArchivedIntegrationEvent notification,
        CancellationToken cancellationToken)
        => await _searchIndexer.DeleteDocumentAsync(
            SearchIndexNames.Tracks, notification.Id.ToString(), cancellationToken);
}
