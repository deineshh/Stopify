using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Catalog.Application.Features.Moods.Queries;

public sealed record MoodList(
    PagedList<MoodSummary> Moods);
