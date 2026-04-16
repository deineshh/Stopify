using SpotifyClone.Catalog.Domain.Aggregates.Albums.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Domain.Aggregates.Albums.Events;

public sealed record AlbumUnlinkedFromCoverImageDomainEvent(
    AlbumId Id,
    ImageId ImageId,
    IEnumerable<TrackId> Tracks)
    : DomainEvent;
