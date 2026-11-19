using Microsoft.Extensions.Logging;
using SpotifyClone.Billing.Application.Abstractions;
using SpotifyClone.Billing.Application.Abstractions.Services;
using SpotifyClone.Billing.Application.Models;
using SpotifyClone.Billing.Domain.Aggregates.Subscriptions;

namespace SpotifyClone.Billing.Application.Jobs;

public class SubscriptionSyncJob(
    IBillingUnitOfWork unit,
    IPaymentProviderService paymentProviderService,
    ILogger<SubscriptionSyncJob> logger)
{
    private readonly IBillingUnitOfWork _unit = unit;
    private readonly IPaymentProviderService _paymentProviderService = paymentProviderService;
    private readonly ILogger<SubscriptionSyncJob> _logger = logger;

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<Subscription> subscriptionsToCheck
            = await _unit.Subscriptions.GetActiveExpiredSubscriptionsAsync(DateTimeOffset.UtcNow, cancellationToken);

        if (!subscriptionsToCheck.Any())
        {
            _logger.LogInformation("No expired subscriptions found to sync.");
            return;
        }

        foreach (Subscription subscription in subscriptionsToCheck)
        {
            if (subscription.ExternalIdentity.SubscriptionId == null)
            {
                _logger.LogWarning(
                    "Subscription {SubId} for user {UserId} has no external subscription ID. Skipping.",
                    subscription.Id.Value, subscription.UserId.Value);
                continue;
            }

            try
            {
                string externalStatus = await _paymentProviderService.GetSubscriptionStatusAsync(
                    subscription.ExternalIdentity.SubscriptionId,
                    cancellationToken);

                if (externalStatus == PaymentProviderSubscriptionStatuses.Canceled ||
                    externalStatus == PaymentProviderSubscriptionStatuses.IncompleteExpired)
                {
                    subscription.Expire();

                    _logger.LogInformation("Subscription {SubId} for user {UserId} marked as Expired.",
                        subscription.Id.Value, subscription.UserId.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing subscription {SubId}", subscription.Id.Value);
            }
        }

        await _unit.CommitAsync(cancellationToken);
    }
}
