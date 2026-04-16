using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Playlists;

public sealed record PlaylistGeneratedCoverUpdatedIntegrationEvent(
    Guid Id,
    IEnumerable<Guid> GeneratedCoverImageIds)
    : IntegrationEvent;
