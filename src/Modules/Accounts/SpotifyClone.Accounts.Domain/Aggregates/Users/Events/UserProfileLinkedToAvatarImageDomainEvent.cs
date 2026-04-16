using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Accounts.Domain.Aggregates.Users.Events;

public sealed record UserProfileLinkedToAvatarImageDomainEvent(
    UserId Id,
    ImageId AvatarImageId)
    : DomainEvent;
