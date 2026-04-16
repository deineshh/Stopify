namespace SpotifyClone.Api.Contracts.v1.Search.GlobalSearch;

public sealed record GlobalSearchRequest
{
    public required string SearchTerm { get; init; }
    public int Limit { get; init; } = 20;
}
