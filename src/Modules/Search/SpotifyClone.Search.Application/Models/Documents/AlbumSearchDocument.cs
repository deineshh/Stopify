using SpotifyClone.Search.Application.Models.Documents.Compacts;

namespace SpotifyClone.Search.Application.Models.Documents;

public sealed record AlbumSearchDocument(
    string Id,
    string Title,
    int ReleaseYear,
    string Type,
    string CoverImageId,
    ArtistCompactDocument[] Artists);
