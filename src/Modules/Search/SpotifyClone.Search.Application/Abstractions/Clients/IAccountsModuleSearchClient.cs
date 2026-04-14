using SpotifyClone.Shared.Kernel.Contracts.Accounts;

namespace SpotifyClone.Search.Application.Abstractions.Clients;

public interface IAccountsModuleSearchClient
{
    Task<IEnumerable<UserSharedDto>> GetAllUsersAsync(
        CancellationToken cancellationToken = default);
}
