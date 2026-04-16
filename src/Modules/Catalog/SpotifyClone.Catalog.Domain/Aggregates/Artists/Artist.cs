using SpotifyClone.Catalog.Domain.Aggregates.Artists.Enums;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.Events;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.Exceptions;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.Rules;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Domain.Aggregates.Artists;

public sealed class Artist : AggregateRoot<ArtistId, Guid>
{
    private const short MaxGalleryImages = 125;

    private readonly List<ArtistGalleryImage> _gallery = [];

    public string Name { get; private set; } = null!;
    public string? Bio { get; private set; }
    public UserId? OwnerId { get; private set; }
    public ArtistStatus Status { get; private set; } = null!;
    public ArtistAvatarImage? Avatar { get; private set; }
    public ArtistBannerImage? Banner { get; private set; }
    public IReadOnlyCollection<ArtistGalleryImage> Gallery => _gallery.AsReadOnly();

    public static Artist Create(ArtistId id, string name, UserId ownerId)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(ownerId);

        ArtistNameRules.Validate(name);

        ArtistStatus status = ArtistStatus.NotVerified;
        var artist = new Artist(id, name, null, ownerId, status, null, null, []);

        artist.RaiseDomainEvent(new ArtistCreatedDomainEvent(artist.Id, artist.Name, artist.Status));

        return artist;
    }

    public void LinkNewAvatar(ArtistAvatarImage avatar)
    {
        ArgumentNullException.ThrowIfNull(avatar);

        ThrowIfBanned("Cannot link a new avatar image to a banned artist.");

        UnlinkAvatar();

        Avatar = avatar;
        RaiseDomainEvent(new ArtistLinkedToAvatarImageDomainEvent(Id, Avatar.ImageId));
    }

    public void LinkNewBanner(ArtistBannerImage banner)
    {
        ArgumentNullException.ThrowIfNull(banner);

        ThrowIfBanned("Cannot link a new banner image to a banned artist.");
        ThrowIfNotVerified("Cannot link a new banner image to a non-verified artist.");

        UnlinkBanner();

        Banner = banner;
        RaiseDomainEvent(new ArtistLinkedToBannerImageDomainEvent(Banner.ImageId));
    }

    public void UnlinkAvatar()
    {
        if (Avatar is null)
        {
            return;
        }

        RaiseDomainEvent(new ArtistUnlinkedFromAvatarImageDomainEvent(Id, Avatar.ImageId));
        Avatar = null;
    }

    public void UnlinkBanner()
    {
        if (Banner is null)
        {
            return;
        }

        RaiseDomainEvent(new ArtistUnlinkedFromBannerImageDomainEvent(Banner.ImageId));
        Banner = null;
    }

    public void UnlinkOwner()
        => OwnerId = null;

    public void Verify()
    {
        ThrowIfBanned("Cannot verify a banned artist.");

        if (Status.IsVerified)
        {
            return;
        }

        if (Avatar is null)
        {
            throw new CannotVerifyArtistDomainException("An artist must have an avatar image to be verified.");
        }

        Status = ArtistStatus.Verified;

        RaiseDomainEvent(new ArtistVerifiedDomainEvent(Id));
    }

    public void Unverify()
    {
        ThrowIfBanned("Cannot unverify a banned artist.");

        if (!Status.IsVerified)
        {
            return;
        }

        Status = ArtistStatus.NotVerified;

        RaiseDomainEvent(new ArtistUnverifiedDomainEvent(Id));
    }

    public void Rename(string name)
    {
        ThrowIfBanned("Cannot rename a banned artist.");

        ArtistNameRules.Validate(name);
        Name = name;

        RaiseDomainEvent(new ArtistRenamedDomainEvent(Id, Name));
    }

    public void UpdateBio(string? bio)
    {
        ThrowIfBanned("Cannot update the bio of a banned artist.");
        ThrowIfNotVerified("Cannot update the bio of a non-verified artist.");

        if (!Status.IsVerified)
        {
            throw new ArtistNotVerifiedDomainException("Only verified artists can have a bio.");
        }

        if (string.IsNullOrEmpty(bio))
        {
            bio = null;
        }
        ArtistBioRules.Validate(bio);

        Bio = bio;
    }

    public void AddGalleryImage(ArtistGalleryImage image)
    {
        ThrowIfBanned("Cannot add a gallery image to a banned artist.");
        ThrowIfNotVerified("Cannot add a new gallery image to a non-verified artist.");

        if (!Status.IsVerified)
        {
            throw new ArtistNotVerifiedDomainException("Only verified artists can have gallery images.");
        }

        if (_gallery.Contains(image))
        {
            return;
        }

        if (_gallery.Count >= MaxGalleryImages)
        {
            throw new InvalidArtistGalleryImageDomainException(
                $"Artist cannot have more than {MaxGalleryImages} gallery images.");
        }

        _gallery.Add(image);
        RaiseDomainEvent(new GalleryImageAddedToArtistDomainEvent(image.ImageId));
    }

    public void RemoveGalleryImage(ImageId imageId)
    {
        ArtistGalleryImage? image = _gallery.FirstOrDefault(i => i.ImageId == imageId)
            ?? throw new InvalidArtistGalleryImageDomainException(
                "Gallery image was not found in artist");

        _gallery.Remove(image);
        RaiseDomainEvent(new GalleryImageRemovedFromArtistDomainEvent(image.ImageId));
    }

    public void Ban()
    {
        if (Status.IsBanned)
        {
            return;
        }

        Status = ArtistStatus.Banned;
        RaiseDomainEvent(new ArtistBannedDomainEvent(Id));
    }

    public void Unban()
    {
        if (!Status.IsBanned)
        {
            return;
        }

        Status = ArtistStatus.NotVerified;

        RaiseDomainEvent(new ArtistUnbannedDomainEvent(Id, Name, Status, Avatar?.ImageId));
    }

    private void ThrowIfBanned(string message)
    {
        if (Status.IsBanned)
        {
            throw new ArtistBannedDomainException(message);
        }
    }

    private void ThrowIfNotVerified(string message)
    {
        if (!Status.IsVerified)
        {
            throw new ArtistNotVerifiedDomainException(message);
        }
    }

    private Artist(
        ArtistId id,
        string name,
        string? bio,
        UserId? ownerId,
        ArtistStatus status,
        ArtistAvatarImage? avatar,
        ArtistBannerImage? banner,
        IReadOnlyCollection<ArtistGalleryImage> gallery)
        : base(id)
    {
        Name = name;
        Bio = bio;
        OwnerId = ownerId;
        Status = status;
        Avatar = avatar;
        Banner = banner;
        _gallery = gallery.ToList();
    }

    private Artist()
    {
    }
}
