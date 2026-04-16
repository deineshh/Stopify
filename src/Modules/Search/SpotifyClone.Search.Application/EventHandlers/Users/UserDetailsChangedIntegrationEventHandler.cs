using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Accounts.Users;

namespace SpotifyClone.Search.Application.EventHandlers.Users;

internal sealed class UserDetailsChangedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<UserDetailsChangedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        UserDetailsChangedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        SearchResult<UserSearchDocument> userResult
            = await _searchProvider.SearchAsync<UserSearchDocument>(
                SearchIndexNames.Users,
                $"id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (userResult.TotalCount != 1)
        {
            return;
        }

        UserSearchDocument user = userResult.Items.First() with
        {
            Name = notification.Name
        };

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Users, user, cancellationToken);

        SearchResult<PlaylistSearchDocument> playlistsResult
            = await _searchProvider.SearchAsync<PlaylistSearchDocument>(
                SearchIndexNames.Playlists,
                $"owner.id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (playlistsResult.TotalCount <= 0)
        {
            return;
        }

        IEnumerable<PlaylistSearchDocument> updatedPlaylists = playlistsResult.Items
            .Select(playlist => playlist with
            {
                Owner = playlist.Owner with
                {
                    Name = notification.Name
                }
            });

        await _searchIndexer.IndexDocumentsAsync(
            SearchIndexNames.Playlists, updatedPlaylists, cancellationToken);
    }
}
