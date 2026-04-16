using MediatR;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Search.Application.Models.Documents.Compacts;
using SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

namespace SpotifyClone.Search.Application.EventHandlers.Tracks;

internal sealed class TrackPublishedIntegrationEventHandler(
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider)
    : INotificationHandler<TrackPublishedIntegrationEvent>
{
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;

    public async Task Handle(
        TrackPublishedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        SearchResult<AlbumSearchDocument> albumSearchResult
            = await _searchProvider.SearchAsync<AlbumSearchDocument>(
                SearchIndexNames.Albums,
                $"id = {notification.AlbumId}",
                cancellationToken: cancellationToken);
        if (albumSearchResult.TotalCount > 1)
        {
            return;
        }

        SearchResult<ArtistSearchDocument> artistsSearchResult
            = await _searchProvider.SearchAsync<ArtistSearchDocument>(
                SearchIndexNames.Artists,
                $"id IN [{string.Join(", ", notification.MainArtists.Concat(notification.FeaturedArtists))}]",
                cancellationToken: cancellationToken);

        SearchResult<GenreSearchDocument> genresSearchResult
            = await _searchProvider.SearchAsync<GenreSearchDocument>(
                SearchIndexNames.Genres,
                $"id IN [{string.Join(", ", notification.Genres)}]",
                cancellationToken: cancellationToken);

        SearchResult<MoodSearchDocument> moodsSearchResult
            = await _searchProvider.SearchAsync<MoodSearchDocument>(
                SearchIndexNames.Moods,
                $"id IN [{string.Join(", ", notification.Moods)}]",
                cancellationToken: cancellationToken);

        var track = new TrackSearchDocument(
            notification.Id.ToString(),
            notification.Title,
            notification.IsExplicit,
            notification.ReleaseDateUtc.Year,
            notification.CoverImageId.ToString(),
            new AlbumCompactDocument(
                notification.AlbumId.ToString(),
                albumSearchResult.Items.Single().Title),
            artistsSearchResult.Items.Select(artist => new ArtistCompactDocument(
                artist.Id.ToString(),
                artist.Name)).ToArray(),
            genresSearchResult.Items.Select(genre => new GenreCompactDocument(
                genre.Id.ToString(),
                genre.Name)).ToArray(),
            moodsSearchResult.Items.Select(mood => new MoodCompactDocument(
                mood.Id.ToString(),
                mood.Name)).ToArray());

        await _searchIndexer.IndexDocumentAsync(
            SearchIndexNames.Tracks, track, cancellationToken);
    }
}
