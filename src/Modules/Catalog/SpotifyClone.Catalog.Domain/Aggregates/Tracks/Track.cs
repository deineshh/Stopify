using SpotifyClone.Catalog.Domain.Aggregates.Albums.Exceptions;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.ValueObjects;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Catalog.Domain.Aggregates.Genres.ValueObjects;
using SpotifyClone.Catalog.Domain.Aggregates.Moods.ValueObjects;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.Enums;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.Events;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.Exceptions;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.Rules;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Domain.Aggregates.Tracks;

public sealed class Track : AggregateRoot<TrackId, Guid>
{
    private readonly HashSet<ArtistId> _mainArtists = [];
    private readonly HashSet<ArtistId> _featuredArtists = [];
    private readonly HashSet<GenreId> _genres = [];
    private readonly HashSet<MoodId> _moods = [];

    public string Title { get; private set; } = null!;
    public TimeSpan? Duration { get; private set; }
    public DateTimeOffset? ReleaseDate { get; private set; }
    public bool ContainsExplicitContent { get; private set; }
    public TrackStatus Status { get; private set; } = null!;
    public AudioFileId? AudioFileId { get; private set; }
    public AlbumId? AlbumId { get; private set; }
    public IReadOnlySet<ArtistId> MainArtists => _mainArtists.AsReadOnly();
    public IReadOnlySet<ArtistId> FeaturedArtists => _featuredArtists.AsReadOnly();
    public IReadOnlySet<GenreId> Genres => _genres.AsReadOnly();
    public IReadOnlySet<MoodId> Moods => _moods.AsReadOnly();

    public static Track Create(
        TrackId id,
        string title,
        bool containsExplicitContent,
        AlbumId albumId,
        bool isAlbumPublished,
        IEnumerable<ArtistId> mainArtists,
        IEnumerable<ArtistId> featuredArtists,
        IEnumerable<GenreId> genres,
        IEnumerable<MoodId> moods)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(albumId);
        ArgumentNullException.ThrowIfNull(mainArtists);
        ArgumentNullException.ThrowIfNull(featuredArtists);
        ArgumentNullException.ThrowIfNull(genres);
        ArgumentNullException.ThrowIfNull(moods);

        TrackTitleRules.Validate(title);

        if (!mainArtists.Any())
        {
            throw new InvalidTrackMainArtistsDomainException(
                "A track must have at least one main artist.");
        }

        if (!genres.Any())
        {
            throw new InvalidTrackGenresDomainException(
                "A track must have at least one genre.");
        }

        if (!moods.Any())
        {
            throw new InvalidTrackMoodsDomainException(
                "A track must have at least one mood.");
        }

        var track = new Track(
            id, title, null, null, containsExplicitContent, TrackStatus.Draft, null, null,
            mainArtists, featuredArtists, genres, moods);

        track.MoveToAlbum(albumId, isAlbumPublished);

