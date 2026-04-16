using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Search.Application.Models.Documents.Compacts;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

namespace SpotifyClone.Search.Application.EventHandlers.Tracks;

internal sealed class TrackArtistsUpdatedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<TrackArtistsUpdatedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        TrackArtistsUpdatedIntegrationEvent notification,
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

        SearchResult<ArtistSearchDocument> artistsResult
            = await _searchProvider.SearchAsync<ArtistSearchDocument>(
                SearchIndexNames.Artists,
                $"id IN [ {string.Join(", ", notification.MainArtists.Concat(notification.FeaturedArtists))} ]",
                cancellationToken: cancellationToken);

        TrackSearchDocument updatedTrack = trackResult.Items.Single() with
        {
            Artists = artistsResult.Items.Select(a => new ArtistCompactDocument(a.Id, a.Name)).ToArray()
        };

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Tracks, updatedTrack, cancellationToken);
    }
}
