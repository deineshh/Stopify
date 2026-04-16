namespace SpotifyClone.Catalog.Application.Features.Genres.Queries;

public sealed record GenreSummary(
    Guid Id,
    string Name,
    Guid? CoverImageId);
