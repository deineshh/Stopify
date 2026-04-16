using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

namespace SpotifyClone.Search.Application.EventHandlers.Albums;

internal sealed class AlbumReleaseDateChangedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<AlbumReleaseDateChangedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        AlbumReleaseDateChangedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        SearchResult<AlbumSearchDocument> albumsResult
            = await _searchProvider.SearchAsync<AlbumSearchDocument>(
                SearchIndexNames.Albums,
                $"id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (albumsResult.TotalCount != 1)
        {
            return;
        }

        AlbumSearchDocument updatedAlbum = albumsResult.Items.Single() with
        {
            ReleaseYear = notification.ReleaseDateUtc.Year,
        };

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Albums, updatedAlbum, cancellationToken);
    }
}
