namespace SpotifyClone.Shared.Kernel.Contracts.Playlists;

public sealed record PlaylistSharedDto(
    Guid Id,
    string Name,
    bool IsPublic,
    Guid OwnerId,
    Guid? CustomCoverImageId,
    IEnumerable<Guid> GeneratedCoverImageIds);
