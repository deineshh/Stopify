using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

public sealed record TrackGenresUpdatedIntegrationEvent(
    Guid Id,
    IEnumerable<Guid> Genres)
    : IntegrationEvent;
