using SpotifyClone.Playlists.Application.Abstractions;
using SpotifyClone.Playlists.Application.Abstractions.Data;
using SpotifyClone.Playlists.Application.Errors;
using SpotifyClone.Playlists.Application.Features.Playlists.Queries;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Commands;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;
using SpotifyClone.Shared.IntegrationEvents.Playlists;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Playlists.Application.Features.Playlists.Commands.MoveTrack;

internal sealed class MoveTrackInPlaylistCommandHandler(
    IPlaylistsUnitOfWork unit,
    IPlaylistReadService playlistReadService,
    ICurrentUser currentUser)
    : ICommandHandler<MoveTrackInPlaylistCommand, MoveTrackInPlaylistCommandResult>
{
    private readonly IPlaylistsUnitOfWork _unit = unit;
    private readonly IPlaylistReadService _playlistReadService = playlistReadService;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<MoveTrackInPlaylistCommandResult>> Handle(
        MoveTrackInPlaylistCommand request,
        CancellationToken cancellationToken)
    {
        Playlist? playlist = await _unit.Playlists.GetByIdAsync(
            PlaylistId.From(request.PlaylistId), cancellationToken);
        if (playlist is null)
        {
            return Result.Failure<MoveTrackInPlaylistCommandResult>(PlaylistErrors.NotFound);
        }

        bool trackAvailable = await _unit.TrackReferences.IsPublishedAsync(
            request.TrackId, cancellationToken);
        if (!trackAvailable)
        {
            return Result.Failure<MoveTrackInPlaylistCommandResult>(PlaylistErrors.InvalidTrack);
        }

        if ((!_currentUser.IsAuthenticated || playlist.Collaborators.Any(c => c.Value != _currentUser.Id)) &&
            !_currentUser.IsInRole(UserRoles.Admin))
        {
            return Result.Failure<MoveTrackInPlaylistCommandResult>(PlaylistErrors.NotOwned);
        }

        var oldTrackIds = playlist.Tracks.Select(t => t.Id.Value).ToList();

        playlist.MoveTrack(
            TrackId.From(request.TrackId),
            request.TargetPositionIndex);

        PlaylistDetails? playlistDetails = await _playlistReadService.GetDetailsAsync(
            PlaylistId.From(request.PlaylistId), cancellationToken);
        if (playlistDetails is null)
        {
            return Result.Failure<MoveTrackInPlaylistCommandResult>(PlaylistErrors.NotFound);
        }

        var newTrackIds = playlistDetails.Tracks.Select(t => t.Id).ToList();
        if (!oldTrackIds.Take(4).SequenceEqual(newTrackIds.Take(4)))
        {
            var integrationEvent = new PlaylistGeneratedCoverUpdatedIntegrationEvent(
                playlistDetails.Id,
                playlistDetails.GeneratedCoverImageIds);
            var message = OutboxMessage.FromIntegrationEvent(integrationEvent);
            await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        }

        return new MoveTrackInPlaylistCommandResult();
    }
}
