using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

namespace SpotifyClone.Search.Application.EventHandlers.Albums;

internal sealed class AlbumUnpublishedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<AlbumUnpublishedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        AlbumUnpublishedIntegrationEvent notification,
        CancellationToken cancellationToken)
        => await _searchIndexer.DeleteDocumentAsync(
            SearchIndexNames.Albums, notification.Id.ToString(), cancellationToken);
}
