namespace SpotifyClone.Search.Application.Models.Documents;

public sealed record UserSearchDocument(
    string Id,
    string Name,
    string? AvatarImageId);
