using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Moods;

namespace SpotifyClone.Search.Application.EventHandlers.Moods;

internal sealed class MoodNameChangedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<MoodNameChangedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        MoodNameChangedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        SearchResult<MoodSearchDocument> moodResult
            = await _searchProvider.SearchAsync<MoodSearchDocument>(
                SearchIndexNames.Moods,
                $"id = {notification.Id}",
                cancellationToken: cancellationToken);
        if (moodResult.TotalCount != 1)
        {
            return;
        }

        MoodSearchDocument mood = moodResult.Items.First() with
        {
            Name = notification.Name
        };

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Moods, mood, cancellationToken);
    }
}
