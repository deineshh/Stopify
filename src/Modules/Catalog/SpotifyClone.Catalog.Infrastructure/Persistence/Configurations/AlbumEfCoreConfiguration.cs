using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotifyClone.Catalog.Domain.Aggregates.Albums;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.Rules;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.ValueObjects;
using SpotifyClone.Catalog.Infrastructure.Persistence.Configurations.Converters;

namespace SpotifyClone.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class AlbumEfCoreConfiguration : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.ToTable("albums");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(CatalogEfCoreValueConverters.AlbumIdConverter)
            .ValueGeneratedNever();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(AlbumTitleRules.MaxLength)
            .IsRequired();

        builder.Property(x => x.ReleaseDate)
            .HasColumnName("release_date");
        builder.HasIndex(x => x.ReleaseDate);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(CatalogEfCoreValueConverters.AlbumStatusConverter)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion(CatalogEfCoreValueConverters.AlbumTypeConverter)
            .HasColumnName("type")
            .IsRequired();

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

        builder.OwnsMany(t => t.MainArtists, a =>
        {
            a.ToTable("album_main_artists");

            a.Property<Guid>("Id")
                .HasColumnName("id");
            a.HasKey("Id");

            a.WithOwner().HasForeignKey("AlbumId");

            a.Property<AlbumId>("AlbumId")
                .HasColumnName("album_id");

            a.Property(x => x.Value)
                .HasColumnName("artist_id")
                .IsRequired();

            a.HasIndex("AlbumId", "Value")
                .IsUnique();
        });
        builder.Navigation(x => x.MainArtists)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasMany(x => x.Tracks)
            .WithOne()
            .HasForeignKey("album_id");
        builder.Navigation(x => x.Tracks)
            .HasField("_tracks")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
    }
}
