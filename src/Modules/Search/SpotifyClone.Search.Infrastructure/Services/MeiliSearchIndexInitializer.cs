using Meilisearch;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpotifyClone.Search.Application.Models;

namespace SpotifyClone.Search.Infrastructure.Services;

internal sealed class MeiliSearchIndexInitializer(
    MeilisearchClient client,
    ILogger<MeiliSearchIndexInitializer> logger)
    : IHostedService
{
    private readonly MeilisearchClient _client = client;
    private readonly ILogger<MeiliSearchIndexInitializer> _logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Meilisearch index configuation starting...");

        await ConfigureUsersIndexAsync(cancellationToken);
        await ConfigureGenresIndexAsync(cancellationToken);
        await ConfigureMoodsIndexAsync(cancellationToken);
        await ConfigureArtistsIndexAsync(cancellationToken);
        await ConfigureAlbumsIndexAsync(cancellationToken);
        await ConfigureTracksIndexAsync(cancellationToken);
        await ConfigurePlaylistsIndexAsync(cancellationToken);

        _logger.LogInformation("Meilisearch index configuration succeeded.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task ConfigureUsersIndexAsync(CancellationToken cancellationToken)
    {
        await _client.CreateIndexAsync(SearchIndexNames.Users, "id", cancellationToken);
        Meilisearch.Index index = _client.Index(SearchIndexNames.Users);

        await index.UpdateSearchableAttributesAsync(
        [
            "name",
        ], cancellationToken);

        await index.UpdateFilterableAttributesAsync(
        [
            "id",
            "avatarImageId"
        ], cancellationToken);
    }

    private async Task ConfigureGenresIndexAsync(CancellationToken cancellationToken)
    {
        await _client.CreateIndexAsync(SearchIndexNames.Genres, "id", cancellationToken);
        Meilisearch.Index index = _client.Index(SearchIndexNames.Genres);

        await index.UpdateSearchableAttributesAsync(
        [
            "name"
        ], cancellationToken);

        await index.UpdateFilterableAttributesAsync(
        [
            "id",
            "coverImageId"
        ], cancellationToken);
    }

    private async Task ConfigureMoodsIndexAsync(CancellationToken cancellationToken)
    {
        await _client.CreateIndexAsync(SearchIndexNames.Moods, "id", cancellationToken);
        Meilisearch.Index index = _client.Index(SearchIndexNames.Moods);

        await index.UpdateSearchableAttributesAsync(
        [
            "name"
        ], cancellationToken);

        await index.UpdateFilterableAttributesAsync(
        [
            "id",
            "coverImageId"
        ], cancellationToken);
    }

    private async Task ConfigureArtistsIndexAsync(CancellationToken cancellationToken)
    {
        await _client.CreateIndexAsync(SearchIndexNames.Artists, "id", cancellationToken);
        Meilisearch.Index index = _client.Index(SearchIndexNames.Artists);

        await index.UpdateSearchableAttributesAsync(
        [
            "name"
        ], cancellationToken);

        await index.UpdateFilterableAttributesAsync(
        [
            "id",
            "isVerified",
            "avatarImageId"
        ], cancellationToken);
    }

    private async Task ConfigureAlbumsIndexAsync(CancellationToken cancellationToken)
    {
        await _client.CreateIndexAsync(SearchIndexNames.Albums, "id", cancellationToken);
        Meilisearch.Index index = _client.Index(SearchIndexNames.Albums);

        await index.UpdateSearchableAttributesAsync(
        [
            "title",
            "artists.name"
        ], cancellationToken);

        await index.UpdateFilterableAttributesAsync(
        [
            "id",
            "artists.id",
            "coverImageId"
        ], cancellationToken);

        await index.UpdateSortableAttributesAsync(
        [
            "releaseYear"
        ], cancellationToken);
    }

    private async Task ConfigureTracksIndexAsync(CancellationToken cancellationToken)
    {
        await _client.CreateIndexAsync(SearchIndexNames.Tracks, "id", cancellationToken);
        Meilisearch.Index index = _client.Index(SearchIndexNames.Tracks);

        await index.UpdateSearchableAttributesAsync(
        [
            "title",
            "artists.name",
            "album.title"
        ], cancellationToken);

        await index.UpdateFilterableAttributesAsync(
        [
            "id",
            "artists.id",
            "album.id",
            "coverImageId",
            "isExplicit"
        ], cancellationToken);

        await index.UpdateSortableAttributesAsync(
        [
            "releaseYear"
        ], cancellationToken);
    }

    private async Task ConfigurePlaylistsIndexAsync(CancellationToken cancellationToken)
    {
        await _client.CreateIndexAsync(SearchIndexNames.Playlists, "id", cancellationToken);
        Meilisearch.Index index = _client.Index(SearchIndexNames.Playlists);

        await index.UpdateSearchableAttributesAsync(
        [
            "name",
            "owner.name"
        ], cancellationToken);

        await index.UpdateFilterableAttributesAsync(
        [
            "id",
            "owner.id",
            "customCoverImageId"
        ], cancellationToken);
    }
}
