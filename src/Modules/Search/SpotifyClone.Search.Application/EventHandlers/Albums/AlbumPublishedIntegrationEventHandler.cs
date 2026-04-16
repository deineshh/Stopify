using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Search.Application.Models.Documents.Compacts;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

namespace SpotifyClone.Search.Application.EventHandlers.Albums;

internal sealed class AlbumPublishedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<AlbumPublishedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        AlbumPublishedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        string artistFilter = string.Join(" OR ", notification.Artists.Select(id => $"id = '{id}'"));
        SearchResult<ArtistSearchDocument> artistsResult
            = await _searchProvider.SearchAsync<ArtistSearchDocument>(
                SearchIndexNames.Artists,
                artistFilter,
                cancellationToken: cancellationToken);

        var album = new AlbumSearchDocument(
            notification.Id.ToString(),
            notification.Title,
            notification.ReleaseDate.Year,
            notification.Type,
            notification.CoverImageId.ToString(),
            artistsResult.Items.Select(a => new ArtistCompactDocument(a.Id.ToString(), a.Name)).ToArray());

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Albums, album, cancellationToken);
    }
}
