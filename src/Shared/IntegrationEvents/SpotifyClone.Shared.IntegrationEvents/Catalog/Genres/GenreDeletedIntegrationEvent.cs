using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Genres;

public sealed record GenreDeletedIntegrationEvent(
    Guid Id)
    : IntegrationEvent;
