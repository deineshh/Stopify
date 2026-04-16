using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Playlists;

public sealed record PlaylistCoverChangedIntegrationEvent(
    Guid Id,
    Guid? CustomCoverImageId,
    IEnumerable<Guid> GeneratedCoverImageIds)
    : IntegrationEvent;
