using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.Rules;
using SpotifyClone.Catalog.Infrastructure.Persistence.Configurations.Converters;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class TrackEfCoreConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.ToTable("tracks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(CatalogEfCoreValueConverters.TrackIdConverter)
            .ValueGeneratedNever();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(TrackTitleRules.MaxLength)
            .IsRequired();

        builder.Property(x => x.Duration)
            .HasColumnName("duration");

        builder.Property(x => x.ReleaseDate)
            .HasColumnName("release_date");

        builder.Property(x => x.ContainsExplicitContent)
            .HasColumnName("contains_explicit_content")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(CatalogEfCoreValueConverters.TrackStatusConverter)
            .IsRequired();

        builder.Property(x => x.AudioFileId)
            .HasColumnName("audio_file_id")
            .HasConversion(CatalogEfCoreValueConverters.AudioFileIdConverter);
        builder.HasIndex(x => x.AudioFileId)
            .IsUnique();

        builder.Property(x => x.AlbumId)
            .HasColumnName("album_id")
            .HasConversion(CatalogEfCoreValueConverters.AlbumIdNullableConverter);
        builder.HasIndex(x => x.AlbumId);

        builder.OwnsMany(t => t.MainArtists, a =>
        {
            a.ToTable("track_main_artists");

            a.WithOwner()
                .HasForeignKey("TrackId");

            a.Property<TrackId>("TrackId")
                .HasColumnName("track_id");

            a.Property(x => x.Value)
                .HasColumnName("artist_id")
                .IsRequired();

            a.HasKey("TrackId", "Value");

            a.HasIndex("TrackId", "Value")
                .IsUnique();
        });
        builder.Navigation(x => x.MainArtists)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(t => t.FeaturedArtists, a =>
        {
            a.ToTable("track_featured_artists");

            a.WithOwner()
                .HasForeignKey("TrackId");

            a.Property<TrackId>("TrackId")
                .HasColumnName("track_id");

            a.Property(x => x.Value)
                .HasColumnName("artist_id")
                .IsRequired();

            a.HasKey("TrackId", "Value");

            a.HasIndex("TrackId", "Value")
                .IsUnique();
        });
        builder.Navigation(x => x.FeaturedArtists)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(t => t.Genres, a =>
        {
            a.ToTable("track_genres");

            a.Property<Guid>("Id")
                .HasColumnName("id");
            a.HasKey("Id");

            a.WithOwner().HasForeignKey("TrackId");

            a.Property<TrackId>("TrackId")
                .HasColumnName("track_id");

            a.Property(x => x.Value)
                .HasColumnName("genre_id")
                .IsRequired();

            a.HasIndex("TrackId", "Value")
                .IsUnique();
        });
        builder.Navigation(x => x.Genres)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(t => t.Moods, a =>
        {
            a.ToTable("track_moods");

            a.Property<Guid>("Id")
                .HasColumnName("id");
            a.HasKey("Id");

            a.WithOwner().HasForeignKey("TrackId");

            a.Property<TrackId>("TrackId")
                .HasColumnName("track_id");

            a.Property(x => x.Value)
                .HasColumnName("mood_id")
                .IsRequired();

            a.HasIndex("TrackId", "Value")
                .IsUnique();
        });
        builder.Navigation(x => x.Moods)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
    }
}
