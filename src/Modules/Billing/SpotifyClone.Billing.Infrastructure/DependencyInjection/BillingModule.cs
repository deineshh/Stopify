using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpotifyClone.Billing.Application;
using SpotifyClone.Billing.Application.Abstractions;
using SpotifyClone.Billing.Application.Abstractions.Data;
using SpotifyClone.Billing.Application.Abstractions.Repositories;
using SpotifyClone.Billing.Application.Abstractions.Services;
using SpotifyClone.Billing.Application.Behaviors;
using SpotifyClone.Billing.Application.Errors;
using SpotifyClone.Billing.Application.Jobs;
using SpotifyClone.Billing.Domain.Aggregates.Subscriptions;
using SpotifyClone.Billing.Infrastructure.Payment;
using SpotifyClone.Billing.Infrastructure.Persistence;
using SpotifyClone.Billing.Infrastructure.Persistence.Database;
using SpotifyClone.Billing.Infrastructure.Persistence.Queries;
using SpotifyClone.Billing.Infrastructure.Persistence.Repositories;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Mappers;

namespace SpotifyClone.Billing.Infrastructure.DependencyInjection;

public static class BillingModule
{
    public static IServiceCollection AddBillingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
            BillingApplicationAssemblyReference.Assembly));

        services.AddValidatorsFromAssembly(BillingApplicationAssemblyReference.Assembly);

        services.AddDbContext<BillingAppDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("MainDb"),
            b => b.MigrationsAssembly(typeof(BillingAppDbContext).Assembly.FullName)));

        services.Configure<StripeSettings>(
            configuration.GetSection(StripeSettings.SectionName));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IBillingUnitOfWork>());
        services.AddScoped<IBillingUnitOfWork, BillingEfCoreUnitOfWork>();

        services.AddScoped<ISubscriptionRepository, SubscriptionEfCoreRepository>();
        services.AddScoped<IOutboxRepository, OutboxEfCoreRepository>();

        services.AddScoped<ISubscriptionReadService, SubscriptionEfCoreReadService>();

        services.AddScoped<IDomainExceptionMapper, BillingDomainExceptionMapper>();
        services.AddScoped<IPaymentProviderService, StripePaymentProviderService>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(BillingTransactionalPipelineBehavior<,>));
        services.AddTransient<ProcessOutboxMessagesJob>();

        return services;
    }

    public static void UseBillingModule(this IApplicationBuilder app)
    {
        IRecurringJobManager recurringJobManager =
            app.ApplicationServices.GetRequiredService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdate<ProcessOutboxMessagesJob>(
            "billing-outbox-processor",
            job => job.ProcessAsync(),
            "*/5 * * * * *" // Every 5 seconds (Cron expression)
        );

        recurringJobManager.AddOrUpdate<SubscriptionSyncJob>(
            "subscription-sync",
            job => job.ProcessAsync(),
            Cron.Daily(3));

        RecurringJob.AddOrUpdate<PaymentWarningJob>(
            "payment-warning",
            job => job.ProcessAsync(CancellationToken.None),
            Cron.Daily(10));
    }
}
