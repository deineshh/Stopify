namespace SpotifyClone.Shared.Kernel.Contracts.Catalog;

public sealed record TrackSharedDto(
    Guid Id,
    string Title,
    bool IsExplicit,
    DateTimeOffset? ReleaseDateUtc,
    Guid? CoverImageId,
    Guid? AlbumId,
    IEnumerable<Guid> MainArtistIds,
    IEnumerable<Guid> FeaturedArtistIds,
    IEnumerable<Guid> GenreIds,
    IEnumerable<Guid> MoodIds);
