using SpotifyClone.Catalog.Domain.Aggregates.Albums.Entities;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.Enums;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.Events;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.Exceptions;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.Rules;
using SpotifyClone.Catalog.Domain.Aggregates.Albums.ValueObjects;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Domain.Aggregates.Albums;

public sealed class Album : AggregateRoot<AlbumId, Guid>
{
    private const int PositionStep = 1000;

    private readonly HashSet<ArtistId> _mainArtists = [];
    private readonly HashSet<AlbumTrack> _tracks = [];

    public string Title { get; private set; } = null!;
    public DateTimeOffset? ReleaseDate { get; private set; }
    public AlbumStatus Status { get; private set; } = null!;
    public AlbumType Type { get; private set; } = null!;
    public AlbumCoverImage? Cover { get; private set; }
    public IReadOnlySet<ArtistId> MainArtists => _mainArtists.AsReadOnly();
    public IReadOnlySet<AlbumTrack> Tracks => _tracks;

    public static Album Create(AlbumId id, string title, IEnumerable<ArtistId> mainArtists)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(mainArtists);

        AlbumTitleRules.Validate(title);

        if (!mainArtists.Any())
        {
            throw new InvalidAlbumMainArtistsDomainException(
                "An album must have at least one main artist.");
        }

        return new Album(id, title, null, AlbumStatus.Draft, AlbumType.Empty, null, mainArtists);
    }

    public void LinkNewCover(AlbumCoverImage cover)
    {
        ArgumentNullException.ThrowIfNull(cover);

        if (Status.IsPublished)
        {
            throw new AlbumAlreadyPublishedDomainException("Cannot link a new cover to a published album.");
        }

        TryUnlinkCover();

        Cover = cover;
        RaiseDomainEvent(new AlbumLinkedToCoverImageDomainEvent(
            Id, Cover.ImageId,
            _tracks.Select(t => t.Id)));
    }

    public void TryUnlinkCover()
    {
        if (Cover is null)
        {
            return;
        }

        RaiseDomainEvent(new AlbumUnlinkedFromCoverImageDomainEvent(
            Id, Cover.ImageId,
            _tracks.Select(t => t.Id)));

        Cover = null;
    }

    public void Publish(DateTimeOffset releaseDate)
    {
        releaseDate = releaseDate.ToUniversalTime();

        if (Status.IsPublished)
        {
            throw new AlbumAlreadyPublishedDomainException("This album has been already published.");
        }

        if (!Status.IsReadyToPublish)
        {
            throw new CannotPublishAlbumDomainException("Album is not ready to publish.");
        }

        if (releaseDate < DateTimeOffset.UtcNow.AddMinutes(-1))
        {
            throw new InvalidAlbumReleaseDateDomainException("Album release date cannot be in the past.");
        }

        ReleaseDate = releaseDate;
        Status = AlbumStatus.Published;

        RaiseDomainEvent(new AlbumPublishedDomainEvent(
            Id, Title, ReleaseDate.Value, Type, Cover!.ImageId,
            _mainArtists, Enumerable.Empty<ArtistId>(),
            _tracks.Select(t => t.Id)));
    }

    public void Unpublish()
    {
        if (!Status.IsPublished)
        {
            return;
        }

        ReleaseDate = null;

        RaiseDomainEvent(new AlbumUnpublishedDomainEvent(Id));
    }

    public void UpdateMainArtists(IReadOnlyCollection<ArtistId> mainArtists)
    {
        ArgumentNullException.ThrowIfNull(mainArtists);

        if (Status.IsPublished)
        {
            throw new AlbumAlreadyPublishedDomainException("Cannot update main artists of a published album.");
        }

        if (mainArtists.Count <= 0)
        {
            throw new InvalidAlbumMainArtistsDomainException(
                "An album must have at least one main artist.");
        }

        _mainArtists.Clear();

        foreach (ArtistId artistId in mainArtists)
        {
            _mainArtists.Add(artistId);
        }

        RaiseDomainEvent(new AlbumMainArtistsUpdatedDomainEvent(Id, MainArtists));
    }

    public void RemoveMainArtist(ArtistId artistId)
    {
        ArgumentNullException.ThrowIfNull(artistId);

        if (!_mainArtists.Remove(artistId))
        {
            throw new MainArtistNotFoundInAlbumDomainException(
                $"Cannot remove main artist '{artistId.Value}' from the album, " +
                $"because it was not found in the album.");
        }

        if (_mainArtists.Count <= 0 && Status.IsPublished)
        {
            Unpublish();
        }

        RaiseDomainEvent(new AlbumMainArtistsUpdatedDomainEvent(Id, MainArtists));
    }

    public void AddTrack(TrackId trackId)
    {
        ArgumentNullException.ThrowIfNull(trackId);

        if (Status.IsPublished)
        {
            throw new AlbumAlreadyPublishedDomainException("Cannot add track to a published album.");
        }

        int nextPosition = _tracks.Count != 0
            ? _tracks.Max(t => t.Position) + PositionStep
            : PositionStep;

        var track = new AlbumTrack(trackId, nextPosition);

        if (_tracks.Any(t => t.Id == track.Id) || !_tracks.Add(track))
        {
            throw new InvalidTrackInAlbumDomainException(
                "Cannot add the same track to the playlist more than once.");
        }
        
        RaiseDomainEvent(new TrackAddedToAlbumDomainEvent(Id, trackId));
    }

    public void RemoveTrack(TrackId trackId)
    {
        ArgumentNullException.ThrowIfNull(trackId);

        if (Status.IsPublished)
        {
            throw new AlbumAlreadyPublishedDomainException("Cannot remove track from a published album.");
        }
        
        AlbumTrack? track = _tracks.FirstOrDefault(t => t.Id == trackId);
        if (track is null || !_tracks.Remove(track!))
        {
            throw new InvalidTrackInAlbumDomainException(
                $"Cannot remove track '{trackId.Value}' from the album, because it was not found in the album.");
        }

        RaiseDomainEvent(new TrackRemovedFromAlbumDomainEvent(Id, trackId));
    }

    public void MoveTrack(TrackId trackId, int targetIndex)
    {
        if (Status.IsPublished)
        {
            throw new AlbumAlreadyPublishedDomainException(
                "Cannot move track to a new position in a published album.");
        }

        AlbumTrack trackToMove = _tracks.SingleOrDefault(t => t.Id == trackId)
            ?? throw new InvalidTrackInAlbumDomainException(trackId.Value.ToString());

        // 1. Get current sorted list to identify neighbors
        var sortedTracks = _tracks.OrderBy(t => t.Position).ToList();
        sortedTracks.Remove(trackToMove); // Remove it from current spot to simulate the "gap"

        // Clamp the index to ensure it's within bounds
        targetIndex = Math.Clamp(targetIndex, 0, sortedTracks.Count);

        int newPosition;

        // 2. Calculate Midpoint
        if (targetIndex == 0)
        {
            // Moving to the very top
            int firstPos = sortedTracks.Count > 0 ? sortedTracks[0].Position : PositionStep;
            newPosition = firstPos / 2;
        }
        else if (targetIndex >= sortedTracks.Count)
        {
            // Moving to the very bottom
            int lastPos = sortedTracks[^1].Position;
            newPosition = lastPos + PositionStep;
        }
        else
        {
            // Moving between two tracks
            int prevPos = sortedTracks[targetIndex - 1].Position;
            int nextPos = sortedTracks[targetIndex].Position;
            newPosition = (prevPos + nextPos) / 2;

            // 3. Collision Detection (The "Rebalance" Trigger)
            if (newPosition == prevPos || newPosition == nextPos)
            {
                RebalanceTrackPositions();
                // Recurse once after rebalancing to find the new midpoint in a fresh 1000-step list
                MoveTrack(trackId, targetIndex);
                return;
            }
        }

        trackToMove.ChangePosition(newPosition);
    }

    private void RebalanceTrackPositions()
    {
        var sorted = _tracks.OrderBy(t => t.Position).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].ChangePosition((i + 1) * PositionStep);
        }
    }

    public void CorrectTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        AlbumTitleRules.Validate(title);
        Title = title;

        RaiseDomainEvent(new AlbumTitleCorrectedDomainEvent(Id, Title));
    }

    public void RescheduleRelease(DateTimeOffset releaseDate)
    {
        releaseDate = releaseDate.ToUniversalTime();

        if (!Status.IsPublished)
        {
            throw new AlbumNotPublishedDomainException("Cannot reschedule the release of an unpublished album.");
        }

        if (ReleaseDate <= DateTimeOffset.UtcNow)
        {
            throw new InvalidAlbumReleaseDateDomainException(
                "Cannot reschedule the release of an album that has been already released.");
        }

        if (releaseDate < DateTimeOffset.UtcNow.AddMinutes(-1))
        {
            throw new InvalidAlbumReleaseDateDomainException("Album release date cannot be in the past.");
        }

        ReleaseDate = releaseDate;
        RaiseDomainEvent(new AlbumReleaseRescheduledDomainEvent(Id, ReleaseDate.Value));
    }

    public void PrepareForDeletion()
    {
        if (Status.IsPublished)
        {
            throw new AlbumAlreadyPublishedDomainException("Cannot delete a published album.");
        }

        if (_tracks.Count > 0)
        {
            RaiseDomainEvent(new AlbumDeletedDomainEvent(Id));
        }

        TryUnlinkCover();
    }

    internal void MarkAsDraft()
        => Status = AlbumStatus.Draft;

    internal bool TryMarkAsReadyToPublish()
    {
        if (Cover is null)
        {
            return false;
        }

        Status = AlbumStatus.ReadyToPublish;
        return true;
    }

    internal void ReevaluateType(int playableTrackCount)
        => Type = AlbumType.From(playableTrackCount);

    private Album(
        AlbumId id, string title, DateTimeOffset? releaseDate, AlbumStatus status, AlbumType type,
        AlbumCoverImage? cover, IEnumerable<ArtistId> mainArtists)
        : base(id)
    {
        Title = title;
        ReleaseDate = releaseDate;
        Status = status;
        Type = type;
        Cover = cover;
        _mainArtists = mainArtists.ToHashSet();
    }

    private Album()
    {
    }
}
