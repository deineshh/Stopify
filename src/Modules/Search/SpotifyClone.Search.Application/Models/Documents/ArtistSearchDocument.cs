namespace SpotifyClone.Search.Application.Models.Documents;

public sealed record ArtistSearchDocument(
    string Id,
    string Name,
    bool IsVerified,
    string? AvatarImageId);
