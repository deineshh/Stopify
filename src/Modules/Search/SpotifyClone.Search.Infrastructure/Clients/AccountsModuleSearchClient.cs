using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SpotifyClone.Search.Application.Abstractions.Clients;
using SpotifyClone.Shared.Kernel.Contracts.Accounts;

namespace SpotifyClone.Search.Infrastructure.Clients;

internal sealed class AccountsModuleSearchClient(
    HttpClient httpClient,
    ILogger<AccountsModuleSearchClient> logger)
    : IAccountsModuleSearchClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<AccountsModuleSearchClient> _logger = logger;

    public async Task<IEnumerable<UserSharedDto>> GetAllUsersAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<UserSharedDto>? response
                = await _httpClient.GetFromJsonAsync<IEnumerable<UserSharedDto>>(
                    "api/v1/shared/users", cancellationToken);

            return response ?? Enumerable.Empty<UserSharedDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting Users from Accounts module");
            return Enumerable.Empty<UserSharedDto>();
        }
    }
}
