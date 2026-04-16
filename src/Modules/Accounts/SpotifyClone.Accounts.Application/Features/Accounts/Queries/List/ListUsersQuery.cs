using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Accounts.Application.Features.Accounts.Queries.List;

public sealed record ListUsersQuery(
    UserFilterParams Filters,
    PaginationParams Pagination)
    : IQuery<UserList>;
