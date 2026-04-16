using SpotifyClone.Catalog.Domain.Aggregates.Albums.ValueObjects;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Catalog.Domain.Aggregates.Genres.ValueObjects;
using SpotifyClone.Catalog.Domain.Aggregates.Moods.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Domain.Aggregates.Tracks.Events;

public sealed record TrackPublishedDomainEvent(
    TrackId Id,
    string Title,
    DateTimeOffset ReleaseDateUtc,
    bool ContainsExplicitContent,
    AlbumId AlbumId,
    IEnumerable<ArtistId> MainArtists,
    IEnumerable<ArtistId> FeaturedArtists,
    IEnumerable<GenreId> Genres,
    IEnumerable<MoodId> Moods)
    : DomainEvent;
