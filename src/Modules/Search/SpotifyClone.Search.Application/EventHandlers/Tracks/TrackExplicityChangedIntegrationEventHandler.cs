using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

namespace SpotifyClone.Search.Application.EventHandlers.Tracks;

internal sealed class TrackExplicityChangedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<TrackExplicityChangedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        TrackExplicityChangedIntegrationEvent notification,
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

        TrackSearchDocument updatedTrack = trackResult.Items.Single() with
        {
            IsExplicit = notification.IsExplicit
        };

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Tracks, updatedTrack, cancellationToken);
    }
}
