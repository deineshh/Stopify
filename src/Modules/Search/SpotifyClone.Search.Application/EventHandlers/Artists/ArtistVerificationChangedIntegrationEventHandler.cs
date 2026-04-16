using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Artists;

namespace SpotifyClone.Search.Application.EventHandlers.Artists;

internal sealed class ArtistVerificationChangedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<ArtistVerificationChangedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        ArtistVerificationChangedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        SearchResult<ArtistSearchDocument> artistResult
            = await _searchProvider.SearchAsync<ArtistSearchDocument>(
                SearchIndexNames.Artists,
                $"id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (artistResult.TotalCount != 1)
        {
            return;
        }

        ArtistSearchDocument artist = artistResult.Items.First() with
        {
            IsVerified = notification.IsVerified
        };

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Artists, artist, cancellationToken);
    }
}
