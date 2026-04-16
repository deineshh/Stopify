using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Accounts.Users;

public sealed record UserAvatarChangedIntegrationEvent(
    Guid Id,
    Guid? AvatarImageId)
    : IntegrationEvent;
