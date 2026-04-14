using SpotifyClone.Shared.Kernel.Contracts.Playlists;

namespace SpotifyClone.Search.Application.Abstractions.Clients;

public interface IPlaylistsModuleSearchClient
{
    Task<IEnumerable<PlaylistSharedDto>> GetAllPlaylistsAsync(
        CancellationToken cancellationToken = default);
}
