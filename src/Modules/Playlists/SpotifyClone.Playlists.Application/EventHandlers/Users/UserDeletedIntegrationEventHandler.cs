using MediatR;
using Microsoft.Extensions.Logging;
using SpotifyClone.Playlists.Application.Abstractions;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists;
using SpotifyClone.Shared.IntegrationEvents.Accounts.Users;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Playlists.Application.EventHandlers.Users;

internal sealed class UserDeletedIntegrationEventHandler(
    IPlaylistsUnitOfWork unit,
    ILogger<UserDeletedIntegrationEventHandler> logger)
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    private readonly IPlaylistsUnitOfWork _unit = unit;
    private readonly ILogger<UserDeletedIntegrationEventHandler> _logger = logger;

    public async Task Handle(
        UserDeletedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await DeleteUserPlaylistsAsync(notification, cancellationToken);
        await DeleteUserReferenceAsync(notification, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }

    private async Task DeleteUserPlaylistsAsync(
        UserDeletedIntegrationEvent notification,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Playlist> playlists = await _unit.Playlists.GetAllByOwnerAsync(
            UserId.From(notification.Id), cancellationToken);
        foreach (Playlist playlist in playlists)
        {
            playlist.PrepareForDeletion(true);
        }
        await _unit.Playlists.DeleteAllAsync(playlists, cancellationToken);
    }

    private async Task DeleteUserReferenceAsync(
        UserDeletedIntegrationEvent notification,
        CancellationToken cancellationToken = default)
    {
        if (!await _unit.UserReferences.ExistsAsync(notification.Id, cancellationToken))
        {
            _logger.LogError(
                "User reference {Id} was not found in Playlists module.",
                notification.Id);
            throw new InvalidOperationException(
                $"User reference {notification.Id} was not found in Playlists module.");
        }
        await _unit.UserReferences.DeleteAsync(notification.Id, cancellationToken);
    }
}
