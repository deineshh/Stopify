using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

namespace SpotifyClone.Search.Application.EventHandlers.Albums;

internal sealed class AlbumLinkedToCoverImageIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<AlbumLinkedToCoverImageIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        AlbumLinkedToCoverImageIntegrationEvent notification,
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
            CoverImageId = notification.CoverImageId.ToString()
        };

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Albums, updatedAlbum, cancellationToken);

        SearchResult<TrackSearchDocument> tracksResult
            = await _searchProvider.SearchAsync<TrackSearchDocument>(
                SearchIndexNames.Tracks,
                $"album.id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (tracksResult.TotalCount <= 0)
        {
            return;
        }

        IEnumerable<TrackSearchDocument> updatedTracks = tracksResult.Items.Select(
            track => track with
            {
                CoverImageId = notification.CoverImageId.ToString()
            });

        await _searchIndexer.IndexDocumentsAsync(
            SearchIndexNames.Tracks, updatedTracks, cancellationToken);
    }
}
