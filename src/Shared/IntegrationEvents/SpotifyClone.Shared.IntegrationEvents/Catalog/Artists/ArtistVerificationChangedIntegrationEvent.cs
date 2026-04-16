using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Artists;

public sealed record ArtistVerificationChangedIntegrationEvent(
    Guid Id,
    bool IsVerified)
    : IntegrationEvent;