        return track;
    }

    public void MoveToAlbum(AlbumId albumId, bool isAlbumPublished)
    {
        if (AlbumId == albumId)
        {
            // Prevent circular domain event raising
            return;
        }

        if (AlbumId is not null)
        {
            throw new TrackAlreadyAttachedToAnAlbumDomainException(
                "This track is already attached to an album. Consider removing it from the album first.");
        }

        if (isAlbumPublished)
        {
            throw new AlbumAlreadyPublishedDomainException(
                "Cannot move track into this album because the album is already published.");
        }

        RaiseDomainEvent(new TrackMovedInAlbumDomainEvent(Id, albumId));
        AlbumId = albumId;
    }

    public void MarkAsReadyToPublish()
    {
        if (AlbumId is null)
        {
            throw new CannotMarkTrackAsReadyToPublishDomainException(
                "Cannot mark track as ready to publish because there's no album attached to it.");
        }

        if (AudioFileId is null)
        {
            throw new CannotMarkTrackAsReadyToPublishDomainException(
                "Cannot mark track as ready to publish because there's no audio file linked to it.");
        }

        if (_genres.Count <= 0)
        {
            throw new InvalidTrackGenresDomainException(
                "A track must have at least one genre.");
        }

        if (_moods.Count <= 0)
        {
            throw new InvalidTrackMoodsDomainException(
                "A track must have at least one mood.");
        }

        Status = TrackStatus.ReadyToPublish;
        RaiseDomainEvent(new TrackMarkedAsReadyToPublishDomainEvent(Id, AlbumId));
    }

    public void LinkAudioFile(AudioFileId audioFileId, TimeSpan duration)
    {
        ArgumentNullException.ThrowIfNull(audioFileId);

        if (AudioFileId is not null)
        {
            throw new TrackAlreadyLinkedToAudioFileDomainException(
                "Track is already linked to an audio file. " +
                "Consider to unlink the linked audio file first.");
        }

        TrackDurationRules.Validate(duration);

        Duration = duration;
        AudioFileId = audioFileId;
    }

    public void UnlinkAudioFile()
    {
        if (AudioFileId is null)
        {
            return;
        }

        if (Status.IsPublished)
        {
            throw new TrackAlreadyPublishedDomainException("Cannot unlink published track from it's audio file.");
        }

        RaiseDomainEvent(new TrackUnlinkedFromAudioFileDomainEvent(AlbumId, AudioFileId));

        AudioFileId = null;
        Status = TrackStatus.Draft;
    }

    public void Archive()
    {
        AlbumId = null;
        Status = TrackStatus.Archived;
        RaiseDomainEvent(new TrackArchivedDomainEvent(Id));
    }

    public void Publish(DateTimeOffset releaseDate)
    {
        releaseDate = releaseDate.ToUniversalTime();

        if (Status.IsPublished)
        {
            throw new TrackAlreadyPublishedDomainException("This track has been already published.");
        }

        if (!Status.IsReadyToPublish)
        {
            throw new CannotPublishTrackDomainException("This track is not ready to publish.");
        }

        if (releaseDate < DateTimeOffset.UtcNow.AddMinutes(-1))
        {
            throw new InvalidTrackReleaseDateDomainException("Track release date cannot be in the past.");
        }

        ReleaseDate = releaseDate;
        Status = TrackStatus.Published;
        RaiseDomainEvent(new TrackPublishedDomainEvent(
            Id, Title, ReleaseDate.Value, ContainsExplicitContent, AlbumId!,
            _mainArtists, _featuredArtists, _genres, _moods));
    }

    public void Unpublish()
    {
        if (!Status.IsPublished)
        {
            return;
        }

        ReleaseDate = null;
        Status = TrackStatus.ReadyToPublish;
        RaiseDomainEvent(new TrackUnpublishedDomainEvent(Id));
    }

    public void PrepareForDeletion()
    {
        if (Status.IsPublished)
        {
            throw new TrackAlreadyPublishedDomainException("Cannot delete a published track.");
        }
        
        RaiseDomainEvent(new TrackDeletedDomainEvent(Id, AlbumId, AudioFileId));
    }

    public void CorrectTitle(string title)
    {
        if (title == Title)
        {
            return;
        }
        
        TrackTitleRules.Validate(title);
        Title = title;

        RaiseDomainEvent(new TrackTitleCorrectedDomainEvent(Id, Title));
    }

    public void RescheduleRelease(DateTimeOffset releaseDate)
    {
        releaseDate = releaseDate.ToUniversalTime();

        if (!Status.IsPublished)
        {
            throw new TrackNotPublishedDomainException("Cannot reschedule the release of an unpublished track.");
        }

        if (ReleaseDate <= DateTimeOffset.UtcNow)
        {
            throw new InvalidTrackReleaseDateDomainException(
                "Cannot reschedule the release of a track that has been already released.");
        }

        if (releaseDate < DateTimeOffset.UtcNow.AddMinutes(-1))
        {
            throw new InvalidTrackReleaseDateDomainException("Track release date cannot be in the past.");
        }

        ReleaseDate = releaseDate;

        RaiseDomainEvent(new TrackReleaseRescheduledDomainEvent(Id, ReleaseDate.Value));
    }

    public void MarkAsExplicit()
    {
        if (!ContainsExplicitContent)
        {
            ContainsExplicitContent = true;
            RaiseDomainEvent(new TrackMarkedAsExplicitDomainEvent(Id));
        }
    }

    public void MarkAsNotExplicit()
    {
        if (ContainsExplicitContent)
        {
            ContainsExplicitContent = false;
            RaiseDomainEvent(new TrackMarkedAsNotExplicitDomainEvent(Id));
        }
    }

    public void UpdateMainArtists(IReadOnlyCollection<ArtistId> artists)
    {
        ArgumentNullException.ThrowIfNull(artists);

        if (Status.IsPublished)
        {
            throw new TrackAlreadyPublishedDomainException(
                "Cannot update main artists of a published track.");
        }

        if (artists.Count <= 0)
        {
            throw new InvalidTrackMainArtistsDomainException(
                "A track must have at least one main artist.");
        }

        _mainArtists.Clear();

        foreach (ArtistId artistId in artists)
        {
            _mainArtists.Add(artistId);
        }

        RaiseDomainEvent(new TrackArtistsUpdatedDomainEvent(Id, _mainArtists, _featuredArtists));
    }

    public void UpdateFeaturedArtists(IReadOnlyCollection<ArtistId> artists)
    {
        ArgumentNullException.ThrowIfNull(artists);

        if (Status.IsPublished)
        {
            throw new TrackAlreadyPublishedDomainException(
                "Cannot update featured artists of a published track.");
        }

        _featuredArtists.Clear();

        foreach (ArtistId artistId in artists)
        {
            _featuredArtists.Add(artistId);
        }

        RaiseDomainEvent(new TrackArtistsUpdatedDomainEvent(Id, _mainArtists, _featuredArtists));
    }

    public void UpdateGenres(IReadOnlyCollection<GenreId> genres)
    {
        ArgumentNullException.ThrowIfNull(genres);

        if (Status.IsPublished)
        {
            throw new TrackAlreadyPublishedDomainException(
                "Cannot update genres of a published track.");
        }

        if (genres.Count <= 0)
        {
            throw new InvalidTrackMainArtistsDomainException(
                "A track must have at least one genre.");
        }

        _genres.Clear();

        foreach (GenreId genreId in genres)
        {
            _genres.Add(genreId);
        }

        RaiseDomainEvent(new TrackGenresUpdatedDomainEvent(Id, _genres));
    }

    public void RemoveGenre(GenreId genreId)
    {
        ArgumentNullException.ThrowIfNull(genreId);

        if (Status.IsPublished)
        {
            throw new TrackAlreadyPublishedDomainException(
                "Cannot remove genre from a published track.");
        }

        _genres.Remove(genreId);

        RaiseDomainEvent(new TrackGenresUpdatedDomainEvent(Id, _genres));
    }

    public void UpdateMoods(IReadOnlyCollection<MoodId> moods)
    {
        ArgumentNullException.ThrowIfNull(moods);

        if (Status.IsPublished)
        {
            throw new TrackAlreadyPublishedDomainException(
                "Cannot update moods of a published track.");
        }

        if (moods.Count <= 0)
        {
            throw new InvalidTrackMainArtistsDomainException(
                "A track must have at least one mood.");
        }

        _moods.Clear();

        foreach (MoodId moodId in moods)
        {
            _moods.Add(moodId);
        }

        RaiseDomainEvent(new TrackMoodsUpdatedDomainEvent(Id, _moods));
    }

    public void RemoveMood(MoodId moodId)
    {
        ArgumentNullException.ThrowIfNull(moodId);

        if (Status.IsPublished)
        {
            throw new TrackAlreadyPublishedDomainException(
                "Cannot remove mood from a published track.");
        }

        _moods.Remove(moodId);

        RaiseDomainEvent(new TrackMoodsUpdatedDomainEvent(Id, _moods));
    }

    private Track(
        TrackId id,
        string title,
        TimeSpan? duration,
        DateTimeOffset? releaseDate,
        bool containsExplicitContent,
        TrackStatus status,
        AudioFileId? audioFileId,
        AlbumId? albumId,
        IEnumerable<ArtistId> mainArtists,
        IEnumerable<ArtistId> featuredArtists,
        IEnumerable<GenreId> genres,
        IEnumerable<MoodId> moods)
        : base(id)
    {
        Title = title;
        Duration = duration;
        ReleaseDate = releaseDate;
        ContainsExplicitContent = containsExplicitContent;
        Status = status;
        AudioFileId = audioFileId;
        AlbumId = albumId;
        _mainArtists = mainArtists.ToHashSet();
        _featuredArtists = featuredArtists.ToHashSet();
        _genres = genres.ToHashSet();
        _moods = moods.ToHashSet();
    }

    private Track()
    {
    }
}
