using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Catalog.Application.Errors;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Application.Features.Tracks.Queries.GetSummary;

internal sealed class GetTrackSummaryQueryHandler(
    ITrackReadService trackReadService)
    : IQueryHandler<GetTrackSummaryQuery, TrackSummary>
{
    private readonly ITrackReadService _trackReadService = trackReadService;

    public async Task<Result<TrackSummary>> Handle(
        GetTrackSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var trackId = TrackId.From(request.TrackId);

        TrackSummary? track = await _trackReadService.GetSummaryAsync(trackId, cancellationToken);
        if (track is null)
        {
            return Result.Failure<TrackSummary>(TrackErrors.NotFound);
        }

        return track;
    }
}
