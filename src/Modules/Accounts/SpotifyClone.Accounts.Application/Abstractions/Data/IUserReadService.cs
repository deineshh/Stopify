using SpotifyClone.Accounts.Application.Features.Accounts.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Accounts.Application.Abstractions.Data;

public interface IUserReadService
{
    Task<UserProfileDetails?> GetProfileDetailsAsync(
        UserId id,
        CancellationToken cancellationToken = default);

    Task<CurrentUserDetails?> GetCurrentDetailsAsync(
        UserId id,
        CancellationToken cancellationToken = default);

    Task<PagedList<UserSummary>> ListAsync(
        UserFilterParams filters,
        PaginationParams pagination,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<UserSummary>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
