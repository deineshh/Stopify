using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Catalog.Application.Features.Moods.Queries.List;

internal sealed class ListMoodsQueryHandler(
    IMoodReadService moodReadService)
    : IQueryHandler<ListMoodsQuery, MoodList>
{
    private readonly IMoodReadService _moodReadService = moodReadService;

    public async Task<Result<MoodList>> Handle(
        ListMoodsQuery request,
        CancellationToken cancellationToken)
    {
        PagedList<MoodSummary> moods = await _moodReadService.ListAsync(
            request.Pagination, cancellationToken);

        return new MoodList(moods);
    }
}
