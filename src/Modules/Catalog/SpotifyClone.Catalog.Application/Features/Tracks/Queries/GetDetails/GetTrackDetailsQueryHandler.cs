using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Catalog.Application.Errors;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;
using SpotifyClone.Shared.Kernel.IDs;

namespace SpotifyClone.Catalog.Application.Features.Tracks.Queries.GetDetails;

internal sealed class GetTrackDetailsQueryHandler(
    ITrackReadService trackReadService)
    : IQueryHandler<GetTrackDetailsQuery, TrackDetails>
{
    private readonly ITrackReadService _trackReadService = trackReadService;

    public async Task<Result<TrackDetails>> Handle(
        GetTrackDetailsQuery request,
        CancellationToken cancellationToken)
    {
        TrackDetails? track = await _trackReadService.GetDetailsAsync(
            TrackId.From(request.TrackId),
            cancellationToken);
        if (track is null)
        {
            return Result.Failure<TrackDetails>(TrackErrors.NotFound);
        }

        return track;
    }
}
