using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Catalog.Application.Features.Genres.Queries;

public sealed record GenreList(
    PagedList<GenreSummary> Genres);
