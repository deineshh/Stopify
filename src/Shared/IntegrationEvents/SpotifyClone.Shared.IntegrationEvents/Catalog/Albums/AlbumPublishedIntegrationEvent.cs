using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

public sealed record AlbumPublishedIntegrationEvent(
    Guid Id,
    string Title,
    DateTimeOffset ReleaseDate,
    string Type,
    Guid CoverImageId,
    IEnumerable<Guid> Artists)
    : IntegrationEvent;
