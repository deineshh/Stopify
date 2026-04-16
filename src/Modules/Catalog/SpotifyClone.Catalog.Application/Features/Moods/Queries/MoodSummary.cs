namespace SpotifyClone.Catalog.Application.Features.Moods.Queries;

public sealed record MoodSummary(
    Guid Id,
    string Name,
    Guid? CoverImageId);
