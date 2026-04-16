namespace SpotifyClone.Catalog.Application.Features.Tracks.Queries;

public sealed record TrackFilterParams(
    string? Title = null,
    TimeSpan? Duration = null,
    DateTimeOffset? ReleaseDate = null,
    bool? Explicit = null,
    string? Status = null,
    Guid? AudioFileId = null,
    Guid? AlbumId = null,
    Guid? OwnerId = null,
    IEnumerable<Guid>? MainArtistIds = null,
    IEnumerable<Guid>? FeaturedArtistIds = null,
    IEnumerable<Guid>? GenreIds = null,
    IEnumerable<Guid>? MoodIds = null);
