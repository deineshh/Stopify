using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Domain.Aggregates.Tracks.Events;

public sealed record TrackArtistsUpdatedDomainEvent(
    TrackId Id,
    IEnumerable<ArtistId> MainArtists,
    IEnumerable<ArtistId> FeaturedArtists)
    : DomainEvent;
