using SpotifyClone.Search.Application.Models.Documents;

namespace SpotifyClone.Search.Application.Features.GlobalSearch;

public sealed record GlobalSearchResponse(
    IEnumerable<UserSearchDocument> Users,
    IEnumerable<GenreSearchDocument> Genres,
    IEnumerable<MoodSearchDocument> Moods,
    IEnumerable<ArtistSearchDocument> Artists,
    IEnumerable<AlbumSearchDocument> Albums,
    IEnumerable<TrackSearchDocument> Tracks,
    IEnumerable<PlaylistSearchDocument> Playlists);
