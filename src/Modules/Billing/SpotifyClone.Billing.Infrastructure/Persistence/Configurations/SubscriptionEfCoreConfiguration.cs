using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotifyClone.Billing.Domain.Aggregates.Subscriptions;
using SpotifyClone.Billing.Infrastructure.Persistence.Configurations.Converters;

namespace SpotifyClone.Billing.Infrastructure.Persistence.Configurations;

internal sealed class SubscriptionEfCoreConfiguration
    : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(BillingEfCoreValueConverters.SubscriptionIdConverter)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasConversion(BillingEfCoreValueConverters.UserIdConverter)
            .IsRequired();

        builder.OwnsOne(x => x.ExternalIdentity, coverBuilder =>
        {
            coverBuilder.Property(x => x.CustomerId)
                .HasColumnName("external_customer_id")
                .IsRequired();

            coverBuilder.Property(x => x.SubscriptionId)
                .HasColumnName("external_subscription_id")
                .IsRequired(false);
        });
        builder.Navigation(x => x.ExternalIdentity).IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.CurrentPeriodStart)
            .HasColumnName("current_period_start")
            .IsRequired(false);

        builder.Property(x => x.CurrentPeriodEnd)
            .HasColumnName("current_period_end")
            .IsRequired(false);

        builder.Property(x => x.CancelAtPeriodEnd)
            .HasColumnName("cancel_at_period_end")
            .IsRequired();

        builder.Ignore(x => x.DomainEvents);
    }
}
