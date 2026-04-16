namespace SpotifyClone.Catalog.Application.Features.Artists.Queries;

public sealed record ArtistFilterParams(
    string? Name = null,
    string? Bio = null,
    string? Status = null,
    IEnumerable<Guid>? GenreIds = null,
    IEnumerable<Guid>? MoodIds = null);
