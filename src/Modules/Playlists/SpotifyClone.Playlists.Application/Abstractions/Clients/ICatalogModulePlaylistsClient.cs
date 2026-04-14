using SpotifyClone.Shared.Kernel.Contracts.Catalog;

namespace SpotifyClone.Playlists.Application.Abstractions.Clients;

public interface ICatalogModulePlaylistsClient
{
    Task<IEnumerable<TrackSharedDto>> GetAllTracksAsync(
        CancellationToken cancellationToken = default);
}
