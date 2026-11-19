using FluentAssertions;
using SpotifyClone.Accounts.Domain.Aggregates.Users;
using SpotifyClone.Accounts.Domain.Aggregates.Users.Enums;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Accounts.Domain.Tests.Aggregates.User;

public sealed class UserProfileTests
{
    [Fact]
    public void Create_ShouldCreateUser_WhenValidParametersAreProvided()
    {
        // Arrange
        var userId = UserId.New();
        string displayName = "John Doe";
        var birthDate = new DateTimeOffset(new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        Gender gender = Gender.Male;

        // Act
        var user = UserProfile.Create(userId, displayName, birthDate, gender);

        // Assert
        user.Id.Should().Be(userId);
        user.DisplayName.Should().Be(displayName);
        user.BirthDateUtc.Should().Be(birthDate);
        user.Gender.Should().Be(gender);
    }
}
