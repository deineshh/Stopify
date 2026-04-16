using SpotifyClone.Search.Application.Models;

namespace SpotifyClone.Search.Application.Abstractions.Services;

public interface ISearchProvider
{
    Task<SearchResult<T>> SearchAsync<T>(
        string indexName,
        string? filter = null,
        string? query = null,
        int? limit = null,
        CancellationToken cancellationToken = default);
}
