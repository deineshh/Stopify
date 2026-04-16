using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Playlists;

public sealed record PlaylistDeletedIntegrationEvent(
    Guid Id)
    : IntegrationEvent;
