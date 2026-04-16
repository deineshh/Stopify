using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotifyClone.Streaming.Domain.Aggregates.AudioAssets;
using SpotifyClone.Streaming.Infrastructure.Persistence.Configurations.Converters;

namespace SpotifyClone.Streaming.Infrastructure.Persistence.Configurations;

internal sealed class AudioAssetEfCoreConfiguration
    : IEntityTypeConfiguration<AudioAsset>
{
    public void Configure(EntityTypeBuilder<AudioAsset> builder)
    {
        builder.ToTable("audio_assets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(StreamingEfCoreValueConverters.AudioAssetIdConverter)
            .ValueGeneratedNever();

        builder.Property(x => x.Duration)
            .HasColumnName("duration")
            .IsRequired();

        builder.Property(x => x.Format)
            .HasColumnName("format")
            .HasConversion(StreamingEfCoreValueConverters.AudioFormatConverter)
            .HasMaxLength(16);

        builder.Property(x => x.SizeInBytes)
            .HasColumnName("size_in_bytes");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(StreamingEfCoreValueConverters.AudioAssetStatusConverter)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.TrackId)
            .HasColumnName("track_id")
            .HasConversion(StreamingEfCoreValueConverters.NullableTrackIdConverter);

        builder.Ignore(x => x.DomainEvents);
    }
}
