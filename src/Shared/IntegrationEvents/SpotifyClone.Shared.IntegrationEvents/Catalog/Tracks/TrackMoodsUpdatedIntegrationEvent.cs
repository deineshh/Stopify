using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

public sealed record TrackMoodsUpdatedIntegrationEvent(
    Guid Id,
    IEnumerable<Guid> Moods)
    : IntegrationEvent;
