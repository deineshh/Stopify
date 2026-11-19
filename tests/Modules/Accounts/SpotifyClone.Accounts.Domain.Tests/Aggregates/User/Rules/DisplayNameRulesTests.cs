using FluentAssertions;
using SpotifyClone.Accounts.Domain.Aggregates.Users.Exceptions;
using SpotifyClone.Accounts.Domain.Aggregates.Users.Rules;

namespace SpotifyClone.Accounts.Domain.Tests.Aggregates.User.Rules;

public sealed class DisplayNameRulesTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_Should_ThrowException_When_DisplayNameIsNullOrWhiteSpace(string? invalidDisplayName)
    {
        // Arrange & Act
        Action result = () => DisplayNameRules.Validate(invalidDisplayName!);

        // Assert
        result.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Validate_Should_ThrowException_When_DisplayNameExceedsMaxLength()
    {
        // Arrange
        string longDisplayName = new string('a', DisplayNameRules.MaxLength + 1);

        // Act
        Action result = () => DisplayNameRules.Validate(longDisplayName);

        // Assert
        result.Should().Throw<InvalidDisplayNameDomainException>()
            .WithMessage($"Display name exceeds maximum length of {DisplayNameRules.MaxLength} characters.");
    }

    [Fact]
    public void Validate_Should_ThrowException_When_DisplayNameContainsControlCharacters()
    {
        // Arrange
        string invalidDisplayName = "Invalid\u0001Name";

        // Act
        Action result = () => DisplayNameRules.Validate(invalidDisplayName);

        // Assert
        result.Should().Throw<InvalidDisplayNameDomainException>()
            .WithMessage("Display name contains invalid characters.");
    }

    [Fact]
    public void Validate_Should_NotThrowException_When_DisplayNameIsValid()
    {
        // Arrange
        string validDisplayName = "Valid Display Name";

        // Act
        Action result = () => DisplayNameRules.Validate(validDisplayName);

        // Assert
        result.Should().NotThrow();
    }
}
