using SpotifyClone.Accounts.Domain.Aggregates.Users.Enums;
using SpotifyClone.Accounts.Domain.Aggregates.Users.Events;
using SpotifyClone.Accounts.Domain.Aggregates.Users.Rules;
using SpotifyClone.Accounts.Domain.Aggregates.Users.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Accounts.Domain.Aggregates.Users;

public sealed class UserProfile : AggregateRoot<UserId, Guid>
{
    public string DisplayName { get; private set; } = null!;
    public DateTimeOffset? BirthDateUtc { get; private set; }
    public Gender Gender { get; private set; } = null!;
    public AvatarImage? Avatar { get; private set; }

    public static UserProfile Create(
        UserId id, string displayName, DateTimeOffset? birthDate, Gender gender)
    {
        DisplayNameRules.Validate(displayName);

        if (birthDate is not null)
        {
            birthDate = birthDate.Value.ToUniversalTime();
            BirthDateRules.Validate(birthDate.Value);
        }

        return new UserProfile(id, displayName, birthDate, gender);
    }

    public void LinkNewAvatar(AvatarImage avatar)
    {
        ArgumentNullException.ThrowIfNull(avatar);

        UnlinkAvatar();

        Avatar = avatar;
        RaiseDomainEvent(new UserProfileLinkedToAvatarImageDomainEvent(Id, Avatar.ImageId));
    }

    public void UnlinkAvatar()
    {
        if (Avatar is null)
        {
            return;
        }

        RaiseDomainEvent(new UserProfileUnlinkedFromAvatarImageDomainEvent(Id, Avatar.ImageId));
        Avatar = null;
    }

    public void EditDetails(string displayName)
    {
        DisplayNameRules.Validate(displayName);
        DisplayName = displayName;

        RaiseDomainEvent(new UserProfileDetailsEditedDomainEvent(Id, DisplayName));
    }

    public void EditPersonalInfo(Gender gender, DateTimeOffset birthDateUtc)
    {
        ArgumentNullException.ThrowIfNull(gender);

        Gender = gender;
        BirthDateUtc = birthDateUtc.ToUniversalTime();
    }

    public void ProcessRegistration(string email, string confirmationToken)
        => RaiseDomainEvent(new UserRegisteredDomainEvent(
            Id, email, confirmationToken, DisplayName, Avatar?.ImageId));

    public void PrepareForDeletion()
    {
        UnlinkAvatar();
        RaiseDomainEvent(new UserProfileDeletedDomainEvent(Id));
    }

    private UserProfile()
    {
    }

    private UserProfile(
        UserId id, string displayName, DateTimeOffset? birthDate, Gender gender, AvatarImage? avatarImage = null)
        : base(id)
    {
        DisplayName = displayName;
        BirthDateUtc = birthDate;
        Gender = gender;
        Avatar = avatarImage;
    }
}
