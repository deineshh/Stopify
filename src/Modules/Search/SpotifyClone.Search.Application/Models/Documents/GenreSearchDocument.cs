namespace SpotifyClone.Search.Application.Models.Documents;

public sealed record GenreSearchDocument(
    string Id,
    string Name,
    string? CoverImageId);
