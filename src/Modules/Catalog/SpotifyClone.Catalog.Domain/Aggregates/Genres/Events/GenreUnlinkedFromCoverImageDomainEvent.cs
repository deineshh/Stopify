using SpotifyClone.Catalog.Domain.Aggregates.Genres.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Domain.Aggregates.Genres.Events;

public sealed record GenreUnlinkedFromCoverImageDomainEvent(
    GenreId Id,
    ImageId ImageId)
    : DomainEvent;
