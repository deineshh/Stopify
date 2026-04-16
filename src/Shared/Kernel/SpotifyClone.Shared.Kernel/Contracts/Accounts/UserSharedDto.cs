namespace SpotifyClone.Shared.Kernel.Contracts.Accounts;

public sealed record UserSharedDto(
    Guid Id,
    string Name,
    Guid? AvatarImageId);
