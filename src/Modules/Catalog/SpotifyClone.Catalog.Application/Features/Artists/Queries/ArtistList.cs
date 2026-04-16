using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Catalog.Application.Features.Artists.Queries;

public sealed record ArtistList(
    PagedList<ArtistSummary> Artists);
