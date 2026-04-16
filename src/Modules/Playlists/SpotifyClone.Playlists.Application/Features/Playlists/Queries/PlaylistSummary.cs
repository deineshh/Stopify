using SpotifyClone.Playlists.Application.Models;

namespace SpotifyClone.Playlists.Application.Features.Playlists.Queries;

public sealed record PlaylistSummary(
    Guid Id,
    string Name,
    string? Description,
    bool IsPublic,
    Guid OwnerId,
    ImageMetadataDetails? CustomCover,
    IEnumerable<Guid> GeneratedCoverImageIds);
