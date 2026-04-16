using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Search.Application.Models.Documents.Compacts;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Moods;

namespace SpotifyClone.Search.Application.EventHandlers.Moods;

internal sealed class MoodDeletedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<MoodDeletedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        MoodDeletedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _searchIndexer.DeleteDocumentAsync(
            SearchIndexNames.Moods, notification.Id.ToString(), cancellationToken);
        SearchResult<TrackSearchDocument> tracksResult
            = await _searchProvider.SearchAsync<TrackSearchDocument>(
                SearchIndexNames.Tracks,
                $"moods.id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (tracksResult.TotalCount <= 0)
        {
            return;
        }

        TrackSearchDocument[] updatedTracks = tracksResult.Items.Select(track =>
        {
            MoodCompactDocument[] updatedMoods = track.Moods
                .Where(mood => mood.Id != notification.Id.ToString())
                .ToArray();
            return track with { Moods = updatedMoods };
        }).ToArray();

        await _searchIndexer.IndexDocumentsAsync(
            SearchIndexNames.Tracks, updatedTracks, cancellationToken);
    }
}
