using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Artists;

public sealed record ArtistCreatedIntegrationEvent(
    Guid Id,
    string Name,
    bool IsVerified)
    : IntegrationEvent;
