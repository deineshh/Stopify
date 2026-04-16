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

namespace SpotifyClone.Playlists.Application.Features.Playlists.Commands.RemoveTrackFromPlaylist;

internal sealed class RemoveTrackFromPlaylistCommandHandler(
    IPlaylistsUnitOfWork unit,
    IPlaylistReadService playlistReadService,
    ICurrentUser currentUser)
    : ICommandHandler<RemoveTrackFromPlaylistCommand, RemoveTrackFromPlaylistCommandResult>
{
    private readonly IPlaylistsUnitOfWork _unit = unit;
    private readonly IPlaylistReadService _playlistReadService = playlistReadService;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<RemoveTrackFromPlaylistCommandResult>> Handle(
        RemoveTrackFromPlaylistCommand request,
        CancellationToken cancellationToken)
    {
        Playlist? playlist = await _unit.Playlists.GetByIdAsync(
            PlaylistId.From(request.PlaylistId), cancellationToken);
        if (playlist is null)
        {
            return Result.Failure<RemoveTrackFromPlaylistCommandResult>(PlaylistErrors.NotFound);
        }

        bool isAdmin = _currentUser.IsInRole(UserRoles.Admin);
        if ((!_currentUser.IsAuthenticated || playlist.Collaborators.Any(c => c.Value != _currentUser.Id)) &&
            !isAdmin)
        {
            return Result.Failure<RemoveTrackFromPlaylistCommandResult>(PlaylistErrors.NotOwned);
        }

        var oldTrackIds = playlist.Tracks.Select(t => t.Id.Value).ToList();

        playlist.RemoveTrack(
            TrackId.From(request.TrackId),
            UserId.From(_currentUser.Id),
            isAdmin);

        PlaylistDetails? playlistDetails = await _playlistReadService.GetDetailsAsync(
            PlaylistId.From(request.PlaylistId), cancellationToken);
        if (playlistDetails is null)
        {
            return Result.Failure<RemoveTrackFromPlaylistCommandResult>(PlaylistErrors.NotFound);
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

        return new RemoveTrackFromPlaylistCommandResult();
    }
}
