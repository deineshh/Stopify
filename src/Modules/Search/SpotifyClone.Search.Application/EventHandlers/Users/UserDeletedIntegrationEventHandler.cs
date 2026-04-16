using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Accounts.Users;

namespace SpotifyClone.Search.Application.EventHandlers.Users;

internal sealed class UserDeletedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        UserDeletedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _searchIndexer.DeleteDocumentAsync(
            SearchIndexNames.Users, notification.Id.ToString(), cancellationToken);

        SearchResult<PlaylistSearchDocument> playlistsResult
            = await _searchProvider.SearchAsync<PlaylistSearchDocument>(
                SearchIndexNames.Playlists,
                $"owner.id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (playlistsResult.TotalCount <= 0)
        {
            return;
        }

        await _searchIndexer.DeleteDocumentsAsync(
            SearchIndexNames.Playlists,
            playlistsResult.Items.Select(p => p.Id),
            cancellationToken);
    }
}
