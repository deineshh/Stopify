namespace SpotifyClone.Search.Infrastructure.Services;

public sealed record MeiliSearchOptions
{
    public const string SectionName = "MeiliSearch";
    public required string Endpoint { get; init; }
    public required string MasterKey { get; init; }
}
