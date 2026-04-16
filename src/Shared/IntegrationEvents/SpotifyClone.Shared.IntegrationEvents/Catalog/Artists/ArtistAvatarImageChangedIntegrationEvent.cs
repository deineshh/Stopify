using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Artists;

public sealed record ArtistAvatarImageChangedIntegrationEvent(
    Guid Id,
    Guid? AvatarImageId)
    : IntegrationEvent;
