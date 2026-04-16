using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Accounts.Users;

namespace SpotifyClone.Search.Application.EventHandlers.Users;

internal sealed class UserRegisteredIntegrationEventHandler(
    ISearchIndexer searchIndexer)
    : INotificationHandler<UserRegisteredIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;

    public async Task Handle(
        UserRegisteredIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        var user = new UserSearchDocument(
            notification.UserId.ToString(), notification.Name, null);

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Users, user, cancellationToken);
    }
}
