using MediatR;
using SpotifyClone.Accounts.Application.Abstractions;
using SpotifyClone.Accounts.Domain.Aggregates.Users.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Accounts.Users;

namespace SpotifyClone.Accounts.Application.EventHandlers.Users;

internal sealed class UserProfileDetailsEditedDomainEventHandler(
    IAccountsUnitOfWork unit)
    : INotificationHandler<UserProfileDetailsEditedDomainEvent>
{
    private readonly IAccountsUnitOfWork _unit = unit;

    public async Task Handle(
        UserProfileDetailsEditedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new UserDetailsChangedIntegrationEvent(
                notification.Id.Value,
                notification.DisplayName);

        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);

        await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
