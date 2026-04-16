namespace SpotifyClone.Search.Application.Models;

public sealed record SearchResult<T>(
    IReadOnlyCollection<T> Items,
    long TotalCount);
