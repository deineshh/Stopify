using MediatR;
using Microsoft.AspNetCore.Identity;
using SpotifyClone.Accounts.Application.Features.Auth.Commands.RegisterUser;
using SpotifyClone.Accounts.Infrastructure.Persistence.Identity;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;

namespace SpotifyClone.Accounts.Infrastructure.Persistence.Accounts.Initialization;

public static class AccountsSeeder
{
    public static async Task SeedAccountsAsync(
        UserManager<ApplicationUser> userManager,
        ISender sender)
    {
        const string adminEmail = "stopify@gmail.com";
        ApplicationUser? existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is null)
        {
            await sender.Send(new RegisterUserCommand(
                Email: adminEmail,
                Password: "Admin_Password123",
                DisplayName: "Stopify",
                BirthDate: new DateTimeOffset(2007, 03, 04, 2, 0, 0, TimeSpan.Zero),
                Gender: "male",
                Role: UserRoles.Admin));
        }

        const string creator1Email = "stopifycreator@gmail.com";
        ApplicationUser? existingCreator1 = await userManager.FindByEmailAsync(creator1Email);
        if (existingCreator1 is null)
        {
            await sender.Send(new RegisterUserCommand(
                Email: creator1Email,
                Password: "Creator_Password123",
                DisplayName: "The Creator",
                BirthDate: new DateTimeOffset(new DateTime(2007, 03, 04, 2, 0, 0, DateTimeKind.Local)),
                Gender: "male",
                Role: UserRoles.Creator));
        }

        const string creator2Email = "stopifycreator2@gmail.com";
        ApplicationUser? existingCreator2 = await userManager.FindByEmailAsync(creator2Email);
        if (existingCreator2 is null)
        {
            await sender.Send(new RegisterUserCommand(
                Email: creator2Email,
                Password: "Creator_Password123",
                DisplayName: "The Creator 2",
                BirthDate: new DateTimeOffset(new DateTime(2007, 03, 04, 2, 0, 0, DateTimeKind.Local)),
                Gender: "male",
                Role: UserRoles.Creator));
        }
    }
}
