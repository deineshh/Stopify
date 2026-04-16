using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotifyClone.Catalog.Domain.Aggregates.Genres;
using SpotifyClone.Catalog.Domain.Aggregates.Genres.Rules;
using SpotifyClone.Catalog.Infrastructure.Persistence.Configurations.Converters;

namespace SpotifyClone.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class GenreEfCoreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable("genres");

        builder.HasKey(g => g.Id);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(g => g.Id)
            .HasColumnName("id")
            .HasConversion(CatalogEfCoreValueConverters.GenreIdConverter)
            .ValueGeneratedNever();

        builder.Property(g => g.Name)
            .HasColumnName("name")
            .HasMaxLength(GenreNameRules.MaxLength)
            .IsRequired();
        builder.HasIndex(g => g.Name)
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
                    .IsRequired();

                metadataBuilder.Property(x => x.SizeInBytes)
                    .HasColumnName("cover_metadata_size_in_bytes")
                    .IsRequired();
            });

            builder.Navigation(x => x.Cover).IsRequired(false);
        });

        builder.Ignore(g => g.DomainEvents);
    }
}
