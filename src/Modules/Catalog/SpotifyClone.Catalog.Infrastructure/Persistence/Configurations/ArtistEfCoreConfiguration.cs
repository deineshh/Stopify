using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotifyClone.Catalog.Domain.Aggregates.Artists;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.Rules;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Catalog.Infrastructure.Persistence.Configurations.Converters;

namespace SpotifyClone.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ArtistEfCoreConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.ToTable("artists");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(CatalogEfCoreValueConverters.ArtistIdConverter)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(ArtistNameRules.MaxLength)
            .IsRequired();

        builder.Property(x => x.Bio)
            .HasColumnName("bio")
            .HasMaxLength(ArtistBioRules.MaxLength);

        builder.Property(x => x.OwnerId)
            .HasColumnName("owner_id")
            .HasConversion(CatalogEfCoreValueConverters.UserIdConverter)
            .IsRequired(false);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(CatalogEfCoreValueConverters.ArtistStatusConverter)
            .IsRequired();

        builder.OwnsOne(x => x.Avatar, avatarBuilder =>
        {
            avatarBuilder.Property(x => x.ImageId)
                .HasColumnName("avatar_image_id")
                .HasConversion(CatalogEfCoreValueConverters.ImageIdConverter)
                .IsRequired();

            avatarBuilder.OwnsOne(x => x.Metadata, metadataBuilder =>
            {
                metadataBuilder.Property(x => x.Width)
                    .HasColumnName("avatar_metadata_width")
                    .IsRequired();

                metadataBuilder.Property(x => x.Height)
                    .HasColumnName("avatar_metadata_height")
                    .IsRequired();

                metadataBuilder.Property(x => x.FileType)
                    .HasColumnName("avatar_metadata_file_type")
                    .HasConversion(CatalogEfCoreValueConverters.ImageFileTypeConverter)
                    .IsRequired();

                metadataBuilder.Property(x => x.SizeInBytes)
                    .HasColumnName("avatar_metadata_size_in_bytes")
                    .IsRequired();
            });

            builder.Navigation(x => x.Avatar).IsRequired(false);
        });

        builder.OwnsOne(x => x.Banner, bannerBuilder =>
        {
            bannerBuilder.Property(x => x.ImageId)
                .HasColumnName("banner_image_id")
                .HasConversion(CatalogEfCoreValueConverters.ImageIdConverter)
                .IsRequired();

            bannerBuilder.OwnsOne(x => x.Metadata, metadataBuilder =>
            {
                metadataBuilder.Property(x => x.Width)
                    .HasColumnName("banner_metadata_width")
                    .IsRequired();

                metadataBuilder.Property(x => x.Height)
                    .HasColumnName("banner_metadata_height")
                    .IsRequired();

                metadataBuilder.Property(x => x.FileType)
                    .HasColumnName("banner_metadata_file_type")
                    .HasConversion(CatalogEfCoreValueConverters.ImageFileTypeConverter)
                    .IsRequired(false);

                metadataBuilder.Property(x => x.SizeInBytes)
                    .HasColumnName("banner_metadata_size_in_bytes")
                    .IsRequired();
            });

            builder.Navigation(x => x.Banner).IsRequired(false);
        });

        builder.OwnsMany(x => x.Gallery, galleryBuilder =>
        {
            galleryBuilder.ToTable("artist_gallery_images");
            galleryBuilder.WithOwner().HasForeignKey("ArtistId");

            galleryBuilder.HasKey("ArtistId", "ImageId");

            galleryBuilder.Property<ArtistId>("ArtistId")
                .HasColumnName("artist_id");

            galleryBuilder.Property(p => p.ImageId)
                .HasColumnName("image_id")
                .HasConversion(CatalogEfCoreValueConverters.ImageIdConverter)
                .IsRequired();

            galleryBuilder.OwnsOne(p => p.Metadata, metadataBuilder =>
            {
                metadataBuilder.Property(m => m.Width)
                    .HasColumnName("metadata_width")
                    .IsRequired();

                metadataBuilder.Property(m => m.Height)
                    .HasColumnName("metadata_height")
                    .IsRequired();

                metadataBuilder.Property(m => m.FileType)
                    .HasColumnName("metadata_file_type")
                    .HasConversion(CatalogEfCoreValueConverters.ImageFileTypeConverter)
                    .IsRequired();

                metadataBuilder.Property(m => m.SizeInBytes)
                    .HasColumnName("metadata_size_in_bytes")
                    .IsRequired();
            });
        });
        builder.Navigation(x => x.Gallery)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
    }
}
