using MediatR;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Domain.Aggregates.Artists;
using SpotifyClone.Shared.IntegrationEvents.Accounts.Users;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Application.EventHandlers.Users;

internal sealed class UserDeletedIntegrationEventHandler(
    ICatalogUnitOfWork unit)
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task Handle(
        UserDeletedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        IEnumerable<Artist> artists = await _unit.Artists.GetAllByOwnerAsync(
            UserId.From(notification.Id), cancellationToken);

        foreach (Artist artist in artists)
        {
            artist.UnlinkOwner();
        }

        await _unit.CommitAsync(cancellationToken);
    }
}
