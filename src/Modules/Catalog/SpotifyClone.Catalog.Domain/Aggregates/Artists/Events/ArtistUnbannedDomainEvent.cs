using SpotifyClone.Catalog.Domain.Aggregates.Artists.Enums;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Domain.Aggregates.Artists.Events;

public sealed record ArtistUnbannedDomainEvent(
    ArtistId Id,
    string Name,
    ArtistStatus Status,
    ImageId? AvatarImageId)
    : DomainEvent;
