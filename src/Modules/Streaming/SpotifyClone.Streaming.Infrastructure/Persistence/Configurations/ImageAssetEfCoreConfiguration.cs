using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotifyClone.Shared.Kernel.Enums;
using SpotifyClone.Streaming.Domain.Aggregates.ImageAssets;
using SpotifyClone.Streaming.Infrastructure.Persistence.Configurations.Converters;

namespace SpotifyClone.Streaming.Infrastructure.Persistence.Configurations;

internal sealed class ImageAssetEfCoreConfiguration
    : IEntityTypeConfiguration<ImageAsset>
{
    public void Configure(EntityTypeBuilder<ImageAsset> builder)
    {
        builder.ToTable("image_assets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(StreamingEfCoreValueConverters.ImageIdConverter)
            .ValueGeneratedNever();

        builder.Property(x => x.IsReady)
            .HasColumnName("is_ready")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.LinkCount)
            .HasColumnName("link_count")
            .IsRequired();

        builder.OwnsOne(x => x.Metadata, metadataBuilder =>
        {
            metadataBuilder.Property(m => m.Width)
                .HasColumnName("metadata_width")
                .IsRequired();

            metadataBuilder.Property(m => m.Height)
                .HasColumnName("metadata_height")
                .IsRequired();

            metadataBuilder.Property(m => m.FileType)
                .HasConversion(StreamingEfCoreValueConverters.ImageFileTypeConverter)
                .HasColumnName("metadata_file_type")
                .HasMaxLength(ImageFileType.MaxLength)
                .IsRequired(false);

            metadataBuilder.Property(m => m.SizeInBytes)
                .HasColumnName("metadata_size_in_bytes")
                .IsRequired();

            builder.Navigation(x => x.Metadata)
                .IsRequired(false);
        });

        builder.Ignore(x => x.DomainEvents);
    }
}
