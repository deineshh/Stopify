using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Genres;

namespace SpotifyClone.Search.Application.EventHandlers.Genres;

internal sealed class GenreNameChangedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<GenreNameChangedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        GenreNameChangedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        SearchResult<GenreSearchDocument> genreResult
            = await _searchProvider.SearchAsync<GenreSearchDocument>(
                SearchIndexNames.Genres,
                $"id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (genreResult.TotalCount != 1)
        {
            return;
        }

        GenreSearchDocument genre = genreResult.Items.First() with
        {
            Name = notification.Name
        };

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Genres, genre, cancellationToken);
    }
}
