using SpotifyClone.Catalog.Domain.Aggregates.Artists.Enums;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;

namespace SpotifyClone.Catalog.Domain.Aggregates.Artists.Events;

public sealed record ArtistCreatedDomainEvent(
    ArtistId Id,
    string Name,
    ArtistStatus Status)
    : DomainEvent;
