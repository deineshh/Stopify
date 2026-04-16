using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Search.Application.Models.Documents.Compacts;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

namespace SpotifyClone.Search.Application.EventHandlers.Tracks;

internal sealed class TrackGenresUpdatedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<TrackGenresUpdatedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        TrackGenresUpdatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        SearchResult<TrackSearchDocument> trackResult
            = await _searchProvider.SearchAsync<TrackSearchDocument>(
                SearchIndexNames.Tracks,
                $"id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (trackResult.TotalCount != 1)
        {
            return;
        }

        SearchResult<GenreSearchDocument> genresResult
            = await _searchProvider.SearchAsync<GenreSearchDocument>(
                SearchIndexNames.Artists,
                $"id IN [ {string.Join(", ", notification.Genres)} ]",
                cancellationToken: cancellationToken);

        TrackSearchDocument updatedTrack = trackResult.Items.Single() with
        {
            Genres = genresResult.Items.Select(g => new GenreCompactDocument(g.Id, g.Name)).ToArray()
        };

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Tracks, updatedTrack, cancellationToken);
    }
}
