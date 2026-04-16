using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Models;
using SpotifyClone.Search.Application.Models.Documents;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Search.Application.Features.GlobalSearch;

public sealed class GlobalSearchQueryHandler(
    ISearchProvider searchProvider)
    : IQueryHandler<GlobalSearchQuery, GlobalSearchResponse>
{
    public async Task<Result<GlobalSearchResponse>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return new GlobalSearchResponse([], [], [], [], [], [], []);
        }

        Task<SearchResult<UserSearchDocument>> usersTask
            = searchProvider.SearchAsync<UserSearchDocument>(
                SearchIndexNames.Users,
                query: request.SearchTerm,
                limit: request.Limit,
                cancellationToken: cancellationToken);

        Task<SearchResult<GenreSearchDocument>> genresTask
            = searchProvider.SearchAsync<GenreSearchDocument>(
                SearchIndexNames.Genres,
                query: request.SearchTerm,
                limit: request.Limit,
                cancellationToken: cancellationToken);

        Task<SearchResult<MoodSearchDocument>> moodsTask
            = searchProvider.SearchAsync<MoodSearchDocument>(
                SearchIndexNames.Moods,
                query: request.SearchTerm,
                limit: request.Limit,
                cancellationToken: cancellationToken);

        Task<SearchResult<ArtistSearchDocument>> artistsTask
            = searchProvider.SearchAsync<ArtistSearchDocument>(
                SearchIndexNames.Artists,
                query: request.SearchTerm,
                limit: request.Limit,
                cancellationToken: cancellationToken);

        Task<SearchResult<AlbumSearchDocument>> albumsTask
            = searchProvider.SearchAsync<AlbumSearchDocument>(
                SearchIndexNames.Albums,
                query: request.SearchTerm,
                limit: request.Limit,
                cancellationToken: cancellationToken);

        Task<SearchResult<TrackSearchDocument>> tracksTask
            = searchProvider.SearchAsync<TrackSearchDocument>(
                SearchIndexNames.Tracks,
                query: request.SearchTerm,
                limit: request.Limit,
                cancellationToken: cancellationToken);

        Task<SearchResult<PlaylistSearchDocument>> playlistsTask
            = searchProvider.SearchAsync<PlaylistSearchDocument>(
                SearchIndexNames.Playlists,
                query: request.SearchTerm,
                limit: request.Limit,
                cancellationToken: cancellationToken);

        await Task.WhenAll(
            usersTask, genresTask, moodsTask, artistsTask, albumsTask, tracksTask, playlistsTask);

        return new GlobalSearchResponse(
            usersTask.Result.Items,
            genresTask.Result.Items,
            moodsTask.Result.Items,
            artistsTask.Result.Items,
            albumsTask.Result.Items,
            tracksTask.Result.Items,
            playlistsTask.Result.Items
        );
    }
}
