using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;

namespace SpotifyClone.Catalog.Application.Features.Tracks.Queries.ListByIds;

public sealed record ListTracksByIdsQuery(
    IEnumerable<Guid> TrackIds)
    : IQuery<TrackList>;
