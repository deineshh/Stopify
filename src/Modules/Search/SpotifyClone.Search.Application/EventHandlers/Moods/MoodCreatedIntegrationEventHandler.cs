using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Moods;

namespace SpotifyClone.Search.Application.EventHandlers.Moods;

internal sealed class MoodCreatedIntegrationEventHandler(
    ISearchIndexer searchIndexer)
    : INotificationHandler<MoodCreatedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;

    public async Task Handle(
        MoodCreatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        var mood = new MoodSearchDocument(
            notification.Id.ToString(),
            notification.Name,
            null);

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Moods, mood, cancellationToken);
    }
}
