using SpotifyClone.Catalog.Domain.Aggregates.Albums.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;

namespace SpotifyClone.Catalog.Domain.Aggregates.Albums.Events;

public sealed record AlbumTitleCorrectedDomainEvent(
    AlbumId Id,
    string Title)
    : DomainEvent;
