using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Catalog.Application.Features.Tracks.Queries;

public sealed record TrackList(
    PagedList<TrackSummary> Tracks);
