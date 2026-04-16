using Meilisearch;
using Microsoft.Extensions.Options;
using SpotifyClone.Search.Application.Abstractions.Services;

namespace SpotifyClone.Search.Infrastructure.Services;

public class MeiliSearchService(
    IOptions<MeiliSearchOptions> options)
    : ISearchIndexer, ISearchProvider
{
    private readonly MeilisearchClient _client = new(
        options.Value.Endpoint, options.Value.MasterKey);

    public async Task IndexDocumentAsync<T>(
        string indexName,
        T document,
        CancellationToken cancellationToken = default)
    {
        Meilisearch.Index index = _client.Index(indexName);

        await index.AddDocumentsAsync(
            [document], cancellationToken: cancellationToken);
    }

    public async Task IndexDocumentsAsync<T>(
        string indexName,
        IEnumerable<T> documents,
        CancellationToken cancellationToken)
    {
        Meilisearch.Index index = _client.Index(indexName);
        await index.AddDocumentsAsync(documents, cancellationToken: cancellationToken);
    }

    public async Task<Application.Models.SearchResult<T>> SearchAsync<T>(
        string indexName,
        string? filter = null,
        string? query = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        Meilisearch.Index index = _client.Index(indexName);

        var searchParams = new SearchQuery
        {
            Limit = limit,
            Filter = filter,
        };
        ISearchable<T> searchResponse = await index.SearchAsync<T>(
            query,
            searchParams,
            cancellationToken);

        return new Application.Models.SearchResult<T>(
            searchResponse.Hits,
            searchResponse.Hits.Count);
    }

    public async Task DeleteDocumentAsync(
        string indexName,
        string documentId,
        CancellationToken cancellationToken = default)
    {
        Meilisearch.Index index = _client.Index(indexName);
        await index.DeleteOneDocumentAsync(documentId, cancellationToken);
    }

    public async Task DeleteDocumentsAsync(
        string indexName,
        IEnumerable<string> documentIds,
        CancellationToken cancellationToken = default)
    {
        Meilisearch.Index index = _client.Index(indexName);
        await index.DeleteDocumentsAsync(documentIds, cancellationToken);
    }
}
