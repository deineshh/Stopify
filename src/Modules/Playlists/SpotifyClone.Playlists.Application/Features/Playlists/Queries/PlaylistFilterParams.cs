namespace SpotifyClone.Playlists.Application.Features.Playlists.Queries;

public sealed record PlaylistFilterParams(
    string? Name = null,
    string? Description = null,
    Guid? OwnerId = null,
    string? Type = null,
    bool? IsPublic = null,
    IEnumerable<Guid>? CollaboratorIds = null,
    IEnumerable<Guid>? TrackIds = null,
    IEnumerable<Guid>? GenreIds = null,
    IEnumerable<Guid>? MoodIds = null);
