using SpotifyClone.Playlists.Domain.Aggregates.Playlists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;

namespace SpotifyClone.Playlists.Domain.Aggregates.Playlists.Events;

public sealed record PlaylistDeletedDomainEvent(
    PlaylistId Id)
    : DomainEvent;
