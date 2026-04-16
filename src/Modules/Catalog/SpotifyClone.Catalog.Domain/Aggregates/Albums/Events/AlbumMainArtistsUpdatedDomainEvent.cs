using SpotifyClone.Catalog.Domain.Aggregates.Albums.ValueObjects;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;

namespace SpotifyClone.Catalog.Domain.Aggregates.Albums.Events;

public sealed record AlbumMainArtistsUpdatedDomainEvent(
    AlbumId Id,
    IEnumerable<ArtistId> MainArtists)
    : DomainEvent;
