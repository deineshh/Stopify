using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Search.Application.Models.Documents.Compacts;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

namespace SpotifyClone.Search.Application.EventHandlers.Tracks;

internal sealed class TrackMoodsUpdatedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<TrackMoodsUpdatedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        TrackMoodsUpdatedIntegrationEvent notification,
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

        SearchResult<MoodSearchDocument> moodsResult
            = await _searchProvider.SearchAsync<MoodSearchDocument>(
                SearchIndexNames.Moods,
                $"id IN [ {string.Join(", ", notification.Moods)} ]",
                cancellationToken: cancellationToken);

        TrackSearchDocument updatedTrack = trackResult.Items.Single() with
        {
            Moods = moodsResult.Items.Select(m => new MoodCompactDocument(m.Id, m.Name)).ToArray()
        };

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Tracks, updatedTrack, cancellationToken);
    }
}
