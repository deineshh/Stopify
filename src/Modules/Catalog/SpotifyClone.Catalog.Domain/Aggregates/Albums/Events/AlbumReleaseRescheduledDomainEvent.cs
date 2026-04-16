using SpotifyClone.Catalog.Domain.Aggregates.Albums.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;

namespace SpotifyClone.Catalog.Domain.Aggregates.Albums.Events;

public sealed record AlbumReleaseRescheduledDomainEvent(
    AlbumId Id,
    DateTimeOffset ReleaseDateUtc)
    : DomainEvent;
