namespace SpotifyClone.Shared.Kernel.Contracts.Catalog;

public sealed record MoodSharedDto(
    Guid Id,
    string Name,
    Guid? CoverImageId);
