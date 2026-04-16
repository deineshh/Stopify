using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotifyClone.Accounts.Domain.Aggregates.Users;
using SpotifyClone.Accounts.Domain.Aggregates.Users.Rules;
using SpotifyClone.Accounts.Infrastructure.Persistence.Accounts.Configurations.Converters;

namespace SpotifyClone.Accounts.Infrastructure.Persistence.Accounts.Configurations;

internal sealed class UserProfileEfCoreConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(AccountsEfCoreValueConverters.UserIdConverter)
            .ValueGeneratedNever();

        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(DisplayNameRules.MaxLength)
            .IsRequired();

        builder.Property(x => x.BirthDateUtc)
            .HasColumnName("birth_date_utc")
            .IsRequired(false);

        builder.Property(x => x.Gender)
            .HasColumnName("gender")
            .HasConversion(AccountsEfCoreValueConverters.GenderConverter)
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(x => x.Avatar, avatarBuilder =>
        {
            avatarBuilder.Property(a => a.ImageId)
                .HasConversion(AccountsEfCoreValueConverters.ImageIdConverter)
                .HasColumnName("avatar_image_id");

            avatarBuilder.OwnsOne(a => a.Metadata, metadataBuilder =>
            {
                metadataBuilder.Property(m => m.Width)
                    .HasColumnName("avatar_width");

                metadataBuilder.Property(m => m.Height)
                    .HasColumnName("avatar_height");

                metadataBuilder.Property(m => m.FileType)
                    .HasConversion(AccountsEfCoreValueConverters.ImageFileTypeConverter)
                    .HasColumnName("avatar_file_type")
                    .HasMaxLength(10);

                metadataBuilder.Property(m => m.SizeInBytes)
                    .HasColumnName("avatar_size_in_bytes")
                    .IsRequired();
            });
            avatarBuilder.Navigation(a => a.Metadata).IsRequired(false);
        });
        builder.Navigation(x => x.Avatar).IsRequired(false);

        builder.Ignore(u => u.DomainEvents);
    }
}
