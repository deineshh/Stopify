using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Queries;

namespace SpotifyClone.Search.Application.Features.GlobalSearch;

public sealed record GlobalSearchQuery(
    string SearchTerm,
    int Limit = 20)
    : IQuery<GlobalSearchResponse>;
