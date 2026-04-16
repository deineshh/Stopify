using Microsoft.AspNetCore.Identity;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;

namespace SpotifyClone.Accounts.Infrastructure.Persistence.Auth.Initialization;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles =
        {
            UserRoles.Listener,
            UserRoles.Creator,
            UserRoles.Admin
        };

        foreach (string role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
}
