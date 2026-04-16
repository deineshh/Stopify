using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

namespace SpotifyClone.Shared.IntegrationEvents.Catalog.Tracks;

public sealed record TrackPublishedIntegrationEvent(
    Guid Id,
    string Title,
    bool IsExplicit,
    DateTimeOffset ReleaseDateUtc,
    Guid CoverImageId,
    Guid AlbumId,
    IEnumerable<Guid> MainArtists,
    IEnumerable<Guid> FeaturedArtists,
    IEnumerable<Guid> Genres,
    IEnumerable<Guid> Moods)
    : IntegrationEvent;
