using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Artists;

namespace SpotifyClone.Search.Application.EventHandlers.Artists;

internal sealed class ArtistUnbannedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<ArtistUnbannedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        ArtistUnbannedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        var artist = new ArtistSearchDocument(
            notification.Id.ToString(),
            notification.Name,
            notification.IsVerified,
            notification.AvatarImageId.ToString());

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Artists, artist, cancellationToken);
    }
}
