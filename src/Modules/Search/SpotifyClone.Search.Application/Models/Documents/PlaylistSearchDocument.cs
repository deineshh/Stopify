using SpotifyClone.Search.Application.Models.Documents.Compacts;

namespace SpotifyClone.Search.Application.Models.Documents;

public sealed record PlaylistSearchDocument(
    string Id,
    string Name,
    UserCompactDocument Owner,
    string? CustomCoverImageId,
    string[] GeneratedCoverImageIds);
