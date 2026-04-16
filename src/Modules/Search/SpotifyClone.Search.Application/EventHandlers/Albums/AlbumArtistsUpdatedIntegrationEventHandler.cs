using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Search.Application.Models.Documents.Compacts;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

namespace SpotifyClone.Search.Application.EventHandlers.Albums;

internal sealed class AlbumArtistsUpdatedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<AlbumArtistsUpdatedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        AlbumArtistsUpdatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        SearchResult<AlbumSearchDocument> albumResult
            = await _searchProvider.SearchAsync<AlbumSearchDocument>(
                SearchIndexNames.Albums,
                $"id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (albumResult.TotalCount != 1)
        {
            return;
        }

        string artistFilter = string.Join(" OR ", notification.Artists.Select(id => $"id = '{id}'"));
        SearchResult<ArtistSearchDocument> artistsResult
            = await _searchProvider.SearchAsync<ArtistSearchDocument>(
                SearchIndexNames.Artists,
                artistFilter,
                cancellationToken: cancellationToken);

        AlbumSearchDocument updatedAlbum = albumResult.Items.Single() with
        {
            Artists = artistsResult.Items.Select(
                artist => new ArtistCompactDocument(artist.Id, artist.Name)).ToArray()
        };

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Albums, updatedAlbum, cancellationToken);
    }
}
