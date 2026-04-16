using SpotifyClone.Catalog.Domain.Aggregates.Albums.Enums;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.ValueObjects;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Domain.Aggregates.Albums.Events;

public sealed record AlbumPublishedDomainEvent(
    AlbumId Id,
    string Title,
    DateTimeOffset ReleaseDate,
    AlbumType Type,
    ImageId CoverImageId,
    IEnumerable<ArtistId> MainArtists,
    IEnumerable<ArtistId> FeaturedArtists,
    IEnumerable<TrackId> Tracks)
    : DomainEvent;
