using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Search.Application.Models.Documents.Compacts;
using SpotifyClone.Shared.IntegrationEvents.Playlists;

namespace SpotifyClone.Search.Application.EventHandlers.Playlists;

internal sealed class PlaylistDetailsChangedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<PlaylistDetailsChangedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        PlaylistDetailsChangedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        SearchResult<PlaylistSearchDocument> playlistResult
            = await _searchProvider.SearchAsync<PlaylistSearchDocument>(
                SearchIndexNames.Playlists,
                $"id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (playlistResult.TotalCount > 1)
        {
            return;
        }

        if (!notification.IsPublic)
        {
            await _searchIndexer.DeleteDocumentAsync(
                SearchIndexNames.Playlists, notification.Id.ToString(), cancellationToken);
            return;
        }

        PlaylistSearchDocument playlist;
        if (playlistResult.TotalCount <= 0)
        {
            SearchResult<UserSearchDocument> ownerResult
                = await _searchProvider.SearchAsync<UserSearchDocument>(
                    SearchIndexNames.Users,
                    $"id = {notification.OwnerId}",
                    cancellationToken: cancellationToken);
            if (ownerResult.TotalCount != 1)
            {
                return;
            }

            playlist = new PlaylistSearchDocument(
                notification.Id.ToString(),
                notification.Name,
                new UserCompactDocument(
                    notification.OwnerId.ToString(),
                    ownerResult.Items.Single().Name),
                notification.CustomCoverImageId.ToString(),
                notification.GeneratedCoverImageIds.Select(x => x.ToString()).ToArray());
        }
        else
        {
            playlist = playlistResult.Items.Single() with
            {
                Name = notification.Name
            };
        }

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Playlists, playlist, cancellationToken);
    }
}
