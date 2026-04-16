using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Api.Contracts.v1.Streaming.Playback.AddTrackToQueue;
using SpotifyClone.Api.Contracts.v1.Streaming.Playback.GetQueue;
using SpotifyClone.Api.Contracts.v1.Streaming.Playback.ManipulatePlayback;
using SpotifyClone.Api.Contracts.v1.Streaming.Playback.RemoveTrackFromQueue;
using SpotifyClone.Api.Contracts.v1.Streaming.Playback.SkipToTrack;
using SpotifyClone.Api.Contracts.v1.Streaming.Playback.StartPlayback;
using SpotifyClone.Api.Contracts.v1.Streaming.Playback.ToggleRepeatMode;
using SpotifyClone.Api.Contracts.v1.Streaming.Playback.ToggleShuffle;
using SpotifyClone.Api.Contracts.v1.Streaming.Playback.UpdatePosition;
using SpotifyClone.Api.Mappers;
using SpotifyClone.Catalog.Application.Features.Albums.Queries;
using SpotifyClone.Catalog.Application.Features.Albums.Queries.GetDetails;
using SpotifyClone.Catalog.Application.Features.Tracks.Queries;
using SpotifyClone.Catalog.Application.Features.Tracks.Queries.GetSummary;
using SpotifyClone.Catalog.Application.Features.Tracks.Queries.ListByIds;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.Enums;
using SpotifyClone.Playlists.Application.Features.Playlists.Queries;
using SpotifyClone.Playlists.Application.Features.Playlists.Queries.GetDetails;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;
using SpotifyClone.Shared.Kernel.IDs;
using SpotifyClone.Streaming.Application.Errors;
using SpotifyClone.Streaming.Application.Features.Playback.Commands.AddTrackToQueue;
using SpotifyClone.Streaming.Application.Features.Playback.Commands.Pause;
using SpotifyClone.Streaming.Application.Features.Playback.Commands.RemoveTrackFromQueue;
using SpotifyClone.Streaming.Application.Features.Playback.Commands.Resume;
using SpotifyClone.Streaming.Application.Features.Playback.Commands.SeekPosition;
using SpotifyClone.Streaming.Application.Features.Playback.Commands.SkipToNext;
using SpotifyClone.Streaming.Application.Features.Playback.Commands.SkipToPrevious;
using SpotifyClone.Streaming.Application.Features.Playback.Commands.Start;
using SpotifyClone.Streaming.Application.Features.Playback.Commands.SyncPosition;
using SpotifyClone.Streaming.Application.Features.Playback.Commands.ToggleRepeatMode;
using SpotifyClone.Streaming.Application.Features.Playback.Commands.ToggleShuffle;
using SpotifyClone.Streaming.Application.Features.Playback.Queries;
using SpotifyClone.Streaming.Application.Features.Playback.Queries.GetDetails;
using SpotifyClone.Streaming.Application.Features.Playback.Queries.GetQueue;
using SpotifyClone.Streaming.Domain.Aggregates.PlaybackSessions.Enums;
using SpotifyClone.Streaming.Domain.ValueObjects;

namespace SpotifyClone.Api.Controllers.Streaming;

