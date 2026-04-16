using MediatR;
using SpotifyClone.Accounts.Application.Abstractions;
using SpotifyClone.Accounts.Domain.Aggregates.Users.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Accounts.Users;

namespace SpotifyClone.Accounts.Application.EventHandlers.Users;

internal sealed class UserProfileLinkedToAvatarImageDomainEventHandler(
    IAccountsUnitOfWork unit)
    : INotificationHandler<UserProfileLinkedToAvatarImageDomainEvent>
{
    private readonly IAccountsUnitOfWork _unit = unit;

    public async Task Handle(
        UserProfileLinkedToAvatarImageDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new UserAvatarChangedIntegrationEvent(
                notification.Id.Value,
                notification.AvatarImageId.Value);

        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);

        await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
