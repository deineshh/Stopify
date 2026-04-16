namespace SpotifyClone.Shared.Kernel.Contracts.Catalog;

public sealed record ArtistSharedDto(
    Guid Id,
    string Name,
    bool IsVerified,
    bool IsBanned,
    Guid? AvatarImageId);
