using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotifyClone.Billing.Application.Abstractions;
using SpotifyClone.Billing.Application.Abstractions.Services;
using SpotifyClone.Billing.Application.Models;
using SpotifyClone.Billing.Domain.Aggregates.Subscriptions;
using SpotifyClone.Shared.BuildingBlocks.Application.Configuration;
using SpotifyClone.Shared.BuildingBlocks.Application.Email;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Billing.Application.Jobs;

public class PaymentWarningJob(
    IBillingUnitOfWork unit,
    IPaymentProviderService paymentProviderService,
    IEmailSender emailSender,
    IOptions<ApplicationSettings> appSettings,
    ILogger<PaymentWarningJob> logger)
{
    private readonly IBillingUnitOfWork _unit = unit;
    private readonly IPaymentProviderService _paymentProviderService = paymentProviderService;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ApplicationSettings _appSettings = appSettings.Value;
    private readonly ILogger<PaymentWarningJob> _logger = logger;

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<Subscription> subscriptions = await _unit.Subscriptions.GetPastDueSubscriptionsAsync(
            cancellationToken);

        foreach (Subscription sub in subscriptions)
        {
            if (sub.ExternalIdentity.SubscriptionId is null)
            {
                _logger.LogInformation(
                    "Subscription {SubscriptionId} has no external subscription ID. Skipping",
                    sub.Id);
                continue;
            }

            string customerEmail = await _paymentProviderService.GetCustomerEmailAsync(
                sub.ExternalIdentity.CustomerId, cancellationToken);
            string status = await _paymentProviderService.GetSubscriptionStatusAsync(
                sub.ExternalIdentity.SubscriptionId, cancellationToken);

            if (status == PaymentProviderSubscriptionStatuses.PastDue)
            {
                var message = new EmailMessage(
                    To: [customerEmail],
                    Subject: $"Проблема з оплатою Premium - {_appSettings.DomainName}",
                    HtmlBody: $@"
                        <h1>Ой! Виникла проблема з оплатою</h1>
                        <p>Ми не змогли списати кошти за вашу підписку Premium.</p>
                        <p>Будь ласка, перевірте баланс вашої картки, щоб не втратити доступ до музики без реклами.</p>
                        <p>Ми спробуємо повторити оплату автоматично протягом найближчого часу.</p>",
                    PlainTextBody: "Ми не змогли списати кошти за вашу підписку. Перевірте баланс картки."
                );

                Result result = await _emailSender.SendAsync(message, cancellationToken: cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Warning email sent to {Email}", customerEmail);
                }
            }
        }
    }
}
