using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Artists;

namespace SpotifyClone.Search.Application.EventHandlers.Artists;

internal sealed class ArtistBannedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<ArtistBannedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        ArtistBannedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _searchIndexer.DeleteDocumentAsync(
            SearchIndexNames.Artists, notification.Id.ToString(), cancellationToken);

        SearchResult<AlbumSearchDocument> albumsResult
            = await _searchProvider.SearchAsync<AlbumSearchDocument>(
                SearchIndexNames.Albums,
                $"artists.id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (albumsResult.TotalCount > 0)
        {
            await _searchIndexer.DeleteDocumentsAsync(
                SearchIndexNames.Albums, albumsResult.Items.Select(a => a.Id), cancellationToken);
        }

        SearchResult<TrackSearchDocument> tracksResult
            = await _searchProvider.SearchAsync<TrackSearchDocument>(
                SearchIndexNames.Tracks,
                $"artists.id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (tracksResult.TotalCount > 0)
        {
            await _searchIndexer.DeleteDocumentsAsync(
                SearchIndexNames.Tracks, tracksResult.Items.Select(t => t.Id), cancellationToken);
        }
    }
}
