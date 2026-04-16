using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Accounts.Users;

public sealed record UserDetailsChangedIntegrationEvent(
    Guid Id,
    string Name)
    : IntegrationEvent;
