using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Accounts.Domain.Aggregates.Users.Events;

public sealed record UserRegisteredDomainEvent(
    UserId UserId,
    string Email,
    string ConfirmationToken,
    string DisplayName,
    ImageId? CoverImageId)
    : DomainEvent;
