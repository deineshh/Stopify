using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

public sealed record AlbumLinkedToCoverImageIntegrationEvent(
    Guid Id,
    Guid CoverImageId,
    IEnumerable<Guid> Tracks)
    : IntegrationEvent;
