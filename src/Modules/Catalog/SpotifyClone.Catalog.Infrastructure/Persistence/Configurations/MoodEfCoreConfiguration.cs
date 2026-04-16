using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotifyClone.Catalog.Domain.Aggregates.Moods;
using SpotifyClone.Catalog.Domain.Aggregates.Moods.Rules;
using SpotifyClone.Catalog.Infrastructure.Persistence.Configurations.Converters;

namespace SpotifyClone.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class MoodEfCoreConfiguration : IEntityTypeConfiguration<Mood>
{
    public void Configure(EntityTypeBuilder<Mood> builder)
    {
        builder.ToTable("moods");

        builder.HasKey(m => m.Id);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .HasConversion(CatalogEfCoreValueConverters.MoodIdConverter)
            .ValueGeneratedNever();

        builder.Property(m => m.Name)
            .HasColumnName("name")
            .HasMaxLength(MoodNameRules.MaxLength)
            .IsRequired();
        builder.HasIndex(m => m.Name)
            .IsUnique();

        builder.OwnsOne(x => x.Cover, coverBuilder =>
        {
            coverBuilder.Property(x => x.ImageId)
                .HasColumnName("cover_image_id")
                .HasConversion(CatalogEfCoreValueConverters.ImageIdConverter)
                .IsRequired();

            coverBuilder.OwnsOne(x => x.Metadata, metadataBuilder =>
            {
                metadataBuilder.Property(x => x.Width)
                    .HasColumnName("cover_metadata_width")
                    .IsRequired();

                metadataBuilder.Property(x => x.Height)
                    .HasColumnName("cover_metadata_height")
                    .IsRequired();

                metadataBuilder.Property(x => x.FileType)
                    .HasColumnName("cover_metadata_file_type")
                    .HasConversion(CatalogEfCoreValueConverters.ImageFileTypeConverter)
                    .IsRequired(false);

                metadataBuilder.Property(x => x.SizeInBytes)
                    .HasColumnName("cover_metadata_size_in_bytes")
                    .IsRequired();
            });

            builder.Navigation(x => x.Cover).IsRequired(false);
        });

        builder.Ignore(m => m.DomainEvents);
    }
}
