using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Artists;

namespace SpotifyClone.Search.Application.EventHandlers.Artists;

internal sealed class ArtistCreatedIntegrationEventHandler(
    ISearchIndexer searchIndexer)
    : INotificationHandler<ArtistCreatedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;

    public async Task Handle(
        ArtistCreatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        var artist = new ArtistSearchDocument(
            notification.Id.ToString(),
            notification.Name,
            notification.IsVerified,
            null);

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Artists, artist, cancellationToken);
    }
}
