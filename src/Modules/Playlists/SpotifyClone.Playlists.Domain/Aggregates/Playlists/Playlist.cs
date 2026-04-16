using SpotifyClone.Playlists.Domain.Aggregates.Playlists.Entities;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.Enums;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.Events;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.Exceptions;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.Rules;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Playlists.Domain.Aggregates.Playlists;

public sealed class Playlist : AggregateRoot<PlaylistId, Guid>
{
    private const int PositionStep = 1000;

    private readonly HashSet<UserId> _collaborators = [];
    private readonly HashSet<PlaylistTrack> _tracks = [];

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public UserId OwnerId { get; private set; } = null!;
    public PlaylistCoverImage? Cover { get; private set; }
    public PlaylistType Type { get; private set; }
    public bool IsPublic { get; private set; }
    public IReadOnlySet<UserId> Collaborators => _collaborators;
    public IReadOnlySet<PlaylistTrack> Tracks => _tracks;

    public static Playlist Create(
        PlaylistId id,
        UserId ownerId,
        int ownerPlaylistCount)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(ownerId);

        return new Playlist(
            id, $"My Playlist #{ownerPlaylistCount + 1}",
            null, ownerId, null, PlaylistType.UserCreated, false);
    }

    public static Playlist CreateSystemPlaylist(
        PlaylistId id,
        UserId ownerId,
        string name,
        string? description,
        PlaylistType type)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(ownerId);

        return new Playlist(id, name, description, ownerId, null, type, false);
    }

    public void LinkNewCover(PlaylistCoverImage cover, bool byAdmin)
    {
        ArgumentNullException.ThrowIfNull(cover);

        if (Type != PlaylistType.UserCreated && !byAdmin)
        {
            throw new PlaylistIsNotUserGeneratedDomainException(
                "System playlists cannot have custom covers.");
        }

        UnlinkCover();

        Cover = cover;
        RaiseDomainEvent(new PlaylistLinkedToCoverImageDomainEvent(Id, Cover.ImageId));
    }

    public void UnlinkCover()
    {
        if (Cover is null)
        {
            return;
        }

        RaiseDomainEvent(new PlaylistUnlinkedFromCoverImageDomainEvent(Id, Cover.ImageId));
        Cover = null;
    }

    public void EditDetails(string name, string? description, bool isPublic, bool byAdmin)
    {
        if (Type != PlaylistType.UserCreated && !byAdmin)
        {
            throw new PlaylistIsNotUserGeneratedDomainException(
                "Details cannot be modified in a system playlist.");
        }

        PlaylistNameRules.Validate(name);
        PlaylistDescriptionRules.Validate(description);
        Name = name;
        Description = description;
        IsPublic = isPublic;

        RaiseDomainEvent(new PlaylistDetailsEditedDomainEvent(Id, Name, IsPublic, OwnerId, Cover?.ImageId));
    }

    public void AddCollaborator(UserId collaboratorId)
    {
        if (OwnerId == collaboratorId)
        {
            return;
        }

        if (Type != PlaylistType.UserCreated)
        {
            throw new PlaylistIsNotUserGeneratedDomainException(
                "Collaborators cannot be added to a system playlist.");
        }

        if (_collaborators.Count >= 1000)
        {
            throw new InvalidPlaylistCollaboratorDomainException(
                "Cannot add more than 1000 collaborators to a playlist.");
        }

        if (!_collaborators.Add(collaboratorId))
        {
            throw new InvalidPlaylistCollaboratorDomainException(
                $"Collaborator {collaboratorId.Value} was not found in the playlist.");
        }
        
        // Domain event can be raised here if needed
    }

    public void RemoveCollaborator(UserId collaboratorId)
    {
        if (!_collaborators.Remove(collaboratorId))
        {
            throw new InvalidPlaylistCollaboratorDomainException(
                $"Cannot remove collaborator {collaboratorId.Value} from the playlist, " +
                $"because they were not found in the playlist.");
        }

        // Domain event can be raised here if needed
    }

    public void AddTrack(TrackId trackId, UserId addedBy, bool isAdmin)
    {
        ArgumentNullException.ThrowIfNull(trackId);

        if (!isAdmin && addedBy != OwnerId && !_collaborators.Contains(addedBy))
        {
            throw new InvalidPlaylistCollaboratorDomainException(
                "No such owner or collaborator exists in this playlist.");
        }

        int nextPosition = _tracks.Count != 0
            ? _tracks.Max(t => t.Position) + PositionStep
            : PositionStep;

        var track = new PlaylistTrack(Id, trackId, addedBy, nextPosition);

        if (_tracks.Any(t => t.Id == track.Id) || !_tracks.Add(track))
        {
            throw new InvalidPlaylistTrackDomainException(
                "Cannot add the same track to the playlist more than once.");
        }

        RaiseDomainEvent(new TrackAddedToPlaylistDomainEvent(Id));
    }

    public void RemoveTrack(TrackId trackId, UserId removedBy, bool isAdmin)
    {
        ArgumentNullException.ThrowIfNull(trackId);

        if (!isAdmin && removedBy != OwnerId && !_collaborators.Contains(removedBy))
        {
            throw new InvalidPlaylistCollaboratorDomainException(
                $"No such owner or collaborator {removedBy.Value} exists in this playlist.");
        }

        PlaylistTrack? track = _tracks.FirstOrDefault(t => t.Id == trackId)
            ?? throw new InvalidPlaylistTrackDomainException(
                $"Track {trackId.Value} was not found in the playlist.");

        if (removedBy != OwnerId && track.CreatorId != removedBy && !isAdmin)
        {
            throw new InvalidPlaylistCollaboratorDomainException(
                "Only the owner or the user who added the track can remove it from the playlist.");
        }

        if (!_tracks.Remove(track!))
        {
            throw new InvalidPlaylistTrackDomainException(
                $"Cannot remove track '{trackId.Value}' from the playlist, " +
                $"because it was not found in the playlist.");
        }

        RaiseDomainEvent(new TrackRemovedFromPlaylistDomainEvent(Id));
    }

    public void MoveTrack(TrackId trackId, int targetIndex)
    {
        PlaylistTrack trackToMove = _tracks.SingleOrDefault(t => t.Id == trackId)
            ?? throw new InvalidPlaylistTrackDomainException(
                $"Track {trackId.Value.ToString()} not found in the playlist");

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

    public void PrepareForDeletion(bool byAdmin)
    {
        if (Type != PlaylistType.UserCreated && !byAdmin)
        {
            throw new PlaylistIsNotUserGeneratedDomainException(
                "System playlists cannot be deleted.");
        }

        UnlinkCover();

        RaiseDomainEvent(new PlaylistDeletedDomainEvent(Id));
    }

    private Playlist(
        PlaylistId id,
        string name,
        string? description,
        UserId ownerId,
        PlaylistCoverImage? cover,
        PlaylistType type,
        bool isPublic)
        : base(id)
    {
        Name = name;
        Description = description;
        OwnerId = ownerId;
        Cover = cover;
        Type = type;
        IsPublic = isPublic;
    }

    private Playlist()
    {
    }
}
