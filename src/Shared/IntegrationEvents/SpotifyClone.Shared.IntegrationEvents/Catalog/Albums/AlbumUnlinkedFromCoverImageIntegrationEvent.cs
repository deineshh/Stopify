using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

public sealed record AlbumUnlinkedFromCoverImageIntegrationEvent(
    Guid Id,
    IEnumerable<Guid> Tracks)
    : IntegrationEvent;
