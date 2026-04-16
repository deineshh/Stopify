using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotifyClone.Streaming.Domain.Aggregates.PlaybackHistoryEntries;
using SpotifyClone.Streaming.Infrastructure.Persistence.Configurations.Converters;

namespace SpotifyClone.Streaming.Infrastructure.Persistence.Configurations;

internal sealed class PlaybackHistoryEntryEfCoreConfiguration
    : IEntityTypeConfiguration<PlaybackHistoryEntry>
{
    public void Configure(EntityTypeBuilder<PlaybackHistoryEntry> builder)
    {
        builder.ToTable("playback_history_entries");

        builder.HasKey(e => e.Id);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasConversion(StreamingEfCoreValueConverters.PlaybackHistoryEntryIdConverter)
            .ValueGeneratedNever();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasConversion(StreamingEfCoreValueConverters.UserIdConverter)
            .IsRequired();

        builder.Property(e => e.TrackId)
            .HasColumnName("track_id")
            .HasConversion(StreamingEfCoreValueConverters.TrackIdConverter)
            .IsRequired();

        builder.OwnsOne(e => e.Context, metadataBuilder =>
        {
            metadataBuilder.Property(c => c.Type)
                .HasColumnName("context_type")
                .IsRequired();

            metadataBuilder.Property(c => c.ExternalId)
                .HasColumnName("context_external_id")
                .IsRequired(false);

            builder.Navigation(e => e.Context)
                .IsRequired();
        });

        builder.Property(e => e.PlayedAtUtc)
            .HasColumnName("played_at_utc")
            .IsRequired();

        builder.Ignore(e => e.DomainEvents);
    }
}
