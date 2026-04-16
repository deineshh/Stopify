using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Artists;

public sealed record ArtistUnbannedIntegrationEvent(
    Guid Id,
    string Name,
    bool IsVerified,
    Guid? AvatarImageId)
    : IntegrationEvent;
