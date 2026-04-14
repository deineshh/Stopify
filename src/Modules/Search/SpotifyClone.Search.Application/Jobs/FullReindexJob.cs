using Microsoft.Extensions.Logging;
using SpotifyClone.Search.Application.Abstractions.Clients;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Search.Application.Models.Documents.Compacts;
using SpotifyClone.Shared.Kernel.Contracts.Accounts;
using SpotifyClone.Shared.Kernel.Contracts.Catalog;

namespace SpotifyClone.Search.Application.Jobs;

public sealed class FullReindexJob(
    IAccountsModuleSearchClient accountsClient,
    ICatalogModuleSearchClient catalogClient,
    IPlaylistsModuleSearchClient playlistsClient,
    ISearchIndexer searchIndexer,
    ISearchProvider searchProvider,
    ILogger<FullReindexJob> logger)
{
    private readonly IAccountsModuleSearchClient _accountsClient = accountsClient;
    private readonly ICatalogModuleSearchClient _catalogClient = catalogClient;
    private readonly IPlaylistsModuleSearchClient _playlistsClient = playlistsClient;
    private readonly ISearchIndexer _searchIndexer = searchIndexer;
    private readonly ISearchProvider _searchProvider = searchProvider;
    private readonly ILogger<FullReindexJob> _logger = logger;

    public async Task ProcessAsync(
        CancellationToken cancellationToken = default)
    {
        await ReindexUsersAsync(cancellationToken);
        await ReindexGenresAsync(cancellationToken);
        await ReindexMoodsAsync(cancellationToken);
        await ReindexArtistsAsync(cancellationToken);
        await ReindexAlbumsAsync(cancellationToken);
        await ReindexTracksAsync(cancellationToken);
        await ReindexPlaylistsAsync(cancellationToken);
    }

    private async Task ReindexUsersAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Users reindexing...");

        IEnumerable<UserSharedDto> userDtos = await _accountsClient.GetAllUsersAsync(cancellationToken);

        IEnumerable<UserSearchDocument> users = userDtos
            .Select(u => new UserSearchDocument(
                u.Id.ToString(),
                u.Name,
                u.AvatarImageId.ToString()));

        await _searchIndexer.IndexDocumentsAsync(SearchIndexNames.Users, users, cancellationToken);

        _logger.LogInformation("Finished Users reindexing. Indexed {Count} users.", users.Count());
    }

    private async Task ReindexGenresAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Genres reindexing...");

        IEnumerable<GenreSharedDto> genreDtos = await _catalogClient.GetAllGenresAsync(cancellationToken);

        IEnumerable<GenreSearchDocument> genres = genreDtos
            .Select(g => new GenreSearchDocument(
                g.Id.ToString(),
                g.Name,
                g.CoverImageId.ToString()));

        await _searchIndexer.IndexDocumentsAsync(SearchIndexNames.Genres, genres, cancellationToken);

        _logger.LogInformation("Finished Genres reindexing. Indexed {Count} genres.", genres.Count());
    }

    private async Task ReindexMoodsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Moods reindexing...");

        IEnumerable<MoodSharedDto> moodDtos = await _catalogClient.GetAllMoodsAsync(cancellationToken);

        IEnumerable<MoodSearchDocument> moods = moodDtos
            .Select(m => new MoodSearchDocument(
                m.Id.ToString(),
                m.Name,
                m.CoverImageId.ToString()));

        await _searchIndexer.IndexDocumentsAsync(SearchIndexNames.Moods, moods, cancellationToken);

        _logger.LogInformation("Finished Moods reindexing. Indexed {Count} moods.", moods.Count());
    }

    private async Task ReindexArtistsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Artists reindexing...");

        IEnumerable<ArtistSharedDto> artistDtos = await _catalogClient.GetAllArtistsAsync(cancellationToken);

        IEnumerable<ArtistSearchDocument> artists = artistDtos
            .Where(a => !a.IsBanned)
            .Select(a => new ArtistSearchDocument(
                a.Id.ToString(),
                a.Name,
                a.IsVerified,
                a.AvatarImageId.ToString()));

        await _searchIndexer.IndexDocumentsAsync(SearchIndexNames.Artists, artists, cancellationToken);

        _logger.LogInformation("Finished Artists reindexing. Indexed {Count} artists.", artists.Count());
    }

    private async Task ReindexAlbumsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Albums reindexing...");

        var albumDtos = (await _catalogClient.GetAllAlbumsAsync(cancellationToken))
            .Where(a => a.ReleaseDateUtc is not null && a.IsPublished).ToList();

        if (albumDtos.Count <= 0)
        {
            return;
        }

        var uniqueArtistIds = albumDtos
            .SelectMany(a => a.ArtistIds)
            .Distinct()
            .Select(id => id.ToString())
            .ToList();

        SearchResult<ArtistSearchDocument> artistSearchResults
            = await _searchProvider.SearchAsync<ArtistSearchDocument>(
                SearchIndexNames.Artists,
                $"id IN [{string.Join(", ", uniqueArtistIds.Select(id => $"\"{id}\""))}]",
                cancellationToken: cancellationToken);

        var artistMap = artistSearchResults.Items
            .ToDictionary(a => Guid.Parse(a.Id), a => new ArtistCompactDocument(a.Id, a.Name));

        IEnumerable<AlbumSearchDocument> albums = albumDtos
            .Select(a =>
            {
                ArtistCompactDocument[] albumArtists = a.ArtistIds
                    .Where(id => artistMap.ContainsKey(id))
                    .Select(id => artistMap[id])
                    .ToArray();

                return new AlbumSearchDocument(
                    a.Id.ToString(),
                    a.Title,
                    a.ReleaseDateUtc!.Value.Year,
                    a.Type,
                    a.CoverImageId!.Value.ToString(),
                    albumArtists);
            });

        await _searchIndexer.IndexDocumentsAsync(SearchIndexNames.Albums, albums, cancellationToken);

        _logger.LogInformation("Finished Albums reindexing. Indexed {Count} albums.", albums.Count());
    }

    private async Task ReindexTracksAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Tracks reindexing...");

        var trackDtos = (await _catalogClient.GetAllTracksAsync(cancellationToken))
            .Where(t => t.ReleaseDateUtc is not null && t.CoverImageId is not null && t.AlbumId is not null)
            .ToList();
        if (trackDtos.Count <= 0)
        {
            return;
        }

        // Збираємо всі унікальні ID для пошуку в інших індексах
        var albumIds = trackDtos.Where(t => t.AlbumId.HasValue)
            .Select(t => t.AlbumId!.Value.ToString())
            .Distinct().ToList();
        var artistIds = trackDtos
            .SelectMany(t => t.MainArtistIds.Concat(t.FeaturedArtistIds))
            .Select(id => id.ToString())
            .Distinct().ToList();
        var genreIds = trackDtos
            .SelectMany(t => t.GenreIds)
            .Select(id => id.ToString())
            .Distinct().ToList();
        var moodIds = trackDtos
            .SelectMany(t => t.MoodIds)
            .Select(id => id.ToString())
            .Distinct().ToList();

        // Завантажуємо всі пов'язані дані паралельно (для швидкості)
        List<AlbumSearchDocument> albumsTask = await FetchFromIndexAsync<AlbumSearchDocument>(
            SearchIndexNames.Albums, albumIds, cancellationToken);
        List<ArtistSearchDocument> artistsTask = await FetchFromIndexAsync<ArtistSearchDocument>(
            SearchIndexNames.Artists, artistIds, cancellationToken);
        List<GenreSearchDocument> genresTask = await FetchFromIndexAsync<GenreSearchDocument>(
            SearchIndexNames.Genres, genreIds, cancellationToken);
        List<MoodSearchDocument> moodsTask = await FetchFromIndexAsync<MoodSearchDocument>(
            SearchIndexNames.Moods, moodIds, cancellationToken);

        // Створюємо словники для миттєвого мапінгу
        var albumMap = albumsTask.ToDictionary(a => Guid.Parse(a.Id));
        var artistMap = artistsTask.ToDictionary(a => Guid.Parse(a.Id));
        var genreMap = genresTask.ToDictionary(g => Guid.Parse(g.Id));
        var moodMap = moodsTask.ToDictionary(m => Guid.Parse(m.Id));

        // Мапимо DTO на Search Documents
        IEnumerable<TrackSearchDocument> tracks = trackDtos
            .Select(t =>
            {
                AlbumCompactDocument? album = t.AlbumId.HasValue && albumMap.TryGetValue(
                    t.AlbumId.Value, out AlbumSearchDocument? a)
                ? new AlbumCompactDocument(a.Id, a.Title)
                : null;

                ArtistCompactDocument[] artists = t.MainArtistIds.Concat(t.FeaturedArtistIds)
                    .Where(id => artistMap.ContainsKey(id))
                    .Select(id => new ArtistCompactDocument(id.ToString(), artistMap[id].Name))
                    .ToArray();

                GenreCompactDocument[] genres = t.GenreIds
                    .Where(id => genreMap.ContainsKey(id))
                    .Select(id => new GenreCompactDocument(id.ToString(), genreMap[id].Name))
                    .ToArray();

                MoodCompactDocument[] moods = t.MoodIds
                    .Where(id => moodMap.ContainsKey(id))
                    .Select(id => new MoodCompactDocument(id.ToString(), moodMap[id].Name))
                    .ToArray();

                return new TrackSearchDocument(
                    t.Id.ToString(),
                    t.Title,
                    t.IsExplicit,
                    t.ReleaseDateUtc!.Value.Year,
                    t.CoverImageId?.ToString() ?? string.Empty,
                    album!, artists, genres, moods
                );
            });

        await _searchIndexer.IndexDocumentsAsync(SearchIndexNames.Tracks, tracks, cancellationToken);

        _logger.LogInformation("Finished Tracks reindexing. Indexed {Count} tracks.", trackDtos.Count);
    }

    private async Task ReindexPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Playlists reindexing...");

        var playlistDtos = (await _playlistsClient.GetAllPlaylistsAsync(cancellationToken))
            .Where(p => p.IsPublic)
            .ToList();

        if (playlistDtos.Count <= 0)
        {
            return;
        }

        // Збираємо унікальні ID власників плейлистів
        var ownerIds = playlistDtos
            .Select(p => p.OwnerId)
            .Distinct()
            .ToList();

        // Отримуємо дані про користувачів (власників) 
        List<UserSearchDocument> users = await FetchFromIndexAsync<UserSearchDocument>(
            SearchIndexNames.Users, ownerIds.Select(id => id.ToString()), cancellationToken);
        var userMap = users.ToDictionary(u => u.Id);

        // Мапимо DTO на документи пошуку
        var playlists = playlistDtos.Select(p =>
        {
            userMap.TryGetValue(p.OwnerId.ToString(), out UserSearchDocument? user);

            var ownerDocument = new UserCompactDocument(
                p.OwnerId.ToString(),
                user?.Name ?? "Unknown User"
            );

            return new PlaylistSearchDocument(
                p.Id.ToString(),
                p.Name,
                ownerDocument,
                p.CustomCoverImageId?.ToString(),
                p.GeneratedCoverImageIds?.Select(id => id.ToString()).ToArray() ?? []
            );
        }).ToList();

        // 5. Відправляємо в Meilisearch
        await _searchIndexer.IndexDocumentsAsync(SearchIndexNames.Playlists, playlists, cancellationToken);

        _logger.LogInformation("Finished Playlists reindexing. Indexed {Count} playlists.", playlists.Count);
    }

    private async Task<List<T>> FetchFromIndexAsync<T>(
        string indexName,
        IEnumerable<string> ids,
        CancellationToken cancellationToken)
    {
        if (!ids.Any())
        {
            return new List<T>();
        }

        SearchResult<T> results = await _searchProvider.SearchAsync<T>(
            indexName,
            $"id IN [{string.Join(", ", ids.Select(id => $"\"{id}\""))}]",
            cancellationToken: cancellationToken);

        return results.Items.ToList();
    }
}
