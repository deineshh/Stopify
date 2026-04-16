using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Domain.Aggregates.Tracks.Events;

public sealed record TrackReleaseRescheduledDomainEvent(
    TrackId Id,
    DateTimeOffset ReleaseDateUtc)
    : DomainEvent;
