using Microsoft.Extensions.Options;
using SpotifyClone.Billing.Application.Abstractions.Services;
using SpotifyClone.Billing.Application.Models;
using SpotifyClone.Shared.BuildingBlocks.Application.Configuration;
using Stripe;
using Stripe.Checkout;

namespace SpotifyClone.Billing.Infrastructure.Payment;

internal sealed class StripePaymentProviderService
    : IPaymentProviderService
{
    private readonly StripeSettings _stripeSettings;
    private readonly ApplicationSettings _appSettings;

    public StripePaymentProviderService(
        IOptions<StripeSettings> stripeSettings,
        IOptions<ApplicationSettings> appSettings)
    {
        _stripeSettings = stripeSettings.Value;
        _appSettings = appSettings.Value;

        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    public async Task<string> CreateCheckoutSessionUrlAsync(
        Guid userId,
        string? email,
        CancellationToken cancellationToken = default)
    {
        var options = new SessionCreateOptions
        {
            CustomerEmail = email,
            Mode = "subscription",

            Metadata = new Dictionary<string, string>
            {
                { "UserId", userId.ToString() }
            },

            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = _stripeSettings.PremiumPriceId,
                    Quantity = 1,
                }
            ],

            SubscriptionData = new SessionSubscriptionDataOptions
            {
                TrialPeriodDays = 30,
                // Метадані потрібні, щоб коли прийде Webhook, ми знали, якому юзеру видати Преміум
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId.ToString() }
                }
            },

            SuccessUrl = $"{_appSettings.FrontendUrl}/premium/success",
            CancelUrl = $"{_appSettings.FrontendUrl}/premium/cancel",
        };

        var service = new SessionService();
        Session session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return session.Url;
    }

    public async Task<string> GetSubscriptionStatusAsync(
        string externalSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new SubscriptionService();

            Subscription stripeSubscription = await service.GetAsync(
                externalSubscriptionId,
                cancellationToken: cancellationToken);

            return stripeSubscription.Status.ToLowerInvariant();
        }
        catch (StripeException ex)
        {
            if (ex.StripeError?.Code == PaymentProviderSubscriptionStatuses.ResourceMissing)
            {
                return PaymentProviderSubscriptionStatuses.Canceled;
            }

            throw;
        }
    }

    public async Task<string> GetCustomerEmailAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var service = new CustomerService();
        Customer customer = await service.GetAsync(customerId, cancellationToken: cancellationToken);
        return customer.Email;
    }

    public async Task CancelSubscriptionAtPeriodEndAsync(
        string externalSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        var service = new SubscriptionService();

        var options = new SubscriptionUpdateOptions
        {
            CancelAtPeriodEnd = true
        };

        await service.UpdateAsync(
            externalSubscriptionId,
            options,
            cancellationToken: cancellationToken);
    }
}
