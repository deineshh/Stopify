using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Artists;

public sealed record ArtistNameChangedIntegrationEvent(
    Guid Id,
    string Name)
    : IntegrationEvent;
