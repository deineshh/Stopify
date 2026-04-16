using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Moods;

public sealed record MoodCreatedIntegrationEvent(
    Guid Id,
    string Name)
    : IntegrationEvent;
