using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Catalog.Application.Features.Albums.Queries;

public sealed record AlbumList(
    PagedList<AlbumSummary> Albums);
