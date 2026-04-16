using MediatR;
using SpotifyClone.Accounts.Application.Abstractions;
using SpotifyClone.Accounts.Domain.Aggregates.Users.Events;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;
using SpotifyClone.Shared.IntegrationEvents.Accounts.Users;

namespace SpotifyClone.Accounts.Application.EventHandlers.Users;

internal sealed class UserProfileDeletedDomainEventHandler(
    IAccountsUnitOfWork unit)
    : INotificationHandler<UserProfileDeletedDomainEvent>
{
    private readonly IAccountsUnitOfWork _unit = unit;

    public async Task Handle(
        UserProfileDeletedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new UserDeletedIntegrationEvent(
                notification.Id.Value);

        var message = OutboxMessage.FromIntegrationEvent(integrationEvent);

        await _unit.OutboxMessages.AddAsync(message, cancellationToken);
        await _unit.CommitAsync(cancellationToken);
    }
}
