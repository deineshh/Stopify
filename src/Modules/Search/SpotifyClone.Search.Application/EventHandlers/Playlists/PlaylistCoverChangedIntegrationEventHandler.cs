using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Playlists;

namespace SpotifyClone.Search.Application.EventHandlers.Playlists;

internal sealed class PlaylistCoverChangedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<PlaylistCoverChangedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        PlaylistCoverChangedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        SearchResult<PlaylistSearchDocument> playlistResult
            = await _searchProvider.SearchAsync<PlaylistSearchDocument>(
                SearchIndexNames.Playlists,
                $"id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (playlistResult.TotalCount != 1)
        {
            return;
        }

        PlaylistSearchDocument updatedPlaylist = playlistResult.Items.Single() with
        {
            CustomCoverImageId = notification.CustomCoverImageId.ToString(),
            GeneratedCoverImageIds = notification.GeneratedCoverImageIds.Select(id => id.ToString()).ToArray()
        };

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Playlists, updatedPlaylist, cancellationToken);
    }
}
