using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Genres;

namespace SpotifyClone.Search.Application.EventHandlers.Genres;

internal sealed class GenreCreatedIntegrationEventHandler(
    ISearchIndexer searchIndexer)
    : INotificationHandler<GenreCreatedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;

    public async Task Handle(
        GenreCreatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        var genre = new GenreSearchDocument(
            notification.Id.ToString(),
            notification.Name,
            null);

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Genres, genre, cancellationToken);
    }
}
