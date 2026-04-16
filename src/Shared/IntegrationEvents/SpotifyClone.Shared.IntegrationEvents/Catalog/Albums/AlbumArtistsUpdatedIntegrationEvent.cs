using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

public sealed record AlbumArtistsUpdatedIntegrationEvent(
    Guid Id,
    IEnumerable<Guid> Artists)
    : IntegrationEvent;
