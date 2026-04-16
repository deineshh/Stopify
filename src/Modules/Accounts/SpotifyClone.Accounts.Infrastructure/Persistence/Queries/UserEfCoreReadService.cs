using Microsoft.EntityFrameworkCore;
using SpotifyClone.Accounts.Application.Abstractions.Data;
using SpotifyClone.Accounts.Application.Features.Accounts.Queries;
using SpotifyClone.Accounts.Application.Models;
using SpotifyClone.Accounts.Domain.Aggregates.Users;
using SpotifyClone.Accounts.Domain.Aggregates.Users.Enums;
using SpotifyClone.Accounts.Infrastructure.Persistence.Accounts.Database;
using SpotifyClone.Accounts.Infrastructure.Persistence.Identity.Database;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Infrastructure.Persistence.Extensions;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Accounts.Infrastructure.Persistence.Queries;

internal sealed class UserEfCoreReadService(
    IdentityAppDbContext identityContext,
    AccountsAppDbContext accountsContext)
    : IUserReadService
{
    private readonly IdentityAppDbContext _identityContext = identityContext;
    private readonly AccountsAppDbContext _accountsContext = accountsContext;

    public async Task<CurrentUserDetails?> GetCurrentDetailsAsync(
        UserId id,
        CancellationToken cancellationToken = default)
    {
        var identityUserInfo = await _identityContext.Users
            .Where(u => u.Id == id.Value)
            .Select(u => new
            {
                u.UserName,
                u.Email,
                u.PhoneNumber,
                u.EmailConfirmed,
                u.PhoneNumberConfirmed,
                u.TwoFactorEnabled,
                Role = _identityContext.UserRoles
                .Where(ur => ur.UserId == u.Id)
                .Join(_identityContext.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Name)
                .OrderBy(roleName => roleName == UserRoles.Admin ? 0 :
                                     roleName == UserRoles.Creator ? 1 : 2)
                .FirstOrDefault() ?? UserRoles.Listener
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (identityUserInfo is null)
        {
            return null;
        }

        var userProfileInfo = await _accountsContext.UserProfiles
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.DisplayName,
                Gender = u.Gender.Value,
                u.BirthDateUtc,
                Avatar = u.Avatar == null ? null : new ImageMetadataDetails(
                    u.Avatar.ImageId.Value,
                    u.Avatar.Metadata.Width,
                    u.Avatar.Metadata.Height,
                    u.Avatar.Metadata.FileType.Value,
                    u.Avatar.Metadata.SizeInBytes)
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (userProfileInfo is null)
        {
            return null;
        }

        return new CurrentUserDetails(
            id.Value,
            identityUserInfo.UserName!,
            identityUserInfo.Email!,
            identityUserInfo.PhoneNumber,
            identityUserInfo.EmailConfirmed,
            identityUserInfo.PhoneNumberConfirmed,
            identityUserInfo.TwoFactorEnabled,
            identityUserInfo.Role,
            userProfileInfo.DisplayName,
            userProfileInfo.Gender,
            userProfileInfo.BirthDateUtc,
            userProfileInfo.Avatar);
    }

    public async Task<UserProfileDetails?> GetProfileDetailsAsync(
        UserId id,
        CancellationToken cancellationToken = default)
    {
        var identityUserInfo = await _identityContext.Users
            .Where(u => u.Id == id.Value)
            .Select(u => new
            {
                Role = _identityContext.UserRoles
                .Where(ur => ur.UserId == u.Id)
                .Join(_identityContext.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Name)
                .OrderBy(roleName => roleName == UserRoles.Admin ? 0 :
                                     roleName == UserRoles.Creator ? 1 : 2)
                .FirstOrDefault() ?? UserRoles.Listener
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (identityUserInfo is null)
        {
            return null;
        }

        var userProfileInfo = await _accountsContext.UserProfiles
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.DisplayName,
                Avatar = u.Avatar == null ? null : new ImageMetadataDetails(
                    u.Avatar.ImageId.Value,
                    u.Avatar.Metadata.Width,
                    u.Avatar.Metadata.Height,
                    u.Avatar.Metadata.FileType.Value,
                    u.Avatar.Metadata.SizeInBytes)
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (userProfileInfo is null)
        {
            return null;
        }

        return new UserProfileDetails(
            id.Value,
            userProfileInfo.DisplayName,
            identityUserInfo.Role,
            userProfileInfo.Avatar);
    }

    public async Task<PagedList<UserSummary>> ListAsync(
        UserFilterParams filters,
        PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        IQueryable<UserProfile> query = _accountsContext.UserProfiles.AsNoTracking();

        if (filters.DisplayName is not null)
        {
            query = query.Where(u => EF.Functions.ILike(u.DisplayName, filters.DisplayName));
        }
        if (filters.Gender is not null)
        {
            var gender = Gender.From(filters.Gender);
            query = query.Where(u => u.Gender == gender);
        }

        return await query
            .OrderBy(u => u.CreatedAtUtc)
            .Select(u => new UserSummary(
                u.Id.Value,
                u.DisplayName,
                u.Avatar == null ? null : new ImageMetadataDetails(
                    u.Avatar.ImageId.Value,
                    u.Avatar.Metadata.Width,
                    u.Avatar.Metadata.Height,
                    u.Avatar.Metadata.FileType.Value,
                    u.Avatar.Metadata.SizeInBytes)))
            .ToPagedListAsync(pagination, cancellationToken);
    }

    public async Task<IEnumerable<UserSummary>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => await _accountsContext.UserProfiles
            .AsNoTracking()
            .OrderBy(u => u.CreatedAtUtc)
            .Select(u => new UserSummary(
                u.Id.Value,
                u.DisplayName,
                u.Avatar == null ? null : new ImageMetadataDetails(
                    u.Avatar.ImageId.Value,
                    u.Avatar.Metadata.Width,
                    u.Avatar.Metadata.Height,
                    u.Avatar.Metadata.FileType.Value,
                    u.Avatar.Metadata.SizeInBytes)))
            .ToListAsync(cancellationToken);
}
