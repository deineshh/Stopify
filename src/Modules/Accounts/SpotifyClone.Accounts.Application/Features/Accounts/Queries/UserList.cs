using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Accounts.Application.Features.Accounts.Queries;

public sealed record UserList(
    PagedList<UserSummary> Users);
