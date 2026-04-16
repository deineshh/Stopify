using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Albums;

public sealed record AlbumTitleChangedIntegrationEvent(
    Guid Id,
    string Title)
    : IntegrationEvent;
