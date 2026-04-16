using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Accounts.Users;

namespace SpotifyClone.Search.Application.EventHandlers.Users;

internal sealed class UserAvatarChangedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<UserAvatarChangedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        UserAvatarChangedIntegrationEvent notification,
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
            AvatarImageId = notification.AvatarImageId?.ToString()
        };

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Users, user, cancellationToken);
    }
}
