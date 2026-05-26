using FluentAssertions;
using SpotifyClone.Accounts.Domain.Aggregates.Users.Exceptions;
using SpotifyClone.Accounts.Domain.Aggregates.Users.Rules;

namespace SpotifyClone.Accounts.Domain.Tests.Aggregates.User.Rules;

public sealed class BirthDateRulesTests
{
    [Fact]
    public void Validate_Should_ThrowException_When_BirthDateIsInTheFuture()
    {
        // Arrange
        DateTimeOffset futureDate = DateTimeOffset.UtcNow.AddDays(1);

        // Act
        Action result = () => BirthDateRules.Validate(futureDate);

        // Assert
        result.Should().Throw<InvalidBirthDateDomainException>()
            .WithMessage("Birth date cannot be in the future.");
    }

    [Fact]
    public void Validate_Should_ThrowException_When_UserIsYoungerThanMinimumAge()
    {
        // Arrange
        DateTimeOffset tooYoungDate = DateTimeOffset.UtcNow.AddYears(-BirthDateRules.MinimumAge + 1);

        // Act
        Action result = () => BirthDateRules.Validate(tooYoungDate);

        // Assert
        result.Should().Throw<InvalidBirthDateDomainException>()
            .WithMessage($"User must be at least {BirthDateRules.MinimumAge} years old.");
    }

    [Fact]
    public void Validate_Should_ThrowException_When_UserIsOlderThanMaximumAge()
    {
        // Arrange
        DateTimeOffset tooOldDate = DateTimeOffset.UtcNow.AddYears(-BirthDateRules.MaximumAge - 1);

        // Act
        Action result = () => BirthDateRules.Validate(tooOldDate);

        // Assert
        result.Should().Throw<InvalidBirthDateDomainException>()
            .WithMessage($"User cannot be older than {BirthDateRules.MaximumAge} years.");
    }

    [Fact]
    public void Validate_Should_NotThrowException_When_BirthDateIsValid()
    {
        // Arrange
        DateTimeOffset validDate = DateTimeOffset.UtcNow.AddYears(-25);

        // Act
        Action result = () => BirthDateRules.Validate(validDate);

        // Assert
        result.Should().NotThrow();
    }
}
