using SpotifyClone.Catalog.Application.Features.Artists.Queries;

namespace SpotifyClone.Catalog.Application.Features.Tracks.Queries;

public sealed record TrackSummary(
    Guid Id,
    string Title,
    TimeSpan? Duration,
    DateTimeOffset? ReleaseDateUtc,
    bool ContainsExplicitContent,
    string Status,
    Guid? AudioFileId,
    Guid? AlbumId,
    IEnumerable<ArtistSummary> MainArtists,
    IEnumerable<ArtistSummary> FeaturedArtists,
    IEnumerable<Guid> Genres,
    IEnumerable<Guid> Moods);
