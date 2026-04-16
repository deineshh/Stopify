using SpotifyClone.Catalog.Domain.Aggregates.Moods.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Domain.Aggregates.Moods.Events;

public sealed record MoodLinkedToCoverImageDomainEvent(
    MoodId Id,
    ImageId ImageId)
    : DomainEvent;
