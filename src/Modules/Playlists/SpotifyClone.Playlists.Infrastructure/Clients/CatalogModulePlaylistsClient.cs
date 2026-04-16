using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SpotifyClone.Playlists.Application.Abstractions.Clients;
using SpotifyClone.Shared.Kernel.Contracts.Catalog;

namespace SpotifyClone.Playlists.Infrastructure.Clients;

internal sealed class CatalogModulePlaylistsClient(
    HttpClient httpClient,
    ILogger<CatalogModulePlaylistsClient> logger)
    : ICatalogModulePlaylistsClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<CatalogModulePlaylistsClient> _logger = logger;

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
