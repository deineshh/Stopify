using SpotifyClone.Accounts.Application.Abstractions.Data;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Accounts.Application.Features.Accounts.Queries.List;

internal sealed class ListUsersQueryHandler(
    IUserReadService userReadService)
    : IQueryHandler<ListUsersQuery, UserList>
{
    private readonly IUserReadService _userReadService = userReadService;

    public async Task<Result<UserList>> Handle(
        ListUsersQuery request,
        CancellationToken cancellationToken)
    {
        PagedList<UserSummary> users = await _userReadService.ListAsync(
            request.Filters, request.Pagination, cancellationToken);

        return new UserList(users);
    }
}
