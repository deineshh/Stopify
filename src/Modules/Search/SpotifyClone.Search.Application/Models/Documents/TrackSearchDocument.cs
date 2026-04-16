using SpotifyClone.Search.Application.Models.Documents.Compacts;

namespace SpotifyClone.Search.Application.Models.Documents;

public sealed record TrackSearchDocument(
    string Id,
    string Title,
    bool IsExplicit,
    int ReleaseYear,
    string CoverImageId,
    AlbumCompactDocument Album,
    ArtistCompactDocument[] Artists,
    GenreCompactDocument[] Genres,
    MoodCompactDocument[] Moods);
