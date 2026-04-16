namespace SpotifyClone.Shared.Kernel.Contracts.Catalog;

public sealed record AlbumSharedDto(
    Guid Id,
    string Title,
    DateTimeOffset? ReleaseDateUtc,
    string Type,
    bool IsPublished,
    Guid? CoverImageId,
    IEnumerable<Guid> ArtistIds);
