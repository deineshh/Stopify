using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

public sealed record TrackReleaseDateChangedIntegrationEvent(
    Guid Id,
    DateTimeOffset ReleaseDateUtc)
    : IntegrationEvent;
