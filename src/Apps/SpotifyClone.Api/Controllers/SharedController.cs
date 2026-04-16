using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Accounts.Application.Abstractions.Data;
using SpotifyClone.Accounts.Application.Features.Accounts.Queries;
using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Catalog.Application.Features.Albums.Queries;
using SpotifyClone.Catalog.Application.Features.Artists.Queries;
using SpotifyClone.Catalog.Application.Features.Genres.Queries;
using SpotifyClone.Catalog.Application.Features.Moods.Queries;
using SpotifyClone.Catalog.Application.Features.Tracks.Queries;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.Enums;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.Enums;
using SpotifyClone.Playlists.Application.Abstractions.Data;
using SpotifyClone.Playlists.Application.Features.Playlists.Queries;
using SpotifyClone.Shared.Kernel.Contracts.Accounts;
using SpotifyClone.Shared.Kernel.Contracts.Catalog;
using SpotifyClone.Shared.Kernel.Contracts.Playlists;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Api.Controllers;

[Tags("Shared")]
[Route("api/v1/shared")]
public sealed class SharedController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("Get all Genres")]
    [EndpointDescription("Returns all Genres from the system.\n" +
                         "Note: This endpoint is only used by the backend to make a bridge without the modules.")]
    [ProducesResponseType(typeof(IEnumerable<GenreSharedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet("genres")]
    public async Task<ActionResult<IEnumerable<GenreSharedDto>>> GetAllGenres(
        IGenreReadService genreReadService,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<GenreSummary> genres = await genreReadService.GetAllAsync(cancellationToken);

        return Ok(genres.Select(g => new GenreSharedDto(
            g.Id, g.Name, g.CoverImageId)));
    }

    [EndpointSummary("Get all Moods")]
    [EndpointDescription("Returns all Moods from the system.\n" +
                         "Note: This endpoint is only used by the backend to make a bridge without the modules.")]
    [ProducesResponseType(typeof(IEnumerable<MoodSharedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet("moods")]
    public async Task<ActionResult<IEnumerable<MoodSharedDto>>> GetAllMoods(
        IMoodReadService moodReadService,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<MoodSummary> moods = await moodReadService.GetAllAsync(cancellationToken);

        return Ok(moods.Select(m => new MoodSharedDto(
            m.Id, m.Name, m.CoverImageId)));
    }

    [EndpointSummary("Get all Artists")]
    [EndpointDescription("Returns all Artists from the system.\n" +
                         "Note: This endpoint is only used by the backend to make a bridge without the modules.")]
    [ProducesResponseType(typeof(IEnumerable<ArtistSharedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet("artists")]
    public async Task<ActionResult<IEnumerable<ArtistSharedDto>>> GetAllArtists(
        IArtistReadService artistReadService,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<ArtistSummary> artists = await artistReadService.GetAllAsync(cancellationToken);

        return Ok(artists.Select(a => new ArtistSharedDto(
            a.Id, a.Name,
            a.Status == ArtistStatus.Verified.Value,
            a.Status == ArtistStatus.Banned.Value,
            a.Avatar?.ImageId)));
    }

    [EndpointSummary("Get all Albums")]
    [EndpointDescription("Returns all Albums from the system.\n" +
                         "Note: This endpoint is only used by the backend to make a bridge without the modules.")]
    [ProducesResponseType(typeof(IEnumerable<AlbumSharedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet("albums")]
    public async Task<ActionResult<IEnumerable<AlbumSharedDto>>> GetAllAlbums(
        IAlbumReadService albumReadService,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<AlbumSummary> albums = await albumReadService.GetAllAsync(cancellationToken);

        return Ok(albums.Select(a => new AlbumSharedDto(
            a.Id, a.Title, a.ReleaseDateUtc, a.Type,
            a.Status == AlbumStatus.Published.Value,
            a.Cover?.ImageId,
            a.MainArtists.Select(ma => ma.Id))));
    }

    [EndpointSummary("Get all Tracks")]
    [EndpointDescription("Returns all Tracks from the system.\n" +
                         "Note: This endpoint is only used by the backend to make a bridge without the modules.")]
    [ProducesResponseType(typeof(IEnumerable<TrackSharedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet("tracks")]
    public async Task<ActionResult<IEnumerable<TrackSharedDto>>> GetAllTracks(
        ITrackReadService trackReadService,
        IAlbumReadService albumReadService,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<TrackSummary> tracks = await trackReadService.GetAllAsync(cancellationToken);

        IEnumerable<AlbumSummary> albums = await albumReadService.GetAllByTracksAsync(
            tracks.Where(t => t.AlbumId.HasValue).Select(t => TrackId.From(t.Id)),
            cancellationToken);

        var albumMap = albums.ToDictionary(a => a.Id);

        return Ok(tracks.Select(t =>
        {
            Guid? coverId = null;
            if (t.AlbumId.HasValue && albumMap.TryGetValue(t.AlbumId.Value, out AlbumSummary? album))
            {
                coverId = album.Cover?.ImageId;
            }

            return new TrackSharedDto(
                t.Id,
                t.Title,
                t.ContainsExplicitContent,
                t.ReleaseDateUtc,
                coverId,
                t.AlbumId,
                t.MainArtists.Select(ma => ma.Id),
                t.FeaturedArtists.Select(fa => fa.Id),
                t.Genres,
                t.Moods);
        }));
    }

    [EndpointSummary("Get All Playlists")]
    [EndpointDescription("Returns all Playlists from the system.\n" +
                         "Note: This endpoint is only used by the backend to make a bridge without the modules.")]
    [ProducesResponseType(typeof(IEnumerable<PlaylistSharedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet("playlists")]
    public async Task<ActionResult<IEnumerable<PlaylistSharedDto>>> GetAllPlaylists(
        IPlaylistReadService playlistReadService,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<PlaylistSummary> playlists = await playlistReadService.GetAllAsync(cancellationToken);

        return Ok(playlists.Select(p => new PlaylistSharedDto(
            p.Id, p.Name, p.IsPublic, p.OwnerId, p.CustomCover?.ImageId, p.GeneratedCoverImageIds)));
    }

    [EndpointSummary("Get All Users")]
    [EndpointDescription("Returns all Users from the system.\n" +
                         "Note: This endpoint is only used by the backend to make a bridge without the modules.")]
    [ProducesResponseType(typeof(IEnumerable<UserSharedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserSharedDto>>> GetAllUsers(
        IUserReadService userReadService,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<UserSummary> users = await userReadService.GetAllAsync(cancellationToken);

        return Ok(users.Select(p => new UserSharedDto(
            p.Id, p.DisplayName, p.Avatar?.ImageId)));
    }
}
