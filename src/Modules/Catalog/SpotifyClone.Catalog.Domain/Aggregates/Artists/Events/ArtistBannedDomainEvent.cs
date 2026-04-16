using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;

namespace SpotifyClone.Catalog.Domain.Aggregates.Artists.Events;

public sealed record ArtistBannedDomainEvent(
    ArtistId Id)
    : DomainEvent;
