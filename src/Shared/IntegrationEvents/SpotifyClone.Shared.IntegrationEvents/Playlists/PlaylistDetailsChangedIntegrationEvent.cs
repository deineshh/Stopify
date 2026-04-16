using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Playlists;

public sealed record PlaylistDetailsChangedIntegrationEvent(
    Guid Id,
    string Name,
    bool IsPublic,
    Guid OwnerId,
    Guid? CustomCoverImageId,
    IEnumerable<Guid> GeneratedCoverImageIds)
    : IntegrationEvent;
