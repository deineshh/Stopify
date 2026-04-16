using SpotifyClone.Catalog.Domain.Aggregates.Moods.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;

namespace SpotifyClone.Catalog.Domain.Aggregates.Moods.Events;

public sealed record MoodRenamedDomainEvent(
    MoodId Id,
    string Name)
    : DomainEvent;
