using SpotifyClone.Catalog.Application.Features.Artists.Queries;
using SpotifyClone.Catalog.Application.Models;

namespace SpotifyClone.Catalog.Application.Features.Albums.Queries;

public sealed record AlbumSummary(
    Guid Id,
    string Title,
    DateTimeOffset? ReleaseDateUtc,
    string Status,
    string Type,
    ImageMetadataDetails? Cover,
    IEnumerable<ArtistSummary> MainArtists);
