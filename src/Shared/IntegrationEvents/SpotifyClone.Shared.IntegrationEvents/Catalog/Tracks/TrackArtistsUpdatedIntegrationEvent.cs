using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

public sealed record TrackArtistsUpdatedIntegrationEvent(
    Guid Id,
    IEnumerable<Guid> MainArtists,
    IEnumerable<Guid> FeaturedArtists)
    : IntegrationEvent;
