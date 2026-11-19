using FluentAssertions;
using SpotifyClone.Accounts.Domain.Aggregates.Users.Exceptions;
using SpotifyClone.Accounts.Domain.Aggregates.Users.ValueObjects;
using SpotifyClone.Shared.Kernel.Enums;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Accounts.Domain.Tests.Aggregates.User.ValueObjects;

public sealed class AvatarImageTests
{
    [Fact]
    public void Constructor_Should_CreateInstance_When_ParametersAreValid()
    {
        // Arrange
        var imageId = ImageId.New();
        int width = 512;
        int height = 512;
        ImageFileType fileType = ImageFileType.Png;
        long sizeInBytes = 150000;

        // Act
        var avatarImage = new AvatarImage(imageId, width, height, fileType, sizeInBytes);

        // Assert
        avatarImage.ImageId.Should().Be(imageId);
        avatarImage.Metadata.Width.Should().Be(width);
        avatarImage.Metadata.Height.Should().Be(height);
        avatarImage.Metadata.FileType.Should().Be(fileType);
    }

    [Fact]
    public void Constructor_Should_ThrowException_When_FileTypeIsNull()
    {
        // Arrange
        var imageId = ImageId.New();
        int width = 512;
        int height = 512;
        ImageFileType fileType = null!;
        long sizeInBytes = 150000;

        // Act
        Action act = () => new AvatarImage(imageId, width, height, fileType, sizeInBytes);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Should_ThrowException_When_WidthAndHeightAreNotEqual()
    {
        // Arrange
        var imageId = ImageId.New();
        int width = 512;
        int height = 256;
        ImageFileType fileType = ImageFileType.Png;
        long sizeInBytes = 150000;

        // Act
        Action act = () => new AvatarImage(imageId, width, height, fileType, sizeInBytes);

        // Assert
        act.Should().Throw<InvalidAvatarImageDomainException>();
    }
}
