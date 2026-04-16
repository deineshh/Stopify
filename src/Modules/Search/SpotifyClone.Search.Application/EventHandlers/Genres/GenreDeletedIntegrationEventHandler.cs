using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Search.Application.Models.Documents.Compacts;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Genres;

namespace SpotifyClone.Search.Application.EventHandlers.Genres;

internal sealed class GenreDeletedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<GenreDeletedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        GenreDeletedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _searchIndexer.DeleteDocumentAsync(
            SearchIndexNames.Genres, notification.Id.ToString(), cancellationToken);

        SearchResult<TrackSearchDocument> tracksResult
            = await _searchProvider.SearchAsync<TrackSearchDocument>(
                SearchIndexNames.Tracks,
                $"genres.id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (tracksResult.TotalCount <= 0)
        {
            return;
        }

        TrackSearchDocument[] updatedTracks = tracksResult.Items.Select(track =>
        {
            GenreCompactDocument[] updatedGenres = track.Genres
                .Where(genre => genre.Id != notification.Id.ToString())
                .ToArray();
            return track with { Genres = updatedGenres };
        }).ToArray();

        await _searchIndexer.IndexDocumentsAsync(
            SearchIndexNames.Tracks, updatedTracks, cancellationToken);
    }
}
