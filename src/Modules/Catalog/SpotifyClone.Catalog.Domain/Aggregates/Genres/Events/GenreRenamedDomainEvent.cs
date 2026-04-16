using SpotifyClone.Catalog.Domain.Aggregates.Genres.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;

namespace SpotifyClone.Catalog.Domain.Aggregates.Genres.Events;

public sealed record GenreRenamedDomainEvent(
    GenreId Id,
    string Name)
    : DomainEvent;
