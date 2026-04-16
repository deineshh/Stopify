namespace SpotifyClone.Search.Application.Abstractions.Services;

public interface ISearchIndexer
{
    Task IndexDocumentAsync<T>(
        string indexName,
        T document,
        CancellationToken cancellationToken = default);

    Task IndexDocumentsAsync<T>(
        string indexName,
        IEnumerable<T> documents,
        CancellationToken cancellationToken);

    Task DeleteDocumentAsync(
        string indexName,
        string documentId,
        CancellationToken cancellationToken = default);

    Task DeleteDocumentsAsync(
        string indexName,
        IEnumerable<string> documentIds,
        CancellationToken cancellationToken = default);
}
