using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Moods;

public sealed record MoodDeletedIntegrationEvent(
    Guid Id)
    : IntegrationEvent;
