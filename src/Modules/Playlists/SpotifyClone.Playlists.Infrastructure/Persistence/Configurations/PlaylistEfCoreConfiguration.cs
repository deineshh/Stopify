using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.Rules;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.ValueObjects;
using SpotifyClone.Playlists.Infrastructure.Persistence.Configurations.Converters;

namespace SpotifyClone.Playlists.Infrastructure.Persistence.Configurations;

internal sealed class PlaylistEfCoreConfiguration
    : IEntityTypeConfiguration<Playlist>
{
    public void Configure(EntityTypeBuilder<Playlist> builder)
    {
        builder.ToTable("playlists");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(PlaylistsEfCoreValueConverters.PlaylistIdConverter)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(PlaylistNameRules.MaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(PlaylistDescriptionRules.MaxLength)
            .IsRequired(false);

        builder.Property(x => x.OwnerId)
            .HasColumnName("owner_id")
            .HasConversion(PlaylistsEfCoreValueConverters.UserIdConverter)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(x => x.IsPublic)
            .HasColumnName("is_public")
            .IsRequired();

        builder.OwnsOne(x => x.Cover, coverBuilder =>
        {
            coverBuilder.Property(x => x.ImageId)
                .HasColumnName("cover_image_id")
                .HasConversion(PlaylistsEfCoreValueConverters.ImageIdConverter)
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
                    .HasConversion(PlaylistsEfCoreValueConverters.ImageFileTypeConverter)
                    .IsRequired();

                metadataBuilder.Property(x => x.SizeInBytes)
                    .HasColumnName("cover_metadata_size_in_bytes")
                    .IsRequired();
            });
            builder.Navigation(x => x.Cover).IsRequired(false);
        });

        builder.OwnsMany(t => t.Collaborators, a =>
        {
            a.ToTable("playlist_collaborators");

            a.Property<Guid>("Id")
                .HasColumnName("id");
            a.HasKey("Id");

            a.WithOwner().HasForeignKey("PlaylistId");

            a.Property<PlaylistId>("PlaylistId")
                .HasColumnName("playlist_id");

            a.Property(x => x.Value)
                .HasColumnName("collaborator_id")
                .IsRequired();

            a.HasIndex("PlaylistId", "Value")
                .IsUnique();
        });
        builder.Navigation(x => x.Collaborators)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasMany(x => x.Tracks)
            .WithOne()
            .HasForeignKey(t => t.PlaylistId);
        builder.Navigation(x => x.Tracks)
            .HasField("_tracks")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
    }
}
