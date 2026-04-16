namespace SpotifyClone.Search.Application.Models.Documents;

public sealed record MoodSearchDocument(
    string Id,
    string Name,
    string? CoverImageId);
