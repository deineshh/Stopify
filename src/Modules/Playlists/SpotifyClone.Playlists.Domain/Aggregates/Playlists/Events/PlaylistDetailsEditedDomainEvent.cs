using SpotifyClone.Playlists.Domain.Aggregates.Playlists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Playlists.Domain.Aggregates.Playlists.Events;

public sealed record PlaylistDetailsEditedDomainEvent(
    PlaylistId Id,
    string Name,
    bool IsPublic,
    UserId OwnerId,
    ImageId? CoverImageId)
    : DomainEvent;
