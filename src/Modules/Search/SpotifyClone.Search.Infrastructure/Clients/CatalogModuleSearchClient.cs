using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SpotifyClone.Search.Application.Abstractions.Clients;
using SpotifyClone.Shared.Kernel.Contracts.Catalog;

namespace SpotifyClone.Search.Infrastructure.Clients;

internal sealed class CatalogModuleSearchClient(
    HttpClient httpClient,
    ILogger<CatalogModuleSearchClient> logger)
    : ICatalogModuleSearchClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<CatalogModuleSearchClient> _logger = logger;

    public async Task<IEnumerable<GenreSharedDto>> GetAllGenresAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<GenreSharedDto>? response
                = await _httpClient.GetFromJsonAsync<IEnumerable<GenreSharedDto>>(
                    "api/v1/shared/genres", cancellationToken);

            return response ?? Enumerable.Empty<GenreSharedDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting Genres from Catalog module");
            return Enumerable.Empty<GenreSharedDto>();
        }
    }

    public async Task<IEnumerable<MoodSharedDto>> GetAllMoodsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<MoodSharedDto>? response
                = await _httpClient.GetFromJsonAsync<IEnumerable<MoodSharedDto>>(
                    "api/v1/shared/moods", cancellationToken);

            return response ?? Enumerable.Empty<MoodSharedDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting Moods from Catalog module");
            return Enumerable.Empty<MoodSharedDto>();
        }
    }

    public async Task<IEnumerable<ArtistSharedDto>> GetAllArtistsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<ArtistSharedDto>? response
                = await _httpClient.GetFromJsonAsync<IEnumerable<ArtistSharedDto>>(
                    "api/v1/shared/artists", cancellationToken);

            return response ?? Enumerable.Empty<ArtistSharedDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting Artists from Catalog module");
            return Enumerable.Empty<ArtistSharedDto>();
        }
    }

    public async Task<IEnumerable<AlbumSharedDto>> GetAllAlbumsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<AlbumSharedDto>? response
                = await _httpClient.GetFromJsonAsync<IEnumerable<AlbumSharedDto>>(
                    "api/v1/shared/albums", cancellationToken);

            return response ?? Enumerable.Empty<AlbumSharedDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting Albums from Catalog module");
            return Enumerable.Empty<AlbumSharedDto>();
        }
    }

    public async Task<IEnumerable<TrackSharedDto>> GetAllTracksAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<TrackSharedDto>? response
                = await _httpClient.GetFromJsonAsync<IEnumerable<TrackSharedDto>>(
                    "api/v1/shared/tracks", cancellationToken);

            return response ?? Enumerable.Empty<TrackSharedDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting Tracks from Catalog module");
            return Enumerable.Empty<TrackSharedDto>();
        }
    }
}
