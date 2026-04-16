using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

namespace SpotifyClone.Search.Application.EventHandlers.Albums;

internal sealed class AlbumTitleChangedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<AlbumTitleChangedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        AlbumTitleChangedIntegrationEvent notification,
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
            Title = notification.Title
        };

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Albums, updatedAlbum, cancellationToken);

        SearchResult<TrackSearchDocument> tracksResult
            = await _searchProvider.SearchAsync<TrackSearchDocument>(
                SearchIndexNames.Tracks,
                $"album.id = {notification.Id}",
                cancellationToken: cancellationToken);

        TrackSearchDocument[] updatedTracks = tracksResult.Items.Select(track => track with
        {
            Album = track.Album with
            {
                Title = notification.Title
            }
        }).ToArray();

        await _searchIndexer.IndexDocumentsAsync(
            SearchIndexNames.Tracks, updatedTracks, cancellationToken);
    }
}