[Route("api/v1/me/playback")]
public sealed class PlaybackController(IMediator mediator)
    : ApiController(mediator)
{
    [EndpointSummary("Start Playback")]
    [EndpointDescription("With the current User, modifies an existing Playback session or create a new one.")]
    [ProducesResponseType(typeof(StartPlaybackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPost("play")]
    public async Task<ActionResult<StartPlaybackResponse>> Start(
        [FromBody] StartPlaybackRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!PlaybackContext.IsValid(request.ContextType, request.ContextExternalId))
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                Result.Failure<StartPlaybackResponse>(PlaybackErrors.InvalidContext),
                HttpContext);
            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        if (request.StartTrackId is not null)
        {
            Result<TrackSummary> startTrackResult = await Mediator.Send(
                new GetTrackSummaryQuery(request.StartTrackId.Value), cancellationToken);
            if (startTrackResult.IsFailure)
            {
                ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                    startTrackResult, HttpContext);
                return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
            }
        }

        Result<IEnumerable<Guid>> tracksResult = await GetTracksByContextTypeAsync(
            request.ContextType, request.ContextExternalId!.Value, cancellationToken);
        if (tracksResult.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                tracksResult, HttpContext);
            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        Result<StartPlaybackCommandResult> playbackResult = await Mediator.Send(
            new StartPlaybackCommand(
                request.DeviceId,
                request.ContextType,
                request.ContextExternalId,
                request.StartTrackId,
                tracksResult.Value),
            cancellationToken);
        if (playbackResult.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                playbackResult, HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return Ok(new StartPlaybackResponse(
            playbackResult.Value.HlsUrl,
            playbackResult.Value.DashUrl,
            playbackResult.Value.StartPositionMs,
            playbackResult.Value.TrackId));
    }

    [EndpointSummary("Get Playback Session details")]
    [EndpointDescription("Get current User' Playback Session details.")]
    [ProducesResponseType(typeof(PlaybackSessionDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpGet]
    public async Task<ActionResult<PlaybackSessionDetails>> GetDetails(
        CancellationToken cancellationToken = default)
    {
        Result<PlaybackSessionDetails> result = await Mediator.Send(
            new GetPlaybackSessionDetailsQuery(),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result, HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return Ok(result.Value);
    }

    [EndpointSummary("Get Playback Queue")]
    [EndpointDescription("Get current User's Playback Queue.")]
    [ProducesResponseType(typeof(GetPlaybackQueueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpGet("queue")]
    public async Task<ActionResult<GetPlaybackQueueResponse>> GetQueue(
        CancellationToken cancellationToken = default)
    {
        Result<PlaybackQueueDetails> queueResult = await Mediator.Send(
            new GetPlaybackQueueQuery(),
            cancellationToken);
        if (queueResult.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                queueResult, HttpContext);
            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        Result<TrackSummary>? currentTrackResult = null;
        if (queueResult.Value.CurrentTrackId is not null)
        {
            currentTrackResult = await Mediator.Send(
            new GetTrackSummaryQuery(queueResult.Value.CurrentTrackId.Value),
            cancellationToken);
            if (queueResult.IsFailure)
            {
                ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                    currentTrackResult, HttpContext);
                return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
            }
        }

        TrackList tracksInQueue = new(new PagedList<TrackSummary>());
        if (queueResult.Value.TracksInQueue.Any())
        {
            Result<TrackList> tracksInQueueResult = await Mediator.Send(
            new ListTracksByIdsQuery(queueResult.Value.TracksInQueue),
            cancellationToken);
            if (tracksInQueueResult.IsFailure)
            {
                ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                    tracksInQueueResult, HttpContext);
                return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
            }
            tracksInQueue = tracksInQueueResult.Value;
        }

        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(new
        {
            page = tracksInQueue.Tracks.Page,
            pageSize = tracksInQueue.Tracks.PageSize,
            hasPreviousPage = tracksInQueue.Tracks.HasPreviousPage,
            hasNextPage = tracksInQueue.Tracks.HasNextPage,
            totalCount = tracksInQueue.Tracks.TotalCount,
        }));

        return Ok(new GetPlaybackQueueResponse(
            currentTrackResult?.Value,
            tracksInQueue.Tracks.Items));
    }

    [EndpointSummary("Add Track to Playback Queue")]
    [EndpointDescription("Adds a Track to the current User's Playback Queue.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPost("queue")]
    public async Task<ActionResult> AddTrackToQueue(
        [FromBody] AddTrackToPlaybackQueueRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<TrackSummary> trackResult = await Mediator.Send(
            new GetTrackSummaryQuery(request.TrackId),
            cancellationToken);
        if (trackResult.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                trackResult, HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        Result<AddTrackToPlaybackQueueCommandResult> addTrackToQueueResult = await Mediator.Send(
            new AddTrackToPlaybackQueueCommand(request.TrackId),
            cancellationToken);
        if (addTrackToQueueResult.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                addTrackToQueueResult, HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }

    [EndpointSummary("Remove Track from Playback Queue")]
    [EndpointDescription("Removes a Track from the current User's Playback Queue.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpDelete("queue/{trackId:guid}")]
    public async Task<ActionResult> RemoveTrackFromQueue(
        [FromRoute] RemoveTrackFromPlaybackQueueRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<TrackSummary> trackResult = await Mediator.Send(
            new GetTrackSummaryQuery(request.TrackId),
            cancellationToken);
        if (trackResult.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                trackResult, HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        Result<RemoveTrackFromPlaybackQueueCommandResult> removeTrackFromQueueResult = await Mediator.Send(
            new RemoveTrackFromPlaybackQueueCommand(request.TrackId),
            cancellationToken);
        if (removeTrackFromQueueResult.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                removeTrackFromQueueResult, HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }

    [EndpointSummary("Resume Playback")]
    [EndpointDescription("With the current User, resumes the playback and updates the Playback session.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPost("resume")]
    public async Task<ActionResult> Resume(
        [FromBody] ManipulatePlaybackRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<ResumePlaybackCommandResult> result = await Mediator.Send(
            new ResumePlaybackCommand(request.DeviceId),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result, HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }

    [EndpointSummary("Pause Playback")]
    [EndpointDescription("With the current User, pauses the playback and updates the Playback session.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPost("pause")]
    public async Task<ActionResult> Pause(
        [FromBody] ManipulatePlaybackRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<PausePlaybackCommandResult> result = await Mediator.Send(
            new PausePlaybackCommand(request.DeviceId),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result, HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }

    [EndpointSummary("Sync Playback position")]
    [EndpointDescription("Syncronize current User Playback Session's position (in milliseconds).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPatch("sync")]
    public async Task<ActionResult> SyncPosition(
        [FromBody] UpdatePlaybackPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<SyncPlaybackPositionCommandResult> result = await Mediator.Send(
            new SyncPlaybackPositionCommand(
                request.DeviceId,
                request.PositionMs),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result, HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }

    [EndpointSummary("Seek Playback position")]
    [EndpointDescription("Seek current User Playback Session's position (in milliseconds).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPatch("seek")]
    public async Task<ActionResult> SeekPosition(
        [FromBody] UpdatePlaybackPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<SeekPlaybackPositionCommandResult> result = await Mediator.Send(
            new SeekPlaybackPositionCommand(
                request.DeviceId,
                request.PositionMs),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result, HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return NoContent();
    }

    [EndpointSummary("Skip to next Track")]
    [EndpointDescription("Skips the Playback to the next Track in the Queue.")]
    [ProducesResponseType(typeof(SkipToTrackResponse), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPost("next")]
    public async Task<ActionResult> SkipToNext(
        [FromBody] ManipulatePlaybackRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<SkipToNextTrackCommandResult> skipToNextResult;

        while (true)
        {
            skipToNextResult = await Mediator.Send(
                new SkipToNextTrackCommand(request.DeviceId),
                cancellationToken);
            if (skipToNextResult.IsFailure)
            {
                ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                    skipToNextResult, HttpContext);

                return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
            }

            // If there's nothing to play, exit
            if (skipToNextResult.Value.TrackId is null &&
                skipToNextResult.Value.IsCurrentQueueEmpty)
            {
                break;
            }

            // If there's a current Track to play, check if it's playable;
            // if it's playable - exit;
            // if it's not - skip to the next track
            if (skipToNextResult.Value.TrackId is not null)
            {
                Result<TrackSummary> trackResult = await Mediator.Send(
                    new GetTrackSummaryQuery(skipToNextResult.Value.TrackId.Value),
                    cancellationToken);
                if (trackResult.IsSuccess && trackResult.Value.Status != TrackStatus.Draft.Value)
                {
                    break;
                }
            }
        }

        return Ok(new SkipToTrackResponse(
            skipToNextResult.Value.HlsUrl,
            skipToNextResult.Value.DashUrl,
            skipToNextResult.Value.StartPositionMs,
            skipToNextResult.Value.TrackId));
    }

    [EndpointSummary("Skip to previous Track")]
    [EndpointDescription("Skips the Playback to the previous Track from the history.")]
    [ProducesResponseType(typeof(SkipToTrackResponse), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPost("previous")]
    public async Task<ActionResult> SkipToPrevious(
        [FromBody] ManipulatePlaybackRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<SkipToPreviousTrackCommandResult> skipToPreviousResult;

        while (true)
        {
            skipToPreviousResult = await Mediator.Send(
                new SkipToPreviousTrackCommand(request.DeviceId),
                cancellationToken);
            if (skipToPreviousResult.IsFailure)
            {
                ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                    skipToPreviousResult, HttpContext);

                return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
            }

            // If there's nothing to play, exit
            if (skipToPreviousResult.Value.TrackId is null &&
                skipToPreviousResult.Value.IsPreviousQueueEmpty)
            {
                break;
            }

            // If there's a Track to play, check if it's playable;
            // if it's playable - exit;
            // if it's not - skip to the previous track
            if (skipToPreviousResult.Value.TrackId is not null)
            {
                Result<TrackSummary> trackResult = await Mediator.Send(
                    new GetTrackSummaryQuery(skipToPreviousResult.Value.TrackId.Value),
                    cancellationToken);
                if (trackResult.IsSuccess && trackResult.Value.Status != TrackStatus.Draft.Value)
                {
                    break;
                }
            }
        }

        return Ok(new SkipToTrackResponse(
            skipToPreviousResult.Value.HlsUrl,
            skipToPreviousResult.Value.DashUrl,
            skipToPreviousResult.Value.StartPositionMs,
            skipToPreviousResult.Value.TrackId));
    }

    [EndpointSummary("Toggle Playback Shuffle")]
    [EndpointDescription("Toggles the Shuffle mode on current User's Playback.")]
    [ProducesResponseType(typeof(TogglePlaybackShuffleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPatch("shuffle")]
    public async Task<ActionResult<TogglePlaybackShuffleResponse>> ToggleShuffle(
        [FromBody] ManipulatePlaybackRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<TogglePlaybackShuffleCommandResult> result = await Mediator.Send(
            new TogglePlaybackShuffleCommand(request.DeviceId),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result, HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return Ok(new TogglePlaybackShuffleResponse(
            result.Value.IsShuffled));
    }

    [EndpointSummary("Toggle Playback Repeat mode")]
    [EndpointDescription("Toggles the Repeat mode on current User's Playback.")]
    [ProducesResponseType(typeof(TogglePlaybackRepeatModeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.Listener)]
    [HttpPatch("repeat")]
    public async Task<ActionResult<TogglePlaybackRepeatModeResponse>> ToggleRepeatMode(
        [FromBody] ManipulatePlaybackRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<TogglePlaybackRepeatModeCommandResult> result = await Mediator.Send(
            new TogglePlaybackRepeatModeCommand(request.DeviceId),
            cancellationToken);
        if (result.IsFailure)
        {
            ProblemDetails problemDetails = ResultToProblemDetailsMapper.MapToProblemDetails(
                result, HttpContext);

            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        return Ok(new TogglePlaybackRepeatModeResponse(
            ((PlaybackRepeatMode)result.Value.RepeatMode).ToString()));
    }

    private async Task<Result<IEnumerable<Guid>>> GetTracksByContextTypeAsync(
        string contextType,
        Guid contextExternalId,
        CancellationToken cancellationToken = default)
    {
        if (contextType == PlaybackContext.AlbumType)
        {
            return await GetTracksByAlbumContextTypeAsync(contextExternalId, cancellationToken);
        }
        else if (contextType == PlaybackContext.PlaylistType)
        {
            return await GetTracksByPlaylistContextTypeAsync(contextExternalId, cancellationToken);
        }
        else if (contextType == PlaybackContext.SearchType)
        {
            return await GetTracksBySearchContextTypeAsync(contextExternalId, cancellationToken);
        }

        return new List<Guid>();
    }

    private async Task<Result<IEnumerable<Guid>>> GetTracksByAlbumContextTypeAsync(
        Guid contextExternalId,
        CancellationToken cancellationToken = default)
    {
        Result<AlbumDetails> albumResult = await Mediator.Send(
            new GetAlbumDetailsQuery(contextExternalId),
            cancellationToken);
        if (albumResult.IsFailure)
        {
            return Result.Failure<IEnumerable<Guid>>(albumResult.Errors);
        }

        return albumResult.Value.Tracks
            .Where(t => t.Status != TrackStatus.Draft.Value)
            .Select(t => t.Id).ToList();
    }

    private async Task<Result<IEnumerable<Guid>>> GetTracksByPlaylistContextTypeAsync(
        Guid contextExternalId,
        CancellationToken cancellationToken = default)
    {
        Result<PlaylistDetails> playlistResult = await Mediator.Send(
            new GetPlaylistDetailsQuery(contextExternalId),
            cancellationToken);
        if (playlistResult.IsFailure)
        {
            return Result.Failure<IEnumerable<Guid>>(playlistResult.Errors);
        }

        return playlistResult.Value.Tracks.Select(t => t.Id).ToList();
    }

    private async Task<Result<IEnumerable<Guid>>> GetTracksBySearchContextTypeAsync(
        Guid contextExternalId,
        CancellationToken cancellationToken = default)
    {
        var trackId = TrackId.From(contextExternalId);

        Result<AlbumDetails> albumResult = await Mediator.Send(
            new GetAlbumDetailsQuery(contextExternalId),
            cancellationToken);
        if (albumResult.IsFailure)
        {
            return Result.Failure<IEnumerable<Guid>>(albumResult.Errors);
        }

        var tracks = albumResult.Value.Tracks
            .Where(t => t.Status != TrackStatus.Draft.Value)
            .Select(t => t.Id).ToList();

        tracks.Remove(trackId.Value);
        tracks.Insert(0, trackId.Value);
        return tracks;
    }
}
