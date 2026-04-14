namespace SpotifyClone.Catalog.Application.Features.Albums.Queries;

public sealed record AlbumFilterParams(
    string? Title = null,
    DateTimeOffset? ReleaseDate = null,
    string? Status = null,
    string? Type = null,
    IEnumerable<Guid>? MainArtistIds = null,
    IEnumerable<Guid>? TrackIds = null,
    IEnumerable<Guid>? GenreIds = null,
    IEnumerable<Guid>? MoodIds = null);
