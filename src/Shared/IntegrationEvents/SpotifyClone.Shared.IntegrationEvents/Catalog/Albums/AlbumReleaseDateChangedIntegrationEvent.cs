using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

public sealed record AlbumReleaseDateChangedIntegrationEvent(
    Guid Id,
    DateTimeOffset ReleaseDateUtc)
    : IntegrationEvent;
