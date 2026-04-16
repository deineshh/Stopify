namespace SpotifyClone.Shared.Kernel.Contracts.Catalog;

public sealed record GenreSharedDto(
    Guid Id,
    string Name,
    Guid? CoverImageId);
